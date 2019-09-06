using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherEffect : BaseImageEffect
{
    [SerializeField]
    private RainData m_rainData = null;
    [SerializeField]
    private SnowData m_snowData = null;
    [SerializeField]
    private ExposureData m_exposureData = null;

    public override void Serialize()
    {
        base.Serialize();
        if (m_shader == null)
        {
            m_shader = Shader.Find("Hidden/RainDropEffect");
        }
    }
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (Mat == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        m_rainData.UpdateData(Mat);
        m_snowData.UpdateData(Mat);
        m_exposureData.UpdateData(Mat);

        Graphics.Blit(source, destination, Mat, 0);
    }

    public void SetExposure(float exposure)
    {
        m_exposureData.SetExposure(exposure);
    }

    [System.Serializable]
    public class RainData
    {
        [SerializeField]
        private Texture m_normalTex = null;

        [SerializeField, Range(0f, 1f)]
        private float m_intensity = 0;

        [SerializeField]
        private Vector2 m_offset1 = Vector2.zero;

        [SerializeField]
        private Vector2 m_scale1 = Vector2.one;

        [SerializeField, Range(0f, 10f)]
        private float m_speed1 = 0;

        [SerializeField]
        private Vector2 m_offset2 = Vector2.zero;

        [SerializeField]
        private Vector2 m_scale2 = Vector2.one;

        [SerializeField, Range(0f, 10f)]
        private float m_speed2 = 0;

        public void UpdateData(Material mat)
        {
            m_normalTex.wrapMode = TextureWrapMode.Repeat;

            mat.SetTexture("_RainNormalTex1", m_normalTex);
            mat.SetTextureOffset("_RainNormalTex1", m_offset1);
            mat.SetTextureScale("_RainNormalTex1", m_scale1);
            mat.SetFloat("_RainSpeed1", m_speed1);

            mat.SetTexture("_RainNormalTex2", m_normalTex);
            mat.SetTextureOffset("_RainNormalTex2", m_offset2);
            mat.SetTextureScale("_RainNormalTex2", m_scale2);
            mat.SetFloat("_RainSpeed2", m_speed2);

            mat.SetFloat("_RainIntensity", m_intensity);
        }
    }

    [System.Serializable]
    public class SnowData
    {
        [SerializeField]
        private Texture2D m_mainTex = null;
        [SerializeField]
        private Texture2D m_normalTex = null;
        [SerializeField, Range(0f, 2f)]
        private float m_cutout = 0f;
        [SerializeField, Range(0f, 2f)]
        private float m_intensity = 1f;

        public void UpdateData(Material mat)
        {
            mat.SetTexture("_SnowTex", m_mainTex);
            mat.SetTexture("_SnowNormalTex", m_normalTex);
            mat.SetFloat("_SnowCutout", m_cutout);
            mat.SetFloat("_SnowIntensity", m_intensity);
        }
    }

    [System.Serializable]
    private class ExposureData
    {
        [SerializeField]
        private float m_exposure = 1;

        private float m_addExposure = 0;

        public void SetExposure(float exposure)
        {
            m_addExposure = exposure;
        }

        public void UpdateData(Material mat)
        {
            mat.SetFloat("_Exposure", m_exposure + m_addExposure);
        }
    }
}
