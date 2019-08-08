Shader "CITT/BaseRim" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpTex ("Bump", 2D) = "bump" {}
		_Gloss ("Gloss", Range(0, 3)) = 1
		_Shininess ("Shininess", Range(0.03, 1)) = 0.5

		[HDR]_EmissionColor("Emission Color", Color) = (255, 255, 255, 0)
		_EmissionTex("Emission Tex", 2D) = "white" {}

		[HDR]_RimColor ("Rim Color", Color) = (255,40,0,0)
		_RimCutout ("Rim Cutout", range(0, 1)) = 0
		_RimFactor("Rim Factor", range(0, 5)) = 2

		_AmbientOcclusionTex ("Ambient Occlusion Tex", 2D) = "white" {}
		_AmbientOcclusionFactor ("Ambient Occlusion Factor", range(0, 1)) = 1

		_OutlineColor ("Outline Color", Color) = (255,40,0,0)
	}

	SubShader 
	{
		Tags { "RenderType"="OUTLINE" }
		
		CGPROGRAM
		
		float4 _Color;
		sampler2D _MainTex;
		sampler2D _BumpTex;

		fixed4 _EmissionColor;
		sampler2D _EmissionTex;

		float _Gloss;
		float _Shininess;

		sampler2D _AmbientOcclusionTex;
		fixed _AmbientOcclusionFactor;
		
		sampler2D _MaterialAreaTex;

		float _RimCutout;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpTex;
			float2 uv_EmissionTex;
			float2 uv2_AmbientOcclusionTex;
			float3 viewDir;
		};
		
		#pragma surface surf Specular nodirlightmap addshadow
		#include "SpecularLighting.cginc"

		void surf (Input IN, inout SpecularOutput o) 
		{
			float4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Emission = tex2D(_EmissionTex, IN.uv_EmissionTex) * _EmissionColor;
			o.Normal = UnpackNormal(tex2D(_BumpTex, IN.uv_BumpTex));
			o.Gloss =  c.a * _Gloss;
			o.Specular = _Shininess;
			float3 ao = tex2D(_AmbientOcclusionTex, IN.uv2_AmbientOcclusionTex);
			o.AmbientOcclusion = lerp(ao, 1, _AmbientOcclusionFactor);
			o.Alpha = 1;

			float rim = 1 - saturate(pow(max(0, dot(o.Normal, IN.viewDir) + _RimCutout), _RimFactor));
			o.Emission += rim * _RimColor;
		}

		ENDCG
	}

	Fallback "CITT/BaseShadowCaster"
}
