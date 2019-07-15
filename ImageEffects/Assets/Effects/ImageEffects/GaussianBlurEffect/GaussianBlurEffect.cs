using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaussianBlurEffect : BaseImageEffect
{
    [SerializeField]
    private Shader m_blurShader;
    public Shader BlurShader
    {
        get
        {
            if (m_blurShader == null)
            {
                m_blurShader = Shader.Find("Hidden/GaussianBlurEffect");
            }
            return m_blurShader;
        }
    }

    private Material m_blurMat;
    protected Material BlurMat
    {
        get
        {
            if (m_blurMat == null)
            {
                m_blurMat = new Material(BlurShader);
            }
            return m_blurMat;
        }
    }

    //分辨率
    [SerializeField, Range(0.1f, 1f)]
    protected float m_blurResolution = 0.5f;
    [SerializeField, Range(0f, 10f)]
    protected float m_blurRange = 2;
    [SerializeField, Range(1, 5)]
    protected int m_blurIteration = 3;

    public override void Serialize()
    {
        base.Serialize();

        if (m_blurShader == null)
        {
            m_blurShader = Shader.Find("Hidden/GaussianBlurEffect");
        }
    }

    public override void Deserialize()
    {
        base.Deserialize();

        m_blurShader = null;
    }

    protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (BlurMat == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        int width = (int)(source.width * m_blurResolution + 0.5f);
        int height = (int)(source.height * m_blurResolution + 0.5f);
        //降低分辨率
        RenderTexture blurTexture = RenderTexture.GetTemporary(width, height, 0, source.format);
        Graphics.Blit(source, blurTexture);
        //模糊处理
        Blur(blurTexture, eBlurType.Normal);
        //矫正倒像
        Graphics.Blit(blurTexture, destination, BlurMat, 2);
        RenderTexture.ReleaseTemporary(blurTexture);
    }

    protected void Blur(RenderTexture source, eBlurType blurType)
    {
        RenderTexture tmpBlurTexture = RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format);

        Blur(source, tmpBlurTexture, blurType);

        RenderTexture.ReleaseTemporary(tmpBlurTexture);
    }

    protected void Blur(RenderTexture source, RenderTexture destination, eBlurType blurType)
    {
        float widthOverHeight = source.width / (1.0f * source.height);
        float oneOverBaseSize = 1.0f / 512.0f;

        int pass = (int)blurType;

        for (int i = 0; i < m_blurIteration; ++i)
        {
            float offsetScale = (1.0f + (i * 0.25f)) * m_blurRange;
            BlurMat.SetVector(ShaderPropertyID.BlurOffset, new Vector4(0, offsetScale * oneOverBaseSize, 0, 0));
            Graphics.Blit(source, destination, BlurMat, pass);
            BlurMat.SetVector(ShaderPropertyID.BlurOffset, new Vector4((offsetScale / widthOverHeight) * oneOverBaseSize, 0, 0, 0));
            Graphics.Blit(destination, source, BlurMat, pass);
        }

        Graphics.Blit(source, destination, BlurMat, pass);
    }

    protected enum eBlurType
    {
        Normal = 0,
        Depth = 1,
    }
}
