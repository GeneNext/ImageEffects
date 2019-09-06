using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ShaderPropertyID
{
    public static int TimeControl;

    public static int TwinkleFactor;

    public static int StartPos;

    public static int OutlineTex;
    public static int OutlineColor;
    public static int OutlineRange;
    public static int OutlineFactor;

    public static int CutoffRange;

    public static int FalloffRange;

    public static int Color;
    public static int MainTex;
    public static int Alpha;
    public static int EmissionColor;

    public static int DepthTex;
    public static int DepthFactor;
    public static int DepthRange;

    public static int FogColor;
    public static int FogFactor;

    public static int BloomThreshold;
    public static int BloomColor;
    public static int BloomSaturation;
    public static int BloomIntensity;

    public static int BlurTex;
    public static int BlurOffset;

    public static int BokehTex;

    public static int FocalDistance;

    public static int TintColor;
    public static int AddColor;
    public static int Brightness;
    public static int Saturation;
    public static int Contrast;

    public static int HueOffset;
    public static int SaturationOffset;
    public static int ValueOffset;

    public static int Kelvins;

    public static int SampleDistance;
    public static int SampleStrength;

    static ShaderPropertyID()
    {
        TimeControl = Shader.PropertyToID("_TimeControl");

        TwinkleFactor = Shader.PropertyToID("_TwinkleFactor");

        StartPos = Shader.PropertyToID("_StartPos");

        OutlineTex = Shader.PropertyToID("_OutlineTex");
        OutlineColor = Shader.PropertyToID("_OutlineColor");
        OutlineRange = Shader.PropertyToID("_OutlineRange");
        OutlineFactor = Shader.PropertyToID("_OutlineFactor");

        CutoffRange = Shader.PropertyToID("_CutoffRange");

        FalloffRange = Shader.PropertyToID("_FalloffRange");

        Color = Shader.PropertyToID("_Color");
        MainTex = Shader.PropertyToID("_MainTex");
        Alpha = Shader.PropertyToID("_Alpha");
        EmissionColor = Shader.PropertyToID("_EmissionColor");

        DepthTex = Shader.PropertyToID("_DepthTex");
        DepthFactor = Shader.PropertyToID("_DepthFactor");
        DepthRange = Shader.PropertyToID("_DepthRange");

        FogColor = Shader.PropertyToID("_FogColor");
        FogFactor = Shader.PropertyToID("_FogFactor");

        BloomThreshold = Shader.PropertyToID("_BloomThreshold");
        BloomColor = Shader.PropertyToID("_BloomColor");
        BloomSaturation = Shader.PropertyToID("_BloomSaturation");
        BloomIntensity = Shader.PropertyToID("_BloomIntensity");

        BlurTex = Shader.PropertyToID("_BlurTex");
        BlurOffset = Shader.PropertyToID("_BlurOffset");

        BokehTex = Shader.PropertyToID("_BokehTex");

        FocalDistance = Shader.PropertyToID("_FocalDistance");

        TintColor = Shader.PropertyToID("_TintColor");
        AddColor = Shader.PropertyToID("_AddColor");
        Brightness = Shader.PropertyToID("_Brightness");
        Saturation = Shader.PropertyToID("_Saturation");
        Contrast = Shader.PropertyToID("_Contrast");

        HueOffset = Shader.PropertyToID("_HueOffset");
        SaturationOffset = Shader.PropertyToID("_SaturationOffset");
        ValueOffset = Shader.PropertyToID("_ValueOffset");

        Kelvins = Shader.PropertyToID("_Kelvins");

        SampleDistance = Shader.PropertyToID("_SampleDistance");
        SampleStrength = Shader.PropertyToID("_SampleStrength");
    }
}
