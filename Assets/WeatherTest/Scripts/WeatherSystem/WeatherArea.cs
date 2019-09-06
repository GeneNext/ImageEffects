using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem
{
    [System.Serializable]
    public class WeatherArea
    {
        [HideInInspector]
        public Color GizmosColor1 = Color.green;
        [HideInInspector]
        public Color GizmosColor2 = Color.red;
        [HideInInspector]
        public bool ShowGizmos = true;
        [HideInInspector]
        public eAreaType AreaType = eAreaType.Global;

        [SerializeField, HideInInspector]
        protected GlobalRange m_globalRange = new GlobalRange();

        [SerializeField, HideInInspector]
        protected SphereRange m_sphereRange = new SphereRange();

        [SerializeField, HideInInspector]
        protected CylinderRange m_cylinderRange = new CylinderRange();

        [SerializeField, HideInInspector]
        protected CubeRange m_cubeRange = new CubeRange();

        public virtual void Serialize()
        {

        }

        public virtual void Deserialize()
        {

        }

        public iAreaRange GetAreaRange()
        {
            return GetAreaRange(AreaType);
        }

        public void SetAreaRange(eAreaType areaType, iAreaRange range)
        {
            switch (areaType)
            {
                case eAreaType.Global:
                    m_globalRange = range as GlobalRange;
                    break;
                case eAreaType.Cube:
                    m_cubeRange = range as CubeRange;
                    break;
                case eAreaType.Cylinder:
                    m_cylinderRange = range as CylinderRange;
                    break;
                case eAreaType.Sphere:
                    m_sphereRange = range as SphereRange;
                    break;
            }
        }

        public iAreaRange GetAreaRange(eAreaType areaType)
        {
            iAreaRange range = null;
            switch (areaType)
            {
                case eAreaType.Global:
                    range = m_globalRange;
                    break;
                case eAreaType.Cube:
                    range = m_cubeRange;
                    break;
                case eAreaType.Cylinder:
                    range = m_cylinderRange;
                    break;
                case eAreaType.Sphere:
                    range = m_sphereRange;
                    break;
            }
            return range;
        }

        public bool CheckInRange(Transform trans, Vector3 position)
        {
            return GetAreaRange().CheckInRange(trans, position);
        }

        public float GetBlendWeight(Transform trans, Vector3 position)
        {
            return GetAreaRange().GetBlendWeight(trans, position);
        }

        public virtual void DrawGizmos(Transform trans)
        {
            if (!ShowGizmos)
            {
                return;
            }

            GetAreaRange().DrawGizmos(trans, GizmosColor1, GizmosColor2);
        }
    }
}