Shader "Unlit/RP_Cubemap_reflection_shader"
{
    Properties
    {
        _Diffuse("Diffuse", Color) = (1, 1, 1, 1)
        _ReflectColor("_ReflectColor", Color) = (1, 1, 1, 1)
        _ReflectAmount("_ReflectAmount", Range(0,1)) = 1
        _Reflection_CubeMap("_Reflection_CubeMap", Cube) = "_Skybox"{}
        
        _Specular("Specular", Color) = (1, 1, 1, 1)
        _Gloss("Gloss", Range(8,255)) = 8.0  
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline"}
        Pass
        {
            Tags {"LightMode" = "UniversalForward" }
            
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS //接收阴影
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE //得到正确的阴影坐标
            #pragma multi_compile _ _SHADOWS_SOFT //软阴影

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
           
          
            
            CBUFFER_START(UnityPerMaterial)
            half4 _Diffuse;
            float4 _ReflectColor;//用于控制反射的天空盒子的颜色量
            float _ReflectAmount;//用于控制反射的天空盒子的颜色和漫反射diffuse在总体反射中的占比
            //samplerCUBE _GlossyEnvironmentCubeMap;
            
            
            half4 _Specular;
            half _Gloss;

            
            CBUFFER_END

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 vertex_normal : NORMAL;
            };

            struct v2f
            {
                float4 position_CS : SV_POSITION;
                float4 position_WS : TEXCOORD0;
                float3 normal_WS : TEXCOORD1;
                float3 view_dir_WS : TEXCOORD2;
                float3 reflect_dir_WS : TEXCOORD3;
                
            };
            

            v2f vert (appdata v)
            {
                v2f o;
                o.position_CS = TransformObjectToHClip(v.vertex);//获取裁剪空间的顶点坐标
                o.position_WS = mul(unity_ObjectToWorld, v.vertex);//获取世界空间的顶点坐标
                o.normal_WS = TransformObjectToWorldNormal(v.vertex_normal);//获取世界空间的法线
                
                
                o.view_dir_WS = GetWorldSpaceViewDir(o.position_WS);
                o.reflect_dir_WS = reflect(-normalize(o.view_dir_WS), normalize(o.normal_WS));//获取反射光线
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float4 SHADOW_COORDS = TransformWorldToShadowCoord(i.position_WS);//获取将世界空间的顶点坐标转换到光源空间获取的阴影坐标,这里在片元着色器里面进行，利用了插值之后的结果
                Light main_light = GetMainLight(SHADOW_COORDS);
                
                float3 light_dir_WS = normalize(TransformObjectToWorld(main_light.direction));//获取世界空间的光照单位矢量
                float3 view_dir_WS = normalize(i.view_dir_WS);
                float3 reflect_dir_WS = normalize(i.reflect_dir_WS);
                float3 normal_WS = normalize(i.normal_WS);
                
                
                half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;//获取环境光强度
                
                half3 diffuse = main_light.color.rgb * _Diffuse.rgb * saturate(dot(normal_WS, light_dir_WS));//获取漫反射强度,light.color.rgb是光照强度
                half3 reflection =SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflect_dir_WS,0 * 6);   // 使用反射控制Cube等级 作者：那个人真狗 https://www.bilibili.com/read/cv17365012/ 出处：bilibili
                
                half3 half_dir = normalize(light_dir_WS + view_dir_WS);//获取半程向量
                half3 specular = main_light.color.rgb * _Specular.rgb * pow(saturate(dot(half_dir,normal_WS)), _Gloss);//获取高光

                
                reflection = lerp(diffuse, reflection,  _ReflectAmount);//通过平滑函数获得最佳的反射
                
                
                half3 return_color = ambient + reflection  + specular;
                return half4(return_color, 1.0);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
    Fallback "Diffuse"
}

