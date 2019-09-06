using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutoutEffect : BaseImageEffect
{
    [SerializeField]
    private Shader m_cutoutReplaceShader;

    [SerializeField]
    private Camera m_camera;

    [SerializeField]
    private CutoutData m_cutoutData = new CutoutData();

    [SerializeField]
    private CutoutData m_cutoutShapeData = new CutoutData();

    public override void Serialize()
    {
        base.Serialize();

        if (m_shader == null)
        {
            m_shader = Shader.Find("Hidden/CutoutEffect");
        }

        if (m_cutoutReplaceShader == null)
        {
            m_cutoutReplaceShader = Shader.Find("Hidden/CutoutEffectReplace");
        }
    }

    public override void Deserialize()
    {
        base.Deserialize();

        m_shader = null;
        m_cutoutReplaceShader = null;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        CheckCameras();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        m_cutoutData.ClearRenderTexture();
        m_cutoutShapeData.ClearRenderTexture();
    }

    private void CheckCameras()
    {
        if (m_camera == null)
        {
            m_camera = this.CheckComponent<Camera>();
        }

        if (m_cutoutData.NeedInit)
        {
            Transform trans = this.transform.CheckChild("CutoutCamera");
            m_cutoutData.Camera = trans.CheckComponent<Camera>();
            m_cutoutData.Init(m_camera);
        }

        if (m_cutoutShapeData.NeedInit)
        {
            Transform trans = this.transform.CheckChild("CutoutShapeData");
            m_cutoutShapeData.Camera = trans.CheckComponent<Camera>();
            m_cutoutShapeData.Init(m_camera);
        }
    }

    void OnPreRender()
    {
        CheckCameras();

        m_cutoutData.CheckSize();
        m_cutoutShapeData.CheckSize();

        m_cutoutData.Camera.Render();
        m_cutoutShapeData.Camera.RenderWithShader(m_cutoutReplaceShader, "RenderType");
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!Mat)
        {
            Graphics.Blit(source, destination);
            return;
        }

        RenderTexture tmp = CreateTexture(m_cutoutData.RenderTexture);
        Mat.SetTexture("_ShapeTex", m_cutoutShapeData.RenderTexture);
        Graphics.Blit(m_cutoutData.RenderTexture, tmp, Mat, 0);

        Mat.SetTexture("_ClipTex", tmp);
        Graphics.Blit(source, destination, Mat, 1);
        RenderTexture.ReleaseTemporary(tmp);
    }

    [System.Serializable]
    private class CutoutData
    {
        public Camera Camera = null;
        public LayerMask LayerMask = 0;
        public RenderTexture RenderTexture = null;

        private Camera m_targetCamera;

        public bool NeedInit
        {
            get { return Camera == null || RenderTexture == null || m_targetCamera == null; }
        }

        public void Init(Camera camera)
        {
            m_targetCamera = camera;

            Camera.CopyFrom(camera);
            Camera.cullingMask = LayerMask;
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = Color.black;
            Camera.depthTextureMode = DepthTextureMode.None;

            InitRenderTexture();
        }

        public void Clear()
        {
            ClearRenderTexture();

            Camera = null;
        }

        public void InitRenderTexture()
        {
            ClearRenderTexture();

            int textureWidth = Screen.width;
            int textureHeight = Screen.height;

            RenderTexture = RenderTexture.GetTemporary(textureWidth, textureHeight, 16, RenderTextureFormat.ARGB32);
            RenderTexture.wrapMode = TextureWrapMode.Clamp;
            RenderTexture.filterMode = FilterMode.Bilinear;

            Camera.targetTexture = RenderTexture;
        }

        public void ClearRenderTexture()
        {
            Camera.targetTexture = null;

            if (RenderTexture != null)
            {
                RenderTexture.ReleaseTemporary(RenderTexture);
                RenderTexture = null;
            }
        }

        public void CheckSize()
        {
            if (Camera.fieldOfView != m_targetCamera.fieldOfView)
            {
                Camera.fieldOfView = m_targetCamera.fieldOfView;
            }

            if (RenderTexture != null && Screen.width == RenderTexture.width && Screen.height == RenderTexture.height)
            {
                return;
            }

            InitRenderTexture();
        }
    }
}
