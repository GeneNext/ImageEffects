#ifndef CHARACTER_LIB_INCLUDED
#define CHARACTER_LIB_INCLUDED

float _DiffuseFactor;

float _RimFactor;
float4 _RimColor;

samplerCUBE _ReflectTex;
float _ReflectFactor;

struct CharacterOutput
{
	fixed3 Albedo;
	fixed3 Normal;
	fixed3 Emission;
	half Specular;
	fixed Gloss;
	fixed3 AmbientOcclusion;
	fixed4 MaterialArea;
	fixed MaterialStrength;
	fixed Alpha;
};

inline float4 UnityCharacterLight(CharacterOutput s, half3 viewDir, UnityLight light)
{
	half3 h = normalize(normalize(light.dir) + normalize(viewDir));

	float diff = saturate(dot(s.Normal, light.dir));

	float3 diffColor = diff * s.AmbientOcclusion;
	diffColor = diffColor * _DiffuseFactor + (1 - _DiffuseFactor);

	float rim = 1.0 - saturate(dot(s.Normal, normalize(viewDir)));
	rim = pow(rim, _RimFactor);
	float3 rimColor = diffColor * _RimColor * rim;

	float3 specColor = 0;

	float NdotH = saturate(dot(normalize(s.Normal), h));
	specColor += saturate(pow(NdotH, s.Specular * 128.0) * s.Gloss) * diffColor * s.MaterialArea.r;

	float aniso = saturate(sin(radians(NdotH * 180)));
	specColor += saturate(pow(aniso, s.Specular * 128) * s.Gloss) * diffColor * s.MaterialArea.g;

	float3 reflectDir = reflect(-viewDir, s.Normal);
	float3 reflectColor = texCUBE(_ReflectTex, reflectDir) * _ReflectFactor * s.MaterialStrength * (diffColor + specColor);

	float4 outputColor;
	outputColor.rgb = s.Albedo * light.color * (diffColor + rimColor + reflectColor + specColor);
	outputColor.a = s.Alpha;

	return saturate(outputColor);
}

inline float4 LightingCharacter(CharacterOutput s, half3 viewDir, UnityGI gi)
{
	float4 c;
	c = UnityCharacterLight(s, viewDir, gi.light);

#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
	c.rgb += s.Albedo * gi.indirect.diffuse;
#endif

	return c;
}

inline half4 LightingCharacter_Deferred(CharacterOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
{
	UnityStandardData data;
	data.diffuseColor = s.Albedo;
	data.occlusion = 1;
	// PI factor come from StandardBDRF(UnityStandardBRDF.cginc:351 for explanation)
	data.specularColor = s.Gloss *(1 / UNITY_PI);
	data.smoothness = s.Specular;
	data.normalWorld = s.Normal;

	UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

	half4 emission = half4(s.Emission, 1);

#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
	emission.rgb += s.Albedo * gi.indirect.diffuse;
#endif

	return emission;
}

inline void LightingCharacter_GI(CharacterOutput s, UnityGIInput data, inout UnityGI gi)
{
	gi = UnityGlobalIllumination(data, 1.0, s.Normal);
}

inline float4 LightingCharacter_PrePass(CharacterOutput s, half4 light)
{
	float spec = light.a * s.Gloss;

	float4 c;
	c.rgb = s.Albedo * light.rgb + light.rgb * spec;
	c.a = s.Alpha;
	return c;
}

#endif