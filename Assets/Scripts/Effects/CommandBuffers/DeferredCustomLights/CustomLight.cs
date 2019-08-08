using UnityEngine;

[ExecuteInEditMode]
public class CustomLight : MonoBehaviour
{
    public enum eLightType
    {
        Sphere,
        Tube,
        Capsule,
    }
    public eLightType LightType;

    public Color Color;
    public float Intensity = 1.0f;
    public float Range = 10.0f;
    public float Size = 0.5f;
    public float TubeLength = 1.0f;

    public void OnEnable()
    {
        CustomLightSystem.instance.Add(this);
    }

    public void Start()
    {
        CustomLightSystem.instance.Add(this);
    }

    public void OnDisable()
    {
        CustomLightSystem.instance.Remove(this);
    }

    public Color GetLinearColor()
    {
        return new Color(
            Mathf.GammaToLinearSpace(Color.r * Intensity),
            Mathf.GammaToLinearSpace(Color.g * Intensity),
            Mathf.GammaToLinearSpace(Color.b * Intensity),
            1.0f
        );
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, LightType == eLightType.Tube ? "AreaLight Gizmo" : "PointLight Gizmo", true);
    }
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.1f, 0.7f, 1.0f, 0.6f);
        if (LightType == eLightType.Tube)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(TubeLength * 2, Size * 2, Size * 2));
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
        else
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawWireSphere(transform.position, Size);
        }
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}
