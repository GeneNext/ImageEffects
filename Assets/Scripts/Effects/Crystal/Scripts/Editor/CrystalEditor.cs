using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Crystal))]
public class CrystalEditor : BaseEditor<Crystal>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("play", m_options))
        {
            m_script.Play();
        }

        if (GUILayout.Button("stop", m_options))
        {
            m_script.Stop();
        }

        if (GUILayout.Button("reset", m_options))
        {
            m_script.ShowCrystal(true, false);
        }

        if (GUILayout.Button("serialize", m_options))
        {
            m_script.SeralizeRenderers();
        }
    }
}
