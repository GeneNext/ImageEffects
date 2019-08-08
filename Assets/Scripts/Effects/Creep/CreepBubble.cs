using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreepBubble : MonoBehaviour
{
    [SerializeField]
    private Transform m_item;

    [SerializeField]
    private BubbleData[] m_datas;

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
        for (int i = 0; m_datas != null && i < m_datas.Length; ++i)
        {
            BubbleData data = m_datas[i];

            float duration = data.GetDuration();
            float timer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float blendWeight = data.GetBlendWeight(timer / duration);
                data.Blend(m_item, blendWeight);
                yield return null;
            }

            data.Blend(m_item, 1);
        }

        if (completeCallback != null)
        {
            completeCallback();
        }

        Stop();
    }

    [System.Serializable]
    private struct BubbleData
    {
        [SerializeField]
        private AnimationCurve m_speedCurve;
        [SerializeField]
        private float m_duration;
        [SerializeField]
        private Vector3 m_fromPosition;
        [SerializeField]
        private Vector3 m_toPosition;
        [SerializeField]
        private Vector3 m_fromScale;
        [SerializeField]
        private Vector3 m_toScale;

        public float GetDuration()
        {
            return m_duration;
        }

        public float GetBlendWeight(float time)
        {
            return m_speedCurve.Evaluate(time);
        }

        public void Blend(Transform item, float blendWeight)
        {
            item.localPosition = Vector3.Lerp(m_fromPosition, m_toPosition, blendWeight);
            item.localScale = Vector3.Lerp(m_fromScale, m_toScale, blendWeight);
        }
    }
}
