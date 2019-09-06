using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem
{
    public class WeatherControl : MonoBehaviour
    {
        [SerializeField, Range(0f, 10f)]
        private float m_duration = 1f;

        [SerializeField]
        private WeatherEffect m_weatherEffect = null;
        [SerializeField]
        private Light m_mainLight = null;

        [SerializeField]
        private Transform m_targetTrans = null;
        public Transform TargetTrans { get { return m_targetTrans; } }

        [SerializeField]
        private WeatherControlData[] m_datas = null;

        public string WeatherName { get; private set; }

        public WeatherParticle Particle { get; private set; }
        public WeatherGlobalProperty GlobalProperty { get; private set; }
        public WeatherSkybox Skybox { get; private set; }
        public WeatherAmbient Ambient { get; private set; }

        private Coroutine m_coroutine;

        private Dictionary<string, WeatherControlData> m_weatherControlDataDict = null;

        private void InitWeatherControlData()
        {
            if (m_datas == null)
            {
                return;
            }

            if (m_weatherControlDataDict == null)
            {
                m_weatherControlDataDict = new Dictionary<string, WeatherControlData>();
                for (int i = 0; i < m_datas.Length; ++i)
                {
                    WeatherControlData data = m_datas[i];
                    if (data == null)
                    {
                        continue;
                    }
                    m_weatherControlDataDict.Add(data.name, data);
                }
            }
        }

        public WeatherControlData GetWeatherControlData()
        {
            return GetWeatherControlData(WeatherName);
        }

        public WeatherControlData GetWeatherControlData(string name)
        {
            WeatherControlData controlData = null;
            return m_weatherControlDataDict.TryGetValue(name, out controlData) ? controlData : null;
        }

        private void Awake()
        {
            Particle = new WeatherParticle();
            GlobalProperty = new WeatherGlobalProperty();
            Skybox = new WeatherSkybox();
            Ambient = new WeatherAmbient();

            InitWeatherControlData();

            WeatherManager.SetWeatherControl(this);
        }

        private void OnDestroy()
        {
            WeatherManager.SetWeatherControl(null);
        }

        private void Update()
        {
            GetBlendWeight();
        }

        private void LateUpdate()
        {
            Particle.UpdatePerFrame();
            Ambient.UpdatePerFrame();
        }

        public void Stop()
        {
            if (m_coroutine != null)
            {
                StopCoroutine(m_coroutine);
                m_coroutine = null;
            }

            Particle.Stop();
        }

        public void PlayWeather(string weatherName, System.Action completeCallback = null)
        {
            Stop();

            WeatherName = weatherName;
            WeatherControlData data = GetWeatherControlData(weatherName);
            data.EnableBehaviours(true);

            Particle.InitData(data.ParticleData, this.transform);
            GlobalProperty.InitData(data.GlobalPropertyData);
            Skybox.InitData(data.SkyboxData);
            Ambient.InitData(data.AmbientData, data.TimeData, m_weatherEffect, m_mainLight);

            m_coroutine = StartCoroutine(PlayWeather_Coroutine(completeCallback));
        }

        private IEnumerator PlayWeather_Coroutine(System.Action completeCallback)
        {
            BeforePlayWeather();

            float timeSpeed = 1 / m_duration;
            float time = 0;
            while (time < 1)
            {
                UpdateWeatherData(time);
                yield return null;
                time += timeSpeed * Time.deltaTime;
            }

            AfterPlayWeather();

            m_coroutine = null;

            if (completeCallback != null)
            {
                completeCallback();
            }
        }

        public void BeforePlayWeather()
        {
            GlobalProperty.EnableKeywords();
            UpdateWeatherData(0);
            Particle.Play();
        }

        public void AfterPlayWeather()
        {
            GlobalProperty.DisableKeywords();
            UpdateWeatherData(1);

            foreach (var pair in m_weatherControlDataDict)
            {
                pair.Value.EnableBehaviours(pair.Key == WeatherName);
            }
        }

        public void UpdateWeatherData(float time)
        {
            GlobalProperty.UpdateByTime(time);
            Ambient.UpdateByTime(time);
            Skybox.UpdateByTime(time);
            Particle.UpdateByTime(time);
        }

        public void PlayThunder()
        {
            WeatherControlData data = GetWeatherControlData();
            if (!data.ThunderData.Use)
            {
                return;
            }

            if (Ambient.ThunderCoroutine != null)
            {
                StopCoroutine(Ambient.ThunderCoroutine);
                Ambient.ThunderCoroutine = null;
            }

            Ambient.ThunderCoroutine = StartCoroutine(Ambient.PlayThunder_Coroutine(data.ThunderData));
        }
        public float GetBlendWeight()
        {
            float blendWeight = 0;
            WeatherControlData globalData = null;
            foreach (var pair in m_weatherControlDataDict)
            {
                WeatherControlData data = pair.Value;
                if (data.Area.AreaType == eAreaType.Global)
                {
                    globalData = pair.Value;
                    continue;
                }
                blendWeight = data.GetBlendWeight(m_targetTrans.position);
            }

            return blendWeight;
        }
    }
}
