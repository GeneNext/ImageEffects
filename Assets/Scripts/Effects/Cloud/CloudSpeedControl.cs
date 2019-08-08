using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudSpeedControl : MonoBehaviour
{
    [SerializeField]
    private Material m_mat;
    [SerializeField, Range(0, 0.05f)]
    private float m_cloudSpeed = 0.001f;

    private float m_time;

    void Start()
    { 
        
    }

    void Update()
    {
        if (m_mat == null)
        {
            this.enabled = false;
            return;
        }

        m_time += Time.deltaTime * m_cloudSpeed;
        m_time %= 1;

        m_mat.SetFloat(ShaderPropertyID.TimeControl, m_time);
    }
}
