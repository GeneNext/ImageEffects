using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CorruptionControl : MonoBehaviour
{
    [SerializeField]
    private CorruptionData m_corruptionData;

    [SerializeField]
    private bool m_playOnAwake;

    [SerializeField]
    private Renderer[] m_renderers;

    private Material[] m_mats;

    private GameObject m_go;
    private Transform m_trans;
    private string m_name;

    void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        m_go = this.gameObject;
        m_trans = this.transform;
        m_name = this.gameObject.name;

        InitMats();

        if (m_playOnAwake)
        {
            Play(null);
        }

        CorruptionMgr.Instance.AddItem(m_name, this);
    }

    void OnDestroy()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        CorruptionMgr.Instance.RemoveItem(m_name);
    }

    private void InitMats()
    {
        if (m_renderers == null || m_renderers.Length == 0)
        {
            return;
        }

        List<Material> matList = new List<Material>();
        for (int i = 0; i < m_renderers.Length; ++i)
        {
            matList.AddRange(m_renderers[i].materials);

            //for (int materialIndex = 0; materialIndex < m_renderers[i].materials.Length; ++materialIndex)
            //{
            //    Material mat = m_renderers[i].materials[materialIndex];
            //    string passName = mat.passCount + " ";
            //    for (int passIndex = 0; passIndex < mat.passCount; ++passIndex)
            //    {
            //        string tmpName = mat.GetPassName(passIndex);
            //        if (passIndex > 5)
            //        {
            //            mat.SetShaderPassEnabled(tmpName, false);
            //        }
            //        passName += " " + tmpName;
            //    }
            //    Debug.LogError(passName, m_renderers[i].gameObject);
            //}
        }
        m_mats = matList.ToArray();
        m_corruptionData.Reset(m_mats);
    }

    public void SerializeRenderersInChildren()
    {
        m_renderers = this.gameObject.GetComponentsInChildren<MeshRenderer>();
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            SerializeRenderersInChildren();
        }
    }

    public void Play(Transform playerTrans, bool autoClose = true)
    {
        ResetData();
        m_corruptionData.Coroutine = StartCoroutine(m_corruptionData.Play_Coroutine(m_mats, playerTrans, () =>
            {
                //if (autoClose)
                //{
                //    Close();
                //}
            }
            ));
    }

    public void ResetData()
    {
        if (m_corruptionData.Coroutine != null)
        {
            StopCoroutine(m_corruptionData.Coroutine);
            m_corruptionData.Coroutine = null;
        }
        m_corruptionData.Reset(m_mats);
    }

    public void Close()
    {
        ResetData();
        m_corruptionData.Close(m_mats);
    }

    private class BaseData
    {
        public AnimationCurve AnimationCurve;
        public float Duration;
    }

    [System.Serializable]
    private class CorruptionData : BaseData
    {
        public PurgeControl PurgeControl;
        public Transform StartTrans;
        public Color OutlineColor;
        public float OutlineRange;
        public float OutlineFactor;
        public float FalloffRange;
        public float OriginCutoffRange;
        public float CutoffRange;
        public bool Loop;

        [HideInInspector]
        public Coroutine Coroutine = null;

        public IEnumerator Play_Coroutine(Material[] mats, Transform playerTrans, System.Action completeCallback = null)
        {
            float timer = 0;
            float speed = 1 / Duration;

            InitMats(mats, playerTrans);

            float totalTime = Loop ? 2 : 1;

            while (timer < totalTime)
            {
                timer += Time.deltaTime * speed;
                if (Loop)
                {
                    timer %= totalTime;
                }
                yield return null;

                UpdateMats(mats, AnimationCurve.Evaluate(timer));
            }

            UpdateMats(mats, AnimationCurve.Evaluate(totalTime));

            Coroutine = null;

            if (completeCallback != null)
            {
                completeCallback();
            }
        }

        private void InitMats(Material[] mats, Transform playerTrans = null)
        {
            Vector3 startPos = playerTrans == null ? StartTrans.position : playerTrans.position + Vector3.up;
            for (int i = 0; i < mats.Length; ++i)
            {
                Material mat = mats[i];
                mat.SetVector(ShaderPropertyID.StartPos, startPos);
                mat.SetColor(ShaderPropertyID.OutlineColor, OutlineColor);
                mat.SetFloat(ShaderPropertyID.OutlineRange, OutlineRange);
                mat.SetFloat(ShaderPropertyID.OutlineFactor, OutlineFactor);
                mat.SetFloat(ShaderPropertyID.FalloffRange, FalloffRange);
            }

            PurgeControl.InitColor(OutlineColor);
            PurgeControl.UpdateData(0, 0);
            PurgeControl.Trans.position = startPos;
        }

        private void UpdateMats(Material[] mats, float value)
        {
            for (int i = 0; i < mats.Length; ++i)
            {
                Material mat = mats[i];
                mat.SetFloat(ShaderPropertyID.CutoffRange, CutoffRange * value + OriginCutoffRange);
            }

            float radius = CutoffRange * value + OriginCutoffRange + (OutlineRange - FalloffRange) * 0.9f;
            float alpha = 1 - value * 2f;
            PurgeControl.UpdateData(radius * 2, alpha);
        }

        public void Close(Material[] mats)
        {
            for (int i = 0; i < mats.Length; ++i)
            {
                Material mat = mats[i];
                mat.SetFloat(ShaderPropertyID.CutoffRange, CutoffRange);
            }

            PurgeControl.UpdateData(0, 0);
        }

        public void Reset(Material[] mats)
        {
            for (int i = 0; i < mats.Length; ++i)
            {
                Material mat = mats[i];
                mat.SetVector(ShaderPropertyID.StartPos, StartTrans.position);
                mat.SetFloat(ShaderPropertyID.CutoffRange, OriginCutoffRange);
                mat.SetFloat(ShaderPropertyID.OutlineRange, OutlineRange);
                mat.SetFloat(ShaderPropertyID.OutlineFactor, OutlineFactor);
                mat.SetFloat(ShaderPropertyID.FalloffRange, FalloffRange);
            }

            PurgeControl.InitColor(OutlineColor);
            PurgeControl.UpdateData(0, 0);
            PurgeControl.Trans.position = StartTrans.position;
        }
    }
}
