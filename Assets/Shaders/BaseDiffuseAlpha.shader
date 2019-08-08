// Upgrade NOTE: replaced '_LightMatrix0' with 'unity_WorldToLight'

Shader "CITT/BaseDiffuseAlpha" 
{
	Properties 
	{
		[HDR]_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main (RGB)", 2D) = "white" {}

		_NormalTex ("Normal", 2D) = "bump" {}

		_AmbientOcclusionTex ("Ambient Occlusion Tex", 2D) = "white" {}
		_AmbientOcclusionFactor ("Ambient Occlusion Factor", range(0, 1)) = 1

		[HDR]_EmissionColor ("Emission Color", Color) = (255, 255, 255, 0)
		_EmissionTex ("Emission Tex", 2D) = "white" {}

		_Alpha ("Alpha", range(0, 1)) = 1

		_OutlineColor ("Outline Color", Color) = (255, 40, 0, 0)
	}

	CGINCLUDE

	#pragma multi_compile_fog

	#include "UnityCG.cginc"
	#include "Lighting.cginc"
	#include "AutoLight.cginc"

	float4 _Color;

	sampler2D _MainTex;
	float4 _MainTex_ST;

	sampler2D _NormalTex;
	float4 _NormalTex_ST;

	sampler2D _AmbientOcclusionTex;
	float4 _AmbientOcclusionTex_ST;

	float _AmbientOcclusionFactor;

	float4 _EmissionColor;

	sampler2D _EmissionTex;
	float4 _EmissionTex_ST;

	float _AlphaCutout;
	float _Alpha;

	float4 _OutlineColor;

	struct VertexInput
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 tangent : TANGENT;
		float4 texcoord : TEXCOORD0;
		float4 texcoord1 : TEXCOORD1;
	};

	struct VertexOutput
	{
		float4 vertex : SV_POSITION;
		float2 uv_MainTex : TEXCOORD0;
		float2 uv_NormalTex : TEXCOORD1;
		float2 uv_AmbientOcclusionTex : TEXCOORD2;
		float2 uv_EmissionTex : TEXCOORD3;
		float3 worldPosition : TEXCOORD4;
		float3 worldNormal : TEXCOORD5;
		float3 tangent : TEXCOORD6;
		float3 bitangent : TEXCOORD7;
		float3 viewDir : TEXCOORD8;
		float3 lightDir : TEXCOORD9;
		float3 shLighting : TEXCOORD10;
		float4 lightmap : TEXCOORD11;
		UNITY_FOG_COORDS(12)
	};

	VertexOutput vert(VertexInput v)
	{
		VertexOutput output;

		UNITY_INITIALIZE_OUTPUT(VertexOutput, output);

		output.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
		output.uv_NormalTex = TRANSFORM_TEX(v.texcoord, _NormalTex);
		output.uv_AmbientOcclusionTex = TRANSFORM_TEX(v.texcoord, _AmbientOcclusionTex);
		output.uv_EmissionTex = TRANSFORM_TEX(v.texcoord, _EmissionTex);

		output.vertex = UnityObjectToClipPos(v.vertex);
		output.worldPosition = mul(unity_ObjectToWorld, v.vertex);
		output.worldNormal = UnityObjectToWorldNormal(v.normal);
		output.tangent = UnityObjectToWorldNormal(v.tangent);
		output.bitangent = cross(output.tangent, output.worldNormal);
		output.viewDir = UnityWorldSpaceViewDir(output.worldPosition);
		output.lightDir = UnityWorldSpaceLightDir(output.worldPosition);
		
		#ifdef LIGHTMAP_ON
			output.lightmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		#else
			#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
				output.shLighting = 0;
				#ifdef VERTEXLIGHT_ON
					output.shLighting += Shade4PointLights (
					unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
					unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
					unity_4LightAtten0, output.worldPosition, output.worldNormal);
				#endif
				output.shLighting = ShadeSHPerVertex (output.worldNormal, output.shLighting);
			#endif
		#endif
		
		UNITY_TRANSFER_FOG(output, output.vertex);

		return output;
	}

	float4 frag(VertexOutput input) : COLOR
	{
		float4 outputColor = 0;
		float4 mainColor = tex2D(_MainTex, input.uv_MainTex) * _Color;
		float4 aoColor = tex2D(_AmbientOcclusionTex, input.uv_AmbientOcclusionTex);
		float4 emissionColor = tex2D(_EmissionTex, input.uv_EmissionTex) * _EmissionColor;

		outputColor.rgb = mainColor.rgb;
		outputColor.rgb *= lerp(aoColor, 1, _AmbientOcclusionFactor);

		float3 normal = UnpackNormal(tex2D(_NormalTex, input.uv_NormalTex));
		normal = float3(input.tangent * normal.x + input.bitangent * normal.y + input.worldNormal * normal.z);

		#ifdef USING_DIRECTIONAL_LIGHT
			fixed3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
			fixed lightAtten = 1.0;
		#else
			fixed3 lightDir = normalize(_WorldSpaceLightPos0.xyz - input.worldPosition.xyz);

			#if defined(POINT)
				float3 lightCoord = mul(unity_WorldToLight, float4(input.worldPosition, 1)).xyz;
				fixed lightAtten = tex2D(_LightTexture0, dot(lightCoord,lightCoord).rr).UNITY_ATTEN_CHANNEL;
			#elif defined(SPOT)
				float4 lightCoord = mul(unity_WorldToLight, float4(input.worldPosition, 1));
				fixed lightAtten = (lightCoord.z > 0)*tex2D(_LightTexture0, lightCoord.xy / lightCoord.w + 0.5).w * tex2D(_LightTextureB0, dot(lightCoord, lightCoord).rr).UNITY_ATTEN_CHANNEL;
			#else
				fixed lightAtten = 1.0;
			#endif
		#endif
		
		float diff = max(0, dot(normal, lightDir));

		fixed3 lightmap = DecodeLightmap (UNITY_SAMPLE_TEX2D(unity_Lightmap, input.lightmap.xy));

		outputColor.rgb *= diff * _LightColor0.rgb + UNITY_LIGHTMODEL_AMBIENT.rgb + input.shLighting + lightmap;
		outputColor.rgb *= lightAtten;

		outputColor.rgb += emissionColor.rgb;
		outputColor.a = _Alpha;

		UNITY_APPLY_FOG(input.fogCoord, outputColor);

		return saturate(outputColor);
	}
	ENDCG

	SubShader 
	{
		Tags 
		{ 
			"Queue"="Transparent"
			"RenderType"="OUTLINE" 
		}
			
		Pass
		{
			Tags { "LightMode"="ForwardBase" }

			Cull Front
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			
			ENDCG
		}

		Pass
		{
			Tags { "LightMode"="ForwardBase" }

			Cull Back
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase

			ENDCG
		}

		Pass
		{
			Tags { "LightMode"="ForwardAdd" }

			Cull Back
			ZWrite On
			Blend SrcAlpha One

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdadd

			ENDCG
		}
	}

	Fallback "CITT/BaseShadowCaster"
}
