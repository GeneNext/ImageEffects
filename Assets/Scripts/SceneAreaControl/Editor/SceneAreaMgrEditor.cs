using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneAreaMgr))]
public class SceneAreaMgrEditor : Editor
{
    private GUILayoutOption[] m_options;

    void OnEnable()
    {
        m_options = new GUILayoutOption[0];
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SceneAreaMgr areaMgrScript = target as SceneAreaMgr;
        CreateAmbient(areaMgrScript);
        CreatePath(areaMgrScript);
    }

    private void CreateAmbient(SceneAreaMgr areaMgrScript)
    {
        if (GUILayout.Button("CreateAmbient", m_options))
        {
            string name = "Ambient_" + areaMgrScript.AreaAmbients.Length;
            if (areaMgrScript.AmbientRoot == null)
            {
                areaMgrScript.AmbientRoot = areaMgrScript.transform.CreateChild("AmbientRoot");
            }
            Transform ambientTrans = areaMgrScript.AmbientRoot.CreateChild(name);
            SceneAreaAmbient ambient = ambientTrans.gameObject.AddComponent<SceneAreaAmbient>();
        }
    }

    private void CreatePath(SceneAreaMgr areaMgrScript)
    {
        if (GUILayout.Button("CreatePath", m_options))
        {
            string name = "Path_" + areaMgrScript.AreaPaths.Length;
            if (areaMgrScript.PathRoot == null)
            {
                areaMgrScript.PathRoot = areaMgrScript.transform.CreateChild("PathRoot");
            }
            Transform pathTrans = areaMgrScript.PathRoot.CreateChild(name);
            SceneAreaPath path = pathTrans.gameObject.AddComponent<SceneAreaPath>();
        }
    }
}
