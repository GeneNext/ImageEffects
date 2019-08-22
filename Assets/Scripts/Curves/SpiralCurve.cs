using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SpiralCurve : BaseCurve
{
    [SerializeField]
    private AnimationCurve m_radiusCurve;
    public AnimationCurve RadiusCurve
    {
        get { return m_radiusCurve; }
        set { m_radiusCurve = value; }
    }

    [SerializeField]
    private float m_radius;
    public float Radius
    {
        get { return m_radius; }
        set { m_radius = value; }
    }

    [SerializeField]
    private Vector3 m_spiralDirection;
    public Vector3 SpiralDirection
    {
        get { return m_spiralDirection; }
        set { m_spiralDirection = value; }
    }

    public override void CheckPoints()
    {
        if (m_root == null || m_root.childCount == 2)
        {
            return;
        }

        ForceResetPoints();
    }

    public override void ForceResetPoints()
    {
        for (int i = m_root.childCount - 1; i >= 0; --i)
        {
            GameObject.DestroyImmediate(m_root.GetChild(i).gameObject);
        }

        for (int i = 0; i < 2; ++i)
        {
            GameObject go = new GameObject();
            go.transform.SetParent(m_root, false);
        }

        m_startTrans = m_root.GetChild(0);
        m_startTrans.name = "StartTrans";

        m_endTrans = m_root.GetChild(1);
        m_endTrans.name = "EndTrans";
    }

    public Quaternion GetRotation(float time)
    {
        return Quaternion.Euler(m_spiralDirection * m_duration * time);
    }

    public override Vector3 GetPoint(float time)
    {
        time = Mathf.Clamp01(time);

        float radius = m_radiusCurve.Evaluate(time) * m_radius;
        Quaternion rotation = GetRotation(time);

        return Vector3.Lerp(GetStartPoint(), GetEndPoint(), time) + m_root.rotation * (rotation * Vector3.back * radius);
    }

    public override void DrawGizmos()
    {
        if (StartTrans == null || EndTrans == null)
        {
            return;
        }

        if (m_radiusCurve == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;

        float stepInterval = 0.003f;

        for (float time = 0; time < 1; time += stepInterval)
        {
            Vector3 from = GetPoint(time);
            Vector3 to = GetPoint(time + stepInterval);

            Gizmos.DrawLine(from, to);
        }
    }
}
