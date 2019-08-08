Shader "CITT/TexelOffset"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_FontColor ("Font Color", Color) = (0,0,0,0)
		_FontTex ("Font", 2D) = "black" {}
		_BlendWeight ("Blend Weight", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        #pragma surface surf Lambert addshadow nodirlightmap
		
        float4 _Color;
        sampler2D _MainTex;
		float4 _TexelOffset;
		float4 _FontColor;
		sampler2D _FontTex;

		float _BlendWeight;

        struct Input
        {
            float2 uv_MainTex;
			float2 uv_FontTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            float4 mainColor = tex2D (_MainTex, IN.uv_MainTex + _TexelOffset.xy) * _Color;
			float4 fontColor = tex2D (_FontTex, IN.uv_FontTex) * _FontColor;

			float4 outputColor = 0;
			outputColor.rgb = lerp(mainColor.rgb, fontColor.rgb, _BlendWeight);

            o.Albedo = outputColor.rgb;
            o.Alpha = 1;
        }

        ENDCG
    }

    FallBack "CITT/BaseShadowCaster"
}
