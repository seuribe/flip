// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Flip/CoinImageShader"
{
	Properties
	{
		_MainTex("Base Texture", 2D) = "" {}
		_Halo("Halo Color", Color) = (0, 0, 0, 0)
		_Alpha("Alpha override", Float) = 1
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			float4 _Halo;
			float _Alpha;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 texcoord0 : TEXCOORD0;
			};

			appdata vert (appdata v)
			{
				appdata o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord0 = TRANSFORM_TEX(v.texcoord0, _MainTex);
				return o;
			}

			fixed4 frag (appdata i) : SV_Target
			{
				float4 col = _Halo + tex2D(_MainTex, i.texcoord0.xy);
				return fixed4(col.rgb, col.a * _Alpha);
			}
			ENDCG
		}
	}
}
