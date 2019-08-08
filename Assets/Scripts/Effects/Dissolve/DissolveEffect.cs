using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveEffect : MonoBehaviour
{
    [SerializeField]
    private Renderer m_renderer;
    public Renderer Renderer { get { return m_renderer; } }

    [SerializeField]
    private DissolveControl.eDataType m_dataType;
    public DissolveControl.eDataType DataType { get { return m_dataType; } }
}
