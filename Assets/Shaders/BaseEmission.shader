Shader "CITT/BaseEmission" 
{
	Properties 
	{
		[HDR]_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Emission (RGB)", 2D) = "white" {}
		_AmbientOcclusionTex ("Ambient Occlusion Tex", 2D) = "white" {}
		_AmbientOcclusionFactor ("Ambient Occlusion Factor", range(0, 1)) = 1

		_OutlineColor ("Outline Color", Color) = (255, 40, 0, 0)
	}

	SubShader 
	{
		Tags { "RenderType"="OUTLINE" }
		
		ZWrite On
		CGPROGRAM
		#pragma surface surf Lambert nodirlightmap addshadow
		
		fixed4 _Color;
		sampler2D _MainTex;

		sampler2D _AmbientOcclusionTex;
		fixed _AmbientOcclusionFactor;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_AmbientOcclusionTex;
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Emission = c.rgb;
			float3 ao = tex2D(_AmbientOcclusionTex, IN.uv_AmbientOcclusionTex);
			o.Emission *= lerp(ao, 1, _AmbientOcclusionFactor);
		}
		ENDCG
	}

	Fallback "CITT/BaseShadowCaster"
}
