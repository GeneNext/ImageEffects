using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SceneAreaAmbientData;

public class SceneAreaAmbient : SceneArea
{
    [SerializeField]
    private LightData[] m_lightsData = new LightData[0];
    [SerializeField]
    private AmbientData m_ambientData;
    [SerializeField]
    private PostEffectsData m_postEffectsData;

    private float m_blendWeight;

    private Transform m_trans;

    void Awake()
    {
        m_trans = this.transform;
    }

    public void Init()
    {
        for (int i = 0; i < m_lightsData.Length; ++i)
        {
            m_lightsData[i].Init();
        }
        m_ambientData.Init();
        m_postEffectsData.Init();
    }

    public void Blend(Vector3 position)
    {
        if (!this.isActiveAndEnabled)
        {
            return;
        }

        UpdateBlendWeight(position);
    }

    private void UpdateBlendWeight(Vector3 position)
    {
        CheckAreaType();

        float blendWeight = m_areaRange.GetBlendWeight(this.transform, position);

        if (m_blendWeight == blendWeight)
        {
            return;
        }

        m_blendWeight = blendWeight;
        for (int i = 0; i < m_lightsData.Length; ++i)
        {
            m_lightsData[i].Blend(blendWeight);
        }
        m_ambientData.Blend(blendWeight);
        m_postEffectsData.Blend(blendWeight);

        Debug.LogError(this.name + " " + blendWeight);
    }

    public void SetDataToScene()
    {
        for (int i = 0; i < m_lightsData.Length; ++i)
        {
            m_lightsData[i].SetDataToScene();
        }
        m_ambientData.SetDataToScene();
        m_postEffectsData.SetDataToScene();
    }

    public void GetDataFromScene()
    {
        for (int i = 0; i < m_lightsData.Length; ++i)
        {
            m_lightsData[i].GetDataFromScene();
        }
        m_ambientData.GetDataFromScene();
        m_postEffectsData.GetDataFromScene();
    }
}
