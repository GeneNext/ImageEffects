using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialBlurEffectControl : BaseImageEffectControl
{
    [SerializeField]
    private RadialBlurEffect m_effect = null;

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
        public float SampleDistance = 0;
        public float SampleStrength = 0;

        public static void UpdateEffect(RadialBlurEffect effect, ControlData from, ControlData to, float lerp)
        {
            float distance = Mathf.Lerp(from.SampleDistance, to.SampleDistance, lerp);
            float strength = Mathf.Lerp(from.SampleStrength, to.SampleStrength, lerp);

            effect.SetSampleData(distance, strength);
        }
    }
}
