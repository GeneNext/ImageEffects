using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexelOffset : MonoBehaviour
{
    [SerializeField]
    private Renderer m_renderer;
    [SerializeField]
    private AnimationCurve m_curve;
    [SerializeField]
    private float m_intervalMin;
    [SerializeField]
    private float m_intervalMax;
    [SerializeField]
    private float m_durationMin;
    [SerializeField]
    private float m_durationMax;
    [SerializeField, Range(0f, 1f)]
    private float m_offsetRange;
    [SerializeField]
    private float m_showFontDelay;
    [SerializeField]
    private float m_showFontDuration;
    [SerializeField]
    private Vector2 m_showFontOffset;
    [SerializeField]
    private bool m_loop;

    private Material[] m_mats;
    private Vector2 m_originOffset;
    private Vector2 m_currentOffset;
    private Vector2 m_targetOffset;
    private Coroutine m_offsetCoroutine = null;

    void Awake()
    {
        m_mats = m_renderer.materials;
    }

    void OnEnable()
    {
        PlayOffset();
    }

    void OnDisable()
    {
        StopOffset();
    }

    private float GetDuration()
    {
        return Random.Range(m_durationMin, m_durationMax);
    }

    private float GetInterval()
    {
        return Random.Range(m_intervalMin, m_intervalMax);
    }

    private Vector2 GetOffset(float time)
    {
        float value = m_curve.Evaluate(time);
        return Vector2.Lerp(m_originOffset, m_targetOffset, value);
    }

    private void UpdateTargetOffset()
    {
        m_targetOffset = new Vector2(Random.Range(-m_offsetRange, m_offsetRange), Random.Range(-m_offsetRange, m_offsetRange));
    }

    public void StopOffset()
    {
        if (m_offsetCoroutine != null)
        {
            StopCoroutine(m_offsetCoroutine);
            m_offsetCoroutine = null;
        }
        m_originOffset = Vector2.zero;
    }

    public void PlayOffset()
    {
        StopOffset();

        m_offsetCoroutine = StartCoroutine(PlayOffset_Coroutine());
    }

    private IEnumerator PlayOffset_Coroutine()
    {
        bool loop = true;
        while (loop)
        {
            UpdateTargetOffset();

            float time = 0;
            float duration = GetDuration();
            float speed = 1 / duration;

            while (time < 1)
            {
                UpdateMaterialOffset(time);
                yield return null;
                time += Time.deltaTime * speed;
            }

            UpdateMaterialOffset(time);

            float interval = GetInterval();
            yield return new WaitForSeconds(interval);

            m_originOffset = m_targetOffset;
            loop = m_loop;
        }
    }

    private void UpdateMaterialOffset(float time)
    {
        m_currentOffset = GetOffset(time);
        UpdateMaterialOffset(m_currentOffset);
    }

    private void UpdateMaterialOffset(Vector2 offset)
    {
        for (int i = 0; i < m_mats.Length; ++i)
        {
            Material mat = m_mats[i];
            mat.SetVector("_TexelOffset", new Vector4(offset.x, offset.y, 0, 0));
        }
    }

    private Coroutine m_showFontCoroutine = null;

    public void StopShowFont()
    {
        if (m_showFontCoroutine != null)
        {
            StopCoroutine(m_showFontCoroutine);
            m_showFontCoroutine = null;
        }

        UpdateBlendWeight(0);
    }

    public void PlayShowFont()
    {
        StopOffset();
        StopShowFont();

        m_showFontCoroutine = StartCoroutine(PlayShowFont_Coroutine());
    }

    private IEnumerator PlayShowFont_Coroutine()
    {
        float time = 0;
        float speed = 1 / m_showFontDelay;

        while (time < 1)
        {
            UpdateMaterialOffset(Vector2.Lerp(m_currentOffset, m_showFontOffset, time));
            yield return null;
            time += Time.deltaTime * speed;
        }
        UpdateMaterialOffset(m_showFontOffset);

        time = 0;
        speed = 1 / m_showFontDuration;
        while (time < 1)
        {
            UpdateBlendWeight(time);
            yield return null;
            time += Time.deltaTime * speed;
        }
        UpdateMaterialOffset(m_showFontOffset);
        UpdateBlendWeight(1);
    }

    private void UpdateBlendWeight(float time)
    {
        for (int i = 0; i < m_mats.Length; ++i)
        {
            m_mats[i].SetFloat("_BlendWeight", time);
        }
    }
}
