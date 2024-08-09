#ifndef CARTOON_LIT_INPUT_INCLUDED
#define CARTOON_LIT_INPUT_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

CBUFFER_START(UnityPerMaterial)
//贴图ST
float4 _MainTex_ST;
float4 _NormalMap_ST;
//half4
//half
//Test,测试用属性
CBUFFER_END

CBUFFER_START(Light)
float4 _LightDirection;//a通道为标记，0为白天，1为晚上
//half3 _lightColor;
CBUFFER_END

//贴图采样
//Test测试贴图

#endif