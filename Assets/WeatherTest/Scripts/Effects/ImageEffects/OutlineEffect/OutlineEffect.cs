using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineEffect : GaussianBlurEffect
{
    [SerializeField]
    private Shader m_outlineReplaceShader = null;

    [SerializeField]
    private Color m_outlineColor = Color.black;

    [SerializeField]
    private float m_outlineFactor = 0;

    [SerializeField]
    private Camera m_camera = null;

    [SerializeField]
    private OutlineData m_outlineData = new OutlineData();

    [SerializeField]
    private OutlineData m_outlineClipData = new OutlineData();

    protected override void Awake()
    {
        base.Awake();
        OutlineEffectMgr.Instance.SetOutlineEffect(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OutlineEffectMgr.Instance.SetOutlineEffect(null);
    }

    public override void Serialize()
    {
        base.Serialize();

        if (m_shader == null)
        {
            m_shader = Shader.Find("Hidden/OutlineEffect");
        }

        if (m_outlineReplaceShader == null)
        {
            m_outlineReplaceShader = Shader.Find("Hidden/OutlineEffectReplace");
        }
    }

    public override void Deserialize()
    {
        base.Deserialize();

        m_shader = null;
        m_outlineReplaceShader = null;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        CheckCameras();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        m_outlineData.ClearRenderTexture();
        m_outlineClipData.ClearRenderTexture();
    }

    private void CheckCameras()
    {
        if (m_camera == null)
        {
            m_camera = this.CheckComponent<Camera>();
        }

        if (m_outlineData.NeedInit)
        {
            Transform trans = this.transform.CheckChild("OutlineCamera");
            m_outlineData.Camera = trans.CheckComponent<Camera>();
            m_outlineData.Init(m_camera);
        }

        if (m_outlineClipData.NeedInit)
        {
            Transform trans = this.transform.CheckChild("OutlineClipData");
            m_outlineClipData.Camera = trans.CheckComponent<Camera>();
            m_outlineClipData.Init(m_camera);
        }
    }

    void OnPreRender()
    {
        CheckCameras();

        m_outlineData.CheckSize();
        m_outlineClipData.CheckSize();

        m_outlineData.Camera.RenderWithShader(m_outlineReplaceShader, "RenderType");
        m_outlineClipData.Camera.RenderWithShader(m_outlineReplaceShader, "RenderType");
    }

    protected override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!Mat)
        {
            Graphics.Blit(source, destination);
            return;
        }

        int width = (int)(source.width * m_blurResolution + 0.5f);
        int height = (int)(source.height * m_blurResolution + 0.5f);
        RenderTexture blurTexture = RenderTexture.GetTemporary(width, height, 0, source.format);
        Graphics.Blit(m_outlineData.RenderTexture, blurTexture);
        //模糊
        Blur(blurTexture, eBlurType.Normal);

        Mat.SetTexture(ShaderPropertyID.BlurTex, blurTexture);
        Graphics.Blit(m_outlineClipData.RenderTexture, m_outlineData.RenderTexture, Mat, 0);

        Mat.SetTexture(ShaderPropertyID.OutlineTex, m_outlineData.RenderTexture);
        Mat.SetFloat(ShaderPropertyID.OutlineFactor, m_outlineFactor);
        Graphics.Blit(source, destination, Mat, 1);

        RenderTexture.ReleaseTemporary(blurTexture);
    }

    [System.Serializable]
    private class OutlineData
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
