using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseImageEffectControl : MonoBehaviour
{
    [SerializeField]
    protected AnimationCurve m_curve;
    [SerializeField]
    protected float m_duration;
    public float Duration { get { return m_duration; } }
    [SerializeField]
    protected bool m_autoPlay;
    [SerializeField]
    protected bool m_autoClose;

    private Coroutine m_coroutine = null;

    public virtual void Play()
    {
        Stop();
        m_coroutine = StartCoroutine(Play_Coroutine());
    }

    private IEnumerator Play_Coroutine()
    {
        float time = 0;
        float speed = 1 / m_duration;

        while (time < 1)
        {
            UpdateEffect(time);
            time += Time.deltaTime * speed;
            yield return null;
        }

        Stop();
    }

    public virtual void Stop()
    {
        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
            m_coroutine = null;
        }

        UpdateEffect(1);
    }

    public virtual void UpdateEffect(float time)
    {

    }

    public virtual void Serialize()
    { 
        
    }

    public virtual void Deserialize()
    { 
        
    }

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
}
