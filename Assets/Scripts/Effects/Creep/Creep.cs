using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creep : MonoBehaviour
{
    [SerializeField]
    private Transform m_boneRoot;
    public Transform BoneRoot
    {
        get { return m_boneRoot; }
        set { m_boneRoot = value; }
    }

    [SerializeField]
    private CreepData m_data;
    public CreepData Data
    {
        get { return m_data; }
        set { m_data = value; }
    }

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
    private float m_duration;

    [SerializeField]
    private bool m_autoPlay;

    [SerializeField]
    private bool m_enableCreep;

    private Quaternion m_targetRotation;
    private Vector3 m_targetPosition;
    private float m_heightMultiplier;

    private Vector3 m_startPosition;

    void Awake()
    {
        m_data.Init();
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

    void Update()
    {
        if (m_enableCreep)
        {
            for (int i = 0; i < m_data.BoneDatas.Length; ++i)
            {
                m_data.BoneDatas[i].UpdateCreep((m_data.BoneDatas.Length - i) / m_data.BoneDatas.Length, Time.deltaTime);
            }
        }
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

        ResetPath();

        this.gameObject.SetActive(true);
        m_coroutine = StartCoroutine(Play_Coroutine());
    }

    private IEnumerator Play_Coroutine()
    {
        float timer = 0;

        while (timer < m_duration)
        {
            timer += Time.deltaTime / m_heightMultiplier;
            yield return null;

            m_boneRoot.position = GetPosition(timer / m_duration);
            //m_boneRoot.LookAt(GetPosition((timer + 0.01f) / m_duration));
        }

        m_boneRoot.position = GetPosition(1);

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
        m_boneRoot.position = m_startPosition;
        m_boneRoot.LookAt(m_targetPosition);
        this.transform.rotation = m_boneRoot.rotation;
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

    [System.Serializable]
    public struct CreepData
    {
        [SerializeField]
        private Transform[] m_bones;
        public Transform[] Bones
        {
            get { return m_bones; }
            set { m_bones = value; }
        }

        private CreepBoneData[] m_boneDatas;
        public CreepBoneData[] BoneDatas { get { return m_boneDatas; } }

        [SerializeField]
        private AnimationCurve m_curve;

        [SerializeField]
        private float m_minScale;

        [SerializeField]
        private float m_maxScale;

        [SerializeField]
        private float m_minDuration;

        [SerializeField]
        private float m_maxDuration;

        public void Init()
        {
            m_boneDatas = new CreepBoneData[m_bones.Length];
            for (int i = 0; i < m_boneDatas.Length; ++i)
            {
                m_boneDatas[i].Init(m_bones[i], m_curve, m_minScale, m_maxScale, m_minDuration, m_maxDuration);
            }
        }
    }

    [System.Serializable]
    public struct CreepBoneData
    {
        private Transform m_bone;
        public Transform Bone { get { return m_bone; } }

        private AnimationCurve m_curve;
        private float m_minScale;
        private float m_maxScale;

        private float m_minDuration;
        private float m_maxDuration;

        private float m_currentScale;
        private float m_fromScale;
        private float m_toScale;
        private float m_duration;
        private float m_timer;

        private Vector3 m_originBoneScale;

        public void Init(Transform bone, AnimationCurve curve, float minScale, float maxScale, float minDuration, float maxDuration)
        {
            m_bone = bone;

            m_curve = curve;

            m_minScale = minScale;
            m_maxScale = maxScale;

            m_minDuration = minDuration;
            m_maxDuration = maxDuration;

            m_originBoneScale = m_bone.localScale;

            Reset();
        }

        public void UpdateCreep(float percent, float deltaTime)
        {
            if (m_timer < m_duration)
            {
                float value = m_curve.Evaluate(m_timer / m_duration);

                m_currentScale = Mathf.Lerp(m_fromScale, m_toScale, value);

                m_bone.localScale = m_originBoneScale * m_currentScale;
            }

            m_timer += deltaTime;

            if (m_timer >= m_duration)
            {
                Reset();
            }
        }

        public void UpdatePosition(Vector3 position1, Vector3 position2)
        {
            m_bone.position = position1;
        }

        public void Reset()
        {
            m_timer = 0;
            m_fromScale = m_currentScale;
            m_toScale = Random.RandomRange(m_minScale, m_maxScale);
            m_duration = Random.RandomRange(m_minDuration, m_maxDuration);
        }
    }
}
