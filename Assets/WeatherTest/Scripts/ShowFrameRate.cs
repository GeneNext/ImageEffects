using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowFrameRate : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Text m_text = null;

    [SerializeField, Range(0f, 1f)]
    private float m_updateInterval = 0.2f;

    private float m_timer = 0;

    private void Update()
    {
        m_timer += Time.deltaTime;
        if (m_timer > m_updateInterval)
        {
            m_timer -= m_updateInterval;

            m_text.text = Screen.height + " " + Screen.width + " " + ((int)(1 / Time.deltaTime + 0.5f)).ToString();
        }
    }
}
