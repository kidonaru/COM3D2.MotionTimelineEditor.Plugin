Shader "MTE/StageLaser"
{
    Properties
    {
        _MainTex ("Noise Texture", 2D) = "white" {}
        _ScrollSpeed ("Scroll Speed", Vector) = (0.2,2.0,0,0)
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.2
        _NoiseScaleInv ("Noise Scale Inverse", Range(0.1, 1)) = 0.2
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest Mode", Float) = 0
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
        ZTest [_ZTest]
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
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
            float4 _ScrollSpeed;
            float _NoiseStrength;
            float _NoiseScaleInv;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.localPos = v.vertex.xyz;
                o.color = v.color;

                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 worldUV_xy = i.worldPos.xy + _Time.x * _ScrollSpeed.xy;
                float2 worldUV_z = float2(i.worldPos.z, i.worldPos.y) + _Time.x * _ScrollSpeed.xy;
                
                worldUV_xy = worldUV_xy * _NoiseScaleInv;
                worldUV_z = worldUV_z * _NoiseScaleInv;

                float2 scrolledUV_xy = frac(worldUV_xy);
                float2 scrolledUV_z = frac(worldUV_z);

                float noise_xy = tex2D(_MainTex, scrolledUV_xy).r;
                float noise_z = tex2D(_MainTex, scrolledUV_z).r;

                float noise = (noise_xy + noise_z) * 0.5;
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