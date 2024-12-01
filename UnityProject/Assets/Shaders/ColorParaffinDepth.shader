Shader "MTE/ColorParaffinDepth"
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
                float4 screenPos : TEXCOORD1;
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
                float padding0;
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
                o.screenPos = ComputeScreenPos(o.vertex);
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
                blendDiff += (BlendOverlay(col, gradientColor) - col) * data.useOverlay;
                blendDiff += (col - gradientColor - col) * data.useSubstruct;

                return blendDiff * gradientColor.a;
            }

            float4 CalculateDepthFactor(ParaffinBuffer data, float depth)
            {
                float depthFactor = step(data.depthMin, depth);
                depthFactor *= 1.0 - step(data.depthMax, depth);
                return depthFactor;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float4 blend = float4(0, 0, 0, 0);
                float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.screenPos));

                [loop]
                for(int idx = 0; idx < _ParaffinCount; idx++)
                {
                    blend += CalculateBlend(col, _ParaffinBuffer[idx], i.uv) * CalculateDepthFactor(_ParaffinBuffer[idx], depth);
                }

                float4 result = col + blend;
                result.a = col.a;

                return result;
            }
            ENDCG
        }
    }
}