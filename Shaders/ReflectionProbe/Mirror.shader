Shader "Hidden/ReflectorProbe/Mirror"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off
			Blend Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _MaskTex;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;

				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.uv.x = 1 - o.uv.x;

				return o;
			}

			half4 frag(v2f IN) : COLOR
			{
				half mask = tex2D(_MaskTex, IN.uv).x;
				half4 col = tex2D(_MainTex, IN.uv);
				col.a = mask;
				return col;
			}
			ENDCG
		}
	}
}
