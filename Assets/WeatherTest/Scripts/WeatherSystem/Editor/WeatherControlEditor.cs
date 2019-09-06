using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WeatherSystem
{
    [CustomEditor(typeof(WeatherControlData))]
    public class WeatherControlDataEditor : Editor
    {
        protected WeatherControlData m_script;
        protected GUILayoutOption[] m_options;

        protected virtual void OnEnable()
        {
            m_script = target as WeatherControlData;
            m_options = new GUILayoutOption[0];
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UpdateArea();
        }

        protected void UpdateArea()
        {
            if (m_script.Area == null)
            {
                m_script.Area = new WeatherArea();
            }
            WeatherArea area = m_script.Area;
            area.GizmosColor1 = EditorGUILayout.ColorField("Gizmos Color 1", area.GizmosColor1, m_options);
            area.GizmosColor2 = EditorGUILayout.ColorField("Gizmos Color 2", area.GizmosColor2, m_options);
            area.ShowGizmos = EditorGUILayout.Toggle("Show Gizmos", area.ShowGizmos, m_options);
            area.AreaType = (eAreaType)EditorGUILayout.EnumPopup("区域类型", area.AreaType, m_options);

            switch (area.AreaType)
            {
                case eAreaType.Sphere:
                    UpdateProperties_Sphere();
                    break;
                case eAreaType.Cylinder:
                    UpdateProperties_Cylinder();
                    break;
                case eAreaType.Cube:
                    UpdateProperties_Cube();
                    break;
            }
        }

        public iAreaRange GetAreaRange(eAreaType areaType)
        {
            WeatherArea area = m_script.Area;
            iAreaRange range = area.GetAreaRange(areaType);
            if (range == null)
            {
                switch (areaType)
                {
                    case eAreaType.Cube:
                        range = new CubeRange();
                        break;
                    case eAreaType.Cylinder:
                        range = new CylinderRange();
                        break;
                    case eAreaType.Sphere:
                        range = new SphereRange();
                        break;
                }
                area.SetAreaRange(areaType, range);
            }

            return range;
        }

        private void UpdateProperties_Cube()
        {
            CubeRange range = GetAreaRange(eAreaType.Cube) as CubeRange;
            range.AreaSize = EditorGUILayout.Vector3Field("区域尺寸", range.AreaSize, m_options);
            range.BlendDistance = EditorGUILayout.FloatField("混合距离", range.BlendDistance, m_options);
        }

        private void UpdateProperties_Cylinder()
        {
            CylinderRange range = GetAreaRange(eAreaType.Cylinder) as CylinderRange;
            range.AreaRadius = EditorGUILayout.FloatField("区域半径", range.AreaRadius, m_options);
            range.AreaHeight = EditorGUILayout.FloatField("区域高度", range.AreaHeight, m_options);
            range.BlendDistance = EditorGUILayout.FloatField("混合距离", range.BlendDistance, m_options);
            range.IgnoreHeight = EditorGUILayout.Toggle("忽略高度", range.IgnoreHeight, m_options);
            if (range.GizmosMesh == null)
            {
                string prefabPathName = "Assets/WeatherTest/Scripts/WeatherSystem/Cylinder.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPathName);
                MeshFilter filter = prefab == null ? null : prefab.GetComponent<MeshFilter>();
                range.GizmosMesh = filter == null ? null : filter.sharedMesh;
            }
        }

        private void UpdateProperties_Sphere()
        {
            SphereRange range = GetAreaRange(eAreaType.Sphere) as SphereRange;
            range.AreaRadius = EditorGUILayout.FloatField("区域半径", range.AreaRadius, m_options);
            range.BlendDistance = EditorGUILayout.FloatField("混合距离", range.BlendDistance, m_options);
        }
    }
}