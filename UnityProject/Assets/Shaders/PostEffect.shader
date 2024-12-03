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
            #pragma multi_compile __ DISTANCE_FOG
            #pragma multi_compile __ DISTANCE_FOG_DEBUG
            #pragma multi_compile __ PARAFFIN
            #pragma multi_compile __ PARAFFIN_DEBUG
            #pragma multi_compile __ PARAFFIN_DEPTH
            #pragma multi_compile __ PARAFFIN_OVERLAY
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
                #if PARAFFIN_DEPTH || DISTANCE_FOG
                float4 screenPos : TEXCOORD1;
                #endif
            };

            sampler2D _MainTex;

            #if PARAFFIN_DEPTH || DISTANCE_FOG
            sampler2D _CameraDepthTexture;
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
                #if PARAFFIN_DEPTH || DISTANCE_FOG
                o.screenPos = ComputeScreenPos(o.vertex);
                #endif
                return o;
            }

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
                float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.screenPos));

                #if DISTANCE_FOG_DEBUG
                dst = float4(0, 0, 0, 0);
                #endif

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

            float4 Paraffin_BlendOverlay(float4 base, float4 blend)
            {
                float4 result;
                result.rgb = base.rgb < 0.5 ? 
                    2.0 * base.rgb * blend.rgb :
                    1.0 - 2.0 * (1.0 - base.rgb) * (1.0 - blend.rgb);
                result.a = base.a;
                return result;
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

                float4 blendDiff = float4(0, 0, 0, 0);
                blendDiff += (gradientColor - col) * data.useNormal;
                blendDiff += (col + gradientColor - col) * data.useAdd;
                blendDiff += (col * gradientColor - col) * data.useMultiply;
                #if PARAFFIN_OVERLAY
                blendDiff += (Paraffin_BlendOverlay(col, gradientColor) - col) * data.useOverlay;
                #endif
                blendDiff += (col - gradientColor - col) * data.useSubstruct;

                return blendDiff * gradientColor.a;
            }

            float4 Paraffin_CalculateDepthFactor(ParaffinBuffer data, float depth)
            {
                float minFactor = smoothstep(data.depthMin - data.depthFade, data.depthMin, depth);
                float maxFactor = 1.0 - smoothstep(data.depthMax, data.depthMax + data.depthFade, depth);
                return minFactor * maxFactor;
            }

            float4 Paraffin_CalculateDebugBlend(ParaffinBuffer data, float2 uv)
            {
                float4 blend = Paraffin_CalculateGradientColor(data, uv);

                float useFactor = data.useNormal + data.useAdd + data.useMultiply + data.useMultiply + data.useOverlay + data.useSubstruct;

                blend.rgb *= blend.a * useFactor;
                return blend;
            }

            float4 Paraffin_frag(v2f i, float4 src)
            {
                float4 dst = src;

                #if PARAFFIN_DEBUG
                dst = float4(0, 0, 0, 0);
                #endif

                #if PARAFFIN_DEPTH
                float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.screenPos));
                #endif

                [loop]
                for(int idx = 0; idx < _ParaffinCount; idx++)
                {
                    #if PARAFFIN_DEBUG
                    dst += Paraffin_CalculateDebugBlend(_ParaffinBuffer[idx], i.uv);
                    #elif PARAFFIN_DEPTH
                    dst += Paraffin_CalculateBlend(src, _ParaffinBuffer[idx], i.uv) * Paraffin_CalculateDepthFactor(_ParaffinBuffer[idx], depth);
                    #else
                    dst += Paraffin_CalculateBlend(src, _ParaffinBuffer[idx], i.uv);
                    #endif
                }

                dst.a = src.a;
                return dst;
            }
            #endif

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

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