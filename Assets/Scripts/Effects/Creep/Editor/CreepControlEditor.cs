using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CreepControl))]
public class CreepControlEditor : BaseEditor<CreepControl>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //if (GUILayout.Button("InitCreeps", m_options))
        //{
        //    m_script.Creeps = m_script.GetComponentsInChildren<Creep>();
        //}
    }
}
