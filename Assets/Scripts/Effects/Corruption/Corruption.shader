Shader "TEST/Corruption" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main Tex", 2D) = "white" {}

		_CorruptionTex ("Corruption Tex", 2D) = "black" {}
		_CorruptionColor ("Corruption Color", Color) = (1,1,1,1)
		_CorruptionCutoff ("Color Cutoff", float) = 0.2

		_NoiseFactor ("Noise Factor", float) = 0.3
		_NoiseSpeed ("Noise Speed", float) = 0.6

		_TwinkleColor ("Twinkle Color", Color) = (1,1,1,1)
		_TwinkleFactor ("Twinkle Factor", float) = 1
		_TwinkleMax ("Twinkle Max", float) = 1
		_TwinkleMin ("Twinkle Min", float) = 0.2
		_TwinkleSpeed ("Twinkle Speed", float) = 0.5

		_StartPos ("StartPosition", Vector) = (0,0,0,0)
		_CutoffRange ("Cutoff Range", float) = 1
		_OutlineColor ("Outline Color", Color) = (1,1,1,1)
		_OutlineRange ("Outline Range", float) = 0
		_OutlineFactor ("Outline Factor", float) = 1
		_FalloffRange ("Falloff Range", float) = 1
	}

	SubShader 
	{
		Tags 
		{
			"Queue"="Geometry"
			"RenderType"="Opaque" 
		}

		CGPROGRAM

		#pragma surface surf Lambert fullforwardshadows
		
		fixed4 _Color;
		sampler2D _MainTex;

		float _EmissionFactor;
		
		float4 _CorruptionColor;
		float4 _StartPos;
		float _CutoffRange;
		float _OutlineRange;

		struct Input 
		{
			float2 uv_MainTex;
			float3 worldPos;
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{		
			float4 mainColor = tex2D(_MainTex, IN.uv_MainTex);
			//以目标点为圆心剔除
			float distance = length(IN.worldPos.xyz -  _StartPos.xyz);
			//o.Alpha = distance - _CutoffRange;

			float outline = _CutoffRange + _OutlineRange;
			float falloff = _CutoffRange - distance;
			if(distance < outline)
			{
				//颜色随距离衰减
				falloff = (outline - distance) / _OutlineRange;
				falloff = 1 - clamp(falloff, 0, 1);
			}
			else
			{
				falloff = 1;
			}
			o.Albedo.rgb = mainColor.rgb * _Color.rgb - falloff * (1 - lerp(1, _CorruptionColor, mainColor.a));
			o.Albedo.r = clamp(o.Albedo.r, 0, 1);
			o.Albedo.g = clamp(o.Albedo.g, 0, 1);
			o.Albedo.b = clamp(o.Albedo.b, 0, 1);
		
			o.Emission = o.Albedo * _EmissionFactor;
			o.Albedo = o.Albedo * (1 - _EmissionFactor);

			o.Alpha = mainColor.a;
		}
		ENDCG
		
		CGPROGRAM

		float4 _Color;
		sampler2D _MainTex;
		sampler2D _CorruptionTex;

		float _NoiseFactor;
		float _NoiseSpeed;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_CorruptionTex;
			float3 worldPos;
		};

		#pragma surface surf Lambert decal:add fullforwardshadows

		#include "CorruptionLib.cginc"

		void surf (Input IN, inout SurfaceOutput o) 
		{
			//主纹理
			float4 mainTex= tex2D (_MainTex, IN.uv_MainTex) * _Color;
			float4 corruptionTex = tex2D (_CorruptionTex, IN.uv_CorruptionTex);
			
			float speed = Luminance(tex2D(_CorruptionTex, IN.uv_CorruptionTex + _Time.x * 0.1).rgb) * _NoiseFactor + _Time.x * _NoiseSpeed;
			//float noiseX = tex2D(_CorruptionTex, IN.uv_CorruptionTex + float2(0, speed));
			//float noiseY = tex2D(_CorruptionTex, IN.uv_CorruptionTex + float2(speed, 0));
			//float2 noiseUV = float2(noiseX, noiseY) * 0.5;
			float2 noiseUV = tex2D(_CorruptionTex, IN.uv_CorruptionTex + float2(speed, speed));

			//闪烁纹理
			float4 twinkleTex = tex2D(_CorruptionTex, IN.uv_CorruptionTex + noiseUV);

			float4 color = CalcCorruption(mainTex, corruptionTex, twinkleTex, IN.worldPos);

			o.Albedo = color.rgb;
			o.Alpha = color.a;
		}
		ENDCG
	}

	FallBack "Diffuse"
}
