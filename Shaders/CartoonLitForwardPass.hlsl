#ifndef CARTOON_LIT_FORWARD_PASS_INCLUDED
#define CARTOON_LIT_FORWARD_PASS_INCLUDED
#include "Packages/com.ahd2.tod-system/ShaderLibrary/VolumetricFog.hlsl"
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
    //inputdata.normalWS = normalize(input.normalWS);
    inputdata.normalWS = input.normalWS;
    inputdata.reflectionDirWS = normalize(reflect(-inputdata.viewDirWS,inputdata.normalWS)); //指向视线反射反向
    inputdata.vertexSH = input.lightmapUVOrVertexSH.xyz;
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
    o.lightmapUVOrVertexSH.xyz = SampleSH9(AHD2_SHArray, o.normalWS);
    return o;
}

half4 CartoonLitFragment (v2f i) : SV_Target
{
    i.shadowCoord = TransformWorldToShadowCoord(i.positionWS);//这里采样才不会出现精度瑕疵
    Light mainlight = GetMainLight(i.shadowCoord);
    half3 normalMap = tex2D(_NormalMap, i.uv * _NormalMap_ST.xy + _NormalMap_ST.zw,ddx(i.uv.x),ddy(i.uv.y));//双重ST，在Maintex的ST上叠加
    normalMap = normalize(normalMap * 2 - 1);
    half3x3 TBN = half3x3(i.tangentWS.xyz, i.bitangentWS.xyz, normalize(i.normalWS.xyz));
    i.normalWS = TransformTangentToWorld(normalMap,TBN);//矫正了normalWS插值造成的误差，后面直接赋值即可
    half4 mainTex = tex2D(_MainTex,i.uv);
    half4 basecol = _BaseColor * mainTex;
    
    Surface surface;
    InitializeSurfaceData(surface,basecol, i.uv, i.normalWS);

    CartoonInputData inputdata;
    InitializeInputData(i , inputdata);

    BRDF brdf = GetBRDF(surface, inputdata);
    half3 finalcolor = GetLighting(surface, brdf, inputdata, mainlight.direction, mainlight.color, mainlight.shadowAttenuation);
    //点光源
    #ifdef USE_FORWARD_PLUS
    uint lightIndex; 
    ClusterIterator _urp_internal_clusterIterator = ClusterInit(i.vertex.xy / _ScreenParams.xy, i.positionWS, 0); 
    [loop] while (ClusterNext(_urp_internal_clusterIterator, lightIndex)) { 
        lightIndex += URP_FP_DIRECTIONAL_LIGHTS_COUNT; //跳过group里面的方向光，拿到正确的点光源Index
        FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
    //填充计算
    Light light = GetAdditionalLight(lightIndex, i.positionWS);
        finalcolor.xyz += (light.color * light.distanceAttenuation) * PointLightBRDF(surface, brdf, light.direction, inputdata);
    }
    #endif
    //自发光
    finalcolor.xyz += surface.emissionMask * _EmissionColor;
    #ifdef  VOLUMETRICFOG_ON
    finalcolor = ApplyVolumetricFog(finalcolor, i.vertex, i.positionWS);
    #endif
    return half4(finalcolor,surface.alpha);
    
    
}
#endif