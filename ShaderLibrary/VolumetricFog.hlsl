#ifndef VOLUMETRICFOG_INCLUDED
#define VOLUMETRICFOG_INCLUDED
TEXTURE3D(_DownBuffer);
SamplerState _DownBuffer_Trilinear_clamp_sampler;
float4 _VBufferDistanceEncodingParams;
float4 AHD2_FoglightColor;

half3 ApplyVolumetricFog(half3 col, float4 positionCS, float3 positionWS)
{
    float3 fogCoord = float3(positionCS/_ScaledScreenParams.xy, 0);
    float t = distance(positionWS, GetCurrentViewPosition());
    fogCoord.z = EncodeLogarithmicDepthGeneralized(t, _VBufferDistanceEncodingParams);
    half4 fogCol = SAMPLE_TEXTURE3D(_DownBuffer, _DownBuffer_Trilinear_clamp_sampler, fogCoord);
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
    return baseFog + col;
}
#endif
