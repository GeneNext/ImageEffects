using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCurve
{
    [SerializeField]
    protected Transform m_startTrans;
    public Transform StartTrans
    {
        get { return m_startTrans; }
        set { m_startTrans = value; }
    }

    [SerializeField]
    protected Transform m_endTrans;
    public Transform EndTrans
    {
        get { return m_endTrans; }
        set { m_endTrans = value; }
    }

    protected Transform m_target;
    protected Transform m_root;
    protected float m_duration;

    public void SetTarget(Transform target)
    {
        m_target = target;
    }

    public void SetRoot(Transform root)
    {
        m_root = root;
    }

    public void SetDuration(float duration)
    {
        m_duration = duration;
    }

    protected Vector3 GetStartPoint()
    {
        if (m_startTrans == null)
        {
            return Vector3.zero;
        }

        return m_startTrans.position;
    }

    protected Vector3 GetEndPoint()
    {
        Transform endTrans = m_target == null ? m_endTrans : m_target;

        if (endTrans == null)
        {
            return Vector3.zero;
        }

        return endTrans.position;
    }

    public virtual void CheckPoints() { }
    public virtual void ForceResetPoints() { }
    public virtual void DrawGizmos() { }
    public virtual Vector3 GetPoint(float time) { return Vector3.zero; }
}
