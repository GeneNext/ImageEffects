using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurgeControl : MonoBehaviour
{
    [SerializeField]
    private Renderer m_renderer;

    private Material m_mat;
    public Material Mat
    {
        get
        {
            if (m_mat == null)
            {
                m_mat = m_renderer.material;
            }
            return m_mat;
        }
    }

    private GameObject m_go;
    public GameObject GO
    {
        get 
        {
            if (m_go == null)
            {
                m_go = this.gameObject;
            }
            return m_go; 
        }
    }
    private Transform m_trans;
    public Transform Trans
    {
        get 
        {
            if (m_trans == null)
            {
                m_trans = this.transform;
            }
            return m_trans; 
        }
    }

    public void InitColor(Color color)
    {
        Mat.SetColor(ShaderPropertyID.Color, color);
    }

    public void UpdateData(float scale, float alpha)
    {
        bool active = scale > 0 && alpha > 0;
        if (GO.activeSelf != active)
        {
            GO.SetActive(active);
        }

        Trans.localScale = Vector3.one * scale;
        Mat.SetTextureScale(ShaderPropertyID.MainTex, Vector2.one * scale * 0.1f);
        Mat.SetFloat(ShaderPropertyID.Alpha, alpha);
        Mat.SetFloat(ShaderPropertyID.DepthRange, 8 * (1 - alpha));
    }
}
