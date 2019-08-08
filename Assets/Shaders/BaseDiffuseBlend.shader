Shader "CITT/BaseDiffuseBlend" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB+Acut)", 2D) = "white" {}
		_BumpTex ("Bump", 2D) = "bump" {}
		[HDR]_EmissionColor ("Emission Color", Color) = (255, 255, 255, 0)
		_EmissionTex ("Emission Tex", 2D) = "white" {}
		_AmbientOcclusionTex ("Ambient Occlusion Tex", 2D) = "white" {}
		_AmbientOcclusionFactor ("Ambient Occlusion Factor", range(0, 1)) = 1

		_BlendTex("Blend Tex", 2D) = "black" {}
		_BlendFactor("BlendFactor", range(0, 5)) = 1

		_OutlineColor ("Outline Color", Color) = (255, 40, 0, 0)
	}

	SubShader 
	{
		Tags { "RenderType"="OUTLINE" }
		
		CGPROGRAM
		#pragma surface surf Diffuse nodirlightmap addshadow
		#pragma target 3.0
		#include  "DiffuseLighting.cginc"
		
		fixed4 _Color;
		sampler2D _MainTex;
		
		sampler2D _BumpTex;

		fixed4 _EmissionColor;
		sampler2D _EmissionTex;

		sampler2D _AmbientOcclusionTex;
		fixed _AmbientOcclusionFactor;

		sampler2D _BlendTex;
		float _BlendFactor;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpTex;
			float2 uv_EmissionTex;
			float2 uv2_AmbientOcclusionTex;
			float2 uv3_BlendTex;
		};

		void surf (Input IN, inout DiffuseOutput o) 
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			fixed4 blend = tex2D(_BlendTex, IN.uv3_BlendTex);
			o.Albedo = c.rgb * (1 - blend.a);// lerp(c.rgb, blend.rgb * _BlendFactor, blend.a);
			o.Normal = UnpackNormal(tex2D(_BumpTex, IN.uv_BumpTex));
			o.Emission = tex2D(_EmissionTex, IN.uv_EmissionTex) * _EmissionColor + blend.rgb * blend.a;
			float3 ao = tex2D(_AmbientOcclusionTex, IN.uv2_AmbientOcclusionTex);
			o.AmbientOcclusion = lerp(ao, 1, _AmbientOcclusionFactor);
			o.Albedo *= o.AmbientOcclusion;
			o.Alpha = c.a;
		}
		ENDCG
	}

	Fallback "CITT/BaseShadowCaster"
}
