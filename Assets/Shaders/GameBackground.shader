// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/GameBack"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_Noise("Noise Factor", Float) = 0.1
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			half4 _Color;
			float _Noise;

			float3 noise(float3 input) {
				return frac(sin(input.x * input.y) * cos(input.y + input.z) * 43758.5453) - 0.5;
			}

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i, UNITY_VPOS_TYPE screenPos : SV_POSITION) : SV_Target
			{
				half4 ret = _Color;
				float dx = (_ScreenParams.x/2 - screenPos.x);
				float dy = (_ScreenParams.y/2 - screenPos.y);
				float mn = min(_ScreenParams.x, _ScreenParams.y);
				float d =  (dx * dx + dy * dy) / (mn * mn);
				float tt = _Time.x/10 > 1 ? 1 : _Time.x/10;
				float ns = noise(screenPos) * tt * _Noise * d * (1 - _SinTime.x / 10);
				ns = clamp(ns, -0.25, 0.25);
				ret.r += ns;
				ret.g += ns;
				ret.b += ns;
			//				ret.g += noise(screenPos) * 0.01;
				return ret;
			}
		ENDCG
		}
	}
}
