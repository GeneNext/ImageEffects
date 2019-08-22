using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInstance_Mono<InstanceType> : MonoBehaviour
    where InstanceType : MonoBehaviour
{
    protected static InstanceType m_instance;
    public static InstanceType Instance
    {
        get
        {
            if (m_instance == null)
            {
                Debug.LogError("instance is missing");
            }
            return m_instance;
        }
    }

    protected virtual void Awake()
    {
        m_instance = this as InstanceType;
    }

    protected virtual void OnDestroy()
    {
        m_instance = null;
    }
}

public class BaseInstance_Mono<InstanceType, ItemType> : BaseInstance_Mono<InstanceType>
    where InstanceType : MonoBehaviour
    where ItemType : class
{
    protected Dictionary<string, ItemType> m_itemDict = new Dictionary<string, ItemType>();

    public void AddItem(string name, ItemType item)
    {
        m_itemDict[name] = item;
    }

    public void RemoveItem(string name)
    {
        if (m_itemDict.ContainsKey(name))
        {
            m_itemDict.Remove(name);
        }
    }

    public ItemType GetItem(string name)
    {
        ItemType item;
        return m_itemDict.TryGetValue(name, out item) ? item : null;
    }
}

public class BaseInstance_Normal<InstanceType>
    where InstanceType : class, new()
{
    protected static InstanceType m_instance;
    public static InstanceType Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new InstanceType();
            }
            return m_instance;
        }
    }
}

public class BaseInstance_Normal<InstanceType, ItemType> : BaseInstance_Normal<InstanceType>
    where InstanceType : class,new()
    where ItemType : class
{
    protected Dictionary<string, ItemType> m_itemDict = new Dictionary<string, ItemType>();

    public void AddItem(string name, ItemType item)
    {
        m_itemDict[name] = item;
    }

    public void RemoveItem(string name)
    {
        if (m_itemDict.ContainsKey(name))
        {
            m_itemDict.Remove(name);
        }
    }

    public ItemType GetItem(string name)
    {
        ItemType item;
        return m_itemDict.TryGetValue(name, out item) ? item : null;
    }
}
