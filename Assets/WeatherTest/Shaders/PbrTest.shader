Shader "Custom/PbrTest"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Albedo ("Diffuse", 2D) = "white" {}
		_Normal ("Normal", 2D) = "white" {}
		_MRAMap ("MRA", 2D) = "white" {}

		_Metalness ("Metalness", Range(0.0, 1)) = 0.8
		_Roughness ("Roughness", Range(0.0, 1)) = 0.8
		_AO ("AO", Range(0.0, 1)) = 1
	}

	CGINCLUDE

	#pragma multi_compile_fog

	#include "UnityCG.cginc"
	#include "Lighting.cginc"
	#include "AutoLight.cginc"
	#include "UnityPBSLighting.cginc"

	//顶点着色器输出
	struct v2f
	{
		fixed4 vertex : SV_POSITION;
		float2 uv : TEXCOORD0;
		float4 TtoW0 : TEXCOORD1;
		float4 TtoW1 : TEXCOORD2;
		float4 TtoW2 : TEXCOORD3;
		float3 SH : TEXCOORD4;
		UNITY_FOG_COORDS(5)
		float4 lmap : TEXCOORD7;
		UNITY_LIGHTING_COORDS(8, 9)
		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO
	};

	uniform float _Rain;

	float4 _Color;

	sampler2D _Albedo;
	fixed4 _Albedo_ST;
	sampler2D _Normal;
	sampler2D _MRAMap;

	fixed _Metalness;
	fixed _Roughness;
	fixed _AO;

	v2f vert(appdata_full v)
	{
		UNITY_SETUP_INSTANCE_ID(v);
		v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _Albedo);

		float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
		fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
		fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
		fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w;

		o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
		o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
		o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
		
		#ifdef DYNAMICLIGHTMAP_ON
		o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
		#endif

		#ifdef LIGHTMAP_ON
		o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		#endif

		#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
		o.SH = 0;
		#ifdef VERTEXLIGHT_ON
		o.SH += Shade4PointLights (
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, worldPos, worldNormal);
		#endif
		o.SH = ShadeSHPerVertex (worldNormal, o.SH);
		#endif
		
		//TRANSFER_SHADOW(o);
		UNITY_TRANSFER_FOG(o, o.vertex);
		
		#ifdef LIGHTMAP_ON
		UNITY_TRANSFER_LIGHTING(o, v.texcoord1.xy);
		#endif

		return o;
	}

	ENDCG

	SubShader
	{
		Tags
		{
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
		}

		LOD 100

		ZWrite On

		Pass
		{
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase

			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandard, o);
				o.Albedo = tex2D(_Albedo, i.uv) * _Color;
				fixed3 normal = UnpackNormal(tex2D(_Normal, i.uv));
				o.Normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));
				fixed4 mra = tex2D(_MRAMap, i.uv);
				o.Metallic = lerp(mra.r * _Metalness, 1, _Rain);
				o.Smoothness = lerp(mra.g * _Roughness, 1, _Rain);
				o.Occlusion = lerp(mra.b * _AO, 1, _Rain);
				o.Emission = 0.0;
				o.Alpha = 0.0;

				float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
				fixed3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				#ifndef USING_DIRECTIONAL_LIGHT
					fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				#else
					fixed3 lightDir = _WorldSpaceLightPos0.xyz;
				#endif
				
				UNITY_LIGHT_ATTENUATION(atten, i, worldPos);

				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = _LightColor0.rgb;
				gi.light.dir = lightDir;
				UnityGIInput giInput;

				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = worldPos;
				giInput.worldViewDir = viewDir;
				giInput.atten = atten;

				#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
				giInput.lightmapUV = i.lmap;
				#else
				giInput.lightmapUV = 0.0;
				#endif

				#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
				giInput.ambient = i.SH;
				#else
				giInput.ambient.rgb = 0.0;
				#endif

				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
				#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
				giInput.boxMin[0] = unity_SpecCube0_BoxMin;
				#endif
				#ifdef UNITY_SPECCUBE_BOX_PROJECTION
				giInput.boxMax[0] = unity_SpecCube0_BoxMax;
				giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
				giInput.boxMax[1] = unity_SpecCube1_BoxMax;
				giInput.boxMin[1] = unity_SpecCube1_BoxMin;
				giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
				#endif
				LightingStandard_GI(o, giInput, gi);

				fixed4 final = LightingStandard(o, viewDir, gi);
				final.a = 0;

				UNITY_APPLY_FOG(i.fogCoord, final);
				UNITY_OPAQUE_ALPHA(final.a);

				return  saturate(final);
			}

			ENDCG
		}

		Pass
		{
			Tags { "LightMode" = "ForwardAdd" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdadd

			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandard, o);
				o.Albedo = tex2D(_Albedo, i.uv) * _Color;
				fixed3 normal = UnpackNormal(tex2D(_Normal, i.uv));
				o.Normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));
				fixed4 mra = tex2D(_MRAMap, i.uv);
				o.Metallic = lerp(mra.r * _Metalness, 1, _Rain);
				o.Smoothness = lerp(mra.g * _Roughness, 1, _Rain);
				o.Occlusion = lerp(mra.b * _AO, 1, _Rain);
				o.Emission = 0.0;
				o.Alpha = 0.0;  
				
				float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);
				fixed3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				#ifndef USING_DIRECTIONAL_LIGHT
					fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				#else
					fixed3 lightDir = _WorldSpaceLightPos0.xyz;
				#endif
				
				UNITY_LIGHT_ATTENUATION(atten, i, worldPos);

				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = _LightColor0.rgb;
				gi.light.dir = lightDir;
				gi.light.color *= atten;
				float4 final = LightingStandard (o, viewDir, gi);
				final.a = 0;
				UNITY_APPLY_FOG(_unity_fogCoord, final);
				UNITY_OPAQUE_ALPHA(final.a);

				return saturate(final);
			}

			ENDCG
		}
	}
    FallBack "Diffuse"
}