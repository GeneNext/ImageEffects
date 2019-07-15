using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderModel : MonoBehaviour
{
    [SerializeField]
    private Camera m_camera;

    [SerializeField]
    private int m_resolution = 100;

    [SerializeField, Range(1, 60)]
    private int m_frameRate = 20;

    [SerializeField]
    private Renderer m_targetRenderer;

    private RenderTexture m_renderTexture;

    void OnEnable()
    {
        CreateTexture();
    }

    void OnDisable()
    {
        ReleaseTexture();
    }

    private float m_timer = 0;

    void Update()
    {
        float updateInterval = 1f / m_frameRate;

        m_timer += Time.deltaTime;

        if (m_timer < updateInterval)
        {
            return;
        }

        m_timer -= updateInterval;

        CreateTexture();

        m_camera.Render();
    }

    private void CreateTexture()
    {
        float aspect = m_targetRenderer.transform.localScale.z / m_targetRenderer.transform.localScale.x;
        int width = m_resolution;
        int height = (int)(m_resolution * aspect + 0.5f);

        if (m_renderTexture != null && m_renderTexture.width == width && m_renderTexture.height == height)
        {
            return;
        }

        ReleaseTexture();

        m_renderTexture = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32);
        m_camera.targetTexture = m_renderTexture;

        Material[] mats = m_targetRenderer.materials;
        for (int i = 0; i < mats.Length; ++i)
        {
            mats[i].SetTexture("_MainTex", m_renderTexture);
        }
    }

    private void ReleaseTexture()
    {
        m_camera.targetTexture = null;

        if (m_renderTexture == null)
        {
            return;
        }

        RenderTexture.ReleaseTemporary(m_renderTexture);
        m_renderTexture = null;
    }
}
