using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class BaseImageEffect : MonoBehaviour
{
    [SerializeField]
    protected Shader m_shader;

    protected Material m_mat;
    public Material Mat
    {
        get
        {
            if (m_mat == null)
            {
                m_mat = new Material(m_shader);
            }
            return m_mat;
        }
    }

    protected virtual void Awake()
    { 
    
    }

    protected virtual void Start()
    { 
        
    }

    protected virtual void OnEnable()
    {
        if (m_shader == null || !m_shader.isSupported)
        {
            this.enabled = false;
        }
    }

    protected virtual void OnDisable()
    {

    }

    protected virtual void OnDestroy()
    {
        if (m_mat != null)
        {
            DestroyImmediate(m_mat);
            m_mat = null;
        }
    }

    protected virtual void Reset()
    {
        Serialize();
    }

    protected virtual void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Serialize();
        }
#endif
    }

    public virtual void Serialize()
    {

    }

    public virtual void Deserialize()
    {

    }

    protected RenderTexture CreateTexture(RenderTexture source, bool copySource = false)
    {
        RenderTexture texture = RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format);
        if (copySource)
        {
            Graphics.Blit(source, texture);
        }
        return texture;
    }

    protected RenderTexture CreateTexture(RenderTexture source, RenderTextureFormat format, float decision, bool copySource = false)
    {
        decision = Mathf.Clamp01(decision);

        int width = (int)(source.width * decision + 0.5f);
        int height = (int)(source.height * decision + 0.5f);

        RenderTexture texture = CreateTexture(width, height, source.depth, format);
        if (copySource)
        {
            Graphics.Blit(source, texture);
        }
        return texture;
    }

    protected RenderTexture CreateTexture(int width, int height, int depth, RenderTextureFormat format)
    {
        RenderTexture texture = RenderTexture.GetTemporary(width, height, depth, format);
        return texture;
    }
}
