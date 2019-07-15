Shader "Hidden/GaussianBlurEffect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;

	sampler2D _DepthTex;
	float4 _DepthTex_TexelSize;

	float4 _BlurOffset;

	struct v2f_blur
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
 
	v2f_blur vert_blur(appdata_img v)
	{
		v2f_blur o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;

#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1 - o.uv.y;
#endif	

		return o;
	}
 
	float4 frag_blur(v2f_blur i) : COLOR
	{
		float4 offset = _BlurOffset.xyxy * float4(1, 1, -1, -1);
		
		float4 color = tex2D(_MainTex, i.uv);
		color += tex2D(_MainTex, i.uv + offset.xy);
		color += tex2D(_MainTex, i.uv + offset.zw);
		color += tex2D(_MainTex, i.uv + offset.xy * 2);
		color += tex2D(_MainTex, i.uv + offset.zw * 2);

		return color * 0.2;
	}

	float4 frag_blurWithDepth(v2f_blur i) : COLOR
	{
		float depth = tex2D(_DepthTex, i.uv).r;

		float4 offset = _BlurOffset.xyxy * float4(1, 1, -1, -1) * depth;
		
		float4 color = tex2D(_MainTex, i.uv);
		color += tex2D(_MainTex, i.uv + offset.xy * 2 * tex2D(_DepthTex, i.uv + offset.xy).r);
		color += tex2D(_MainTex, i.uv + offset.zw * 2 * tex2D(_DepthTex, i.uv + offset.zw).r);
		color += tex2D(_MainTex, i.uv + offset.xy * 5 * tex2D(_DepthTex, i.uv + offset.xy * 2).r);
		color += tex2D(_MainTex, i.uv + offset.zw * 5 * tex2D(_DepthTex, i.uv + offset.zw * 2).r);
		
		return color * 0.2;
	}

	struct v2f_output
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};

	v2f_output vert_output(appdata_img v)
	{
		v2f_output o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;

#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1 - o.uv.y;
#endif	

		return o;
	}

	float4 frag_output(v2f_output i) : COLOR
	{
		return tex2D(_MainTex, i.uv);
	}

	ENDCG

	SubShader
	{
		//pass 0: 模糊
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off
 
			CGPROGRAM
			#pragma vertex vert_blur
			#pragma fragment frag_blur
			ENDCG
		}

		//pass 1: 模糊(基于深度值)
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off
 
			CGPROGRAM
			#pragma vertex vert_blur
			#pragma fragment frag_blurWithDepth
			ENDCG
		}

		//pass 2: 矫正倒像，输出
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off
 
			CGPROGRAM
			#pragma vertex vert_output
			#pragma fragment frag_output
			ENDCG
		}
	}
}
