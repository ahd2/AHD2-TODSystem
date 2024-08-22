// //光源结构体。这个URP也是自己实现的。SRP没提供。
// #ifndef CUSTOM_LIGHT_INCLUDED
// #define CUSTOM_LIGHT_INCLUDED
//
// #define MAX_DIRECTIONAL_LIGHT_COUNT 4
// //接收从CPU设置的全局变量。（URP也是这样传的）
// CBUFFER_START(_CustomLight)
// 	//float4 _DirectionalLightColor;
// 	//float4 _DirectionalLightDirection;
// 	float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
// 	float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
// 	int _DirectionalLightCount;
// CBUFFER_END
//
// struct Light {
//     float3 color;
//     float3 direction;
// };
// int GetDirectionalLightCount () {
// 	return _DirectionalLightCount;
// }
//
// Light GetDirectionalLight (int index) {
// 	Light light;
// 	light.color = _DirectionalLightColors[index].rgb;
// 	light.direction = normalize(_DirectionalLightDirections[index].xyz);
// 	return light;
// }
//
// #endif