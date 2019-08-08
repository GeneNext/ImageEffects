Shader "CITT/BaseDiffuseDissolve" 
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB+Acut)", 2D) = "white" {}
		_BumpTex("Bump", 2D) = "bump" {}
		[HDR]_EmissionColor("Emission Color", Color) = (255, 255, 255, 0)
		_EmissionTex("Emission Tex", 2D) = "white" {}
		_AmbientOcclusionTex("Ambient Occlusion Tex", 2D) = "white" {}
		_AmbientOcclusionFactor("Ambient Occlusion Factor", range(0, 1)) = 1

		_NoiseTex("NoiseTex", 2D) = "white" {}
		[HDR]_EdgeColor("Edge Color", Color) = (1,1,1,1)
		_EdgeSize("EdgeSize", Range(0,1)) = 0.2
		_Cutoff("cutoff", Range(0,1)) = 0.0
	}

	SubShader
	{
		Tags 
		{ 
			"Queue" = "AlphaTest" 
			"RenderType" = "Geometry" 
			"IgnoreProjector" = "True" 
		}
	
		Cull Off
	
		CGPROGRAM
	
		#pragma surface surf Diffuse nodirlightmap addshadow
		#pragma target 3.0
		#include "DiffuseLighting.cginc"
		
		fixed4 _Color;
		sampler2D _MainTex;

		sampler2D _BumpTex;

		fixed4 _EmissionColor;
		sampler2D _EmissionTex;

		sampler2D _AmbientOcclusionTex;
		fixed _AmbientOcclusionFactor;

		sampler2D _NoiseTex;
	
		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpTex;
			float2 uv_EmissionTex;
			float2 uv2_AmbientOcclusionTex;
			float2 uv_NoiseTex;
		};
	
	
		half _EdgeSize;
		half4 _EdgeColor;
		half _Cutoff;
	
		void surf(Input IN, inout DiffuseOutput o) 
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpTex, IN.uv_BumpTex));
			o.Emission = tex2D(_EmissionTex, IN.uv_EmissionTex) * _EmissionColor;
			float3 ao = tex2D(_AmbientOcclusionTex, IN.uv2_AmbientOcclusionTex);
			o.AmbientOcclusion = lerp(ao, 1, _AmbientOcclusionFactor);
			o.Albedo *= o.AmbientOcclusion;

			half3 noise = tex2D(_NoiseTex, IN.uv_NoiseTex);
			float cutoff = lerp(0, _Cutoff + _EdgeSize, _Cutoff);
			half edge = smoothstep(cutoff + _EdgeSize, cutoff, clamp(noise.r, _EdgeSize, 1));
	
			o.Emission += _EdgeColor * edge;

			clip(noise - cutoff);
		}
		ENDCG
	}
	
	FallBack "CITT/BaseShadowCaster"
}