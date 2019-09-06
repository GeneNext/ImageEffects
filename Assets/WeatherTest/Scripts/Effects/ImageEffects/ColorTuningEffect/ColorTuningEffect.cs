using UnityEngine;
using System;
using System.Collections;

public class ColorTuningEffect : BaseImageEffect
{
    public readonly static Color DefaultTintColor = Color.white;
    public readonly static Color DefaultAddColor = Color.black;
    public const float DefaultBrightness = 1;
    public const float DefaultContrast = 1;
    public const float DefaultSaturation = 1;

    [SerializeField]
    private BaseColorData m_baseColor = null;
    public BaseColorData BaseColor
    {
        get 
        {
            if (!m_baseColor.IsInit)
            {
                m_baseColor.Init(Mat);
            }
            return m_baseColor; 
        }
    }

    [SerializeField]
    private HSVData m_hsv = null;
    public HSVData HSV
    {
        get 
        {
            if (!m_hsv.IsInit)
            {
                m_hsv.Init(Mat);
            }
            return m_hsv; 
        }
    }

    [SerializeField]
    private ColorTemperatureData m_colorTemperature = null;
    public ColorTemperatureData ColorTemperature
    {
        get 
        {
            if (!m_colorTemperature.IsInit)
            {
                m_colorTemperature.Init(Mat);
            }
            return m_colorTemperature; 
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Serialize()
    {
        base.Serialize();

        if (m_shader == null)
        {
            m_shader = Shader.Find("Hidden/ColorTuningEffect");
        }
    }

    public override void Deserialize()
    {
        base.Deserialize();
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (m_shader == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        RenderTexture tmp1 = CreateTexture(source, true);
        RenderTexture tmp2 = CreateTexture(source);

        BaseColor.Render(tmp1, tmp2, 0);
        //HSV.Render(destination, source, 1);
        ColorTemperature.Render(tmp2, tmp1, 2);

        Graphics.Blit(tmp1, destination);

        RenderTexture.ReleaseTemporary(tmp1);
        RenderTexture.ReleaseTemporary(tmp2);
    }

    [Serializable]
    public class BaseData
    {
        [SerializeField]
        private bool m_enabled = false;
        public bool Enabled
        {
            get { return m_enabled; }
        }

        protected Material m_mat;

        public bool IsInit { get; private set; }

        public virtual void Init(Material mat)
        {
            if (IsInit)
            {
                return;
            }

            m_mat = mat;
            IsInit = true;
        }

        protected virtual void UpdateMaterial()
        {

        }

        public bool Render(RenderTexture source, RenderTexture destination, int pass)
        {
            if (Enabled)
            {
                UpdateMaterial();
                Graphics.Blit(source, destination, m_mat, pass);
            }
            else
            {
                Graphics.Blit(source, destination);
            }

            return Enabled;
        }
    }

    [Serializable]
    public class BaseColorData : BaseData
    {
        [SerializeField]
        private Color m_tintColor;
        public Color TintColor { get { return m_tintColor; } }

        [SerializeField]
        private Color m_addColor;
        public Color AddColor { get { return m_addColor; } }

        [SerializeField, Range(0, 2)]
        private float m_brightness;
        public float Brightness { get { return m_brightness; } }

        [SerializeField, Range(0, 2)]
        private float m_saturation;
        public float Saturation { get { return m_saturation; } }

        [SerializeField, Range(0, 2)]
        private float m_contrast;
        public float Contrast { get { return m_contrast; } }

        public void SetData(Color tintColor, Color addColor, float brightness, float contrast, float saturation)
        {
            m_tintColor = tintColor;
            m_addColor = addColor;
            m_brightness = brightness;
            m_contrast = contrast;
            m_saturation = saturation;
        }

        protected override void UpdateMaterial()
        {
            if (m_mat == null)
            {
                return;
            }

            m_mat.SetColor(ShaderPropertyID.TintColor, m_tintColor);
            m_mat.SetColor(ShaderPropertyID.AddColor, m_addColor);
            m_mat.SetFloat(ShaderPropertyID.Brightness, m_brightness);
            m_mat.SetFloat(ShaderPropertyID.Saturation, m_saturation);
            m_mat.SetFloat(ShaderPropertyID.Contrast, m_contrast);
        }
    }

    [Serializable]
    public class HSVData : BaseData
    {
        [SerializeField, Range(-0.5f, 0.5f)]
        private float m_hue;
        [SerializeField, Range(-1f, 1f)]
        private float m_saturation;
        [SerializeField, Range(-1f, 1f)]
        private float m_value;

        public void SetData(float hue, float saturation, float value)
        {
            m_hue = hue;
            m_saturation = saturation;
            m_value = value;
        }

        protected override void UpdateMaterial()
        {
            if (m_mat == null)
            {
                return;
            }

            m_mat.SetFloat(ShaderPropertyID.HueOffset, m_hue);
            m_mat.SetFloat(ShaderPropertyID.SaturationOffset, m_saturation);
            m_mat.SetFloat(ShaderPropertyID.ValueOffset, m_value);
        }
    }

    [Serializable]
    public class ColorTemperatureData : BaseData
    {
        [SerializeField, Range(1000, 40000)]
        private int m_kelvins;

        public void SetData(int kelvins)
        {
            m_kelvins = kelvins;
        }

        protected override void UpdateMaterial()
        {
            if (m_mat == null)
            {
                return;
            }

            m_mat.SetInt(ShaderPropertyID.Kelvins, m_kelvins);
        }
    }
}
