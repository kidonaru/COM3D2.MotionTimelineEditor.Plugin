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
                #if PARAFFIN_DEPTH
                float4 screenPos : TEXCOORD1;
                #endif
            };

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

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            StructuredBuffer<ParaffinBuffer> _ParaffinBuffer;
            int _ParaffinCount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                #if PARAFFIN_DEPTH
                o.screenPos = ComputeScreenPos(o.vertex);
                #endif
                return o;
            }

            float2 AdjustUV(float2 uv, float2 radiusScale)
            {
                float2 centeredUV = uv - 0.5;
                centeredUV.x *= radiusScale.x;
                centeredUV.y *= radiusScale.y;
                return centeredUV + 0.5;
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

            float4 CalculateGradientColor(ParaffinBuffer data, float2 uv)
            {
                float2 adjustedUV = AdjustUV(uv, data.radiusScale);
                float dist = distance(adjustedUV, data.centerPosition);
                float t = smoothstep(data.radiusNear, data.radiusFar, dist);
                return lerp(data.color1, data.color2, t);
            }

            float4 CalculateBlend(float4 col, ParaffinBuffer data, float2 uv)
            {
                float4 gradientColor = CalculateGradientColor(data, uv);

                float4 blendDiff = float4(0, 0, 0, 0);
                blendDiff += (gradientColor - col) * data.useNormal;
                blendDiff += (col + gradientColor - col) * data.useAdd;
                blendDiff += (col * gradientColor - col) * data.useMultiply;
                #if PARAFFIN_OVERLAY
                blendDiff += (BlendOverlay(col, gradientColor) - col) * data.useOverlay;
                #endif
                blendDiff += (col - gradientColor - col) * data.useSubstruct;

                return blendDiff * gradientColor.a;
            }

            float4 CalculateDepthFactor(ParaffinBuffer data, float depth)
            {
                float minFactor = smoothstep(data.depthMin - data.depthFade, data.depthMin, depth);
                float maxFactor = 1.0 - smoothstep(data.depthMax, data.depthMax + data.depthFade, depth);
                return minFactor * maxFactor;
            }

            float4 CalculateDebugBlend(ParaffinBuffer data, float2 uv)
            {
                float4 blend = CalculateGradientColor(data, uv);

                float useFactor = data.useNormal + data.useAdd + data.useMultiply + data.useMultiply + data.useOverlay + data.useSubstruct;

                blend.rgb *= blend.a * useFactor;
                return blend;
            }

            float4 CalculateParaffin(v2f i, float4 src)
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
                    dst += CalculateDebugBlend(_ParaffinBuffer[idx], i.uv);
                    #elif PARAFFIN_DEPTH
                    dst += CalculateBlend(src, _ParaffinBuffer[idx], i.uv) * CalculateDepthFactor(_ParaffinBuffer[idx], depth);
                    #else
                    dst += CalculateBlend(src, _ParaffinBuffer[idx], i.uv);
                    #endif
                }

                dst.a = src.a;
                return dst;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                #if PARAFFIN
                col = CalculateParaffin(i, col);
                #endif

                return col;
            }
            ENDCG
        }
    }
}