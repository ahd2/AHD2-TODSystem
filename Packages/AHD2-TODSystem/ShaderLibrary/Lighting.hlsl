//计算光照的函数
#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

//返回方向光的radiance
half3 IncomingLight (Surface surface, half3 mainlightDir,half4 lightCol, half directShadow) {
    //return saturate(dot(surface.normalWS, mainlightDir)) * lightCol.xyz * lightCol.a * saturate((directShadow + 1 - 0.5 * lightCol.a));
    return saturate(dot(surface.normalWS, mainlightDir)) * lightCol.xyz * lightCol.a * saturate((directShadow + 1 - lightCol.a));
}
half3 GetLighting (Surface surface, BRDF brdf, CartoonInputData inputdata, half3 mainlightDir, half4 lightCol, half direcctShadow) {
	return IncomingLight(surface, mainlightDir, lightCol, direcctShadow) * DirectBRDF(surface, brdf, mainlightDir, inputdata);
}
#endif