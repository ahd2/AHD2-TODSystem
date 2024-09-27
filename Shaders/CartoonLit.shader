Shader "AHD2TODSystem/CartoonLit"
{
    Properties
    {
        [Header(MainColor)]
        [Space(10)]
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        [Header(PBR)]
        [Space(10)]
        _NormalMap("NormalMap", 2D) = "bump" { }
        _RMOMap("RMOMap",2D) = "white" {}
        _Metallic ("Metallic", Range(0, 1)) = 0
		_Roughness ("Roughness", Range(0, 1)) = 0.5
        _AOMap("AOMap", 2D) = "white" { }
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            //光照Pass
            Name "CartoonForwardLit"
            HLSLPROGRAM
            #pragma vertex CartoonLitVertex
            #pragma fragment CartoonLitFragment
            //接收投影变体
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            //光照贴图变体（只开启静态）
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK //这两个宏用于控制阴影的烘焙和采样。当你在Unity的Lighting窗口中选择了Shadowmask或者Subtractive模式，这两个宏就会被激活。
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED //这个宏用于控制是否将定向光源的光照信息烘焙到光照贴图中。当你在Unity的Lighting窗口中选择了Baked GI，并且选择了Directional Mode，这个宏就会被激活。
            #pragma multi_compile _ LIGHTMAP_ON
            //材质面板keyword
            #pragma shader_feature_local_fragment _RMOMAP //_local_fragment表示只在片元着色器生效
            #include "CartoonLitInput.hlsl"
	        #include "CartoonLitForwardPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            //阴影写入Pass（目前不支持alphaTest阴影，需要的话再加）
            Name "CartoonShadowCaster"
            Tags{ "LightMode" = "ShadowCaster" }
            HLSLPROGRAM
            #pragma vertex CartoonShadowCasterVertex
            #pragma fragment CartoonShadowCasterFragment
            #include "CartoonLitInput.hlsl"
	        #include "CartoonShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            //深度法线Pass
            Name "CartoonDepthNormals"
            Tags{ "LightMode" = "DepthNormals" }
            HLSLPROGRAM
            #pragma vertex CartoonDepthNormalsVertex
            #pragma fragment CartoonDepthNormalsFragment
            #include "CartoonLitInput.hlsl"
	        #include "CartoonDepthNormalsPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            //深度Pass
            Name "CartoonDepthOnly"
            Tags{ "LightMode" = "DepthOnly" }
            HLSLPROGRAM
            #pragma vertex CartoonDepthOnlyVertex
            #pragma fragment CartoonDepthOnlyFragment
            #include "CartoonLitInput.hlsl"
	        #include "CartoonDepthOnlyPass.hlsl"
            ENDHLSL
        }

//        Pass
//        {
//            //烘焙光照Pass
//            Name "Meta"
//            Tags{ "LightMode" = "Meta" }
//            HLSLPROGRAM
//            #pragma vertex UniversalVertexMeta
//            #pragma fragment CartoonMetaFragment
//            #include "CartoonLitInput.hlsl"
//	        #include "CartoonMetaPass.hlsl"
//            ENDHLSL
//        }

    }
    CustomEditor "AHD2PBRGUI"
}
