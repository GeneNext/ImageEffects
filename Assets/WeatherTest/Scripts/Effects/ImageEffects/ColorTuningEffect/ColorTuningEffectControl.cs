using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorTuningEffectControl : BaseImageEffectControl
{
    [SerializeField]
    private ColorTuningEffect m_effect = null;

    [SerializeField]
    private ControlData m_controlDataFrom = null;

    [SerializeField]
    private ControlData m_controlDataTo = null;

    public override void Play()
    {
        base.Play();
        m_effect.enabled = true;
    }

    public override void Stop()
    {
        base.Stop();

        if (m_autoClose)
        {
            if (m_effect != null)
            {
                m_effect.enabled = false;
            }
        }
    }

    public override void UpdateEffect(float time)
    {
        base.UpdateEffect(time);
        ControlData.UpdateEffect(m_effect, m_controlDataFrom, m_controlDataTo, m_curve.Evaluate(time));
    }

    [System.Serializable]
    private class ControlData
    {
        public Color TintColor = Color.white;
        public Color AddColor = Color.black;
        public float Brightness = 1;
        public float Contract = 1;
        public float Saturation = 1;

        public static void UpdateEffect(ColorTuningEffect effect, ControlData from, ControlData to, float lerp)
        {
            Color tintColor = Color.Lerp(from.TintColor, to.TintColor, lerp);
            Color addColor = Color.Lerp(from.AddColor, to.AddColor, lerp);
            float brightness = Mathf.Lerp(from.Brightness, to.Brightness, lerp);
            float contract = Mathf.Lerp(from.Contract, to.Contract, lerp);
            float saturation = Mathf.Lerp(from.Saturation, to.Saturation, lerp);

            effect.BaseColor.SetData(tintColor, addColor, brightness, contract, saturation);
        }
    }
}
