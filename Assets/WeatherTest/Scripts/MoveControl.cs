using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveControl : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.AI.NavMeshAgent m_agent;

    [SerializeField]
    private Animator m_animator;

    [SerializeField]
    private Camera m_camera;

    [SerializeField]
    private LayerMask m_groundLayer;

    private eMoveType m_moveType;
    private Vector3 m_priorPosition;

    private UnityEngine.AI.NavMeshPath m_path = null;

    private void Awake()
    {
        m_path = new UnityEngine.AI.NavMeshPath();

        WeatherSystem.WeatherManager.SetMoveControl(this);
    }

    private void OnDestroy()
    {
        WeatherSystem.WeatherManager.SetMoveControl(null);
    }

    private void Update()
    {
        if (Vector3.Distance(m_priorPosition, m_agent.transform.position) > 0.01f)
        {
            SetMoveType(eMoveType.Move);
        }
        else
        {
            SetMoveType(eMoveType.Idle);
        }

        m_priorPosition = m_agent.transform.position;

        Vector3[] cornors = m_path.corners;
        for (int i = 0; i < cornors.Length - 1; ++i)
        {
            Debug.DrawLine(cornors[i], cornors[i + 1], Color.red);
        }
    }

    private void SetMoveType(eMoveType moveType)
    {
        if (m_moveType != moveType)
        {
            m_moveType = moveType;
            string stateName = moveType == eMoveType.Idle ? "idle_001" : "run_001";
            m_animator.CrossFade(stateName, 0.1f);
        }
    }

    private enum eMoveType
    {
        Idle,
        Move,
    }

    public void MoveByScreenPoint(Vector3 position)
    {
        Ray ray = m_camera.ScreenPointToRay(position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 999, m_groundLayer))
        {
            UnityEngine.AI.NavMesh.CalculatePath(m_agent.transform.position, hit.point, -1, m_path);
            m_agent.SetPath(m_path);
        }
    }
}
