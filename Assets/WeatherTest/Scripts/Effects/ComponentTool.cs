using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentTool
{
    public static T CheckComponent<T>(this GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
        {
            component = go.AddComponent<T>();
        }
        return component;
    }

    public static T CheckComponent<T>(this Component component) where T : Component
    {
        return CheckComponent<T>(component.gameObject);
    }

    public static bool HasComponent<T>(this GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null;
    }

    public static void RemoveComponent<T>(this GameObject go, bool immediate = true) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component != null)
        {
            if (immediate)
            {
                Component.DestroyImmediate(component);
            }
            else
            {
                Component.Destroy(component);
            }
        }
    }

    public static void RemoveComponents<T>(this GameObject go, bool immediate = true, bool includeChildren = false) where T : Component
    {
        T[] components = includeChildren ? go.GetComponentsInChildren<T>() : go.GetComponents<T>();
        if (components == null)
        {
            return;
        }

        for (int i = 0; i < components.Length; ++i)
        {
            Component component = components[i];
            if (component != null)
            {
                if (immediate)
                {
                    Component.DestroyImmediate(component);
                }
                else
                {
                    Component.Destroy(component);
                }
            }
        }
    }

    public static void DebugNullComponents(this GameObject go)
    {
        MonoBehaviour[] behaviours = go.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; ++i)
        {
            if (behaviours[i] == null)
            {
                Debug.Log(go.name + " has a missing script", go);
            }
        }
    }

    public static void EnableBehaviours<T>(this GameObject go, bool enable) where T : Behaviour
    {
        if (go == null)
        {
            return;
        }

        T[] behaviours = go.GetComponents<T>();
        for (int i = 0; i < behaviours.Length; ++i)
        {
            T behaviour = behaviours[i];
            behaviour.enabled = enable;
        }
    }

    public static void CopyGameObjectInfo(ref GameObject origin, ref GameObject target)
    {
        if (origin == null || target == null)
        {
            return;
        }

        Transform originTrans = origin.transform;
        Transform targetTrans = target.transform;

        targetTrans.parent = originTrans.parent;
        targetTrans.position = originTrans.position;
        targetTrans.rotation = originTrans.rotation;
        targetTrans.localScale = originTrans.localScale;
    }

    public static void SetActive(GameObject go, bool active)
    {
        if (go == null)
        {
            return;
        }

        go.SetActive(active);
    }

    public static void SetEnable(Behaviour behaviour, bool enable)
    {
        if (behaviour == null)
        {
            return;
        }

        behaviour.enabled = enable;
    }

    public static List<Material> GetMatList(this GameObject go, bool includeChildren = false)
    {
        Renderer[] renderers = includeChildren ? go.GetComponentsInChildren<Renderer>() : go.GetComponents<Renderer>();

        List<Material> matList = new List<Material>();
        for (int i = 0; i < renderers.Length; ++i)
        {
            matList.AddRange(renderers[i].materials);
        }

        return matList;
    }

    public static void SetLayer(this GameObject go, int layer, bool includeChildren = true)
    {
        SetLayer(go.transform, layer, includeChildren);
    }

    public static void SetLayer(this Transform trans, int layer, bool includeChildren = true)
    {
        trans.gameObject.layer = layer;
        if (includeChildren)
        {
            for (int i = 0; i < trans.childCount; ++i)
            {
                SetLayer(trans.GetChild(i), layer, includeChildren);
            }
        }
    }

    public static void SetTag(GameObject go, string tag, bool includeChildren = true)
    {
        go.tag = tag;
        if (includeChildren)
        {
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                SetTag(go.transform.GetChild(i).gameObject, tag, includeChildren);
            }
        }
    }

    public static void SetParentRoot(this Transform root, Transform parent)
    {
        root.parent = parent;
        root.localPosition = Vector3.zero;
        root.localRotation = Quaternion.identity;
        root.localScale = Vector3.one;
    }

    public static Transform CreateChild(this Transform root, string name)
    {
        GameObject go = new GameObject(name);
        Transform child = go.transform;
        child.parent = root;
        child.gameObject.layer = root.gameObject.layer;
        child.localPosition = Vector3.zero;
        child.localEulerAngles = Vector3.zero;
        child.localScale = Vector3.one;
        return child;
    }

    public static T CreateChild<T>(this Transform root, string name) where T : Behaviour
    {
        Transform child = CreateChild(root, name);
        T behaviour = child.gameObject.CheckComponent<T>();
        return behaviour;
    }

    public static T CreateChild<T>(this Transform root, GameObject prefab) where T : Behaviour
    {
        GameObject go = GameObject.Instantiate(prefab) as GameObject;
        Transform child = go.transform;
        child.parent = root;
        child.gameObject.layer = root.gameObject.layer;
        child.localPosition = Vector3.zero;
        child.localEulerAngles = Vector3.zero;
        child.localScale = Vector3.one;
        T behaviour = go.CheckComponent<T>();
        return behaviour;
    }

    public static Transform CheckChild(this Transform root, string name)
    {
        Transform child = root.Find(name);
        if (child == null)
        {
            child = CreateChild(root, name);
        }
        return child;
    }

    public static Transform FindRoot(this GameObject go)
    {
        return FindRoot(go.transform);
    }

    public static Transform FindRoot(this Transform trans)
    {
        Transform root = trans;
        while (root.parent != null)
        {
            root = root.parent;
        }
        return root;
    }

    public static Transform GetChild(this Transform root, string childName, bool isCaseSensitive = false)
    {
        Transform child = null;
        for (int i = 0; i < root.childCount; ++i)
        {
            Transform tmpChild = root.GetChild(i);
            if (isCaseSensitive)
            {
                if (tmpChild.name == childName)
                {
                    child = tmpChild;
                    break;
                }
            }
            else
            {
                if (tmpChild.name.ToLower() == childName.ToLower())
                {
                    child = tmpChild;
                    break;
                }
            }
        }

        return child;
    }

    public static void ResetTrans(this Transform trans)
    {
        trans.localPosition = Vector3.zero;
        trans.localRotation = Quaternion.identity;
        trans.localScale = Vector3.one;
    }

    public static HashSet<string> GetEnumNamesHashSet(Type type, bool toLower = false)
    {
        string[] enumNames = GetEnumNames(type, toLower);

        HashSet<string> enumNamesHashSet = new HashSet<string>(enumNames);

        return enumNamesHashSet;
    }

    public static string[] GetEnumNames(Type type, bool toLower = false)
    {
        string[] enumNames = Enum.GetNames(type);

        if (toLower)
        {
            for (int i = 0; i < enumNames.Length; ++i)
            {
                enumNames[i] = enumNames[i].ToLower();
            }
        }
        return enumNames;
    }
}
