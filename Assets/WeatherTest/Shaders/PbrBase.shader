Shader "WeatherTest/PbrBase"
{
	Properties
	{
		_MainTex("Main Tex", 2D) = "white" {}
		_NormalTex("Normal Tex ", 2D) = "bump" {}
		_MRATex("MRA Tex", 2D) = "white" {}

		_Metalness("Metalness", Range(0.0, 1)) = 0
		_Roughness("Roughness", Range(0.0, 1)) = 0
		_AO("AO", Range(0.0, 1)) = 1
		
		[NoScaleOffset] _ReflectMapLDR ("Reflect Map LDR", CUBE) = "cube" { }       
        _ReflectLuminace ("Reflect Map Luminace", Range(0, 3)) = 1
		        
		_FresnelColor("Fresnel Color", Color) = (1,1,1,1)
		_FresnelBase("Fresnel Base", Range(0, 2)) = 0
        _FresnelScale("Fresnel Scale", Range(0, 5)) = 3
        _FresnelIntensity("Fresnel Intensity", Range(0, 2)) = 1
		
        _SnowColor ("Snow Color", Color) = (1.0,1.0,1.0,1.0)
		_SnowTex ("Snow Tex", 2D) = "white" {}
		_SnowLevel ("Snow Level", Range(0,1)) = 0.5
        _SnowDirection ("Snow Direction", Vector) = (0,0.3,0)
        _SnowDepth ("Snow Depth", Range(0, 3)) = 3
		_SnowWetness ("Snow Wetness", Range(0, 0.5)) = 0.5

		[Toggle] ALPHA_CUTOUT ("Alpha Cutout", float) = 0
	}

	CGINCLUDE
	
	#pragma shader_feature ALPHA_CUTOUT_ON

	#pragma multi_compile WEATHER_RAIN_OFF WEATHER_RAIN_ON
	#pragma multi_compile WEATHER_SNOW_OFF WEATHER_SNOW_ON

	#pragma multi_compile_fog

	#include "UnityCG.cginc"
	#include "Lighting.cginc"
	#include "AutoLight.cginc"

	//顶点着色器输入
	struct appdata
	{
		half4 vertex : POSITION;
		float2 uv : TEXCOORD0;
		float2 uv1 : TEXCOORD1;
		half3 normal : NORMAL;
		float4 tangent : TANGENT;//切线方向
	};

	//顶点着色器输出
	struct v2f
	{
		half4 vertex : SV_POSITION;
		float2 uv_MainTex : TEXCOORD0;
		float2 uv_NormalTex : TEXCOORD1;
		float2 uv_MRATex : TEXCOORD2;
		float2 uv_SnowTex : TEXCOORD3;
		float4 TtoW0 : TEXCOORD4;
		float4 TtoW1 : TEXCOORD5;
		float4 TtoW2 : TEXCOORD6;
		float3 SH : TEXCOORD7;
		SHADOW_COORDS(8)
		UNITY_FOG_COORDS(9)
		#ifdef LIGHTMAP_ON
		float2 uv_LightMap : TEXCOORD10;
		#endif
	};

	uniform float _Rain;
	uniform float _Snow;

	sampler2D _MainTex;
	half4 _MainTex_ST;
	sampler2D _NormalTex;
	half4 _NormalTex_ST;
	sampler2D _MRATex;
	half4 _MRATex_ST;

	half _Metalness;
	half _Roughness;
	half _AO;
	
	samplerCUBE _ReflectMapLDR;
	half _ReflectLuminace;

	float4 _FresnelColor;
	float _FresnelBase;
	float _FresnelScale;
	float _FresnelIntensity;
	
	#if WEATHER_SNOW_ON
	sampler2D _SnowTex;
	half4 _SnowTex_ST;
	float _SnowLevel;
    float4 _SnowColor;
    float4 _SnowDirection;
    float _SnowDepth;
	float _SnowWetness;
	#endif

	v2f vert(appdata v)
	{
		v2f o;

		UNITY_INITIALIZE_OUTPUT(v2f, o);

		float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
		half3 worldNormal = UnityObjectToWorldNormal(v.normal);
		half3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
		half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
		half3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;

		o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
		o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
		o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);

		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv_MainTex = TRANSFORM_TEX(v.uv, _MainTex);
		o.uv_NormalTex = TRANSFORM_TEX(v.uv, _NormalTex);
		o.uv_MRATex = TRANSFORM_TEX(v.uv, _MRATex);
		
		#if WEATHER_SNOW_ON
		o.uv_SnowTex = TRANSFORM_TEX(v.uv, _SnowTex);
		#endif
		
		#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
		o.SH = 0;
		#if VERTEXLIGHT_ON
		o.SH += Shade4PointLights (
			unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
			unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
			unity_4LightAtten0, worldPos, worldNormal);
		#endif
		o.SH = ShadeSHPerVertex (worldNormal, o.SH);
		#endif
		
		TRANSFER_SHADOW(o);
		UNITY_TRANSFER_FOG(o, o.vertex);

		#if LIGHTMAP_ON
		o.uv_LightMap = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
		#endif

		return o;
	}

	//法线分布函数
	half DistributionGGX(half3 NdotH, half roughness)
	{
		half NdotH2 = NdotH * NdotH;
		half roughness2 = roughness * roughness;

		half nom = roughness2 * roughness2;
		half denom = (NdotH2 * (nom - 1.0) + 1.0);
		denom = UNITY_PI * denom * denom;

		return nom / denom;
	}

	//几何遮蔽函数
	half GeometrySchlickGGX(half NdotV, half roughness)
	{
		half r = roughness + 1.0;
		half k = (r * r) / 8.0;

		half nom = NdotV;
		half denom = NdotV * (1.0 - k) + k;

		return nom / denom;
	}

	//双向几何遮蔽函数
	half GeometrySmith(half3 NdotV, half3 NdotL, half roughness)
	{
		half ggx2 = GeometrySchlickGGX(NdotV, roughness);
		half ggx1 = GeometrySchlickGGX(NdotL, roughness);

		return ggx1 * ggx2;
	}

	//菲涅尔方程
	half3 fresnelSchlick(half3 NdotV, half3 metal)
	{
		return metal + (1.0 - metal) * pow(1.0 - NdotV, 5.0);
	}

	half4 frag(v2f i) : SV_Target
	{
		half4 albedo = tex2D(_MainTex, i.uv_MainTex);
		
		#ifdef ALPHA_CUTOUT_ON
		clip(albedo.a - 0.01);
		#endif

		half3 normal = UnpackNormal(tex2D(_NormalTex, i.uv_NormalTex));
		normal = normalize(half3(dot(i.TtoW0.xyz, normal), dot(i.TtoW1.xyz, normal), dot(i.TtoW2.xyz, normal)));

		float3 worldPos = float3(i.TtoW0.w, i.TtoW1.w, i.TtoW2.w);

		half3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
		half3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
		half3 halfDir = normalize(viewDir + lightDir);

		half NdotV = max(dot(normal, viewDir), 0.0);
		half NdotL = max(dot(normal, lightDir), 0.0);
		half NdotH = max(dot(normal, halfDir), 0.0);

		half4 mra = tex2D(_MRATex, i.uv_MRATex);
		half metalness = mra.r * _Metalness;
		half roughness = mra.g * _Roughness;
		half ao = mra.b * _AO;

		#ifdef WEATHER_RAIN_ON
		metalness = lerp(metalness, 1, _Rain);
		roughness = lerp(roughness, 0.2, _Rain);
		ao = lerp(ao, 0.8, _Rain);
		#endif

		#ifdef WEATHER_SNOW_ON
		half difference = dot(normal, _SnowDirection.xyz) - lerp(1, -1, _SnowLevel);
        difference = saturate(difference / _SnowWetness);
		half4 snow = tex2D (_SnowTex, i.uv_SnowTex) * _SnowColor;
		albedo.rgb = (lerp(albedo.rgb, snow.rgb, difference * _SnowDepth * _Snow));

		metalness = lerp(metalness, 0, _Snow);
		roughness = lerp(roughness, 1, _Snow);
		#endif
		
		//Fresnel
		half3 metal = lerp(half3(0.04, 0.04, 0.04), albedo, metalness);
		half3 fresnel = fresnelSchlick(NdotV, metal);

		//Fresnel Color
		half fresnel2 = _FresnelBase + pow(1 - NdotV, _FresnelScale);
		fresnel2 = fresnel2 * _FresnelIntensity;
		half3 fresnelColor = albedo * fresnel2 * _FresnelColor;

		//Cuebmap
		half3 reflectDir = normalize(reflect(-viewDir, normal));
		half3 reflectColor = texCUBElod(_ReflectMapLDR, half4(reflectDir, roughness * 8)) * _ReflectLuminace;
		
		half shadow = SHADOW_ATTENUATION(i);

		//Diffuse
		half lambert = NdotL * 0.5 + 0.5;
		albedo = albedo * lambert;
		half3 diffuse = lerp(albedo.rgb, 0, metalness);
		half3 specular = lerp(0.01, albedo.rgb, metalness);

		//Specular
		half NDF = DistributionGGX(NdotH, roughness);
		half G = GeometrySmith(NdotV, NdotL, roughness);
		specular *= NDF * G * fresnel / max(NdotV * NdotL, 0.001) * 0.3;
		specular = lerp(specular, reflectColor, fresnel);
		
		half3 lightColor = lambert * shadow * ao * _LightColor0.rgb;
		#if LIGHTMAP_ON
		lightColor = lightColor * DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv_LightMap));
		#endif
		lightColor = lightColor + UNITY_LIGHTMODEL_AMBIENT.rgb + i.SH;
		
		half4 finalColor = 0;
		finalColor.rgb = diffuse + fresnelColor + specular;

		//整合
		finalColor.rgb = finalColor.rgb * lightColor;
		finalColor.a = 1;

		UNITY_APPLY_FOG(i.fogCoord, finalColor);

		return saturate(finalColor);
	}
	ENDCG

	SubShader
	{
		Tags
		{
			"Queue"="Geometry"
		}

		ZWrite On
		//Cull Off

		Pass
		{
			Tags {	"LightMode" = "ForwardBase" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase

			ENDCG

		}

		Pass
		{
			Blend One One

			Tags {	"LightMode" = "ForwardAdd" }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdadd

			ENDCG
		}
	}

	Fallback "Diffuse"
}