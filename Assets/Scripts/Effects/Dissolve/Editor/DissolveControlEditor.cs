using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DissolveControl))]
public class DissolveControlEditor : BaseEditor<DissolveControl>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("fade in", m_options))
        {
            m_script.FadeIn(() => { Debug.LogError("fade in complete !"); });
        }

        if (GUILayout.Button("fade out", m_options))
        {
            m_script.FadeOut(() => { Debug.LogError("fade out complete !"); });
        }
    }
}
