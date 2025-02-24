Shader "Custom/PlaneReflection"
{
    Properties
    {
    }
    SubShader
    {
        Tags {/*"Queue" = "Transparent" "RenderType" = "Transparent" */"RenderPipeline" = "UniversalPipeline"}
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
            
            CBUFFER_END

            sampler2D _PlanarReflection;
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 vertex_normal : NORMAL;
            };

            struct v2f
            {
                float4 position_CS : SV_POSITION;
                float4 screenUV : TEXCOORD4;
            };
            

            v2f vert (appdata v)
            {
                v2f o;
                o.position_CS = TransformObjectToHClip(v.vertex);
                o.screenUV = ComputeScreenPos(o.position_CS);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 uv = i.screenUV.xy / i.screenUV.w;
                half4 col = tex2D(_PlanarReflection,uv);
                return col * 0.7;
                return col;
            }
            ENDHLSL
        }
        UsePass "SSR Sample/SSR Forward Lit/DepthNormals"
    }
    Fallback "Diffuse"
}

