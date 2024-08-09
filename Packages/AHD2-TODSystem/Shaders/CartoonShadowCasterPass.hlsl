#ifndef CARTOON_SHADOW_CASTER_PASS_INCLUDED
#define CARTOON_SHADOW_CASTER_PASS_INCLUDED

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

v2f CartoonShadowCasterVertex (appdata v)
{
    v2f o;
    o.vertex = TransformObjectToHClip(v.vertex);
    //应用阴影bias
    VertexNormalInputs normalInput = GetVertexNormalInputs(v.normal, v.tangentOS);
    o.normalWS = normalize(half3(normalInput.normalWS));
    float3 posWS = TransformObjectToWorld(v.vertex.xyz);
    float3 lightDirectionWS = _MainLightPosition;
    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(posWS, o.normalWS, lightDirectionWS));
    //not UNITY_REVERSE_Z!!!!
    #if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w*UNITY_NEAR_CLIP_VALUE);
    #else
    positionCS.z = max(positionCS.z, positionCS.w*UNITY_NEAR_CLIP_VALUE);
    #endif
    o.vertex = positionCS;
    return o;
}

half4 CartoonShadowCasterFragment (v2f i) : SV_Target
{
    return 0;
}
#endif