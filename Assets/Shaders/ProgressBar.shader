// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/ProgressBar"
{
	Properties
	{
		_BackColor("Background Color", Color) = (0, 0, 0, 1)
		_FrontColor("Foreground Color", Color) = (1, 1, 1, 1)
		_Progress   ("Progress", Float) = 0.5
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Overlay" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			fixed4 _BackColor;
			fixed4 _FrontColor;
			float _Progress;

			struct appdata
			{
				float4 vertex : POSITION;
				float4 texcoord0 : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 texcoord0 : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord0 = v.texcoord0;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				if (i.texcoord0.x > _Progress) {
					return _BackColor;
				}
				else {
					return _FrontColor;
				}
			}
			ENDCG
		}
	}
}
