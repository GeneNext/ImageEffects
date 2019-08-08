Shader "CITT/BaseCartoon" 
{
	Properties 
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo(RGB)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}
		_Shininess("Shininess", Range(0.01, 1)) = 0.078125

		_RimFactor("Rim Factor", range(0, 5)) = 2
		_Steps("Steps of toon", range(0, 5)) = 1.5
		
		_OutlineColor("Outline Color", Color) = (255,40,0,0)
	}

	SubShader 
	{
		Tags 
		{ 
			"Queue" = "Geometry"
			"RenderType" = "OUTLINE" 
		}
		
		CGPROGRAM

		#pragma surface surf Cartoon nodirlightmap nodynlightmap addshadow

		#include "CartoonLighting.cginc"
		
		float4 _Color;
		sampler2D _MainTex;
		sampler2D _BumpMap;
		float _Shininess;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};

		void surf(Input IN, inout SurfaceOutput o) 
		{
			float4 tex = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = tex.rgb * _Color.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			o.Gloss = 1;
			o.Specular = _Shininess;
			o.Alpha = _Color.a;
		}

		ENDCG
	}

	Fallback "CITT/BaseShadowCaster"
}
