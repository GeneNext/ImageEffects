Shader "CITT/BaseAlpha" 
{
	Properties 
	{
		[HDR]_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Emission (RGB)", 2D) = "white" {}
		_AmbientOcclusionTex ("Ambient Occlusion Tex", 2D) = "white" {}
		_AmbientOcclusionFactor ("Ambient Occlusion Factor", range(0, 1)) = 1

		_AlphaCutout ("Alpha Cutout", range(0, 1)) = 0

		_OutlineColor ("Outline Color", Color) = (255, 40, 0, 0)
	}

	CGINCLUDE

	#pragma multi_compile_fog

	#include "UnityCG.cginc"

	float4 _Color;

	sampler2D _MainTex;
	float4 _MainTex_ST;

	sampler2D _AmbientOcclusionTex;
	float4 _AmbientOcclusionTex_ST;

	float _AmbientOcclusionFactor;
	float _AlphaCutout;

	float4 _OutlineColor;

	struct VertexInput
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 texcoord : TEXCOORD0;
	};

	struct VertexOutput
	{
		float4 vertex : SV_POSITION;
		float2 uv_MainTex : TEXCOORD0;
		float2 uv_AmbientOcclusionTex : TEXCOORD1;
	};

	VertexOutput vert(VertexInput v)
	{
		VertexOutput output;

		UNITY_INITIALIZE_OUTPUT(VertexOutput, output);

		output.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
		output.uv_AmbientOcclusionTex = TRANSFORM_TEX(v.texcoord, _AmbientOcclusionTex);

		output.vertex = UnityObjectToClipPos(v.vertex);

		return output;
	}

	float4 frag(VertexOutput input) : COLOR
	{
		float4 outputColor = tex2D(_MainTex, input.uv_MainTex) * _Color;
		float3 ao = tex2D(_AmbientOcclusionTex, input.uv_AmbientOcclusionTex);
		outputColor.rgb *= lerp(ao, 1, _AmbientOcclusionFactor);
		outputColor.a = saturate(outputColor.a - _AlphaCutout);

		return saturate(outputColor);
	}
	ENDCG

	SubShader 
	{
		Tags 
		{ 
			"Queue"="Transparent"
			"RenderType"="OUTLINE" 
		}
			
		Pass
		{
			Cull Front
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			ENDCG
		}

		Pass
		{
			Cull Back
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			ENDCG
		}
	}

	Fallback "CITT/BaseShadowCaster"
}
