using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreepDrip : MonoBehaviour
{
    [SerializeField]
    private Transform m_item;

    [SerializeField]
    private Transform m_startTrans;

    [SerializeField]
    private float m_minRadius;
    [SerializeField]
    private float m_maxRadius;

    [SerializeField]
    private AnimationCurve m_heightCurve;
    [SerializeField]
    private float m_height;

    [SerializeField]
    private AnimationCurve m_speedCurve;

    [SerializeField]
    private float m_delayMin;
    [SerializeField]
    private float m_delayMax;

    [SerializeField]
    private float m_duration;

    [SerializeField]
    private bool m_autoPlay;

    private Quaternion m_targetRotation;
    private Vector3 m_targetPosition;
    private float m_heightMultiplier;

    private Vector3 m_startPosition;

    void OnEnable()
    {
        if (m_autoPlay)
        {
            Play();
        }
    }

    void OnDisable()
    {
        Stop();
    }

    private Coroutine m_coroutine = null;

    public void Stop()
    {
        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
            m_coroutine = null;
        }

        this.gameObject.SetActive(false);
    }

    public void Play()
    {
        Stop();

        this.gameObject.SetActive(true);
        m_item.transform.localPosition = Vector3.zero;

        m_coroutine = StartCoroutine(Play_Coroutine());
    }

    private IEnumerator Play_Coroutine()
    {
        yield return new WaitForSeconds(Random.Range(m_delayMin, m_delayMax));

        float timer = Random.Range(0f, 0.3f);

        ResetPath();

        while (timer < m_duration)
        {
            timer += Time.deltaTime / m_heightMultiplier;
            yield return null;

            m_item.position = GetPosition(timer / m_duration);
        }

        m_item.position = GetPosition(1);

        Stop();
    }

    private Vector3 GetPosition(float time)
    {
        if (m_speedCurve == null || m_heightCurve == null)
        {
            return this.transform.position;
        }

        time = m_speedCurve.Evaluate(time);
        Vector3 position = Vector3.Lerp(m_startPosition, m_targetPosition, time);
        position.y += m_heightCurve.Evaluate(time) * m_height * m_heightMultiplier;
        return position;
    }

    private void ResetPath()
    {
        float radius = Random.RandomRange(m_minRadius, m_maxRadius);
        m_targetRotation = Quaternion.Euler(0, Random.RandomRange(0f, 360f), 0);
        m_targetPosition = this.transform.position + m_targetRotation * Vector3.forward * radius;
        m_heightMultiplier = radius / m_maxRadius;
        m_startPosition = m_startTrans.position;
        m_item.position = m_startPosition;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        m_minRadius = Mathf.Max(0, m_minRadius);
        m_maxRadius = Mathf.Max(0, m_maxRadius);

        if (m_minRadius >= m_maxRadius)
        {
            m_minRadius = m_maxRadius - 0.01f;
        }

        if (m_maxRadius <= m_minRadius)
        {
            m_maxRadius = m_minRadius + 0.01f;
        }

        //内圈
        for (int i = 0; i < 100; ++i)
        {
            Quaternion rotation = Quaternion.Euler(0, i / 100f * 360f, 0);
            Quaternion nextRotation = Quaternion.Euler(0, (i + 1) / 100f * 360f, 0);
            Gizmos.DrawLine(this.transform.position + rotation * Vector3.forward * m_minRadius, this.transform.position + nextRotation * Vector3.forward * m_minRadius);
        }

        //外圈
        for (int i = 0; i < 100; ++i)
        {
            Quaternion rotation = Quaternion.Euler(0, i / 100f * 360f, 0);
            Quaternion nextRotation = Quaternion.Euler(0, (i + 1) / 100f * 360f, 0);
            Gizmos.DrawLine(this.transform.position + rotation * Vector3.forward * m_maxRadius, this.transform.position + nextRotation * Vector3.forward * m_maxRadius);
        }

        if (Application.isPlaying)
        {
            for (int i = 0; i < 100; ++i)
            {
                Vector3 position = GetPosition(i / 100f);
                Vector3 nextPosition = GetPosition((i + 1) / 100f);
                Gizmos.DrawLine(position, nextPosition);
            }
        }
    }
#endif
}
