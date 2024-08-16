#ifndef CARTOON_LIT_FORWARD_PASS_INCLUDED
#define CARTOON_LIT_FORWARD_PASS_INCLUDED
#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/CartoonInputData.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"
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
//初始化输入结构体
void InitializeInputData(v2f input , out CartoonInputData inputdata)
{
    inputdata.viewDirWS = normalize(_WorldSpaceCameraPos - input.positionWS); //指向摄像机方向
    inputdata.normalWS = normalize(input.normalWS);
    inputdata.reflectionDirWS = normalize(reflect(-inputdata.viewDirWS,inputdata.normalWS)); //指向视线反射反向
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
    i.shadowCoord = TransformWorldToShadowCoord(i.positionWS);//这里采样才不会出现精度瑕疵
    Light mainlight = GetMainLight(i.shadowCoord);
    half3x3 TBN = half3x3(i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
    half4 mainTex = tex2D(_MainTex,i.uv);
    half4 basecol = _BaseColor * mainTex;
    
    Surface surface;
    surface.normalWS = normalize(i.normalWS);//归一化是必须的
    surface.color = basecol.xyz;
    surface.alpha = basecol.a;
    #if defined (_METALLICMAP)
    surface.metallic = tex2D(_MetalicMap,i.uv).r; //跟diffuse一个ST
    #else
    surface.metallic = _Metallic;
    #endif
    surface.roughness = _Roughness;

    CartoonInputData inputdata;
    InitializeInputData(i , inputdata);

    BRDF brdf = GetBRDF(surface, inputdata);
    half3 finalcolor = GetLighting(surface, brdf, inputdata, mainlight.direction, _lightColor, mainlight.shadowAttenuation);
    return half4(finalcolor,surface.alpha);
    
    
}
#endif