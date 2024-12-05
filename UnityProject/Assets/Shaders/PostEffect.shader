Shader "MTE/PostEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend Off
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma multi_compile __ DEBUG_VIEW
            #pragma multi_compile __ RIMLIGHT
            #pragma multi_compile __ RIMLIGHT_EDGE
            #pragma multi_compile __ RIMLIGHT_HEIGHT
            #pragma multi_compile __ DISTANCE_FOG
            #pragma multi_compile __ PARAFFIN
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            sampler2D _CameraDepthTexture;

            #if RIMLIGHT
            sampler2D _CameraDepthNormalsTexture;
            #endif

            #if RIMLIGHT
            struct RimlightBuffer
            {
                float4 color1;
                float4 color2;
                float3 direction;
                float lightArea;
                float fadeRange;
                float fadeExp;
                float depthMin;
                float depthMax;
                float depthFade;
                float useNormal;
                float useAdd;
                float useMultiply;
                float useOverlay;
                float useSubstruct;
                float edgeDepth;
                float2 edgeRange;
                float heightMin;
            };

            float4x4 _CameraToWorldMatrix;

            StructuredBuffer<RimlightBuffer> _RimlightBuffer;
            int _RimlightCount;
            #endif

            #if DISTANCE_FOG
            float4 _DistanceFogColor1;
            float4 _DistanceFogColor2;
            float _DistanceFogStart;
            float _DistanceFogEnd;
            float _DistanceFogExp;
            #endif

            #if PARAFFIN
            struct ParaffinBuffer
            {
                float4 color1;
                float4 color2;
                float2 centerPosition;
                float radiusFar;
                float radiusNear;
                float2 radiusScale;
                float useNormal;
                float useAdd;
                float useMultiply;
                float useOverlay;
                float useSubstruct;
                float depthMin;
                float depthMax;
                float depthFade;
            };

            StructuredBuffer<ParaffinBuffer> _ParaffinBuffer;
            int _ParaffinCount;
            #endif

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 BlendOverlay(float4 base, float4 blend)
            {
                float4 result;
                result.rgb = base.rgb < 0.5 ? 
                    2.0 * base.rgb * blend.rgb :
                    1.0 - 2.0 * (1.0 - base.rgb) * (1.0 - blend.rgb);
                result.a = base.a;
                return result;
            }

            float4 CalculateBlend(
                float4 src,
                float4 dst,
                float4 useNormal,
                float4 useAdd,
                float4 useMultiply,
                float4 useOverlay,
                float4 useSubstruct)
            {
                float4 blend = float4(0, 0, 0, 0);
                blend += (dst - src) * useNormal;
                blend += (dst) * useAdd;
                blend += (src * dst - src) * useMultiply;
                blend += (BlendOverlay(src, dst) - src) * useOverlay;
                blend += (-dst) * useSubstruct;
                return blend * dst.a;
            }

            float4 CalculateBlendDebug(
                float4 src,
                float4 dst,
                float4 useNormal,
                float4 useAdd,
                float4 useMultiply,
                float4 useOverlay,
                float4 useSubstruct)
            {
                float4 blend = dst;

                float useFactor = useNormal + useAdd + useMultiply + useMultiply + useOverlay + useSubstruct;

                blend.rgb *= blend.a * useFactor;
                return blend;
            }

            float SampleDepth(float2 uv)
            {
                return LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
            }

            #if RIMLIGHT

            #if RIMLIGHT_EDGE
            float Rimlight_CalculateEdgeFactor(RimlightBuffer data, float2 uv)
            {
                float2 range = data.edgeRange;

                float d0 = SampleDepth(uv + float2(0, 0));
                float m0 = 0;
                float rangeFactor = 0.5;

                UNITY_UNROLL
                for (int i = 1; i <= 2; i++)
                {
                    float d1 = SampleDepth(uv + float2(range.x, range.y) * i * rangeFactor);
                    float d2 = SampleDepth(uv + float2(-range.x, -range.y) * i * rangeFactor);
                    float d3 = SampleDepth(uv + float2(-range.x, range.y) * i * rangeFactor);
                    float d4 = SampleDepth(uv + float2(range.x, -range.y) * i * rangeFactor);

                    m0 = max(m0, max(max(d1, d2), max(d3, d4)));
                }

                return (m0 - d0 > data.edgeDepth) ? 1.0 : 0.0;
            }
            #endif

            float Rimlight_CalculateDepthFactor(RimlightBuffer data, float depth)
            {
                float minFactor = smoothstep(data.depthMin - data.depthFade, data.depthMin, depth);
                float maxFactor = 1.0 - smoothstep(data.depthMax, data.depthMax + data.depthFade, depth);
                return minFactor * maxFactor;
            }

            #if RIMLIGHT_HEIGHT
            float Rimlight_CalculateHeightFactor(RimlightBuffer data, float2 screenUV)
            {
                float depth = SampleDepth(screenUV);
                float4 ndcPos = float4(screenUV * 2 - 1, 1, 1);
                float4 viewPos = mul(unity_CameraInvProjection, ndcPos);
                viewPos.xyz *= depth;
                viewPos.w = 1.0;

                float3 worldPos = mul(_CameraToWorldMatrix, viewPos).xyz;

                return step(data.heightMin, worldPos.y);
            }
            #endif

            float4 Rimlight_CalculateBlend(float4 src, RimlightBuffer data, float2 uv, float depth, float3 normal)
            {
                float rimFactor = 1.0 - dot(normal, data.direction);
                float basicRim = smoothstep(data.lightArea - data.fadeRange, data.lightArea, rimFactor);
                float rimIntensity = pow(basicRim, data.fadeExp);
                float4 rimColor = lerp(data.color2, data.color1, rimIntensity);

                #if DEBUG_VIEW
                return CalculateBlendDebug(src, rimColor, data.useNormal, data.useAdd, data.useMultiply, data.useOverlay, data.useSubstruct);
                #else
                return CalculateBlend(src, rimColor, data.useNormal, data.useAdd, data.useMultiply, data.useOverlay, data.useSubstruct);
                #endif
            }

            float4 Rimlight_frag(v2f i, float4 src)
            {
                float4 dst = src;
                float3 normal;
                float depth;
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.uv), depth, normal);

                [loop]
                for(int idx = 0; idx < _RimlightCount; idx++)
                {
                    float4 blend = Rimlight_CalculateBlend(dst, _RimlightBuffer[idx], i.uv, depth, normal);

                    blend *= Rimlight_CalculateDepthFactor(_RimlightBuffer[idx], depth);

                    #if RIMLIGHT_EDGE
                    blend *= Rimlight_CalculateEdgeFactor(_RimlightBuffer[idx], i.uv);
                    #endif

                    #if RIMLIGHT_HEIGHT
                    blend *= Rimlight_CalculateHeightFactor(_RimlightBuffer[idx], i.uv);
                    #endif

                    dst += blend;
                }

                dst.a = src.a;

                return dst;
            }
            #endif

            #if DISTANCE_FOG
            float4 DistanceFog_CalculateFog(float4 src, float depth)
            {
                float range = _DistanceFogEnd - _DistanceFogStart;
                float fogFactor = pow(saturate((depth - _DistanceFogStart) / range), _DistanceFogExp);
                fogFactor = smoothstep(0, 1, fogFactor);

                float4 fogColor = lerp(_DistanceFogColor1, _DistanceFogColor2, fogFactor);

                float4 dst = lerp(src, fogColor, fogColor.a);
                dst.a = src.a;
                return dst;
            }

            float4 DistanceFog_frag(v2f i, float4 src)
            {
                float4 dst = src;
                float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));

                dst = DistanceFog_CalculateFog(dst, depth);

                return dst;
            }
            #endif

            #if PARAFFIN
            float2 Paraffin_AdjustUV(float2 uv, float2 radiusScale)
            {
                float2 centeredUV = uv - 0.5;
                centeredUV.x *= radiusScale.x;
                centeredUV.y *= radiusScale.y;
                return centeredUV + 0.5;
            }

            float4 Paraffin_CalculateGradientColor(ParaffinBuffer data, float2 uv)
            {
                float2 adjustedUV = Paraffin_AdjustUV(uv, data.radiusScale);
                float dist = distance(adjustedUV, data.centerPosition);
                float t = smoothstep(data.radiusNear, data.radiusFar, dist);
                return lerp(data.color1, data.color2, t);
            }

            float4 Paraffin_CalculateBlend(float4 col, ParaffinBuffer data, float2 uv)
            {
                float4 gradientColor = Paraffin_CalculateGradientColor(data, uv);

                #if DEBUG_VIEW
                return CalculateBlendDebug(col, gradientColor, data.useNormal, data.useAdd, data.useMultiply, data.useOverlay, data.useSubstruct);
                #else
                return CalculateBlend(col, gradientColor, data.useNormal, data.useAdd, data.useMultiply, data.useOverlay, data.useSubstruct);
                #endif
            }

            float Paraffin_CalculateDepthFactor(ParaffinBuffer data, float depth)
            {
                float minFactor = smoothstep(data.depthMin - data.depthFade, data.depthMin, depth);
                float maxFactor = 1.0 - smoothstep(data.depthMax, data.depthMax + data.depthFade, depth);
                return minFactor * maxFactor;
            }

            float4 Paraffin_frag(v2f i, float4 src)
            {
                float4 dst = src;
                float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv));

                [loop]
                for(int idx = 0; idx < _ParaffinCount; idx++)
                {
                    float4 blend = Paraffin_CalculateBlend(src, _ParaffinBuffer[idx], i.uv);

                    blend *= Paraffin_CalculateDepthFactor(_ParaffinBuffer[idx], depth);

                    dst += blend;
                }

                dst.a = src.a;
                return dst;
            }
            #endif

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                #if DEBUG_VIEW
                col = float4(0, 0, 0, 1);
                #endif

                #if RIMLIGHT
                col = Rimlight_frag(i, col);
                #endif

                #if DISTANCE_FOG
                col = DistanceFog_frag(i, col);
                #endif

                #if PARAFFIN
                col = Paraffin_frag(i, col);
                #endif

                return col;
            }
            ENDCG
        }
    }
}