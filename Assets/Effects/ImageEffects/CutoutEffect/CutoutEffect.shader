Shader "Hidden/CutoutEffect"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_OutlineTex("Outline", 2D) = "white" {}
		_BlurTex("Blur", 2D) = "white" {}
	}
 
	CGINCLUDE

	#include "UnityCG.cginc"
	
	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
 
	sampler2D _MainTex;
	float4 _MainTex_TexelSize;

	sampler2D _ShapeTex;
	float4 _ShapeTex_TexelSize;

	sampler2D _ClipTex;
	float4 _ClipTex_TexelSize;

	v2f vert_shape(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv.xy = v.texcoord.xy;
		return o;
	}
 
	float4 frag_shape(v2f i) : SV_Target
	{
		float4 main = tex2D(_MainTex, i.uv);

		float4 shape = tex2D(_ShapeTex, i.uv);

		return saturate(main * shape);
	}
 
	v2f vert_combine(appdata_img v)
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
 
	float4 frag_combine(v2f i) : SV_Target
	{
		float4 main = tex2D(_MainTex, i.uv);

		float4 clip = tex2D(_ClipTex, i.uv);

		return saturate(main + clip);
	}
 
	ENDCG
 
	SubShader
	{
		//pass 0: 剔除
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off
 
			CGPROGRAM
			#pragma vertex vert_shape
			#pragma fragment frag_shape
			ENDCG
		}
 
		//pass 1: 合并
		Pass
		{
			ZTest Off
			Cull Off
			ZWrite Off
 
			CGPROGRAM
			#pragma vertex vert_combine
			#pragma fragment frag_combine
			ENDCG
		}
	}
}
