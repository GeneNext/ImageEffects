using UnityEngine;

namespace WeatherSystem
{
    [System.Serializable]
    public struct WeatherGlobalPropertyData
    {
        [Range(0f, 1f)]
        public float RainIntensity;
        [Range(0f, 1f)]
        public float SnowIntensity;

        public WeatherGlobalPropertyData GetDataByBlendWeight(float blendWeight)
        {
            WeatherGlobalPropertyData data = new WeatherGlobalPropertyData();
            data.RainIntensity = RainIntensity * blendWeight;
            data.SnowIntensity = SnowIntensity * blendWeight;
            return data;
        }

        public static WeatherGlobalPropertyData operator +(WeatherGlobalPropertyData data1, WeatherGlobalPropertyData data2)
        {
            WeatherGlobalPropertyData data = new WeatherGlobalPropertyData();
            data.RainIntensity = data1.RainIntensity + data2.RainIntensity;
            data.SnowIntensity = data1.SnowIntensity + data2.SnowIntensity;
            return data;
        }
    }

    [System.Serializable]
    public class WeatherGlobalProperty
    {
        public bool RainEnabled { get { return m_data.RainIntensity > 0; } }
        public bool SnowEnabled { get { return m_data.SnowIntensity > 0; } }

        private ShaderPropertyFloat m_rain;
        public ShaderPropertyFloat Rain
        {
            get
            {
                if (m_rain == null)
                {
                    m_rain = new ShaderPropertyFloat("_Rain");
                }
                return m_rain;
            }
        }

        private ShaderPropertyFloat m_snow;
        public ShaderPropertyFloat Snow
        {
            get
            {
                if (m_snow == null)
                {
                    m_snow = new ShaderPropertyFloat("_Snow");
                }
                return m_snow;
            }
        }

        private WeatherGlobalPropertyData m_data;

        public void InitData(WeatherGlobalPropertyData data)
        {
            m_data = data;
            Rain.Init(m_data.RainIntensity);
            Snow.Init(m_data.SnowIntensity);
        }

        public void UpdateByTime(float time)
        {
            Rain.Update(time);
            Snow.Update(time);
        }

        public void SetData(WeatherGlobalPropertyData data)
        {
            Rain.SetFloat(data.RainIntensity);
            Snow.SetFloat(data.SnowIntensity);
        }

        public void EnableKeywords()
        {
            if (RainEnabled)
            {
                //Debug.LogError("enable : rain");
                Shader.EnableKeyword("WEATHER_RAIN_ON");
            }
            if (SnowEnabled)
            {
                //Debug.LogError("enable : snow");
                Shader.EnableKeyword("WEATHER_SNOW_ON");
            }
        }

        public void DisableKeywords()
        {
            if (!RainEnabled)
            {
                //Debug.LogError("disable : rain");
                Shader.DisableKeyword("WEATHER_RAIN_ON");
            }
            if (!SnowEnabled)
            {
                //Debug.LogError("disable : snow");
                Shader.DisableKeyword("WEATHER_SNOW_ON");
            }
        }
    }
}