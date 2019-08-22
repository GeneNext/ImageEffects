using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class BezierCurve : BaseCurve
{
    /// <summary>
    /// 曲线类型，同时表示控制点数量
    /// </summary>
    public enum eBezierCurveType
    {
        Line = 0,
        TwoOrder = 1,
        ThreeOrder = 2,
    }

    [SerializeField]
    private eBezierCurveType m_curveType;
    public eBezierCurveType CurveType
    {
        get { return m_curveType; }
        set { m_curveType = value; }
    }

    [SerializeField]
    private Transform m_controlTrans1;
    public Transform ControlTrans1
    {
        get { return m_controlTrans1; }
        set { m_controlTrans1 = value; }
    }

    [SerializeField]
    private Transform m_controlTrans2;
    public Transform ControlTrans2
    {
        get { return m_controlTrans2; }
        set { m_controlTrans2 = value; }
    }

    [SerializeField]
    private float m_curveLength;
    public float CurveLength { get { return m_curveLength; } }

    private Vector3 GetControlPoint1()
    {
        if (m_controlTrans1 == null)
        {
            return Vector3.zero;
        }

        return m_controlTrans1.position;
    }

    private Vector3 GetControlPoint2()
    {
        if (m_controlTrans2 == null)
        {
            return Vector3.zero;
        }

        return m_controlTrans2.position;
    }

    public override void CheckPoints()
    {
        if (m_root == null || m_root.childCount == 4)
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

        for (int i = 0; i < 4; ++i)
        {
            GameObject go = new GameObject();
            go.transform.SetParent(m_root, false);
        }

        m_startTrans = m_root.GetChild(0);
        m_startTrans.name = "StartTrans";

        m_endTrans = m_root.GetChild(1);
        m_endTrans.name = "EndTrans";

        m_controlTrans1 = m_root.GetChild(2);
        m_controlTrans1.name = "ControlTrans1";

        m_controlTrans2 = m_root.GetChild(3);
        m_controlTrans2.name = "ControlTrans2";
    }

    public override Vector3 GetPoint(float time)
    {
        if (StartTrans != null && EndTrans == null)
        {
            return GetStartPoint();
        }

        time = Mathf.Clamp01(time);

        Vector3 point = Vector3.zero;
        switch (m_curveType)
        {
            case eBezierCurveType.Line:
                point = GetPoint_1(time);
                break;
            case eBezierCurveType.TwoOrder:
                point = GetPoint_2(time);
                break;
            case eBezierCurveType.ThreeOrder:
                point = GetPoint_3(time);
                break;
            default:
                point = Vector3.zero;
                break;
        }
        return point;
    }

    private Vector3 GetPoint_1(float time)
    {
        Vector3 startPoint = GetStartPoint();
        Vector3 endPoint = GetEndPoint();

        return Vector3.Lerp(startPoint, endPoint, time);
    }

    private Vector3 GetPoint_2(float time)
    {
        Vector3 startPoint = GetStartPoint();
        Vector3 endPoint = GetEndPoint();
        Vector3 controlPoint1 = GetControlPoint1();

        Vector3 p0 = Vector3.Lerp(startPoint, controlPoint1, time);
        Vector3 p1 = Vector3.Lerp(controlPoint1, endPoint, time);

        return Vector3.Lerp(p0, p1, time);
    }

    private Vector3 GetPoint_3(float time)
    {
        Vector3 startPoint = GetStartPoint();
        Vector3 endPoint = GetEndPoint();
        Vector3 controlPoint1 = GetControlPoint1() ;
        Vector3 controlPoint2 = GetControlPoint2();

        Vector3 p0 = Vector3.Lerp(startPoint, controlPoint1, time);
        Vector3 p1 = Vector3.Lerp(controlPoint1, controlPoint2, time);
        Vector3 p2 = Vector3.Lerp(controlPoint2, endPoint, time);

        Vector3 p3 = Vector3.Lerp(p0, p1, time);
        Vector3 p4 = Vector3.Lerp(p1, p2, time);

        return Vector3.Lerp(p3, p4, time);
    }

    public void UpdateCurveLength()
    {
        m_curveLength = 0;

        float stepInterval = 0.003f;

        for (float time = 0; time < 1; time += stepInterval)
        {
            Vector3 from = GetPoint(time);
            Vector3 to = GetPoint(time + stepInterval);

            m_curveLength += Vector3.Distance(from, to);
        }
    }

    public override void DrawGizmos()
    {
        if (m_startTrans == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawRay(StartTrans.position, StartTrans.forward);

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(StartTrans.position, 0.1f);

        if (EndTrans == null)
        {
            return;
        }

        float stepInterval = 0.003f;

        for (float time = 0; time < 1; time += stepInterval)
        {
            Vector3 from = GetPoint(time);
            Vector3 to = GetPoint(time + stepInterval);

            Gizmos.DrawLine(from, to);
        }

        UpdateCurveLength();
    }
}
