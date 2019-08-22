using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using SceneAreaRange;

public class SceneAreaEditor<ScriptType> : BaseEditor<ScriptType> where ScriptType : SceneArea
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UpdateProperties();
    }

    protected void UpdateProperties()
    {
        m_script.AreaType = (eAreaType)EditorGUILayout.EnumPopup("区域类型", m_script.AreaType, m_options);

        switch (m_script.AreaType)
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

    private void UpdateProperties_Sphere()
    {
        m_script.SphereRange.AreaRadius = EditorGUILayout.FloatField("区域半径", m_script.SphereRange.AreaRadius, m_options);
        m_script.SphereRange.BlendDistance = EditorGUILayout.FloatField("混合距离", m_script.SphereRange.BlendDistance, m_options);
    }

    private void UpdateProperties_Cylinder()
    {
        m_script.CylinderRange.AreaRadius = EditorGUILayout.FloatField("区域半径", m_script.CylinderRange.AreaRadius, m_options);
        m_script.CylinderRange.AreaHeight = EditorGUILayout.FloatField("区域高度", m_script.CylinderRange.AreaHeight, m_options);
        m_script.CylinderRange.BlendDistance = EditorGUILayout.FloatField("混合距离", m_script.CylinderRange.BlendDistance, m_options);
        m_script.CylinderRange.IgnoreHeight = EditorGUILayout.Toggle("忽略高度", m_script.CylinderRange.IgnoreHeight, m_options);
        if (m_script.CylinderRange.GizmosMesh == null)
        {
            string prefabPathName = "Assets/Art/Scripts/SceneAreaControl/Cylinder.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPathName);
            MeshFilter filter = prefab == null ? null : prefab.GetComponent<MeshFilter>();
            m_script.CylinderRange.GizmosMesh = filter == null ? null : filter.sharedMesh;
        }
    }

    private void UpdateProperties_Cube()
    {
        m_script.CubeRange.AreaSize = EditorGUILayout.Vector3Field("区域尺寸", m_script.CubeRange.AreaSize, m_options);
        m_script.CubeRange.BlendDistance = EditorGUILayout.FloatField("混合距离", m_script.CubeRange.BlendDistance, m_options);
    }
}
