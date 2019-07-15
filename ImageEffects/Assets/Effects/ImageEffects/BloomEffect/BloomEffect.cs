using UnityEngine;
using System.Collections;

public class BloomEffect : GaussianBlurEffect
{
    //采样阈值
    [SerializeField]
    private Color m_bloomThreshold = Color.white;
    //Bloom泛光颜色
    [SerializeField]
    private Color m_bloomColor = Color.white;
    //Bloom饱和度
    [SerializeField, Range(0.0f, 2.0f)]
    private float m_bloomSaturation = 1f;
    public float BloomSaturation { get { return m_bloomSaturation; } }
    //Bloom泛光强度
    [SerializeField, Range(0.0f, 10.0f)]
    private float m_bloomIntensity = 0;
    public float BloomIntensity { get { return m_bloomIntensity; } }

    public override void Serialize()
    {
        base.Serialize();

        if (m_shader == null)
        {
            m_shader = Shader.Find("Hidden/BloomEffect");
        }
    }

    public override void Deserialize()
    {
        base.Deserialize();

        m_shader = null;
    }

    public void SetData(float bloomSaturation, float bloomIntensity)
    {
        m_bloomSaturation = bloomSaturation;
        m_bloomIntensity = bloomIntensity;
    }

    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!Mat)
        {
            Graphics.Blit(source, destination);
            return;
        }

        int textureWidth = (int)(source.width * m_blurResolution + 0.5f);
        int textureHeight = (int)(source.height * m_blurResolution + 0.5f);

        RenderTexture blurTexture = RenderTexture.GetTemporary(textureWidth, textureHeight, 0, source.format);


        Mat.SetVector(ShaderPropertyID.BloomColor, m_bloomColor);
        Mat.SetFloat(ShaderPropertyID.BloomSaturation, m_bloomSaturation);
        Mat.SetFloat(ShaderPropertyID.BloomIntensity, m_bloomIntensity);
        Mat.SetVector(ShaderPropertyID.BloomThreshold, m_bloomThreshold);
        Graphics.Blit(source, blurTexture, Mat, 0);

        Blur(blurTexture, eBlurType.Normal);

        Mat.SetTexture(ShaderPropertyID.BlurTex, blurTexture);
        Graphics.Blit(source, destination, Mat, 1);

        RenderTexture.ReleaseTemporary(blurTexture);
    }
}