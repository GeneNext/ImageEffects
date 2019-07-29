using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AutoCameraInput;
using AutoCameraMode;

public class AutoCamera : MonoBehaviour
{
    [SerializeField]
    private Transform m_targetTrans;
    [SerializeField]
    private Camera m_camera;
    [SerializeField]
    private ModeData.eModeType m_modeType = ModeData.eModeType.Free;
    [SerializeField]
    private float m_cameraDistanceMax = 15;
    [SerializeField]
    private float m_cameraDistanceMin = 3;
    [SerializeField]
    private float m_cameraDistance = 10;
    [SerializeField]
    private float m_cameraHeight = 5;
    [SerializeField]
    private float m_axisMax = 55;
    [SerializeField]
    private float m_axisMin = -5;
    [SerializeField]
    private float m_rotateSpeed = 180;
    [SerializeField]
    private float m_smoothSpeed = 10;
    [SerializeField]
    private float m_scrollSpeed = 6;

    [SerializeField]
    private bool m_smoothFollow;

    [SerializeField]
    private LayerMask m_targetLayer;
    [SerializeField]
    private bool m_canSelectTarget;

    private bool m_isClickButton0;
    private bool m_isClickButton1;

    private Vector3 m_targetPoint;

    private float m_distance;
    private float m_targetDistance;

    private float m_angleX;
    private float m_angleY;


    private GameObject m_go;
    private Transform m_trans;
    private Transform m_cameraTrans;

    private InputData m_input;
    public InputData Input
    {
        get
        {
            if (m_input == null)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                m_input = new InputData_PC(this);
#else
                m_input = new InputData_Mobile(this);
#endif
                m_input.TargetLayer = m_targetLayer;
                m_input.CanSelectTarget = m_canSelectTarget;
            }

            return m_input;
        }
    }

    private ModeData m_mode;

    public Camera GetCamera()
    {
        return m_camera;
    }

    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;

        m_go = this.gameObject;
        m_trans = this.transform;

        SetCameraMode(m_modeType);
        m_targetDistance = m_cameraDistance;

        m_cameraTrans = m_camera.transform;
        m_cameraTrans.localPosition = Vector3.back * m_cameraDistance;
    }

    void LateUpdate()
    {
        if (m_camera == null)
        {
            return;
        }

        Input.Update();

        if (m_mode.ModeType != m_modeType)
        {
            SetCameraMode(m_modeType);
        }

        FollowTarget();

        CheckCameraDistance();
    }

    public void SetCameraMode(ModeData.eModeType modeType)
    {
        m_mode = ModeData.GetModeData(modeType);
        Rotate(Vector3.zero);
    }

    public void SetTarget(Transform target)
    {
        m_targetTrans = target;
    }

    private void FollowTarget()
    {
        float axisX = m_trans.eulerAngles.x;
        axisX = axisX < 180 ? axisX : axisX - 360;
        float value = (axisX - m_axisMin) / (m_axisMax - m_axisMin);
        value *= (m_cameraDistance - m_cameraDistanceMin) / (m_cameraDistanceMax - m_cameraDistanceMin);
        float height = m_cameraHeight;
        if (m_smoothFollow)
        {
            m_targetPoint = Vector3.Lerp(m_targetPoint, m_targetTrans.position + Vector3.up * height, m_smoothSpeed * Time.deltaTime);
        }
        else
        {
            m_targetPoint = m_targetTrans.position + Vector3.up * height;
        }
        m_trans.position = m_targetPoint;
    }

    public void Zoom(float deltaDistance)
    {
        m_cameraDistance -= deltaDistance * m_scrollSpeed;
        m_cameraDistance = Mathf.Clamp(m_cameraDistance, m_cameraDistanceMin, m_cameraDistanceMax);
    }

    private void CheckCameraDistance()
    {
#if UNITY_EDITOR
        Debug.DrawRay(m_trans.position, m_cameraTrans.position - m_trans.position, Color.white);
#endif

        Ray ray = new Ray(m_trans.position, m_cameraTrans.position - m_trans.position);
        RaycastHit hit;
        if (Physics.SphereCast(ray, Mathf.Max(m_cameraHeight - 0.5f, 0.01f), out hit, m_cameraDistance, m_mode.CameraCollisionMask))
        {
            m_distance = Vector3.Distance(m_trans.position, hit.point);
        }
        else
        {
            m_distance = m_cameraDistance;
        }

        float speed = m_targetDistance < m_distance ? 1 : 100;
        m_targetDistance = Mathf.Lerp(m_targetDistance, m_distance, m_smoothSpeed * Time.deltaTime * speed);

        if (QualitySettings.shadowDistance != m_targetDistance + 5f)
        {
            QualitySettings.shadowDistance = m_targetDistance + 5f;
        }

        m_cameraTrans.localPosition = Vector3.back * m_targetDistance;
    }

    public void Rotate(Vector3 deltaAngles)
    {
        if (m_mode.EnableAxisX)
        {
            m_angleX = (m_angleX + deltaAngles.y * m_rotateSpeed) % 360;
            m_angleX = Mathf.Clamp(m_angleX, m_axisMin, m_axisMax);
        }
        else
        {
            m_angleX = 45;
        }

        if (m_mode.EnableAxisY)
        {
            m_angleY = (m_angleY - deltaAngles.x * m_rotateSpeed) % 360;
        }
        else
        {
            m_angleY = 45;
        }

        m_trans.rotation = Quaternion.Euler(m_angleX, m_angleY, 0);
    }
}
