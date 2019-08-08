#ifndef SPECULLAR_LIB_INCLUDED
#define SPECULLAR_LIB_INCLUDED

float _DiffuseFactor;

float _RimFactor;
float4 _RimColor;
		
struct SpecularOutput 
{
    fixed3 Albedo;
    fixed3 Normal;
    fixed3 Emission;
    fixed Gloss;
    half Specular;
	fixed3 AmbientOcclusion;
    fixed Alpha;
};

inline float4 UnitySpecularLight(SpecularOutput s, half3 viewDir, UnityLight light)
{
    half3 h = normalize(normalize(light.dir) + normalize(viewDir));

    float diff = max(0, dot(s.Normal, light.dir));

	float3 diffColor = diff * _DiffuseFactor + (1 - _DiffuseFactor);
	diffColor = diffColor * s.AmbientOcclusion;

	float nh = max(0, dot(s.Normal, h)); 
	float3 specColor = saturate(pow(nh, s.Specular * 128.0) * s.Gloss);

	float4 outputColor;
	outputColor.rgb = s.Albedo * light.color * (diffColor + specColor);
    outputColor.a = s.Alpha;

    return saturate(outputColor);
}

inline float4 LightingSpecular(SpecularOutput s, half3 viewDir, UnityGI gi)
{
    float4 c;
    c = UnitySpecularLight(s, viewDir, gi.light);

    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif

    return c;
}

inline half4 LightingSpecular_Deferred(SpecularOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
{
    UnityStandardData data;
    data.diffuseColor = s.Albedo;
    data.occlusion = 1;
    // PI factor come from StandardBDRF(UnityStandardBRDF.cginc:351 for explanation)
    data.specularColor = s.Gloss *(1/UNITY_PI);
    data.smoothness = s.Specular;
    data.normalWorld = s.Normal;

    UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

    half4 emission = half4(s.Emission, 1);

    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        emission.rgb += s.Albedo * gi.indirect.diffuse;
    #endif

    return emission;
}

inline void LightingSpecular_GI(SpecularOutput s, UnityGIInput data, inout UnityGI gi)
{
    gi = UnityGlobalIllumination(data, 1.0, s.Normal);
}

inline float4 LightingSpecular_PrePass(SpecularOutput s, half4 light)
{
    float spec = light.a * s.Gloss;

    float4 c;
    c.rgb = s.Albedo * light.rgb + light.rgb * spec;
    c.a = s.Alpha;
    return c;
}

#endif