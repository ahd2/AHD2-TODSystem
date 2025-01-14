Shader "AHD2TODSystem/SphericalHarmonicsShader"
{
    Properties
    {
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

    struct Attributes
    {
        float4 positionOS   : POSITION;
        float2 uv           : TEXCOORD0;
        float3 normalOS     : NORMAL;
    };

    struct Varyings
    {
        float4 positionCS   : SV_POSITION;
        float2 texcoord     : TEXCOORD0;
        float3 normalWS     : TEXCOORD1;
    };
    uniform float4 shArray[7];
    Varyings Vert(Attributes input)
    {
        Varyings output = (Varyings)0;
        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
        VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
        output.positionCS = vertexInput.positionCS;
        output.texcoord = input.uv;
        output.normalWS = normalInput.normalWS;
        return output;
    }

    float4 Frag(Varyings input) : SV_TARGET
    {
        float3 shColor = SampleSH9(shArray, normalize(input.normalWS));
        return float4(shColor, 1);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Spherical Harmonics Pass"
            Tags{"LightMode" = "UniversalForward"}
            Cull Back
            ZTest LEqual
            ZWrite On
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}
