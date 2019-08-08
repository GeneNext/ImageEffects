using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveControlData : MonoBehaviour
{
    public Texture2D Texture;
    public Color Color;
    public float ColorIntensity;
    public CutoutData FadeInData;
    public CutoutData FadeOutData;

    [System.Serializable]
    public class CutoutData
    {
        public AnimationCurve Curve;
        public float Delay;
        public float Duration;
        public DissolveControl.eEffectType EffectType;
        [Range(0f, 1f)]
        public float DataFrom;
        [Range(0f, 1f)]
        public float DataTo;
    }
}
