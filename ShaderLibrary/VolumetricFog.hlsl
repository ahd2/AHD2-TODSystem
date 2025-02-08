#ifndef VOLUMETRICFOG_INCLUDED
#define VOLUMETRICFOG_INCLUDED
TEXTURE3D(_DownBuffer);
SAMPLER(sampler_ScatterBuffer);
SamplerState my_Trilinear_clamp_sampler;
TEXTURE2D(_SampleNoiseTex);
SAMPLER(sampler_SampleNoiseTex);
float4 _VBufferDistanceEncodingParams;
float4 AHD2_FoglightColor;

half3 ApplyVolumetricFog(half3 col, float4 positionCS, float3 positionWS)
{
    float3 fogCoord = float3(positionCS/_ScaledScreenParams.xy, 0);
    float noise = SAMPLE_TEXTURE2D(_SampleNoiseTex, sampler_SampleNoiseTex, fogCoord.xy *float2(1.6,0.9));
    float t = distance(positionWS, GetCurrentViewPosition());
    fogCoord.z = EncodeLogarithmicDepthGeneralized(t, _VBufferDistanceEncodingParams);
    half4 fogCol = SAMPLE_TEXTURE3D(_DownBuffer, my_Trilinear_clamp_sampler, fogCoord);
    col.xyz = col.xyz * fogCol.a + fogCol.xyz;
    //基础雾效
    //距离雾
    float distancefogdendity = 0.1;
    float distancefog = distancefogdendity * saturate((t - 70) / 500);
    half3 baseFog;
    //雾密度
    float fogdensity = saturate(exp(-positionWS.y * 0.01));
    //深度雾
    baseFog = AHD2_FoglightColor.xyz * AHD2_FoglightColor.a * distancefog * fogdensity;
    //baseFog = baseFog *  fogdensity + _MainLightColor * 0.2 * fogdensity;
    //return float3(fogCoord.xy,0);
    //col.xyz = lerp(col.xyz, baseFog, smoothstep(90, 110, t));
    //return baseFog;
    //return distancefog * fogdensity;
    return baseFog + col;
}
#endif
