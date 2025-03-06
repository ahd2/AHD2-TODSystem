#ifndef VOLUMETRICFOG_INCLUDED
#define VOLUMETRICFOG_INCLUDED
TEXTURE3D(_ScatterBuffer);
SamplerState _ScatterBuffer_Trilinear_clamp_sampler;
float4 _VBufferDistanceEncodingParams;
float4 AHD2_FoglightColor;
float _FogFarPlaneDistance;

half3 ApplyVolumetricFog(half3 col, float4 positionCS, float3 positionWS)
{
    float3 fogCoord = float3(positionCS.xy/_ScaledScreenParams.xy, 0);
    //float t = distance(positionWS, GetCurrentViewPosition());
    //fogCoord.z = EncodeLogarithmicDepthGeneralized(t, _VBufferDistanceEncodingParams);
    half4 fogCol = SAMPLE_TEXTURE3D(_ScatterBuffer, _ScatterBuffer_Trilinear_clamp_sampler, fogCoord);
    //col.xyz = col.xyz * fogCol.a + fogCol.xyz;
    //基础雾效
    //距离雾
    float distancefogdendity = 0.1;
    //float distancefog = distancefogdendity * saturate((t - _FogFarPlaneDistance * 0.7) / 500);//从70m开始，其实应该是从体积雾的0.7倍距离开始
    half3 baseFog;
    //雾密度
    float fogdensity = saturate(exp(-positionWS.y * 0.01));
    //深度雾
    //baseFog = AHD2_FoglightColor.xyz * AHD2_FoglightColor.a * distancefog * fogdensity;

    //解析高度雾
    float b = 0.01;//全局雾密度
    float c = 0.02;//雾衰减系数
    // float3 campos = GetCurrentViewPosition();
    // float3 d = positionWS - campos;
    // float fogdesity = b * exp(-c * positionWS.y);
    // float3 finalfogcol = b * exp(-c * campos.y) * sqrt(dot(d,d)) * (1 - exp(-c * d.y)) / (c * d.y);
    //
    // float cVolFogHeightDensityAtViewer = exp( -c * campos.y );
    // float fogInt = length( d ) * cVolFogHeightDensityAtViewer;
    // const float cSlopeThreshold = 0.01;
    // if( abs( d.y ) > cSlopeThreshold )
    // {
    //     float t = c * d.y;
    //     fogInt *= ( 1.0- exp( -t ) ) / t;
    // }
    // //return exp( -b * fogInt );
    // return finalfogcol;

    float3 campos = GetCurrentViewPosition();
    float3 d = positionWS - campos; // 注意方向

    // 计算积分结果
    float cVolFogHeightDensityAtViewer = exp(-c * campos.y);
    float fogInt = length(d) * cVolFogHeightDensityAtViewer;

    // 处理接近水平的视线
    const float cSlopeThreshold = 0.01;
    if (abs(d.y) > cSlopeThreshold) {
        float t = c * d.y;
        fogInt *= (1.0 - exp(-t)) / t;
    }

    // 计算透射率
    float transmittance = exp(-b * fogInt);

    // 混合雾色与场景色
    half3 fogColor = AHD2_FoglightColor.rgb; // 从引擎参数获取雾色
    return col * transmittance + fogColor * (1.0 - transmittance);
}
#endif
