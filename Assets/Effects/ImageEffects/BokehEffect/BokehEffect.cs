using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BokehEffect : GaussianBlurEffect
{
    [SerializeField]
    private bool m_blurDepthTex;
    [SerializeField]
    private Camera m_camera;
    [SerializeField]
    private Transform m_targetTrans;

    public override void Serialize()
    {
        base.Serialize();

        if (m_shader == null)
        {
            m_shader = Shader.Find("Hidden/BokehEffect");
        }
    }

    public override void Deserialize()
    {
        base.Deserialize();

        m_shader = null;
    }

    public void SetTarget(Transform targetTrans)
    {
        m_targetTrans = targetTrans;

        this.enabled = m_targetTrans != null;
    }

    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!Mat)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (m_camera == null || m_targetTrans == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        RenderTexture bokehTexture = CreateTexture(source);
        RenderTexture tmpTexture = CreateTexture(source, true);
        RenderTexture depthTexture = GetDepthTexture(source);
        if (m_blurDepthTex)
        {
            RenderTexture blurDepthTexture = CreateTexture(depthTexture);
            Graphics.Blit(depthTexture, blurDepthTexture);
            Blur(blurDepthTexture, eBlurType.Normal);
            BlurMat.SetTexture(ShaderPropertyID.DepthTex, blurDepthTexture);
            Blur(tmpTexture, bokehTexture, eBlurType.Depth);
            RenderTexture.ReleaseTemporary(blurDepthTexture);
        }
        else
        {
            BlurMat.SetTexture(ShaderPropertyID.DepthTex, depthTexture);
            Blur(tmpTexture, bokehTexture, eBlurType.Depth);
        }

        Mat.SetTexture(ShaderPropertyID.DepthTex, bokehTexture);
        Mat.SetTexture(ShaderPropertyID.BokehTex, bokehTexture);
        Graphics.Blit(source, destination, Mat, 1);

        RenderTexture.ReleaseTemporary(tmpTexture);
        RenderTexture.ReleaseTemporary(depthTexture);
        RenderTexture.ReleaseTemporary(bokehTexture);
    }

    private RenderTexture GetDepthTexture(RenderTexture source)
    {
        RenderTexture depthTexture = CreateTexture(source, RenderTextureFormat.R8, m_blurResolution);

        Vector3 viewportPoint = m_camera.WorldToViewportPoint(m_targetTrans.position);
        float focalDistance01 = (viewportPoint.z) / (m_camera.farClipPlane);
        Mat.SetFloat(ShaderPropertyID.FocalDistance, focalDistance01);

        Graphics.Blit(source, depthTexture, Mat, 0);

        return depthTexture;
    }
}
