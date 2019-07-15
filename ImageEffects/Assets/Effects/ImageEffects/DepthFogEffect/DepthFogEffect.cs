using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthFogEffect : GaussianBlurEffect
{
    [SerializeField]
    private bool m_blurDepthTex;
    [SerializeField]
    private Color m_fogColor;
    [SerializeField, Range(0, 1)]
    private float m_fogFactor;

    public override void Serialize()
    {
        base.Serialize();

        if (m_shader == null)
        {
            m_shader = Shader.Find("Hidden/DepthFogEffect");
        }
    }

    public override void Deserialize()
    {
        base.Deserialize();

        m_shader = null;
    }

    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!Mat)
        {
            Graphics.Blit(source, destination);
            return;
        }

        RenderTexture depthTexture = GetDepthTexture(source.width, source.height, source.format);
        Mat.SetTexture("_DepthTex", depthTexture);
        Mat.SetColor("_FogColor", m_fogColor);
        Mat.SetFloat("_FogFactor", m_fogFactor);
        Graphics.Blit(source, destination, Mat, 1);

        RenderTexture.ReleaseTemporary(depthTexture);
    }

    private RenderTexture GetDepthTexture(float width, float height, RenderTextureFormat format)
    {
        int textureWidth = (int)(width * m_blurResolution + 0.5f);
        int textureHeight = (int)(height * m_blurResolution + 0.5f);

        RenderTexture depthTexture = RenderTexture.GetTemporary(textureWidth, textureHeight, 0, RenderTextureFormat.RFloat);
        Graphics.Blit(null, depthTexture, Mat, 0);

        if (m_blurDepthTex)
        {
            Blur(depthTexture, eBlurType.Depth);
        }

        return depthTexture;
    }
}
