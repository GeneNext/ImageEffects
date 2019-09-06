﻿using UnityEngine;
using System;
using System.Collections;

public class RadialBlurEffect : BaseImageEffect
{
    [SerializeField, Range(0, 1)]
    private float m_sampleDistance;
    public float SampleDistance { get { return m_sampleDistance; } }
    [SerializeField, Range(0, 5)]
    private float m_sampleStrength;
    public float SampleStrength { get { return m_sampleStrength; } }

    public override void Serialize()
    {
        base.Serialize();

        if (m_shader == null)
        {
            m_shader = Shader.Find("Hidden/RadialBlurEffect");
        }
    }

    public override void Deserialize()
    {
        base.Deserialize();

        m_shader = null;
    }

    public void SetSampleData(float distance, float strength)
    {
        m_sampleDistance = distance;
        m_sampleStrength = strength;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Mat == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        int scale = (int)(m_sampleDistance * m_sampleStrength * 1.3f) + 1;

        int rtW = source.width / scale;
        int rtH = source.height / scale;

        Mat.SetFloat(ShaderPropertyID.SampleDistance, m_sampleDistance);
        Mat.SetFloat(ShaderPropertyID.SampleStrength, m_sampleStrength);

        RenderTexture rtTempA = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default);
        RenderTexture rtTempB = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.Default);

        rtTempA.filterMode = FilterMode.Bilinear;
        Graphics.Blit(source, rtTempA);

        rtTempB.filterMode = FilterMode.Bilinear;
        Graphics.Blit(rtTempA, rtTempB, Mat, 0);

        Mat.SetTexture(ShaderPropertyID.BlurTex, rtTempB);
        Graphics.Blit(source, destination, Mat, 1);

        RenderTexture.ReleaseTemporary(rtTempA);
        RenderTexture.ReleaseTemporary(rtTempB);
    }
}