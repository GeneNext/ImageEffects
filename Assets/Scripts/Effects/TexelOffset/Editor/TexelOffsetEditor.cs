using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TexelOffset))]
public class TexelOffsetEditor : BaseEditor<TexelOffset>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("show font", m_options))
        {
            m_script.PlayShowFont();
        }

        if (GUILayout.Button("reset", m_options))
        {
            m_script.StopShowFont();
            m_script.PlayOffset();
        }
    }
}
