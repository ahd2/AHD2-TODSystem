Shader "AHD2TODSystem/BaseSky"
{
    Properties
    {
        //[HideInInspect]_MainTex ("Texture", 2D) = "black" {}

        [Space(10)]
        [HDR]_sunScatterColorLookAt("太阳核心散射颜色", Color) = (0.00326,0.18243,0.63132,1)
        [HDR]_sunScatterColorBeside("太阳边缘散射颜色", Color) = (0.02948,0.1609,0.27936,1)
        [HDR]_sunOrgColorLookAt("太阳核心原始颜色(地平线)", Color) = (0.30759,0.346,0.24592,1)
        [HDR]_sunOrgColorBeside("太阳边缘原始颜色（地平线）", Color) = (0.04305,0.26222,0.46968,1)
        _sun_scatter("地平线太阳散射系数", Range(0, 1)) = 0.44837

        [Space(10)]
        [HDR]_sky_color("天空颜色（地平线）", Color) = (0.90409,0.7345,0.13709, 1)
        _sky_color_intensity("天空颜色强度（地平线）", Range(0, 3)) = 1.48499
        _sky_scatter("地平线天空散射系数", Range(0, 1)) = 0.69804
        
        [Space(10)]
        _LDotV_damping_factor("太阳边缘/核心抑制参数", Range(0, 2)) = 0.31277
        [NoScaleOffset]_TransmissionRGMap("TransmissionRGMap", 2D) = "white" {}

        [Space(10)]
        _sun_disk_power_999("太阳光晕扩散度", Range(0, 1000)) = 1000
        [HDR]_SunColor ("太阳光晕色",Color) = (1,1,1,1)
        _SunRange("太阳光晕强度",Range(0,10))=1
        [Space(10)]
        [HDR]_SunInnercolor ("太阳内核色",Color) = (1,1,1,1)
        _SunRange0("太阳内核大小",Range(0.97,0.999))=0.98
        _SunRange1("太阳内核大小",Range(0.999,1))=0.999

        [Space(10)]
        [NoScaleOffset]_StarMap("星空", Cube) = "white" {}
        _StarRange("星空范围",Range(1,6))=5

        _CloudMap("云絮", 2D) = "" { }
    }
    SubShader
    {
        Tags { "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" "Queue"="Background"}
        LOD 100
        //放到天空盒就不用Cull Front
        //Cull Front

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	    	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            //#include "Assets/Fog/HLSLFogHeader.cginc"
            //cg库,感觉还是用hlsl吧，跟上时代
            //#include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 posWS :  TEXCOORD1;

                float4 Varying_ViewDirAndMiuResult    : TEXCOORD2;
                float4 Varying_ColorAndLDotDamping    : TEXCOORD3;
            };

            TEXTURE2D(_MainTex); 
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            
            float3 _sunScatterColorLookAt;
            float3 _sunScatterColorBeside;
            float3 _sunOrgColorLookAt;
            float3 _sunOrgColorBeside;

            float _LDotV_damping_factor;
            float _sun_scatter;
            //密度贴图
            sampler2D _TransmissionRGMap;

            float3 _sky_color;
            float _sky_color_intensity;
            float _sky_scatter;

            float _sun_disk_power_999;
            float4 _SunColor;
            float _SunRange;

            float4 _SunInnercolor;
            float _SunRange0;
            float _SunRange1;

            //
            TEXTURECUBE(_StarMap);
            SAMPLER(sampler_StarMap);
            float _StarRange;

            sampler2D _CloudMap;
            CBUFFER_START(Light)
            half4 _lightDirection;//a通道为标记，0为白天，1为晚上
            float4 _lightColor;//a通道为强度
            CBUFFER_END

            float3 RGB2HSV(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
            // HLSL 函数，用于将HSV颜色转换为RGB颜色  
            float3 hsb2rgb( float3 c ){
                float3 rgb = clamp( abs(fmod(c.x*6.0+float3(0.0,4.0,2.0),6)-3.0)-1.0, 0, 1);
                rgb = rgb*rgb*(3.0-2.0*rgb);
                return c.z * lerp( float3(1,1,1), rgb, c.y);
            }
            //偏移原始值函数
            float3 RGB2HSV_Modified(float3 hsv,float hueOffset)  
            {  
                //float3 hsv = RGB2HSV(c); // 使用原始的RGB2HSV函数获取标准的HSV值  
            
                // 将色相从0-1映射到0-360度  
                float hueInDegrees = hsv.x * 360.0;  
            
                // 将色相范围从0-360度更改为90-450度  
                //float hueOffset = 90.0; // 偏移量，使90度对应0（在0-1范围内）  
                float hueRange = 450.0 - 90.0; // 新的色相范围大小  
                float hueModifiedInDegrees = (hueInDegrees + hueOffset) % hueRange; // 应用偏移并取模以确保值在范围内  
            
                // 将修改后的色相值重新映射回0-1范围  
                hsv.x = hueModifiedInDegrees / hueRange;  
            
                return hsv;  
            }

            float FastAcosForAbsCos(float in_abs_cos) {
                float _local_tmp = ((in_abs_cos * -0.0187292993068695068359375 + 0.074261002242565155029296875) * in_abs_cos - 0.212114393711090087890625) * in_abs_cos + 1.570728778839111328125;
                return _local_tmp * sqrt(1.0 - in_abs_cos);
            }

            float FastAcos(float in_cos) {
                float local_abs_cos = abs(in_cos);
                float local_abs_acos = FastAcosForAbsCos(local_abs_cos);
                return in_cos < 0.0 ?  PI - local_abs_acos : local_abs_acos;
            }

            // 兼容原本的 GetFinalMiuResult(float u)
            // 真正的含义是 acos(u) 并将 angle 映射到 up 1，middle 0，down -1
            float GetFinalMiuResult(float u)
            {
                float _acos = FastAcos(u);
                float angle1_to_n1 = (HALF_PI - _acos) * INV_HALF_PI;
                return angle1_to_n1;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                //视角方向
                float3 _worldPos = TransformObjectToWorld( v.vertex.xyz );
                float3 _viewDir = normalize(_worldPos.xyz - _WorldSpaceCameraPos);
                //光源方向（指向光源
                float3 _lightDir = _lightDirection.xyz;

                //核心
                //LDotV
                float _LDotV = dot(_lightDir, _viewDir.xyz);
                //UpDotV
                float _miu = clamp( dot(float3(0,1,0), _viewDir.xyz), -1, 1 );
                float finalMiuResult = GetFinalMiuResult(_miu);
                //remapLDotV
                float _LDotV_remap = clamp((_LDotV * 0.5) + 0.5, 0.0, 1.0);  // f(x)
                _LDotV_remap = max(_LDotV_remap * 1.4285714626312255859375 - 0.42857145581926658906013, 0);         // g(x)
                float _LDotV01_smooth = smoothstep(0, 1, _LDotV_remap);
                //remapLightDir
                float _lightDir_y_remap = clamp((abs(_lightDir.y) - 0.2) * 10.0/3.0, 0.0, 1.0);
                _lightDir_y_remap = smoothstep(0, 1, _lightDir_y_remap);

                //采样传递贴图，获取大气浓度遮罩
                //太阳散射
                float _sun_T = tex2Dlod(_TransmissionRGMap, float4( abs(finalMiuResult)/max(_sun_scatter, 0.0001), 0.5, 0.0, 0.0 )).x;
                //天空散射
                float _sky_T = tex2Dlod(_TransmissionRGMap, float4( abs(finalMiuResult)/max(_sky_scatter, 0.0001), 0.5, 0.0, 0.0 )).y;
                float3 _sky_T_color = _sky_T * _sky_color * _sky_color_intensity;

                //天空散射部分
                //遮罩
                float _cubic_LDotV_damping = lerp( 1, _LDotV, _LDotV_damping_factor );
                _cubic_LDotV_damping = max(_cubic_LDotV_damping, 0);
                _cubic_LDotV_damping = _cubic_LDotV_damping * _cubic_LDotV_damping * _cubic_LDotV_damping;
                // 优先考虑 LDotV 作为太阳强度权重，其次使用 太阳光 Y 的高度
                float _sun_T_color_Instensity = lerp(_LDotV01_smooth, 1, _lightDir_y_remap);
                //颜色混合
                float3 _sunOrgColor_adapt_LDotV = lerp(_sunOrgColorBeside, _sunOrgColorLookAt, _cubic_LDotV_damping);
                float3 _sunScatterColor_adapt_LDotV = lerp(_sunScatterColorBeside, _sunScatterColorLookAt, _cubic_LDotV_damping);//天空色
                float3 _sunFinalColor = lerp(_sunScatterColor_adapt_LDotV, _sunOrgColor_adapt_LDotV, _sun_T);
                

                //输出
                float3 _final_color =  _sky_T_color * _sun_T_color_Instensity + _sunFinalColor;
                //float3 _final_color = lerp( _sunFinalColor,_sky_T_color * _sun_T_color_Instensity ,_sun_T_color_Instensity);
                o.Varying_ColorAndLDotDamping=float4(_final_color,1);
                o.Varying_ViewDirAndMiuResult.xyz = float3( _viewDir.x, _viewDir.y, _viewDir.z );
                o.posWS= mul(unity_ObjectToWorld,v.vertex).xyz;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                //VDotUp
                float3 _viewDirNormalize = normalize(i.Varying_ViewDirAndMiuResult.xyz);
                float _VDotUp = dot(_viewDirNormalize, float3(0,1,0));
                float _VDotUp_Multi999 = abs(_VDotUp) * _sun_disk_power_999;
                //VDotL
                float3 _lightDir = _lightDirection.xyz;
                float _LDotV = dot(_lightDir, _viewDirNormalize);
                //_LDotV_remap
                float _LDotV_remap01 = (_LDotV * 0.5) + 0.5;
                _LDotV_remap01 = clamp(_LDotV_remap01, 0.0, 1.0);
                //LDotVsmooth
                float _LDotV_smooth = smoothstep(0, 1, _LDotV);

                //大气散射
                float _LDotV_Pow_0_1  = pow(_LDotV_remap01, _VDotUp_Multi999 * 0.1);
                float _LDotV_Pow_0_01 = pow(_LDotV_remap01, _VDotUp_Multi999 * 0.01);
                float _LDotV_Pow      = pow(_LDotV_remap01, _VDotUp_Multi999);
                _LDotV_Pow_0_1        = min(_LDotV_Pow_0_1, 1.0);
                _LDotV_Pow_0_01       = min(_LDotV_Pow_0_01, 1.0);
                _LDotV_Pow            = min(_LDotV_Pow, 1.0);

                float _LDotV_Pow_Scale = (_LDotV_Pow_0_01 * 0.03) + (_LDotV_Pow_0_1 * 0.12) + _LDotV_Pow.x;
                float3 _sun_disk = _LDotV_Pow_Scale * _SunRange * _SunColor;

                // float3 _sun_disk = _LDotV_Pow_Scale * _SunColor;
                // _sun_disk = RGB2HSV(_sun_disk);
                // _sun_disk.y *= _SunRange;
                // _sun_disk.z *= _SunRange;
                // _sun_disk = hsb2rgb(_sun_disk);
                //
                //太阳内核
                half innerRange = smoothstep(_SunRange0, _SunRange1, _LDotV_remap01);

                //(_LDotV_smooth * _sun_disk )：太阳光晕以及地平线光晕
                //innerRange：太阳内核
                float3 sunpart =  (_LDotV_smooth * _sun_disk ) + i.Varying_ColorAndLDotDamping.xyz;
                sunpart = lerp(sunpart,_SunInnercolor,innerRange);

                //星空
                float4 starcol = SAMPLE_TEXTURECUBE(_StarMap, sampler_StarMap, i.uv.xyz)*2*pow((1-_LDotV_remap01),_StarRange);

                float4 finalcolor=float4(sunpart+starcol.xyz,1);

                
                float n = smoothstep(-3.5,3,atan2(i.uv.z,i.uv.x));//环形x轴
                float4 cloud = tex2D(_CloudMap,float2(n,i.uv.y));

                finalcolor.xyz += cloud.a;

                //finalcolor.xyz = ExponentialHeightFog(finalcolor.xyz,i.posWS);
                //finalcolor.xyz=  i.Varying_ColorAndLDotDamping.xyz;
                return  finalcolor;
            }
            ENDHLSL
        }
        
    }
}
