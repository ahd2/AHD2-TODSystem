#ifndef CARTOON_DEPTH_NORMALS_PASS_INCLUDED
#define CARTOON_DEPTH_NORMALS_PASS_INCLUDED

struct appdata
{
    float4 vertex : POSITION;
    float4 tangentOS : TANGENT;
    float3 normal : NORMAL;
};

struct v2f
{
    float4 vertex : SV_POSITION;
    half3 normalWS  : TEXCOORD2;
};

v2f CartoonDepthNormalsVertex (appdata v)
{
    v2f o;
    o.vertex = TransformObjectToHClip(v.vertex);
    o.normalWS = TransformObjectToWorldNormal(v.normal);
    return o;
}

half4 CartoonDepthNormalsFragment (v2f i) : SV_Target
{
    return float4(normalize(i.normalWS),0.0);
}
#endif