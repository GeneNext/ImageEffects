Shader "CITT/BaseSnow" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpTex ("Bump", 2D) = "bump" {}
		_Gloss ("Gloss", Range(0, 3)) = 1
		_Shininess ("Shininess", Range(0.03, 1)) = 0.5
		
		_DiffuseFactor ("Diffuse Factor", Range(0, 1)) = 1
		_EmissionFactor ("Emission Factor", Range(0, 1)) = 0

		_RimColor ("Rim Color", Color) = (255,40,0,0)
		_RimFactor("Rim Factor", range(0, 5)) = 2

		_AmbientOcclusionTex ("Ambient Occlusion Tex", 2D) = "white" {}
		_AmbientOcclusionFactor ("Ambient Occlusion Factor", range(0, 1)) = 1

		_OutlineColor ("Outline Color", Color) = (255,40,0,0)

		_Snow ("Snow Level", Range(0,1)) = 0
        _SnowColor ("Snow Color", Color) = (1.0,1.0,1.0,1.0)
        _SnowDirection ("Snow Direction", Vector) = (0,1,0)
        _SnowDepth ("Snow Depth", Range(0,0.3)) = 0.1    
		_Wetness ("Wetness", Range(0, 0.5)) = 0.3
	}

	SubShader 
	{
		Tags { "RenderType"="OUTLINE" }

		Cull Back

		CGPROGRAM
		
		float4 _Color;
		sampler2D _MainTex;
		sampler2D _BumpTex;

		float _Gloss;
		float _Shininess;

		float _EmissionFactor;

		sampler2D _AmbientOcclusionTex;
		fixed _AmbientOcclusionFactor;
		
		sampler2D _MaterialAreaTex;

		float _Snow;
        float4 _SnowColor;
        float4 _SnowDirection;
        float _SnowDepth;
		float _Wetness;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_BumpTex;
			float2 uv_AmbientOcclusionTex;
			float3 worldNormal;
			INTERNAL_DATA
		};
		
		#pragma surface surf Specular vertex:vert alpha:add fullforwardshadows
		#include "SpecularLighting.cginc"
		 
        void vert (inout appdata_full v) 
		{
		   //将_SnowDirection转化到模型局部坐标系下
		   float4 sn = mul(UNITY_MATRIX_IT_MV, _SnowDirection);
		
		  if(dot(v.normal, sn.xyz) >= lerp(1,-1, (_Snow*2)/3))
		  {
		      v.vertex.xyz += (sn.xyz + v.normal) * _SnowDepth * _Snow;
		  }
		}

		void surf (Input IN, inout SpecularOutput o) 
		{
			float4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Normal = UnpackNormal(tex2D(_BumpTex, IN.uv_BumpTex));            
			float difference = dot(WorldNormalVector(IN, o.Normal), _SnowDirection.xyz) - lerp(1,-1,_Snow);
            difference = saturate(difference / _Wetness);
            c.rgb = difference*_SnowColor.rgb + (1-difference) *c.rgb;

			o.Albedo = c.rgb * (1 - _EmissionFactor);
			o.Emission = c.rgb * _EmissionFactor;
			o.Gloss =  c.a * _Gloss;
			o.Specular = _Shininess;
			float3 ao = tex2D(_AmbientOcclusionTex, IN.uv_AmbientOcclusionTex);
			o.AmbientOcclusion = lerp(ao, 1, _AmbientOcclusionFactor);
			o.Alpha = difference;
		}

		ENDCG
	}

	Fallback "CITT/BaseShadowCaster"
}
