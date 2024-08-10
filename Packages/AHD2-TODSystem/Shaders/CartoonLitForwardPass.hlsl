#ifndef CARTOON_LIT_FORWARD_PASS_INCLUDED
#define CARTOON_LIT_FORWARD_PASS_INCLUDED

struct appdata
{
    float4 vertex : POSITION;
    float3 normalOS  : NORMAL;
    float4 tangentOS  : TANGENT;
    float2 uv : TEXCOORD0;
    float2 staticLightmapUV   : TEXCOORD1; //静态光照贴图UV
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    half3 tangentWS  : TEXCOORD3;
    float3 viewDirWS : TEXCOORD4;
    float2 capUV : TEXCOORD5;
    float4 shadowCoord : TEXCOORD6;
    float4 lightmapUVOrVertexSH : TEXCOORD7;
    //DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 7);//第七通道为光照贴图uv
    half3 bitangentWS : TEXCOORD8;
};
float3 IndirectDiffuse( float2 uvStaticLightmap, float3 normalWS )
{
    #ifdef LIGHTMAP_ON
    return SampleLightmap( uvStaticLightmap, normalWS );
    #else
    return SampleSH(normalWS);
    #endif
}
v2f CartoonLitVertex (appdata v)
{
    v2f o;
    o.vertex = TransformObjectToHClip(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    
    o.positionWS = TransformObjectToWorld(v.vertex);
    VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, v.tangentOS);
    o.normalWS = normalInput.normalWS;
    o.tangentWS = normalInput.tangentWS;
    o.bitangentWS = normalInput.bitangentWS;

    // 处理烘培光照
    OUTPUT_LIGHTMAP_UV(v.staticLightmapUV, unity_LightmapST, o.lightmapUVOrVertexSH.xy);
    OUTPUT_SH(o.normalWS, o.lightmapUVOrVertexSH.xyz);
    return o;
}

half4 CartoonLitFragment (v2f i) : SV_Target
{
    half3x3 TBN = half3x3(i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
    half lambert = saturate(dot(normalize(_lightDirection.xyz),i.normalWS));
    half4 col = half4(1,1,1,1);
    col.xyz = _lightColor.xyz;
    col.xyz *= lambert;
    return col;
}
#endif