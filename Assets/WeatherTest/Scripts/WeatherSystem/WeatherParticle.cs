using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem
{
    [System.Serializable]
    public class WeatherParticleData
    {
        public List<ParticleData> ParticleDataList = new List<ParticleData>();

        public Transform Target = null;
        public bool FollowTarget = false;
        public bool IgnoreHeight = false;

        public void Play()
        {
            if (ParticleDataList == null)
            {
                return;
            }

            for (int i = 0; i < ParticleDataList.Count; ++i)
            {
                ParticleDataList[i].Play();
            }
        }

        public void Stop()
        {
            if (ParticleDataList == null)
            {
                return;
            }

            for (int i = 0; i < ParticleDataList.Count; ++i)
            {
                ParticleDataList[i].Stop();
            }
        }

        public void UpdateEmissionRate(float percent)
        {
            for (int i = 0; i < ParticleDataList.Count; ++i)
            {
                ParticleDataList[i].UpdateEmissionRate(percent);
            }
        }

        [System.Serializable]
        public class ParticleData
        {
            [SerializeField]
            private ParticleSystem m_particle = null;
            [SerializeField]
            private int m_emissionRate = 0;
            [SerializeField]
            private float m_height = 0;

            public void UpdateEmissionRate(float percent)
            {
                if (m_particle == null)
                {
                    return;
                }

                percent = Mathf.Clamp01(percent);

                if (percent > 0)
                {
                    Play();
                }
                else
                {
                    Stop();
                }
                ParticleSystem.EmissionModule emission = m_particle.emission;
                emission.rateOverTime = m_emissionRate * percent;
            }

            public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
            {
                if (m_particle == null)
                {
                    return;
                }

                position.y = position.y + m_height;
                m_particle.transform.position = position;
                m_particle.transform.rotation = rotation;
            }

            public void Play()
            {
                if (m_particle == null)
                {
                    return;
                }

                if (!m_particle.isPlaying)
                {
                    m_particle.Play();
                }
            }

            public void Stop()
            {
                if (m_particle == null)
                {
                    return;
                }

                if (m_particle.isPlaying)
                {
                    m_particle.Stop();
                }
            }
        }
    }

    public class WeatherParticle
    {
        private WeatherParticleData m_data;

        private Transform m_trans;

        public void InitData(WeatherParticleData data, Transform trans)
        {
            m_data = data;
            m_trans = trans;
        }

        public void UpdatePerFrame()
        {
            if (m_data == null)
            {
                return;
            }

            if (m_data.FollowTarget && m_data.Target != null)
            {
                Vector3 position = m_data.Target.position;
                position.y = m_data.IgnoreHeight ? m_trans.position.y : position.y;

                Vector3 eulerAngles = m_data.Target.eulerAngles;
                eulerAngles.x = 0;
                eulerAngles.z = 0;
                Quaternion rotation = Quaternion.Euler(eulerAngles);

                for (int i = 0; i < m_data.ParticleDataList.Count; ++i)
                {
                    WeatherParticleData.ParticleData particleData = m_data.ParticleDataList[i];
                    if (particleData == null)
                    {
                        continue;
                    }
                    particleData.SetPositionAndRotation(position, rotation);
                }
            }
        }

        public void UpdateByTime(float time)
        {
            if (m_data == null)
            {
                return;
            }

            m_data.UpdateEmissionRate(time);
        }

        public void Play()
        {
            if (m_data == null)
            {
                return;
            }

            m_data.Play();
        }

        public void Stop()
        {
            if (m_data == null)
            {
                return;
            }

            m_data.Stop();
        }
    }
}