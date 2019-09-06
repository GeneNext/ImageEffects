using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem
{
    public class WeatherControlData : MonoBehaviour
    {
        public WeatherParticleData ParticleData = null;

        public WeatherGlobalPropertyData GlobalPropertyData;

        public WeatherSkyboxData SkyboxData;

        public WeatherAmbientData AmbientData;

        public WeatherThunderData ThunderData;

        public WeatherTimeData TimeData;

        [HideInInspector]
        public WeatherArea Area = null;

        public MonoBehaviour[] Behaviours = new MonoBehaviour[0];

        public bool CheckInRange(Vector3 position)
        {
            return Area.CheckInRange(this.transform, position);
        }

        public float GetBlendWeight(Vector3 position)
        {
            return Area.GetBlendWeight(this.transform, position);
        }

        public void EnableBehaviours(bool enable)
        {
            if (Behaviours == null)
            {
                return;
            }

            for (int i = 0; i < Behaviours.Length; ++i)
            {
                if (Behaviours[i] == null)
                {
                    continue;
                }

                Behaviours[i].enabled = enable;
            }
        }

        private void OnDrawGizmos()
        {
            Area.DrawGizmos(this.transform);
        }
    }
}