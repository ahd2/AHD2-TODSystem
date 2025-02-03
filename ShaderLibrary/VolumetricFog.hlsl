#ifndef VOLUMETRICFOG_INCLUDED
#define VOLUMETRICFOG_INCLUDED
TEXTURE3D(_DownBuffer);
SAMPLER(sampler_ScatterBuffer);
SamplerState my_Trilinear_clamp_sampler;
TEXTURE2D(_SampleNoiseTex);
SAMPLER(sampler_SampleNoiseTex);
float4 _VBufferDistanceEncodingParams;

half3 ApplyVolumetricFog(half3 col, float4 positionCS, float3 positionWS)
{
    float3 fogCoord = float3(positionCS/_ScaledScreenParams.xy, 0);
    float noise = SAMPLE_TEXTURE2D(_SampleNoiseTex, sampler_SampleNoiseTex, fogCoord.xy *float2(1.6,0.9));
    float t = distance(positionWS, GetCurrentViewPosition());
    fogCoord.z = EncodeLogarithmicDepthGeneralized(t, _VBufferDistanceEncodingParams);
    half4 fogCol = SAMPLE_TEXTURE3D(_DownBuffer, my_Trilinear_clamp_sampler, fogCoord);
    col.xyz = col.xyz * fogCol.a + fogCol.xyz;
    // //基础雾效
    // half3 baseFog;
    // //雾密度
    // float fogdensity = exp(-positionWS.y * 0.1);
    // //深度雾
    // baseFog = col.xyz *  exp(-(t - 10) * 0.05) + _MainLightColor * 0.2 * (1 - exp(-(t - 10) * 0.05));
    // baseFog = baseFog *  fogdensity + _MainLightColor * 0.2 * fogdensity;
    // //return float3(fogCoord.xy,0);
    // col.xyz = lerp(col.xyz, baseFog, smoothstep(98,105,t));
    //return baseFog;
    return col;
}
#endif
