//计算光照的函数
#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

//返回方向光的radiance
half3 IncomingLight (Surface surface, half3 mainlightDir,half4 lightCol) {
    return saturate(dot(surface.normalWS, mainlightDir)) * lightCol.xyz * lightCol.a;
}
half3 GetLighting (Surface surface, BRDF brdf, CartoonInputData inputdata, half3 mainlightDir, half4 lightCol) {
	return IncomingLight(surface, mainlightDir, lightCol) * DirectBRDF(surface, brdf, mainlightDir, inputdata);
}
#endif