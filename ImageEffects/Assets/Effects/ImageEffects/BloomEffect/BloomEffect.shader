Shader "Hidden/BloomEffect"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_BlurTex("Blur", 2D) = "white" {}
	}
 
	CGINCLUDE

	#include "UnityCG.cginc"
	
	struct v2f_threshold
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
 
	struct v2f_bloom
	{
		float4 pos : SV_POSITION;
		float2 uv  : TEXCOORD0;
		float2 uv1 : TEXCOORD1;
	};
 
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;

	sampler2D _BlurTex;
	float4 _BlurTex_TexelSize;

	float4 _BloomThreshold;
	float4 _BloomColor;
	float _BloomSaturation;
	float _BloomIntensity;
 
	//提取高亮部分
	v2f_threshold vert_threshold(appdata_img v)
	{
		v2f_threshold o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;

#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1 - o.uv.y;
#endif	
		return o;
	}
 
	float4 frag_threshold(v2f_threshold i) : SV_Target
	{
		float4 color = tex2D(_MainTex, i.uv);

		float grayScale = Luminance(color.rgb);

		float4 bloomColor = color - _BloomThreshold;

		float4 outputColor = bloomColor * _BloomIntensity;

		outputColor = lerp(grayScale, outputColor, _BloomSaturation) * _BloomColor;

		return outputColor;
	}
 
	v2f_bloom vert_bloom(appdata_img v)
	{
		v2f_bloom o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv.xy = v.texcoord.xy;
		o.uv1.xy = o.uv.xy;
#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1 - o.uv.y;
#endif	
		return o;
	}
 
	float4 frag_bloom(v2f_bloom i) : SV_Target
	{
		float4 main = tex2D(_MainTex, i.uv1);
		float4 blur = tex2D(_BlurTex, i.uv);
		float4 final = 1 - (1 - main) * (1 - blur);
		//float4 final = main + blur;
		
		return final;
	}
 
	ENDCG
 
	SubShader
	{
		//pass 0: 提取高亮部分
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off
 
			CGPROGRAM
			#pragma vertex vert_threshold
			#pragma fragment frag_threshold
			ENDCG
		}
 
		//pass 1: 合并
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off
 
			CGPROGRAM
			#pragma vertex vert_bloom
			#pragma fragment frag_bloom
			ENDCG
		}
	}
}
