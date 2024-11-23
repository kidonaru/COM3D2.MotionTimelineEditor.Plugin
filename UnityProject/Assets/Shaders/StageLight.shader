Shader "MTE/StageLight"
{
    Properties
    {
        _MainTex ("Noise Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _SubColor ("Sub Color", Color) = (1,1,1,1)
        _ScrollSpeed ("Scroll Speed", Vector) = (0.2,2.0,0,0)
        _FalloffExp ("Falloff Exponent", Range(0.1, 1)) = 0.5
        _EdgeSoftness ("Edge Softness", Range(0, 1)) = 1
        _SpotRange ("Spot Range", Float) = 10
        _SpotAngle ("Spot Angle", Range(1, 179)) = 45
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.2
        _NoiseScaleInv ("Noise Scale Inverse", Range(0.1, 1)) = 0.2
        _CoreRadius ("Core Radius Ratio", Range(0, 1)) = 0.2
        _TanHalfAngle ("Tan Half Angle", Float) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True"
        }
        
        Blend One One
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 localPos : TEXCOORD2;
                float4 color : COLOR;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _SubColor;
            float4 _ScrollSpeed;
            float _FalloffExp;
            float _EdgeSoftness;
            float _SpotRange;
            float _SpotAngle;
            float _NoiseStrength;
            float _NoiseScaleInv;
            float _CoreRadius;
            float _TanHalfAngle;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.localPos = v.vertex.xyz;
                
                // 頂点でのライティング計算
                float zDist = o.localPos.z;
                float distanceFalloff = pow(1 - saturate(zDist / _SpotRange), _FalloffExp);
                distanceFalloff = smoothstep(0, _EdgeSoftness, distanceFalloff);
                
                float radius = length(o.localPos.xy);
                float normalizedRadius = radius / (zDist * _TanHalfAngle);
                float t = saturate((normalizedRadius - _CoreRadius) / (1.0 - _CoreRadius));
                float angleFalloff = 1.0 - smoothstep(0, 1, t);
                angleFalloff = smoothstep(0, _EdgeSoftness, angleFalloff);
                
                float alpha = _Color.a * distanceFalloff * angleFalloff;
                alpha = saturate(alpha);

                o.color = lerp(_SubColor, _Color, alpha);
                o.color.a = alpha;

                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // ワールド座標を使用してUVを作成
                float2 worldUV = i.worldPos.xy + _Time.x * _ScrollSpeed.xy;
                worldUV = worldUV * _NoiseScaleInv;

                // スクロール処理
                float2 scrolledUV = worldUV;
                scrolledUV = frac(scrolledUV);

                float noise = tex2D(_MainTex, scrolledUV).r;
                noise = 1.0 + (noise - 0.5) * _NoiseStrength;

                float alpha = i.color.a * noise;
                alpha = saturate(alpha);

                float4 finalColor = i.color;
                finalColor.rgb *= alpha;
                finalColor.a = alpha;

                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/VertexLit"
}