using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Creep))]
public class CreepEditor : BaseEditor<Creep>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("InitBones", m_options))
        {
            if (m_script.BoneRoot == null)
            {
                return;
            }

            Transform[] bones = new Transform[m_script.BoneRoot.childCount];
            for (int i = 0; i < bones.Length; ++i)
            {
                bones[i] = m_script.BoneRoot.GetChild(i);
            }

            Creep.CreepData data = m_script.Data;
            data.Bones = bones;
            m_script.Data = data;
        }
    }
}
