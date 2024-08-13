#ifndef CARTOON_LIT_INPUT_INCLUDED
#define CARTOON_LIT_INPUT_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
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
//half
half _Metallic;
half _Smoothness;
//Test,测试用属性
CBUFFER_END

CBUFFER_START(IBL)
half3 _irradianceMap0;
half3 _irradianceMap1;
half3 _specularMap0;
half3 _specularMap1;
CBUFFER_END

CBUFFER_START(Light)
half4 _lightColor;//a通道为强度
half4 _lightDirection;//a通道为标记，0为白天，1为晚上
CBUFFER_END


//贴图采样
sampler2D _MainTex;
//Test测试贴图

#endif