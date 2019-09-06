Shader "Hidden/DepthFogEffect"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "black" {}
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _CameraDepthTexture;

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;

	sampler2D _DepthTex;
	float4 _DepthTex_TexelSize;

	float4 _FogColor;
	float _FogFactor;
	
	v2f vert(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv.xy = v.texcoord.xy;

#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1 - o.uv.y;
#endif	

		return o;
	}

	float4 frag_outputDepth(v2f i) : COLOR
	{
		float4 color = tex2D(_CameraDepthTexture, i.uv);
		float depth = Linear01Depth(UNITY_SAMPLE_DEPTH(color));
		color.r = depth;
		return color;
	}

	float4 frag_blur(v2f i) : COLOR
	{
		float depth = saturate(tex2D(_DepthTex, i.uv).r * _FogFactor + (1 - _FogFactor));

		float4 color = tex2D(_MainTex, i.uv);

		float4 tmpColor = lerp(color, _FogColor, depth);
		color = lerp(color, tmpColor, _FogColor.a);

		return color;
	}

	ENDCG

	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag_outputDepth

			ENDCG
		}

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag_blur

			ENDCG
		}
	}
}
