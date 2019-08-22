using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneAreaAmbient))]
public class SceneAreaAmbientEditor : SceneAreaEditor<SceneAreaAmbient>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("设置到场景", m_options))
        {
            if (EditorUtility.DisplayDialog("", "当前数据设置到场景，确认操作？", "确认", "取消"))
            {
                Undo.RecordObject(m_script, "save");
                m_script.SetDataToScene();
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            }
        }

        if (GUILayout.Button("从场景获取", m_options))
        {
            if (EditorUtility.DisplayDialog("", "获取当前场景数据，确认操作？", "确认", "取消"))
            {
                Undo.RecordObject(m_script, "load");
                m_script.GetDataFromScene();
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
            }
        }
    }
}
