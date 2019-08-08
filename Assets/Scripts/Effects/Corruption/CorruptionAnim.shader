// Upgrade NOTE: commented out 'sampler2D unity_Lightmap', a built-in variable

Shader "TEST/CorruptionAnim" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main Tex", 2D) = "white" {}

		_CorruptionColor ("Corruption Color", Color) = (1,1,1,1)
		_CorruptionTex ("Corruption Tex", 2D) = "black" {}
		_CorruptionCutoff ("Color Cutoff", float) = 0.2

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

	CGINCLUDE

	#pragma vertex vert
	#pragma fragment frag

	#pragma multi_compile_fog
	#pragma multi_compile LIGHTMAP_ON LIGHTMAP_OFF
			
	#include "UnityCG.cginc"
	#include "Lighting.cginc"

	float4 _Color;
	sampler2D _MainTex;
	float4 _MainTex_ST;

	float4 unity_Lightmap_ST;
	
	float4 _CorruptionColor;
	sampler2D _CorruptionTex;
	float4 _CorruptionTex_ST;
	float _CorruptionCutoff;

	float4 _TwinkleColor;
	float _TwinkleFactor;
	float _TwinkleMax;
	float _TwinkleMin;
	float _TwinkleSpeed;

	float4 _StartPos;
	float _CutoffRange;
	float4 _OutlineColor;
	float _OutlineRange;
	float _OutlineFactor;
	float _FalloffRange;

	struct VertexInput
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 texcoord : TEXCOORD0;
		float4 texcoord1 : TEXCOORD1;
		float4 color : COLOR;
	};

	struct VertexOutput 
	{
		float4 vertex : SV_POSITION;

	    float3 worldNormal : TEXCOORD0;
		float3 viewDir : TEXCOORD1;

		float2 uv_MainTex : TEXCOORD2;
		float2 uv_CorruptionTex : TEXCOORD3;

	    float4 position : TEXCOORD4;

		UNITY_FOG_COORDS(5)
		
		#ifdef LIGHTMAP_ON
		float2 uv_Lightmap : TEXCOORD6;
		#endif

		float3 SHLighting : COLOR;
	};

	VertexOutput vert (VertexInput v)
	{
		VertexOutput output;

		UNITY_INITIALIZE_OUTPUT(VertexOutput, output);
		output.vertex = UnityObjectToClipPos(v.vertex);

		//uv
		output.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
		output.uv_CorruptionTex = TRANSFORM_TEX(v.texcoord, _CorruptionTex);

		#ifdef LIGHTMAP_ON
		output.uv_Lightmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		#endif

		//
		output.position = mul(unity_ObjectToWorld, v.vertex);
		output.worldNormal = UnityObjectToWorldNormal(v.normal);
		output.viewDir = normalize(_WorldSpaceCameraPos - output.position);
		
		output.SHLighting = ShadeSH9(float4(output.worldNormal,1));

		//雾
		UNITY_TRANSFER_FOG(output, output.position);

		return output;
	}

	inline float4 CalcLighting(float4 color, VertexOutput input)
	{
		float3 lightColor;

		#ifdef LIGHTMAP_ON
			fixed3 lightmap = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, input.uv_Lightmap));
			lightColor = lightmap;
		#else
			fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;
			fixed3 diffuse = _LightColor0.rgb * saturate(dot(input.worldNormal, normalize(_WorldSpaceLightPos0.xyz)));
			fixed3 spheric = input.SHLighting;
			lightColor = ambient + diffuse + spheric;
		#endif

		color.rgb = color.rgb * lightColor.rgb;

		return color;
	}

	inline float4 ClampColor(float4 color)
	{
		color.r = clamp(color.r, 0, 1);
		color.g = clamp(color.g, 0, 1);
		color.b = clamp(color.b, 0, 1);
		color.a = clamp(color.a, 0, 1);
		return color;
	}

	ENDCG

	SubShader 
	{
		Tags 
		{
			"Queue"="Geometry"
			"RenderType"="Opaque" 
		}
		
        Pass
		{
			CGPROGRAM

			#pragma multi_compile_fwdbase

			float4 frag (VertexOutput input) : COLOR
			{		
				//主纹理
				float4 mainTex = tex2D(_MainTex, input.uv_MainTex);
				float4 mainColor = mainTex * _Color;
				//以目标点为圆心剔除
				float distance = length(input.position.xyz -  _StartPos.xyz);
				
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
				
				float4 c;
				c.rgb = mainColor.rgb * _Color.rgb - falloff * (1 - lerp(1, _CorruptionColor, mainColor.a));
				c.a = mainColor.a;

				//float4 c = mainColor * _Color;
				c = CalcLighting(c, input);
				c = ClampColor(c);

				UNITY_APPLY_FOG(input.fogCoord, c);

				return c;
			}

			ENDCG
		}
		
		
        Pass
		{
			//Blend SrcAlpha OneMinusSrcAlpha
			//Blend One One

			CGPROGRAM

			#pragma multi_compile_fwdadd
			
			float4 frag (VertexOutput input) : COLOR
			{
				//主纹理
				float4 mainTex = tex2D(_MainTex, input.uv_MainTex);
				float4 mainColor = mainTex * _Color;
				float4 corruptionTex = tex2D (_CorruptionTex, input.uv_CorruptionTex);
				float grayScale = clamp(mainTex.a - _CorruptionCutoff, 0, mainTex.a);
				float4 corruptionColor = grayScale * corruptionTex * _CorruptionColor;

				//
				float4 c;
				c.rgb = lerp(mainColor.rgb, corruptionColor.rgb, mainTex.a);
				//c.rgb = corruptionColor.rgb;
				//c.a = grayScale;
				c.a = 1;
				clip(grayScale);
				
				float noiseX = tex2D(_CorruptionTex, input.uv_CorruptionTex + float2(0, 1) * _Time.x * 0.5);
				float noiseY = tex2D(_CorruptionTex, input.uv_CorruptionTex + float2(1, 0) * _Time.x * 0.5);
				float2 noiseUV = float2(noiseX, noiseY) * 0.5;

				//叠加闪烁纹理
				float4 twinkle = tex2D(_CorruptionTex, input.uv_CorruptionTex + noiseUV) * _TwinkleColor * _TwinkleFactor * clamp((sin(_Time.y * _TwinkleSpeed) + 1), _TwinkleMin, _TwinkleMax);
				c.rgb += corruptionColor * twinkle.rgb * grayScale;                                                                                                                                                                                                                                                                                                                                 

				//以目标点为圆心剔除
				float distance = length(input.position.xyz -  _StartPos.xyz);
				//c.a = distance - _CutoffRange;
				clip(distance - _CutoffRange);

				float outline = _CutoffRange + _OutlineRange;

				if(distance < outline)
				{
					//颜色随距离衰减
					float falloff = (outline - distance) / _OutlineRange;
					c.rgb += _OutlineColor.rgb * _OutlineFactor * falloff;

					//根据纹理形状和衰减范围剔除
					float4 falloffTex = tex2D(_CorruptionTex, input.uv_CorruptionTex);
					//c.a = Luminance(falloffTex.rgb) - falloff + _FalloffRange;

					clip(Luminance(falloffTex.rgb) - falloff + _FalloffRange);
				}

				c = CalcLighting(c, input);
				c = ClampColor(c);

				UNITY_APPLY_FOG(input.fogCoord, c);

				return c;
			}

			ENDCG
		}
	}

	FallBack "Diffuse"
}
