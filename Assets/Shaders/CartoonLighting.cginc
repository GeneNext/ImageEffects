#ifndef CARTOON_LIB_INCLUDED
#define CARTOON_LIB_INCLUDED

float _LightFactor;

float _RimFactor;
float4 _RimColor;
float _Steps;

inline float4 UnityCartoonLight(SurfaceOutput s, half3 viewDir, UnityLight light)
{
    half3 h = normalize(normalize(light.dir) + normalize(viewDir));

    float diff = max(0, dot(s.Normal, light.dir));
	diff = smoothstep(0, 1, diff);
	float toonDiff = floor(diff * _Steps) / _Steps;

	float rim = 1.0 - saturate(dot(s.Normal, normalize(viewDir)));
	rim = pow(rim, _RimFactor);
	float toonRim = toonDiff * floor(rim * _Steps) / _Steps;

    float nh = max(0, dot(s.Normal, h));
    float spec = pow(nh, s.Specular * 128.0) * s.Gloss;
	float toonSpec = toonDiff * floor(spec * _Steps) / _Steps;//¿Î…¢ªØ

    float4 c;
    c.rgb = s.Albedo * light.color * ( toonRim + toonSpec);
    c.a = s.Alpha;

    return saturate(c);
}

inline float4 LightingCartoon(SurfaceOutput s, half3 viewDir, UnityGI gi)
{
    float4 c;
    c = UnityCartoonLight(s, viewDir, gi.light);

    #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
        c.rgb += s.Albedo * gi.indirect.diffuse;
    #endif

    return c;
}

inline half4 LightingCartoon_Deferred(SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
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

inline void LightingCartoon_GI(SurfaceOutput s, UnityGIInput data, inout UnityGI gi)
{
    gi = UnityGlobalIllumination(data, 1.0, s.Normal);
}

inline float4 LightingCartoon_PrePass(SurfaceOutput s, half4 light)
{
    float spec = light.a * s.Gloss;

    float4 c;
    c.rgb = s.Albedo * light.rgb + light.rgb * spec;
    c.a = s.Alpha;
    return c;
}

#endif