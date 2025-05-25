Shader "MTE/GTToneMap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MaxBrightness ("Max Brightness", Range(1, 100)) = 1.0
        _Contrast ("Contrast", Range(0, 5)) = 1.0
        _LinearStart ("Linear Section Start", Range(0, 1)) = 0.22
        _LinearLength ("Linear Section Length", Range(0, 1)) = 0.4
        _BlackTightness ("Black Tightness", Range(1, 3)) = 1.33
        _BlackOffset ("Black Offset", Range(0, 1)) = 0.0
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MaxBrightness;
            float _Contrast;
            float _LinearStart;
            float _LinearLength;
            float _BlackTightness;
            float _BlackOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // GT Tonemap
            // Copyright(c) 2017 by Hajime UCHIMURA @ Polyphony Digital Inc.
            // https://www.desmos.com/calculator/gslcdxvipg?lang=ja
            // https://www.slideshare.net/nikuque/hdr-theory-and-practicce-jp

            float W(float x, float e0, float e1) {
                if (x <= e0) return 0;
                if (x >= e1) return 1;

                float a = (x - e0) / (e1 - e0);
                return a * a * (3 - 2 * a);
            }

            float H(float x, float e0, float e1) {
                if (x <= e0) return 0;
                if (x >= e1) return 1;
                return (x - e0) / (e1 - e0);
            }

            float3 W3(float3 x, float e0, float e1) {
                return float3(
                    W(x.r, e0, e1),
                    W(x.g, e0, e1),
                    W(x.b, e0, e1)
                );
            }
            
            float3 H3(float3 x, float e0, float e1) {
                return float3(
                    H(x.r, e0, e1),
                    H(x.g, e0, e1),
                    H(x.b, e0, e1)
                );
            }

            float3 GTToneMap(float3 x) {
                float P = _MaxBrightness;
                float a = _Contrast;
                float m = _LinearStart;
                float l = _LinearLength;
                float c = _BlackTightness;
                float b = _BlackOffset;

                float l0 = (P - m) * l / a;
                float L0 = m - m / a;
                float L1 = m + (1 - m) / a;
                float3 L_x = m + a * (x - m);
                float3 T_x = m * pow(x / m, c) + b;
                float S0 = m + l0;
                float S1 = m + a * l0;
                float C2 = a * P / (P - S1);
                float e = 2.71828;
                float3 S_x = P - (P - S1) * pow(e, -(C2 * (x - S0) / P));
                float3 w0_x = 1 - W3(x, 0, m);
                float3 w2_x = H3(x, m + l0, m + l0);
                float3 w1_x = 1 - w0_x - w2_x;
                float3 f_x = T_x * w0_x + L_x * w1_x + S_x * w2_x;
                return f_x;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                //uv.y = 1.0 - uv.y; // Flip the UVs for correct texture sampling
                fixed4 col = tex2D(_MainTex, uv);
                float3 r = GTToneMap(col.rgb);
                col = float4(r, col.a);
                return col;
            }
            ENDCG
        }
    }
}
