// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/FourSegmentBar"
{
	Properties
	{
		_BackColor("Background Color", Color) = (0, 0, 0, 1)
		_FrontColor1("Foreground Color 1", Color) = (1, 0, 1, 1)
		_FrontColor2("Foreground Color 2", Color) = (1, 1, 0, 1)
		_FrontColor3("Foreground Color 3", Color) = (0, 1, 1, 1)
		_Percent1("Percent 1", Float) = 0.25
		_Percent2("Percent 2", Float) = 0.125
		_Percent3("Percent 3", Float) = 0.1
		_Vertical("Vertical", Float) = 0
	}
	SubShader
	{
		Tags {
			"RenderType" = "Opaque"
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
		}
		LOD 100

		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			fixed4 _BackColor;
			fixed4 _FrontColor1;
			fixed4 _FrontColor2;
			fixed4 _FrontColor3;
			float _Percent1;
			float _Percent2;
			float _Percent3;
			float _Vertical;

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
				float progress = i.texcoord0.x;
				if (_Vertical != 0) {
					progress = i.texcoord0.y;
				}
				if (progress < _Percent3) {
					return _FrontColor3;
				}
				else if (progress < _Percent3 + _Percent2) {
					return _FrontColor2;
				}
				else if (progress < _Percent3 + _Percent2 + _Percent1) {
					return _FrontColor1;
				}
				else {
					return _BackColor;
				}
			}
			ENDCG
		}
	}
}
