Shader "CITT/BaseCharacter"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpTex("Bump", 2D) = "bump" {}
		[HideInInspector]_DefaultBumpTex("DefaultBump", 2D) = "bump" {}
		_Gloss("Gloss", Range(0, 3)) = 1
		_Shininess("Shininess", Range(0.03, 1)) = 0.5

		_DiffuseFactor("Diffuse Factor", Range(0, 1)) = 1
		_EmissionFactor("Emission Factor", Range(0, 1)) = 0

		_RimColor("Rim Color", Color) = (255,40,0,0)
		_RimFactor("Rim Factor", range(0, 5)) = 2

		_ReflectTex("Reflect Tex", Cube) = ""{}
		_ReflectFactor("Reflect Factor", range(0, 5)) = 1

		_AmbientOcclusionTex("Ambient Occlusion Tex", 2D) = "white" {}
		_AmbientOcclusionFactor("Ambient Occlusion Factor", range(0, 1)) = 1

		_MaterialAreaTex("Material Area Tex", 2D) = "black" {}

		_AlphaTex("Alpha", 2D) = "white" {}
		_Cutoff("Cutoff", range(0, 1)) = 0.0001

		_OutlineColor("Outline Color", Color) = (255,40,0,0)
	}

	SubShader
	{
		Tags 
		{
			"Queue" = "Geometry"
			"RenderType" = "OUTLINE" 
		}

		ZWrite On

		CGPROGRAM

		float4 _Color;
		sampler2D _MainTex;
		sampler2D _BumpTex;
		sampler2D _DefaultBumpTex;

		float _Gloss;
		float _Shininess;

		float _EmissionFactor;

		sampler2D _AmbientOcclusionTex;
		fixed _AmbientOcclusionFactor;

		sampler2D _MaterialAreaTex;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpTex;
			float2 uv_DefaultBumpTex;
			float2 uv_AmbientOcclusionTex;
			float2 uv_MaterialAreaTex;
		};

		#pragma surface surf Character fullforwardshadows 
		#pragma target 3.0
		#include "CharacterLighting.cginc"

		void surf(Input IN, inout CharacterOutput o)
		{
			float4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb * (1 - _EmissionFactor);
			o.Emission = c.rgb * _EmissionFactor;
			o.Gloss = _Gloss;
			o.Specular = _Shininess;
			o.Alpha = 1;

			float3 ao = tex2D(_AmbientOcclusionTex, IN.uv_AmbientOcclusionTex).rgb;
			o.AmbientOcclusion = lerp(ao, 1, _AmbientOcclusionFactor);

			o.MaterialArea = tex2D(_MaterialAreaTex, IN.uv_MaterialAreaTex);
			o.MaterialStrength = saturate(o.MaterialArea.r + o.MaterialArea.g + o.MaterialArea.b);

			float3 defaultNormal = UnpackNormal(tex2D(_DefaultBumpTex, IN.uv_DefaultBumpTex));
			float3 normal = UnpackNormal(tex2D(_BumpTex, IN.uv_BumpTex));
			o.Normal = lerp(defaultNormal, normal, o.MaterialStrength);
		}

		ENDCG
	}

	Fallback "CITT/BaseShadowCaster"
}
