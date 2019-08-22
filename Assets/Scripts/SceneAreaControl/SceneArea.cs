using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SceneAreaRange;

public class SceneArea : MonoBehaviour
{
    [SerializeField]
    private Color m_gizmosColor1 = Color.green;
    [SerializeField]
    private Color m_gizmosColor2 = Color.red;
    [SerializeField]
    private bool m_drawGizmos = true;

    [SerializeField, HideInInspector]
    protected eAreaType m_areaType;
    public eAreaType AreaType
    {
        get { return m_areaType; }
        set { m_areaType = value; }
    }

    [SerializeField, HideInInspector]
    protected SphereRange m_sphereRange;
    public SphereRange SphereRange { get { return m_sphereRange; } }

    [SerializeField, HideInInspector]
    protected CylinderRange m_cylinderRange;
    public CylinderRange CylinderRange { get { return m_cylinderRange; } }

    [SerializeField, HideInInspector]
    protected CubeRange m_cubeRange;
    public CubeRange CubeRange { get { return m_cubeRange; } }

    protected iAreaRange m_areaRange;

    protected void CheckAreaType()
    {
        switch (m_areaType)
        {
            case eAreaType.Sphere:
                m_areaRange = m_sphereRange;
                break;
            case eAreaType.Cylinder:
                m_areaRange = m_cylinderRange;
                break;
            case eAreaType.Cube:
                m_areaRange = m_cubeRange;
                break;
            default:
                m_areaRange = m_sphereRange;
                break;
        }
    }

    public virtual void Serialize()
    { 
        
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        if (!m_drawGizmos)
        {
            return;
        }

        CheckAreaType();
        m_areaRange.DrawGizmos(this.transform, m_gizmosColor1, m_gizmosColor2);
    }
#endif
}
