using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SceneAreaRange;

public class SceneAreaPath : SceneArea
{
    [SerializeField]
    private float m_duration;

    [SerializeField]
    private BezierCurve m_bezier;
    public BezierCurve BezierCurve
    {
        get
        {
            m_bezier.SetRoot(this.transform);
            m_bezier.SetDuration(m_duration);
            return m_bezier;
        }
    }

    [SerializeField]
    private ePathShotType m_shotType;

    private Transform m_trans;
    private Transform m_targetTrans;
    private Transform m_cameraTrans;

    void Awake()
    {
        m_trans = this.transform;
    }

    public override void Serialize()
    {
        base.Serialize();

        BezierCurve.CheckPoints();
    }

    public void SetTargetTrans(Transform targetTrans)
    {
        m_targetTrans = targetTrans;
    }

    public void SetCameraTrans(Transform cameraTrans)
    {
        m_cameraTrans = cameraTrans;
    }

    public void UpdateTargetTrans()
    {
        Transform cameraTrans = m_cameraTrans.GetChild(0);
        if (CheckInRange(m_targetTrans))
        {
            m_cameraTrans.gameObject.EnableBehaviours<Behaviour>(false);
            cameraTrans.localPosition = new Vector3(2.4f, 3.6f, -8.5f);
            cameraTrans.localRotation = Quaternion.Euler(20f, 5f, -5f);
            UpdateCameraTrans();
        }
        else
        {
            m_cameraTrans.gameObject.EnableBehaviours<Behaviour>(true);
            cameraTrans.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    private void UpdateCameraTrans()
    {
        float ratio = GetRatio(m_targetTrans.position);

        switch (m_shotType)
        {
            case ePathShotType.Base:
                m_cameraTrans.position = m_bezier.GetPoint(ratio);
                m_cameraTrans.rotation = Quaternion.Slerp(m_bezier.StartTrans.rotation, m_bezier.EndTrans.rotation, ratio);
                break;
            case ePathShotType.AerialShot:
                m_cameraTrans.position = m_targetTrans.position + Vector3.up * 10;
                m_cameraTrans.rotation = Quaternion.Euler(90, 0, 0);
                break;
            case ePathShotType.Focus:
                m_cameraTrans.position = m_bezier.GetPoint(ratio);
                m_cameraTrans.LookAt(m_targetTrans);
                break;
            case ePathShotType.OverTheShoulder:
                m_cameraTrans.rotation = m_targetTrans.rotation;
                m_cameraTrans.position = m_targetTrans.position + m_cameraTrans.rotation * new Vector3(3, 3, 0);
                break;
            case ePathShotType.PointOfView:
                m_cameraTrans.rotation = m_targetTrans.rotation;
                m_cameraTrans.position = m_targetTrans.position + m_cameraTrans.rotation * new Vector3(0, 2, 1);
                break;
        }
    }

    public bool CheckInRange(Transform targetTrans)
    {
        CheckAreaType();
        return m_areaRange.CheckInRange(m_trans, targetTrans.position);
    }

    private Vector3 GetStartPoint()
    {
        return BezierCurve.StartTrans == null ? Vector3.zero : BezierCurve.StartTrans.position;
    }

    private Vector3 GetEndPoint()
    {
        return BezierCurve.EndTrans == null ? Vector3.zero : BezierCurve.EndTrans.position;
    }

    private float GetRatio(Vector3 position)
    {
        Vector3 startPoint = GetStartPoint();
        Vector3 endPoint = GetEndPoint();

        float totalDistance = Vector3.Distance(startPoint, endPoint);
        float distanceToStart = Vector3.Distance(startPoint, position);
        float distanceToEnd = Vector3.Distance(endPoint, position);

        float ratio;
        if (distanceToStart > totalDistance)
        {
            ratio = 1;
        }
        else if (distanceToEnd > totalDistance)
        {
            ratio = 0;
        }
        {
            ratio = distanceToStart / (distanceToStart + distanceToEnd);
        }

        return ratio;
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        BezierCurve.DrawGizmos();
        base.OnDrawGizmos();
    }
#endif

    public enum ePathShotType
    {
        /// <summary>
        /// 基本
        /// </summary>
        Base,
        /// <summary>
        /// 航拍
        /// </summary>
        AerialShot,
        /// <summary>
        /// 特写
        /// </summary>
        Focus,
        /// <summary>
        /// 过肩
        /// </summary>
        OverTheShoulder,
        /// <summary>
        /// 主观视角
        /// </summary>
        PointOfView,
    }
}
