using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OffsetCurve : BaseCurve
{
    [SerializeField]
    private AnimationCurve m_controlCurve;
    public AnimationCurve ControlCurve
    {
        get { return m_controlCurve; }
        set { m_controlCurve = value; }
    }

    [SerializeField]
    private Vector2 m_offsetDirection;
    public Vector2 OffsetDirection
    {
        get { return m_offsetDirection; }
        set { m_offsetDirection = value; }
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

    public override Vector3 GetPoint(float time)
    {
        time = Mathf.Clamp01(time);

        Vector3 position = Vector3.Lerp(GetStartPoint(), GetEndPoint(), time);
        Vector3 radiusOffset = StartTrans.rotation * m_offsetDirection * m_controlCurve.Evaluate(time);
        return position + radiusOffset;
    }

    public override void DrawGizmos()
    {
        if (StartTrans == null || EndTrans == null)
        {
            return;
        }

        if (m_controlCurve == null)
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
