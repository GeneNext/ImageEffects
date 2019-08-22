using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneAreaAmbientData
{
    [System.Serializable]
    public class LightData
    {
        [SerializeField]
        private Light m_light;
        [SerializeField]
        private Color m_color;
        [SerializeField]
        private float m_intensity;
        [SerializeField]
        private Vector3 m_eulerAngles;

        private Color m_originColor;
        private float m_originIntensity;
        private Vector3 m_originEulerAngles;
        private float m_originRange;

        public bool EnableLight
        {
            get { return m_light != null; }
        }

        public void Init()
        {
            if (EnableLight)
            {
                m_originColor = m_light.color;
                m_originIntensity = m_light.intensity;
                m_originRange = m_light.range;
                m_originEulerAngles = m_light.transform.eulerAngles;
            }
        }

        public void Blend(float blendWeight)
        {
            if (EnableLight)
            {
                m_light.color = Color.Lerp(m_originColor, m_color, blendWeight);
                if (m_light.type == LightType.Point)
                {
                    m_light.range = Mathf.Lerp(m_originRange, m_intensity, blendWeight);
                }
                else
                {
                    m_light.intensity = Mathf.Lerp(m_originIntensity, m_intensity, blendWeight);
                }
                m_light.transform.rotation = Quaternion.Lerp(Quaternion.Euler(m_originEulerAngles), Quaternion.Euler(m_eulerAngles), blendWeight);
            }
        }

        public void SetDataToScene()
        {
            if (EnableLight)
            {
                m_light.color = m_color;
                if (m_light.type == LightType.Point)
                {
                    m_light.range = m_intensity;
                }
                else
                {
                    m_light.intensity = m_intensity;
                }
                m_light.transform.rotation = Quaternion.Euler(m_eulerAngles);
            }
        }

        public void GetDataFromScene()
        {
            if (EnableLight)
            {
                m_color = m_light.color;
                if (m_light.type == LightType.Point)
                {
                    m_intensity = m_light.range;
                }
                else
                {
                    m_intensity = m_light.intensity;
                }
                m_eulerAngles = m_light.transform.eulerAngles;
            }
        }
    }

    [System.Serializable]
    public class AmbientData
    {
        [SerializeField]
        private Color m_ambientColor;
        [SerializeField]
        private Color m_fogColor;
        [SerializeField]
        private float m_fogStartDistance;
        [SerializeField]
        private float m_fogEndDistance;
        [SerializeField]
        private float m_fogFactor;

        private Color m_originAmbientColor;
        private Color m_originFogColor;
        private float m_originFogStartDistance;
        private float m_originFogEndDistance;
        private float m_originFogFactor;

        public void Init()
        {
            m_originAmbientColor = RenderSettings.ambientLight;
            m_originFogColor = RenderSettings.fogColor;
            m_originFogStartDistance = RenderSettings.fogStartDistance;
            m_originFogEndDistance = RenderSettings.fogEndDistance;
            m_originFogFactor = RenderSettings.skybox.GetFloat(ShaderPropertyID.FogFactor);
        }

        public void Blend(float blendWeight)
        {
            RenderSettings.ambientLight = Color.Lerp(m_originAmbientColor, m_ambientColor, blendWeight);
            RenderSettings.fogColor = Color.Lerp(m_originFogColor, m_fogColor, blendWeight);
            RenderSettings.fogStartDistance = Mathf.Lerp(m_originFogStartDistance, m_fogStartDistance, blendWeight);
            RenderSettings.fogEndDistance = Mathf.Lerp(m_originFogEndDistance, m_fogEndDistance, blendWeight);
            RenderSettings.skybox.SetColor(ShaderPropertyID.FogColor, RenderSettings.fogColor);
            RenderSettings.skybox.SetFloat(ShaderPropertyID.FogFactor, Mathf.Lerp(m_originFogFactor, m_fogFactor, blendWeight));
        }

        public void SetDataToScene()
        {
            RenderSettings.ambientLight = m_ambientColor;
            RenderSettings.fogColor = m_fogColor;
            RenderSettings.fogStartDistance = m_fogStartDistance;
            RenderSettings.fogEndDistance = m_fogEndDistance;
            RenderSettings.skybox.SetColor(ShaderPropertyID.FogColor, RenderSettings.fogColor);
            RenderSettings.skybox.SetFloat(ShaderPropertyID.FogFactor, m_fogFactor);
        }

        public void GetDataFromScene()
        {
            m_ambientColor = RenderSettings.ambientLight;
            m_fogColor = RenderSettings.fogColor;
            m_fogStartDistance = RenderSettings.fogStartDistance;
            m_fogEndDistance = RenderSettings.fogEndDistance;
            m_fogFactor = RenderSettings.skybox.GetFloat(ShaderPropertyID.FogFactor);
        }
    }

    [System.Serializable]
    public class PostEffectsData
    {
        [SerializeField]
        private ColorTuningEffect m_colorTuningEffect;
        [SerializeField]
        private Color m_tintColor;
        [SerializeField]
        private Color m_addColor;
        [SerializeField, Range(0, 2)]
        private float m_brightness;
        [SerializeField, Range(0, 2)]
        private float m_saturation;
        [SerializeField, Range(0, 2)]
        private float m_contrast;

        private Color m_originTintColor;
        private Color m_originAddColor;
        private float m_originBrightness;
        private float m_originSaturation;
        private float m_originContrast;

        public bool EnableColorTuning
        {
            get
            {
                return m_colorTuningEffect != null;
            }
        }

        [SerializeField]
        private BloomEffect m_bloomEffect;
        [SerializeField]
        private float m_bloomSaturation;
        [SerializeField]
        private float m_bloomIntensity;

        private float m_originBloomSaturation;
        private float m_originBloomIntensity;

        public bool EnableBloom
        {
            get
            {
                return m_bloomEffect != null;
            }
        }

        public void Init()
        {
            if (EnableColorTuning)
            {
                m_originTintColor = m_colorTuningEffect.BaseColor.TintColor;
                m_originAddColor = m_colorTuningEffect.BaseColor.AddColor;
                m_originBrightness = m_colorTuningEffect.BaseColor.Brightness;
                m_originSaturation = m_colorTuningEffect.BaseColor.Saturation;
                m_originContrast = m_colorTuningEffect.BaseColor.Contrast;
            }

            if (EnableBloom)
            {
                m_originBloomSaturation = m_bloomEffect.BloomSaturation;
                m_originBloomIntensity = m_bloomEffect.BloomIntensity;
            }
        }

        public void Blend(float blendWeight)
        {
            if (EnableColorTuning)
            {
                Color tintColor = Color.Lerp(m_originTintColor, m_tintColor, blendWeight);
                Color addColor = Color.Lerp(m_originAddColor, m_addColor, blendWeight);
                float brightness = Mathf.Lerp(m_originBrightness, m_brightness, blendWeight);
                float saturation = Mathf.Lerp(m_originSaturation, m_saturation, blendWeight);
                float contrast = Mathf.Lerp(m_originContrast, m_contrast, blendWeight);
                m_colorTuningEffect.BaseColor.SetData(tintColor, addColor, brightness, contrast, saturation);
            }

            if (EnableBloom)
            {
                float bloomSaturation = Mathf.Lerp(m_originBloomSaturation, m_bloomSaturation, blendWeight);
                float bloomIntensity = Mathf.Lerp(m_originBloomIntensity, m_bloomIntensity, blendWeight);
                m_bloomEffect.SetData(bloomSaturation, bloomIntensity);
            }
        }

        public void SetDataToScene()
        {
            if (EnableColorTuning)
            {
                m_colorTuningEffect.BaseColor.SetData(m_tintColor, m_addColor, m_brightness, m_contrast, m_saturation);
            }

            if (EnableBloom)
            {
                m_bloomEffect.SetData(m_bloomSaturation, m_bloomIntensity);
            }
        }

        public void GetDataFromScene()
        {
            if (m_colorTuningEffect == null)
            {
                m_colorTuningEffect = Camera.main.GetComponent<ColorTuningEffect>();
            }

            if (EnableColorTuning)
            {
                m_tintColor = m_colorTuningEffect.BaseColor.TintColor;
                m_addColor = m_colorTuningEffect.BaseColor.AddColor;
                m_brightness = m_colorTuningEffect.BaseColor.Brightness;
                m_contrast = m_colorTuningEffect.BaseColor.Contrast;
                m_saturation = m_colorTuningEffect.BaseColor.Saturation;
            }

            if (m_bloomEffect == null)
            {
                m_bloomEffect = Camera.main.GetComponent<BloomEffect>();
            }

            if (EnableBloom)
            {
                m_bloomSaturation = m_bloomEffect.BloomSaturation;
                m_bloomIntensity = m_bloomEffect.BloomIntensity;
            }
        }
    }
}