using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem
{
    [System.Serializable]
    public struct WeatherAmbientData
    {
        public LightShadows ShadowType;
        [Range(0f, 8f)]
        public float MainLightIntensity;
        public Color AmbientColor;
        public Color FogColor;
        public float FogStartDistance;
        public float FogEndDistance;
    }
    [System.Serializable]
    public struct WeatherThunderData
    {
        public bool Use;
        [Range(0f, 3f)]
        public float Intensity;
        [Range(0f, 1f)]
        public float Duration;
        [Range(0f, 1f)]
        public float FadeOutDuration;
        [Range(0f, 1f)]
        public float IntervalIntensity;
        [Range(0f, 1f)]
        public float IntervalDuration;
        [Range(0, 3)]
        public int ThunderCount;
    }

    [System.Serializable]
    public struct WeatherTimeData
    {
        public bool AutoTime;
        [Range(0f, 180f)]
        public float SunAngle;
        [Range(0.3f, 1f)]
        public float AtmosphereThicknessMin;
        [Range(1f, 2f)]
        public float AtmosphereThicknessMax;
        [Range(0f, 1f)]
        public float Time;
        [Range(0f, 0.1f)]
        public float TimeSpeed;
    }

    public class WeatherAmbient
    {
        public Coroutine ThunderCoroutine;

        private WeatherAmbientData m_currentAmbientData;
        private WeatherAmbientData m_targetAmbientData;
        private WeatherAmbientData m_ambientData;

        private WeatherTimeData m_currentTimeData;
        private WeatherTimeData m_targetTimeData;
        private WeatherTimeData m_timeData;

        private WeatherEffect m_weatherEffect;
        private Light m_light;

        public void InitData(WeatherAmbientData targetData, WeatherTimeData timeData, WeatherEffect weatherEffect, Light light)
        {
            m_currentAmbientData = m_ambientData;
            m_targetAmbientData = targetData;

            m_currentTimeData = m_timeData;
            m_targetTimeData = timeData;
            m_weatherEffect = weatherEffect;
            m_light = light;

            m_light.shadows = targetData.ShadowType;
        }

        private WeatherAmbientData GetAmbientData(float time)
        {
            WeatherAmbientData data = new WeatherAmbientData();
            data.MainLightIntensity = Mathf.Lerp(m_currentAmbientData.MainLightIntensity, m_targetAmbientData.MainLightIntensity, time);
            data.AmbientColor = Color.Lerp(m_currentAmbientData.AmbientColor, m_targetAmbientData.AmbientColor, time);
            data.FogColor = Color.Lerp(m_currentAmbientData.FogColor, m_targetAmbientData.FogColor, time);
            data.FogStartDistance = Mathf.Lerp(m_currentAmbientData.FogStartDistance, m_targetAmbientData.FogStartDistance, time);
            data.FogEndDistance = Mathf.Lerp(m_currentAmbientData.FogEndDistance, m_targetAmbientData.FogEndDistance, time);
            return data;
        }

        private void SetAmbientData(WeatherAmbientData data)
        {
            m_light.intensity = data.MainLightIntensity;
            RenderSettings.ambientLight = data.AmbientColor;
            RenderSettings.fogColor = data.FogColor;
            RenderSettings.fogStartDistance = data.FogStartDistance;
            RenderSettings.fogEndDistance = data.FogEndDistance;
        }

        private WeatherTimeData GetTimeData(float time)
        {
            WeatherTimeData timeData = new WeatherTimeData();
            timeData.SunAngle = Mathf.Lerp(m_currentTimeData.SunAngle, m_targetTimeData.SunAngle, time);
            timeData.AtmosphereThicknessMin = Mathf.Lerp(m_currentTimeData.AtmosphereThicknessMin, m_targetTimeData.AtmosphereThicknessMin, time);
            timeData.AtmosphereThicknessMax = Mathf.Lerp(m_currentTimeData.AtmosphereThicknessMax, m_targetTimeData.AtmosphereThicknessMax, time);
            timeData.Time = Mathf.Lerp(m_currentTimeData.Time, m_targetTimeData.Time, time);
            timeData.TimeSpeed = Mathf.Lerp(m_currentTimeData.TimeSpeed, m_targetTimeData.TimeSpeed, time);
            timeData.AutoTime = m_targetTimeData.AutoTime;
            return timeData;
        }

        private void SetTimeData(WeatherTimeData timeData)
        {
            Quaternion rotation = Quaternion.Euler(new Vector3(0, 0, timeData.SunAngle));
            m_light.transform.rotation = rotation * Quaternion.Euler(Vector3.up * (timeData.Time * 360f + 90f));

            float atmosphereThickness = Mathf.Abs(timeData.Time - 0.5f) * 4;
            atmosphereThickness = Mathf.Lerp(timeData.AtmosphereThicknessMin, timeData.AtmosphereThicknessMax, Mathf.Clamp01(atmosphereThickness));
            RenderSettings.skybox.SetFloat("_AtmosphereThickness", atmosphereThickness);
        }

        public void UpdateByTime(float time)
        {
            m_ambientData = GetAmbientData(time);
            SetAmbientData(m_ambientData);

            m_timeData = GetTimeData(time);
            SetTimeData(m_timeData);
        }

        public void UpdatePerFrame()
        {
            if (m_light == null)
            {
                return;
            }

            if (m_timeData.AutoTime)
            {
                m_timeData.Time = m_timeData.Time + Time.deltaTime * m_timeData.TimeSpeed;
                m_timeData.Time = m_timeData.Time % 1;
                SetTimeData(m_timeData);
            }
        }

        public IEnumerator PlayThunder_Coroutine(WeatherThunderData data)
        {
            for (int i = 0; i < data.ThunderCount; ++i)
            {
                UpdateThunderIntensity(data.Intensity);
                yield return new WaitForSeconds(data.Duration);
                UpdateThunderIntensity(data.IntervalIntensity);
                yield return new WaitForSeconds(data.IntervalDuration);
            }

            float time = 0;
            float speed = 1 / data.FadeOutDuration;
            while (time < 1)
            {
                float intensity = Mathf.Lerp(data.Intensity, 0, time);
                UpdateThunderIntensity(intensity);
                yield return null;
                time += speed * Time.deltaTime;
            }

            UpdateThunderIntensity(0);
            ThunderCoroutine = null;
        }

        private void UpdateThunderIntensity(float intensity)
        {
            m_light.intensity = m_ambientData.MainLightIntensity + intensity;
            m_weatherEffect.SetExposure(intensity);
        }
    }
}