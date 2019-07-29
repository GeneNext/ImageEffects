using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraDepthControl : MonoBehaviour
{
    [SerializeField]
    private Camera m_camera;
    [SerializeField]
    private DepthTextureMode m_depthMode;

    public event System.Action OnCameraPreRender;

    void Awake()
    {
        CheckDepthMode();
    }

    void Reset()
    {
        if (m_camera == null)
        {
            m_camera = this.gameObject.GetComponent<Camera>();
        }
    }

    public void CheckDepthMode()
    {
        if (m_camera == null)
        {
            return;
        }

        if (m_camera.depthTextureMode == m_depthMode)
        {
            return;
        }

        m_camera.depthTextureMode = m_depthMode;
    }

    void Update()
    {
        CheckDepthMode();
    }

    void OnPreRender()
    {
        if (OnCameraPreRender != null)
        {
            OnCameraPreRender();
        }
    }
}
