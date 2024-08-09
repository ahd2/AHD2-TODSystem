#ifndef CARTOON_DEPTH_ONLY_PASS_INCLUDED
#define CARTOON_DEPTH_ONLY_PASS_INCLUDED

struct appdata
{
    float4 vertex : POSITION;
};

struct v2f
{
    float4 vertex : SV_POSITION;
};

v2f CartoonDepthOnlyVertex (appdata v)
{
    v2f o;
    o.vertex = TransformObjectToHClip(v.vertex);
    return o;
}

float4 CartoonDepthOnlyFragment (v2f i) : SV_Target
{
    return 0;
}
#endif