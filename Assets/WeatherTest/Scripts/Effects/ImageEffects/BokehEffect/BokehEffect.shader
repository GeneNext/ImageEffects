Shader "Hidden/BokehEffect"
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
	
	sampler2D _BokehTex;
	float4 _BokehTex_TexelSize;

	float _FocalDistance;
	float _DepthOfField;

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
		color = abs(_FocalDistance - Linear01Depth(UNITY_SAMPLE_DEPTH(color)));
		
		 return saturate(color - _DepthOfField);
		 //return color;
	}

	float4 frag_outputBlend(v2f i) : COLOR
	{
		float depth = tex2D(_DepthTex, i.uv).r;

		float4 mainTex = tex2D(_MainTex, i.uv);
		float4 bokehTex = tex2D(_BokehTex, i.uv);

		if(depth	< 0.001)
		{
			return mainTex;
		}
		else
		{
			return bokehTex;
		}
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
			#pragma fragment frag_outputBlend
			ENDCG
		}
	}
}
