#ifndef DIFFUSE_LIB_INCLUDED
#define DIFFUSE_LIB_INCLUDED

float _RimFactor;
float4 _RimColor;

struct DiffuseOutput
{
    fixed3 Albedo;
    fixed3 Normal;
    fixed3 Emission;
    half Specular;
    fixed Gloss;
	fixed3 AmbientOcclusion;
	fixed3 Alpha;
};

inline float4 UnityDiffuseLight(DiffuseOutput s, half3 viewDir, UnityLight light)
{
    half3 h = normalize(normalize(light.dir) + normalize(viewDir));

    float diff = max(0, dot(s.Normal, light.dir));
	//float3 diffColor = diff * s.AmbientOcclusion;

    float4 c;
    c.rgb = s.Albedo * light.color * diff;
    c.a = s.Alpha;

    return saturate(c);
}

inline float4 LightingDiffuse(DiffuseOutput s, half3 viewDir, UnityGI gi)
{
    float4 c;
    c = UnityDiffuseLight(s, viewDir, gi.light);

    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif

    return c;
}

inline half4 LightingDiffuse_Deferred(DiffuseOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
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

inline void LightingDiffuse_GI(DiffuseOutput s, UnityGIInput data, inout UnityGI gi)
{
    gi = UnityGlobalIllumination(data, 1.0, s.Normal);
}

inline float4 LightingDiffuse_PrePass(DiffuseOutput s, half4 light)
{
    float spec = light.a * s.Gloss;

    float4 c;
    c.rgb = s.Albedo * light.rgb + light.rgb * spec;
    c.a = s.Alpha;
    return c;
}

#endif