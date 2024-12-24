// 参考文献
// https://qiita.com/kaneta1992/items/af7793e5450b891c2e27

Shader "MTE/Psyllium"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color1 ("Color1", Color) = (1, 1, 1, 1)
        _Color2 ("Color2", Color) = (1, 1, 1, 1)
        _Color3 ("Color3", Color) = (1, 1, 1, 1)
        _CutoffAlpha ("Cutoff Alpha", Float) = 0.5
        _CutoffHeight ("Cutoff Height", Float) = 0
        _Position1 ("Position1", Vector) = ( 0,  0, 0, 0)
        _Rotation1 ("Rotation1", Vector) = ( 0,  0, 0, 0)
        _Position2 ("Position2", Vector) = ( 0, 10, 0, 0)
        _Rotation2 ("Rotation2", Vector) = (45,  0, 0, 0)
        _BarPosition ("Bar Position", Vector) = (0, 0, 0, 0)
        _BarRotation ("Bar Rotation", Vector) = (0, 0, 0, 0)
        _AnimationTime ("Animation Time", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend One One
            Lighting Off
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "PsylliumVert.cginc"

            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float _CutoffAlpha;
            float _CutoffHeight;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb = _Color3.rgb * _Color3.a * col.a;
                col.a = 1;

                clip(i.worldPos.y - _CutoffHeight);

                return col;
            }
            ENDCG
        }

        Pass
        {
            Cull Off
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            Lighting Off
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "PsylliumVert.cginc"

            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float _CutoffAlpha;
            float _CutoffHeight;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb = lerp(_Color2.rgb, _Color1.rgb, col.r);

                clip(col.a - _CutoffAlpha);
                clip(i.worldPos.y - _CutoffHeight);

                return col;
            }
            ENDCG
        }
    }
}
