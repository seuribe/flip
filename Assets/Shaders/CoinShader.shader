// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Flip/CoinShader"
{
    Properties
    {
        _MainTex("Base Texture", 2D) = "" {}
        _SBaseTex("Symbol base Texture", 2D) = "" {}
        _SymbolTex("Symbol Texture", 2D) = "" {}
        _MaskTex("Color Shade Mask", 2D) = "" {}
        _MaskThreshold("Mask White Threshold", Float) = 0.5
        _ReplaceColor("Replacement Color", Color) = (0, 0.75, 0.5, 1)
        _SymbolPos("Symbol position", Float) = 0.5
        _SymbolIndex("Symbol index", Int) = 0
        _Halo("Halo Color", Color) = (0, 0, 0, 0)
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

            sampler2D _MainTex;
            sampler2D _MaskTex;
            sampler2D _SBaseTex;
            sampler2D _SymbolTex;
            float _SymbolPos;
            int _SymbolIndex;
            fixed4 _ReplaceColor;
            float _MaskThreshold;
            float4 _Halo;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 texcoord0 : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 symboluv : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord0 = v.texcoord0;
                int col = fmod(_SymbolIndex, 3);
                int row = 2 - (_SymbolIndex / 3);
                o.symboluv = float2(col / 3.0, row / 3.0);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float maskWhite = tex2D(_MaskTex, i.texcoord0.xy).r;
                fixed4 baseColor = tex2D(_MainTex, i.texcoord0.xy).rgba;

                if (maskWhite > _MaskThreshold) {
                    float avg = (baseColor.r + baseColor.b + baseColor.g) / 3.0;
                    baseColor = float4(_ReplaceColor.rgb * avg, baseColor.a) - _Halo*0.75;
                }

                if (i.texcoord0.y > _SymbolPos && i.texcoord0.y < _SymbolPos + 0.5) {
                    fixed4 overlay = tex2D(_SBaseTex, float2(i.texcoord0.x, (i.texcoord0.y - _SymbolPos) * 2));
                    baseColor = fixed4(lerp(baseColor.rgb, overlay.rgb, overlay.a).rgb, baseColor.a);

                    if (i.texcoord0.y > _SymbolPos + 0.125 && i.texcoord0.y < _SymbolPos + 0.375) {
                        float2 disp = float2(i.texcoord0.x / 3.0, (i.texcoord0.y - _SymbolPos + 0.075) * 3.85 / 3.0 - 0.25);
                        fixed4 symbol = tex2D(_SymbolTex, i.symboluv + disp) - _Halo*0.75;
                        baseColor = fixed4(lerp(baseColor.rgb, symbol.rgb, symbol.a).rgb, baseColor.a);
                    }
                }

                return _Halo + baseColor;
            }
            ENDCG
        }
    }
}
