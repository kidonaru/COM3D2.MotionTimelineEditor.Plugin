#ifndef PSYLLIUM_VERT_INCLUDED
#define PSYLLIUM_VERT_INCLUDED

#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _Color1a;
float4 _Color1b;
float4 _Color1c;
float4 _Color2a;
float4 _Color2b;
float4 _Color2c;
float _CutoffAlpha;

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float2 uv2 : TEXCOORD1; // uv2.x = Radius, uv2.y = ColorRate
};

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float2 uv2 : TEXCOORD1;
};

v2f vert(appdata v)
{
    v2f o;
    float4x4 mat = unity_ObjectToWorld;

    float3 barPos = mat._m03_m13_m23;
    float3 barUp = mat._m01_m11_m21;
    float3 cameraToBar = barPos - _WorldSpaceCameraPos;
    float3 barSide = normalize(cross(barUp, cameraToBar));
    float3 barForward = normalize(cross(barSide, barUp));

    mat._m00_m10_m20 = barSide;
    mat._m01_m11_m21 = barUp;
    mat._m02_m12_m22 = barForward;
    mat._m03_m13_m23 = barPos;

    float4 vertex = float4(v.vertex.xyz, 1.0);
    vertex = mul(mat, vertex);

    float3 offsetVec = normalize(cross(cameraToBar, barSide));
    vertex.xyz += offsetVec * v.uv2.x;

    o.pos = mul(UNITY_MATRIX_VP, vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.uv2 = v.uv2;
    return o;
}

#endif 