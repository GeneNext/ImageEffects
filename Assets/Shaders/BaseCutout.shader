Shader "CITT/BaseCutout" 
{
	Properties 
	{
		[HDR]_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("MainTex (RGB)", 2D) = "white" {}
		_BumpTex("Bump", 2D) = "bump" {}
		_Gloss("Gloss", Range(0, 3)) = 1
		_Shininess("Shininess", Range(0.03, 1)) = 0.5

		_DiffuseFactor("Diffuse Factor", Range(0, 1)) = 1
		_EmissionFactor("Emission Factor", Range(0, 1)) = 0

		_RimColor("Rim Color", Color) = (255,40,0,0)
		_RimFactor("Rim Factor", range(0, 5)) = 2

		[HDR]_EmissionColor ("EmissionColor", Color) = (0,0,0,0)
		_EmissionTex ("EmissionTex", 2D) = "black" {}
		_AmbientOcclusionTex ("Ambient Occlusion Tex", 2D) = "white" {}
		_AmbientOcclusionFactor ("Ambient Occlusion Factor", range(0, 1)) = 1

		_AlphaCutout ("Alpha Cutout", range(0, 1)) = 0

		_OutlineColor ("Outline Color", Color) = (255, 40, 0, 0)
	}

	SubShader 
	{
		Tags { "RenderType"="OUTLINE" }
		
		ZWrite On
		CGPROGRAM
		#pragma surface surf Specular nodirlightmap nodynlightmap addshadow
		#pragma target 3.0
		#include "SpecularLighting.cginc"
		
		float4 _Color;
		sampler2D _MainTex;

		sampler2D _BumpTex;

		float _Gloss;
		float _Shininess;

		float4 _EmissionColor;
		sampler2D _EmissionTex;

		sampler2D _AmbientOcclusionTex;
		float _AmbientOcclusionFactor;

		float _AlphaCutout;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpTex;
			float2 uv_EmissionTex;
			float2 uv_AmbientOcclusionTex;
		};

		void surf (Input IN, inout SpecularOutput o)
		{
			fixed4 main = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = main.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpTex, IN.uv_BumpTex));
			o.Gloss = main.a * _Gloss;
			o.Specular = _Shininess;
			fixed4 emission = tex2D(_EmissionTex, IN.uv_EmissionTex) * _EmissionColor;
			o.Emission = emission.rgb;
			float3 ao = tex2D(_AmbientOcclusionTex, IN.uv_AmbientOcclusionTex);
			o.Albedo *= lerp(ao, 1, _AmbientOcclusionFactor);
			clip(main.a - _AlphaCutout);
		}
		ENDCG
	}

	Fallback "CITT/BaseShadowCaster"
}
