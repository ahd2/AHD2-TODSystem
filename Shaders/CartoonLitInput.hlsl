#ifndef CARTOON_LIT_INPUT_INCLUDED
#define CARTOON_LIT_INPUT_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/CartoonInputData.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"


CBUFFER_START(UnityPerMaterial)
//贴图ST
float4 _MainTex_ST;
float4 _NormalMap_ST;
//half4
half4 _BaseColor;
half3 _EmissionColor;
//half
half _Metallic;
half _Roughness;
//Test,测试用属性
CBUFFER_END
uniform float4 AHD2_SHArray[7];
//贴图采样
sampler2D _MainTex;
//PBR贴图
sampler2D _NormalMap;
sampler2D _RMOMap;
sampler2D _AOMap;
//sampler2D 
//Test测试贴图

inline void InitializeSurfaceData(out Surface surface,half4 basecol, float2 uv, half3 normalWS)
{
    surface.normalWS = normalWS;//归一化是必须的
    //surface.normalWS = normalize(normalWS);//归一化是必须的
    surface.color = basecol.xyz;
    surface.alpha = basecol.a;
    #if defined (_RMOMAP)
    half4 RMO = tex2D(_RMOMap,uv);
    surface.metallic = RMO.g; //跟diffuse一个ST
    surface.roughness = RMO.r;
    surface.ambientOcclusion = RMO.b;
    surface.emissionMask = RMO.a;
    #else
    surface.metallic = _Metallic;
    surface.roughness = _Roughness;
    surface.ambientOcclusion = tex2D(_AOMap, uv);
    surface.emissionMask = 1;
    #endif
}

#endif