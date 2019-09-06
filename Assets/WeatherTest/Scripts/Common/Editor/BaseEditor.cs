using UnityEngine;
using UnityEditor;
using System.Collections;

public class BaseEditor<ScriptType> : Editor where ScriptType : MonoBehaviour
{
    protected ScriptType m_script;
    protected GUILayoutOption[] m_options;

    protected virtual void OnEnable()
    {
        m_script = target as ScriptType;
        m_options = new GUILayoutOption[0];
    }

    protected void CreateChild<T>(string buttonName, string itemName, Transform root, System.Action<T> createCallback) where T : Behaviour
    {
        if (GUILayout.Button(buttonName, m_options))
        {
            GameObject go = new GameObject(itemName);
            go.transform.SetParent(root);
            T behaviour = go.CheckComponent<T>();
            createCallback(behaviour);
        }
    }
}
