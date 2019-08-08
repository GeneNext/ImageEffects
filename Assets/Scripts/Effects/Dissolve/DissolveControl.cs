using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveControl : MonoBehaviour
{
    [SerializeField]
    private DissolveControlData m_dissolveControlData;

    private List<Material> m_matList = null;
    private Coroutine m_coroutine;

    private static Dictionary<eDataType, DissolveControlData> m_controlDataDict = new Dictionary<eDataType, DissolveControlData>();

    public void InitData(Renderer renderer, DissolveControlData dissolveControlData)
    {
        m_matList = new List<Material>();
        m_matList.AddRange(renderer.materials);

        //for (int i = 0; i < m_matList.Count; ++i)
        //{
        //    Material mat = m_matList[i];
        //    if (!mat.shader.name.EndsWith("Dissolve"))
        //    {
        //        Shader shader = Shader.Find(mat.shader.name + "Dissolve");
        //        if (shader != null)
        //        {
        //            mat.shader = shader;
        //        }
        //    }

        //    mat.SetTexture("_NoiseTex", dissolveControlData.Texture);
        //    mat.SetColor("_EdgeColor", dissolveControlData.Color * dissolveControlData.ColorIntensity);
        //}

        m_dissolveControlData = dissolveControlData;
    }

    public static DissolveControl CreateDissolveControl(eDataType dataType, Renderer renderer)
    {
        //如果目标物体上有DissolveControl，直接使用其参数。
        DissolveControl dissolveControl = renderer.gameObject.GetComponent<DissolveControl>();
        if (dissolveControl == null)
        {
            //不存在时， 加载溶解样式， 并初始化
            dissolveControl = renderer.gameObject.AddComponent<DissolveControl>();
            DissolveControlData controlData = TryGetControlData(dataType);
            dissolveControl.InitData(renderer, controlData);
        }
        return dissolveControl;
    }

    private static DissolveControlData TryGetControlData(eDataType dataType)
    {
        DissolveControlData controlData = null;
        if (!m_controlDataDict.TryGetValue(dataType, out controlData))
        {
            GameObject prefab = Resources.Load("DissolveControlData_" + dataType) as GameObject;
            if (prefab != null)
            {
                controlData = prefab.GetComponent<DissolveControlData>();
            }
            m_controlDataDict.Add(dataType, controlData);
        }
        return controlData;
    }

    public static void Play(eDataType dataType, Renderer renderer, System.Action completeCallback = null)
    {
        DissolveControl dissolveControl = CreateDissolveControl(dataType, renderer);
        dissolveControl.Play(renderer, completeCallback);
    }

    public void Play(Renderer renderer, System.Action completeCallback = null)
    {
        Stop();

        m_coroutine = StartCoroutine(Play_Coroutine(completeCallback));
    }

    private IEnumerator Play_Coroutine(System.Action completeCallback = null)
    {
        DissolveControlData.CutoutData data = m_dissolveControlData.FadeInData;

        UpdateDissolve(data, 0);
        yield return new WaitForSeconds(data.Delay);

        float timer = 0;
        float speed = 1 / data.Duration;
        while (timer < 1)
        {
            UpdateDissolve(data, timer);
            timer += speed * Time.deltaTime;
            yield return null;
        }

        UpdateDissolve(data, 1);

        data = m_dissolveControlData.FadeOutData;

        yield return new WaitForSeconds(data.Delay);
        UpdateDissolve(data, 0);

        timer = 0;
        speed = 1 / data.Duration;

        while (timer < 1)
        {
            UpdateDissolve(data, timer);
            timer += speed * Time.deltaTime;
            yield return null;
        }

        UpdateDissolve(data, 1);

        if (completeCallback != null)
        {
            completeCallback();
        }

        Stop();
    }

    public static void FadeIn(eDataType dataType, Renderer renderer, System.Action completeCallback = null)
    {
        DissolveControl dissolveControl = CreateDissolveControl(dataType, renderer);
        dissolveControl.FadeIn(completeCallback);
    }

    public void FadeIn(System.Action completeCallback)
    {
        Dissolve(m_dissolveControlData.FadeInData, completeCallback);
    }

    public static void FadeOut(eDataType dataType, Renderer renderer, System.Action completeCallback = null)
    {
        DissolveControl dissolveControl = CreateDissolveControl(dataType, renderer);
        dissolveControl.FadeOut(completeCallback);
    }

    public void FadeOut(System.Action completeCallback)
    {
        Dissolve(m_dissolveControlData.FadeOutData, completeCallback);
    }

    public void Stop()
    {
        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
            m_coroutine = null;
        }
    }

    private void Dissolve(DissolveControlData.CutoutData data, System.Action completeCallback)
    {
        Stop();

        m_coroutine = StartCoroutine(Dissolve_Coroutine(data, completeCallback));
    }

    private IEnumerator Dissolve_Coroutine(DissolveControlData.CutoutData data, System.Action completeCallback)
    {
        UpdateDissolve(data, 0);
        yield return new WaitForSeconds(data.Delay);

        float timer = 0;
        float speed = 1 / data.Duration;
        while (timer < 1)
        {
            UpdateDissolve(data, timer);
            timer += speed * Time.deltaTime;
            yield return null;
        }

        UpdateDissolve(data, 1);

        if (completeCallback != null)
        {
            completeCallback();
        }

        Stop();
    }

    private void UpdateDissolve(DissolveControlData.CutoutData data, float time)
    {
        time = data.Curve.Evaluate(time);
        float value = Mathf.Lerp(data.DataFrom, data.DataTo, time);
        string propertyName = GetPropertyName(data.EffectType);
        for (int i = 0; i < m_matList.Count; ++i)
        {
            Material mat = m_matList[i];
            mat.SetFloat(propertyName, value);
        }
    }

    private string GetPropertyName(eEffectType effectType)
    {
        string propertyName = null;
        switch (effectType)
        {
            case eEffectType.Dissolve:
                propertyName = "_Cutoff";
                break;
            case eEffectType.Fade:
                propertyName = "_Alpha";
                break;
        }
        return propertyName;
    }

    public enum eDataType
    {
        TypeA = 0,
        TypeB = 1,
    }

    public enum eEffectType
    {
        Dissolve = 0,
        Fade = 1,
    }
}
