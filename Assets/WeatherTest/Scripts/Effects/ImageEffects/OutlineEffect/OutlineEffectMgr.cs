using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineEffectMgr
{
    private static OutlineEffectMgr m_instance;
    public static OutlineEffectMgr Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = new OutlineEffectMgr();
            }
            return m_instance;
        }
    }

    private OutlineEffect m_outlineEffect;
    public OutlineEffect OutlineEffect
    {
        get { return m_outlineEffect; }
    }

    public void SetOutlineEffect(OutlineEffect outlineEffect)
    {
        m_outlineEffect = outlineEffect;
    }

    private int m_outlineCounter;

    private Dictionary<int, int> m_originLayerDict = new Dictionary<int, int>();
    private int m_outlineLayer = -1;
    public int OutlintLayer
    {
        get
        {
            if (m_outlineLayer == -1)
            {
                m_outlineLayer = LayerMask.NameToLayer("Outline");
            }
            return m_outlineLayer;
        }
    }

    public void EnableOutline(GameObject go, Material mat, Color color, bool enable, bool updateChildrenLayer)
    {
        //获得原始层
        int originLayer = 0;
        int instanceID = go.GetInstanceID();
        if (!m_originLayerDict.TryGetValue(instanceID, out originLayer))
        {
            originLayer = go.layer;
            m_originLayerDict[instanceID] = originLayer;
        }

        //设置显示层
        float alpha = enable ? 1 : 0;
        color.a = alpha;
        int layer = enable ? OutlintLayer : originLayer;
        if (updateChildrenLayer)
        {
            ComponentTool.SetLayer(go, layer);
        }
        else
        {
            go.layer = layer;
        }
        mat.SetColor(ShaderPropertyID.OutlineColor, color);

        //计数
        m_outlineCounter = m_outlineCounter + (enable ? 1 : -1);
        if (m_outlineCounter <= 0)
        {
            m_outlineCounter = 0;
        }
        m_outlineEffect.enabled = m_outlineCounter > 0;
    }
}
