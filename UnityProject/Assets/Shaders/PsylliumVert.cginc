#ifndef PSYLLIUM_VERT_INCLUDED
#define PSYLLIUM_VERT_INCLUDED

#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _Position1;
float4 _Rotation1;
float4 _Position2;
float4 _Rotation2;
float4 _BarPosition;
float4 _BarRotation;
float _AnimationTime;

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float2 uv2 : TEXCOORD1; // uv2.x = Radius
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
    float3 worldPos : TEXCOORD1;
};

// 度数法からラジアンに変換
float3 DegreesToRadians(float3 degrees)
{
    return degrees * UNITY_PI / 180.0;
}

// オイラー角をクォータニオンに変換
float4 EulerToQuaternion(float3 euler)
{
    euler = DegreesToRadians(euler);
    float3 s = sin(euler * 0.5);
    float3 c = cos(euler * 0.5);
    
    return float4(
        s.x * c.y * c.z - c.x * s.y * s.z,
        c.x * s.y * c.z + s.x * c.y * s.z,
        c.x * c.y * s.z - s.x * s.y * c.z,
        c.x * c.y * c.z + s.x * s.y * s.z
    );
}

// クォータニオンから回転行列に変換
float4x4 QuaternionToMatrix(float4 q)
{
    float4x4 m;
    float x2 = q.x * q.x;
    float y2 = q.y * q.y;
    float z2 = q.z * q.z;
    float xy = q.x * q.y;
    float xz = q.x * q.z;
    float yz = q.y * q.z;
    float wx = q.w * q.x;
    float wy = q.w * q.y;
    float wz = q.w * q.z;

    m[0] = float4(1 - 2 * (y2 + z2), 2 * (xy - wz), 2 * (xz + wy), 0);
    m[1] = float4(2 * (xy + wz), 1 - 2 * (x2 + z2), 2 * (yz - wx), 0);
    m[2] = float4(2 * (xz - wy), 2 * (yz + wx), 1 - 2 * (x2 + y2), 0);
    m[3] = float4(0, 0, 0, 1);
    
    return m;
}

// オイラー角から回転行列を作成
float4x4 EulerToMatrix(float3 euler)
{
    return QuaternionToMatrix(EulerToQuaternion(euler));
}

v2f vert(appdata v)
{
    v2f o;
    float t = _AnimationTime;

    // バーの回転を適用
    float4x4 localMat = EulerToMatrix(_BarRotation);

    // ローカル行列を適用
    float4x4 mat = mul(unity_ObjectToWorld, localMat);

    float3 barPos = mat._m03_m13_m23;

    // 位置アニメの適用
    float3 localAnmPos = lerp(_Position1, _Position2, t);
    float3 anmPos = mul(unity_ObjectToWorld, localAnmPos);
    barPos += anmPos;

    float3 barUp = mat._m01_m11_m21;

    // 回転アニメを適用
    float3 lerpedRotation = lerp(_Rotation1, _Rotation2, t);
    float4x4 localRotMatrix = EulerToMatrix(lerpedRotation);
    float4x4 rotMatrix = mul(unity_ObjectToWorld, localRotMatrix);
    barUp = mul(rotMatrix, barUp);

    // バーの位置を適用
    float3 localBarPos = _BarPosition.xyz;
    barPos += mul(rotMatrix, localBarPos);

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

    o.worldPos = vertex.xyz;
    o.vertex = mul(UNITY_MATRIX_VP, vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

#endif 