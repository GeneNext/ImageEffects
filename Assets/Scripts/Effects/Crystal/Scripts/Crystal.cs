using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystal : MonoBehaviour
{
    [SerializeField]
    private bool m_autoPlay;

    [SerializeField]
    private Renderer m_defaultRenderer;

    [SerializeField]
    private Transform m_crackRoot;

    [SerializeField]
    private Renderer[] m_crackRenderers;

    [SerializeField]
    private MatData m_emissionData;
    [SerializeField]
    private MatData m_fadeData;
    [SerializeField]
    private CrackData m_crackData;

    private List<Material> m_defaultMatList = new List<Material>();

    private List<Material> m_crackMatList = new List<Material>();

    public void SeralizeRenderers()
    {
        m_crackRenderers = m_crackRoot.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < m_crackRenderers.Length; ++i)
        {
            Renderer renderer = m_crackRenderers[i];
            MeshFilter meshFilter = renderer.gameObject.CheckComponent<MeshFilter>();
            MeshCollider meshCollider = renderer.gameObject.CheckComponent<MeshCollider>();
            Rigidbody rigidbody = renderer.gameObject.CheckComponent<Rigidbody>();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshCollider.convex = true;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = false;
        }
    }

    private string m_name;

    void Awake()
    {
        m_defaultMatList.AddRange(m_defaultRenderer.materials);

        for (int i = 0; i < m_crackRenderers.Length; ++i)
        {
            m_crackMatList.AddRange(m_crackRenderers[i].materials);
        }

        m_emissionData.Init(ShaderPropertyID.EmissionColor);
        m_fadeData.Init(ShaderPropertyID.Color);
        m_crackData.Init(m_crackRenderers);

        m_name = this.gameObject.name;
        CrystalMgr.Instance.AddItem(m_name, this);
    }

    void OnDestroy()
    {
        CrystalMgr.Instance.RemoveItem(m_name);
    }

    void OnEnable()
    {
        ShowCrystal(true, false);

        if (m_autoPlay)
        {
            Play();
        }
    }

    void OnDisable()
    {
        Stop();
    }

    public void Play()
    {
        this.gameObject.SetActive(true);

        PlayEmission();
        PlayFade();
        PlayCracks();
    }

    public void Stop()
    {
        StopEmission();
        StopFade();
        StopCracks();

        ShowCrystal(false, false);
    }

    public void ShowCrystal(bool showDefault, bool showCrack)
    {
        m_defaultRenderer.gameObject.SetActive(showDefault);
        m_crackRoot.gameObject.SetActive(showCrack);
    }

    public void StopEmission()
    {
        if (m_emissionData.Coroutine == null)
        {
            return;
        }

        m_emissionData.UpdateColor(m_defaultMatList, 0);

        StopCoroutine(m_emissionData.Coroutine);
        m_emissionData.Coroutine = null;
    }

    public void PlayEmission()
    {
        StopEmission();

        m_emissionData.Coroutine = StartCoroutine(PlayEmission_Coroutine());
    }

    private IEnumerator PlayEmission_Coroutine()
    {
        ShowCrystal(true, false);

        yield return new WaitForSeconds(m_emissionData.Delay);
        float timer = 0;
        float speed = 1 / m_emissionData.Duration;
        while (timer < 1)
        {
            m_emissionData.UpdateColor(m_defaultMatList, timer);
            timer += speed * Time.deltaTime;
            yield return null;
        }

        m_emissionData.UpdateColor(m_defaultMatList, 1);

        ShowCrystal(false, true);
    }

    public void StopFade()
    {
        if (m_fadeData.Coroutine == null)
        {
            return;
        }

        m_fadeData.UpdateColor(m_crackMatList, 0);
        m_emissionData.UpdateColor(m_crackMatList, 1);

        StopCoroutine(m_fadeData.Coroutine);
        m_fadeData.Coroutine = null;
    }

    public void PlayFade()
    {
        StopFade();

        m_fadeData.Coroutine = StartCoroutine(PlayFade_Coroutine());
    }

    private IEnumerator PlayFade_Coroutine()
    {
        yield return new WaitForSeconds(m_fadeData.Delay);
        float timer = 0;
        float speed = 1 / m_fadeData.Duration;
        while (timer < 1)
        {
            m_fadeData.UpdateColor(m_crackMatList, timer);
            m_emissionData.UpdateColor(m_crackMatList, 1 - timer);
            timer += speed * Time.deltaTime;
            yield return null;
        }

        m_fadeData.UpdateColor(m_crackMatList, 1);
        m_emissionData.UpdateColor(m_crackMatList, 0);

        yield return new WaitForSeconds(0.2f);

        Stop();
    }

    public void StopCracks()
    {
        if (m_crackData.Coroutine == null)
        {
            return;
        }

        m_crackData.Stop();

        StopCoroutine(m_crackData.Coroutine);
        m_crackData.Coroutine = null;
    }

    public void PlayCracks()
    {
        StopCracks();

        m_crackData.Coroutine = StartCoroutine(PlayCracks_Coroutine());
    }

    private IEnumerator PlayCracks_Coroutine()
    {
        m_crackData.Stop();

        yield return new WaitForSeconds(m_crackData.Delay);

        m_crackData.PlayCrack();
    }

    [System.Serializable]
    private class BaseData
    {
        public float Delay;
        [HideInInspector]
        public Coroutine Coroutine;
    }

    [System.Serializable]
    private class MatData : BaseData
    {
        [SerializeField]
        protected float m_duration;
        public float Duration
        {
            get { return m_duration; }
        }

        [SerializeField]
        protected AnimationCurve m_curve;

        [SerializeField]
        protected Color m_from;

        [SerializeField]
        protected Color m_to;

        protected int m_colorNameID;

        public void Init(int colorNameID)
        {
            m_colorNameID = colorNameID;
        }

        private Color GetColor(float time)
        {
            float value = m_curve.Evaluate(time);
            return Color.Lerp(m_from, m_to, value);
        }

        public void UpdateColor(List<Material> matList, float time)
        {
            for (int i = 0; i < matList.Count; ++i)
            {
                matList[i].SetColor(m_colorNameID, GetColor(time));
            }
        }
    }

    [System.Serializable]
    private class CrackData : BaseData
    {
        [SerializeField]
        private float m_speed;
        [SerializeField]
        private Transform m_startTrans;

        private List<TransData> m_transDataList = new List<TransData>();

        public void Init(Renderer[] renderers)
        {
            m_transDataList.Clear();

            for (int i = 0; i < renderers.Length; ++i)
            {
                TransData transData = new TransData();
                transData.Init(renderers[i].transform, m_startTrans);
                m_transDataList.Add(transData);
            }
        }

        public void Stop()
        {
            for (int i = 0; i < m_transDataList.Count; ++i)
            {
                m_transDataList[i].Reset();
            }
        }

        public void PlayCrack()
        {
            for (int i = 0; i < m_transDataList.Count; ++i)
            {
                m_transDataList[i].PlayCrack(m_speed);
            }
        }

        public void SetGravity(bool enable)
        {
            for (int i = 0; i < m_transDataList.Count; ++i)
            {
                m_transDataList[i].SetGravity(enable);
            }
        }
    }

    private class TransData
    {
        private Transform m_startTrans;
        private Transform m_trans;
        private Rigidbody m_rigidbody;
        private Vector3 m_originPosition;
        private Quaternion m_originRotation;

        public void Init(Transform trans, Transform startTrans)
        {
            m_trans = trans;
            m_rigidbody = trans.GetComponent<Rigidbody>();
            m_originPosition = m_trans.localPosition;
            m_originRotation = m_trans.localRotation;
            m_startTrans = startTrans;
        }

        public void Reset()
        {
            SetGravity(false);
            m_trans.localPosition = m_originPosition;
            m_trans.localRotation = m_originRotation;
        }

        public void SetGravity(bool enable)
        {
            m_rigidbody.useGravity = enable;
            m_rigidbody.velocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
            if (enable)
            {
                m_rigidbody.WakeUp();
            }
            else
            {
                m_rigidbody.Sleep();
            }
        }

        public void PlayCrack(float speed)
        {
            SetGravity(true);
            m_rigidbody.velocity = (m_trans.position - m_startTrans.position) * speed;
            m_rigidbody.angularVelocity = Random.Range(-10f, 10f) * Vector3.one * speed;
        }
    }
}
