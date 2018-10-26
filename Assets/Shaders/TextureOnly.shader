Shader "Texture Only" {
 
	Properties {
		_MainTex ("Texture", 2D) = "" {}
	}
	
	SubShader {
    	Tags { "Queue" = "Transparent" }
		Pass {
			Lighting Off
	        ZWrite Off

			Blend SrcAlpha OneMinusSrcAlpha
			SetTexture[_MainTex]
		}
	}

}