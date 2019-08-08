using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleControl : MonoBehaviour
{
    [SerializeField]
    private Transform m_item;

    [SerializeField]
    private AnimationCurve m_speedCurve;

    [SerializeField]
    private float m_duration;

    [SerializeField]
    private ScaleData m_fromData;

    [SerializeField]
    private ScaleData m_toData;

    [SerializeField]
    private bool m_autoPlay;

    void OnEnable()
    {
        if (m_autoPlay)
        {
            Play(null);
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
    }

    public void Play(System.Action completeCallback)
    {
        m_coroutine = StartCoroutine(Play_Coroutine(completeCallback));
    }

    private IEnumerator Play_Coroutine(System.Action completeCallback)
    {
        float speed = 1 / m_duration;

        float timer = 0;

        while (timer < 1)
        {
            timer += Time.deltaTime * speed;
            Blend(timer);
            yield return null;
        }

        if (completeCallback != null)
        {
            completeCallback();
        }

        Stop();
    }

    public void Blend(float timer)
    {
        float blendWeight = m_speedCurve.Evaluate(timer);
        m_item.localPosition = Vector3.Lerp(m_fromData.Position, m_toData.Position, blendWeight);
        m_item.localScale = Vector3.Lerp(m_fromData.Scale, m_toData.Scale, blendWeight);
    }

    [System.Serializable]
    private struct ScaleData
    {
        public Vector3 Position;
        public Vector3 Scale;
    }
}
