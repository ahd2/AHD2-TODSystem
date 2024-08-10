#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

//仿造URPLit，记录BRDF参数
struct BRDF {
	half3 diffuse; //漫反射项
	//half3 specular;//镜面反射颜色（注意不是项）
	half roughness;
};

#define MIN_REFLECTIVITY 0.04

float OneMinusReflectivity (float metallic) {
	float range = 1.0 - MIN_REFLECTIVITY;
	return range - metallic * range;
}

//漫反射项（不需要乘以漫反射系数了，省计算而且差别不大）
half3 GetDiffuseBRDF(Surface surface)
{
	return (1 - surface.metallic) * surface.color / PI;
}
//镜面反射比例。因为预先计算了颜色。这里只用计算一维比例。
half GetSprecularBRDF(Surface surface ,half3 normalWS, half3 lightDirectionWS,half3  viewDirectionWS)
{
	return 0;
}

BRDF GetBRDF (Surface surface) {
	BRDF brdf;
	brdf.diffuse = GetDiffuseBRDF(surface);//漫反射颜色
	brdf.roughness = max(1 - surface.smoothness,0.04);
	return brdf;
}

BRDF GetBRDFPremul (Surface surface) {
	BRDF brdf;
	brdf.diffuse = GetDiffuseBRDF(surface) * surface.alpha;//漫反射颜色
	brdf.roughness = max(1 - surface.smoothness,0.04);
	return brdf;
}

//F项
half3 F_Schlick(half HdotV, half3 F0)
{
	return F0 + (1 - F0) * pow(1 - HdotV , 5.0);
}

//D项（Trowbridge-Reitz GGX）
half DistributionGGX(half NdotH, half roughness)
{
	half r2     = roughness*roughness*roughness*roughness;
	half NdotH2 = NdotH*NdotH;
	
	half nom    = r2;
	half denom  = (NdotH2 * (r2 - 1.0) + 1.0);
	denom        = PI * denom * denom;
	
	return nom / max(denom, 0.000001);
}

//G项
half GeometrySchlickGGX(half Roughness, half NdotV)
{
	half k = Roughness * Roughness / 8.0f;

	return NdotV / max(0.001, (NdotV * (1 - k) + k));
}

//直接光的BRDF（包括漫反射项和镜面反射项）
half3 DirectBRDF(Surface surface, BRDF brdf, half3 mainLightDir, CartoonInputData inputdata)
{
	half3 F0 = lerp(0.04 , surface.color ,surface.metallic);
	half3 halfDirWS = normalize(inputdata.viewDirWS + mainLightDir);
	half HdotV = saturate(dot(halfDirWS, inputdata.viewDirWS));
	half NdotL = saturate(dot(inputdata.normalWS, mainLightDir));
	half NdotV = saturate(dot(inputdata.normalWS, inputdata.viewDirWS));
	half NdotH = saturate(dot(inputdata.normalWS, halfDirWS));
	half3 fTerm = F_Schlick(HdotV, F0);
	half dTerm = DistributionGGX(NdotH, brdf.roughness);
	half gTerm = GeometrySchlickGGX(brdf.roughness, NdotV) * GeometrySchlickGGX(brdf.roughness, NdotL);
	//return  gTerm * dTerm * fTerm;
	return brdf.diffuse + fTerm * dTerm * gTerm / max(4 * NdotL * NdotV ,0.001);//漫反射项+镜面反射项
}
#endif