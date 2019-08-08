using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CorruptionControl))]
public class CorruptionControlEditor : Editor
{
    private CorruptionControl m_script;
    private GUILayoutOption[] m_options;

    void OnEnable()
    {
        m_script = target as CorruptionControl;
        m_options = new GUILayoutOption[0];
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("播放", m_options))
        {
            m_script.Play(null);
        }

        if (GUILayout.Button("关闭", m_options))
        {
            m_script.Close();
        }

        if (GUILayout.Button("重置", m_options))
        {
            m_script.ResetData();
        }

        if (GUILayout.Button("序列化子节点渲染器"))
        {
            m_script.SerializeRenderersInChildren();
        }
    }
}
