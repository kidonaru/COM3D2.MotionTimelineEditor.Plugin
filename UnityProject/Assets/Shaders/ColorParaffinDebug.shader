Shader "MTE/ColorParaffinDebug"
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
            StructuredBuffer<ParaffinBuffer> _ParaffinBuffer;
            int _ParaffinCount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float2 AdjustUV(float2 uv, float2 radiusScale)
            {
                float2 centeredUV = uv - 0.5;
                centeredUV.x *= radiusScale.x;
                centeredUV.y *= radiusScale.y;
                return centeredUV + 0.5;
            }

            float4 CalculateGradientColor(ParaffinBuffer data, float2 uv)
            {
                float2 adjustedUV = AdjustUV(uv, data.radiusScale);
                float dist = distance(adjustedUV, data.centerPosition);
                float t = smoothstep(data.radiusNear, data.radiusFar, dist);
                return lerp(data.color1, data.color2, t);
            }

            float4 CalculateDebugBlend(ParaffinBuffer data, float2 uv)
            {
                float4 blend = CalculateGradientColor(data, uv);
                blend.rgb *= blend.a;
                return blend;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float4 blend = float4(0, 0, 0, 0);

                [loop]
                for(int idx = 0; idx < _ParaffinCount; idx++)
                {
                    blend += CalculateDebugBlend(_ParaffinBuffer[idx], i.uv);
                }

                float4 result = blend;
                result.a = col.a;
                return result;
            }
            ENDCG
        }
    }
}