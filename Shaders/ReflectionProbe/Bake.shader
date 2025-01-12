Shader "Hidden/ReflectorProbe/Bake"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (1,1,1,1)
    }
    HLSLINCLUDE
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

    struct appdata
    {
        float4 positionOS : POSITION;
        float2 uv : TEXCOORD0;
        float3 normalOS : NORMAL;
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        float4 positionCS : SV_POSITION;
        float3 positionWS : TEXCOORD1;
        float3 normalWS  : TEXCOORD2;
    };
	TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);
    
    CBUFFER_START(UnityPerMaterial)
    float4 _MainTex_ST;
	half4 _BaseColor;
    CBUFFER_END

    v2f vert (appdata v)
    {
        v2f o;
        o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
        o.normalWS = TransformObjectToWorldNormal(v.normalOS);//向量记得在片元归一化
        o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        return o;
    }

    half4 fragDiffuse (v2f i) : SV_Target
    {
        half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
        col.xyz *= _BaseColor.xyz;
        return col;
    }
	half4 fragNormal (v2f i) : SV_Target
    {
        i.normalWS = normalize(i.normalWS);
        //还要编码
        return half4(i.normalWS * 0.5 + 0.5, 1);
    }
	half4 fragMask (v2f i) : SV_Target
    {
        return 1;
    }
	ENDHLSL
    SubShader
    {
        //烘焙基础色。
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragDiffuse
            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragNormal
            ENDHLSL
        }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragMask
            ENDHLSL
        }
    }
}
