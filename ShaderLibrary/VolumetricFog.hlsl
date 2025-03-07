#ifndef VOLUMETRICFOG_INCLUDED
#define VOLUMETRICFOG_INCLUDED
TEXTURE3D(_ScatterBuffer);
SamplerState _ScatterBuffer_Trilinear_clamp_sampler;
float4 _VBufferDistanceEncodingParams;
float4 AHD2_FoglightColor;
float _FogFarPlaneDistance;
float _FogStartHeight;
float _FogDensity;

half3 ApplyVolumetricFog(half3 col, float4 positionCS, float3 positionWS)
{
    float3 cameraPosWS = GetCurrentViewPosition();
    float3 d = positionWS - cameraPosWS; //未归一化
    float3 fogCoord = float3(positionCS.xy/_ScaledScreenParams.xy, 0);
    float t = length(d);
    fogCoord.z = EncodeLogarithmicDepthGeneralized(t, _VBufferDistanceEncodingParams);
    half4 fogCol = SAMPLE_TEXTURE3D(_ScatterBuffer, _ScatterBuffer_Trilinear_clamp_sampler, fogCoord);
    //col.xyz = col.xyz * fogCol.a + fogCol.xyz;
    //基础雾效
    //解析高度雾
    float c = 0.02;//雾衰减系数(越大衰减越快，越小衰减越慢)
    
    // 计算积分结果
    float cVolFogHeightDensityAtViewer = exp(-c * (cameraPosWS.y - 10));
    float fogInt = t * cVolFogHeightDensityAtViewer;
    
    // 处理接近水平的视线(避免除以0)
    if (abs(d.y) > 0.01) {
        float tTerm = max(c * d.y, -87.0f);
        float debug = (1.0 - exp(-tTerm)) / tTerm;
        //debug = min(debug, 1000000000);
        fogInt *= debug;
        // if(debug != debug)
        // {
        //     return 1;
        // }
        // if(fogInt != fogInt)
        // {
        //     return debug;
        // }
    }
    // if(fogInt != fogInt)
    // {
    //     if(fogInt!= fogInt)
    //     {
    //         return 1;
    //     }
    // }
    // if(fogInt<0)
    // {
    //     return 0;
    // }
    //fogInt = max(0.0, fogInt);
    // 计算透射率
    float transmittanceHeight = exp(-_FogDensity * 0.1 * fogInt);
    //return transmittanceHeight;
    // 混合雾色与场景色
    half3 fogColor = AHD2_FoglightColor.rgb; // 从引擎参数获取雾色
    return col * transmittanceHeight + fogColor * (1.0 - transmittanceHeight);
}
#endif
