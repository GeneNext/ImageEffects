using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreepControl : MonoBehaviour
{
    [SerializeField]
    private CreepOoze m_ooze;

    [SerializeField]
    private CreepBubble m_bubble;

    [SerializeField]
    private CreepDrip m_drip;

    [SerializeField, Range(0, 100)]
    private int m_dripCount;

    [SerializeField]
    private float m_loopInterval;

    [SerializeField]
    private bool m_loop;

    [SerializeField]
    private bool m_autoPlay;

    private List<CreepDrip> m_dripList = new List<CreepDrip>();

    void Awake()
    {
        for (int i = 0; i < m_dripCount; ++i)
        {
            GameObject dripGO = GameObject.Instantiate(m_drip.gameObject);
            dripGO.transform.parent = this.transform;
            dripGO.transform.position = this.transform.position;
            dripGO.SetActive(true);
            CreepDrip drip = dripGO.GetComponent<CreepDrip>();
            m_dripList.Add(drip);
        }

        m_drip.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (m_autoPlay)
        {
            Play();
        }
    }

    void OnDisable()
    {
        Stop();
    }

    private Coroutine m_coroutine = null;

    private void Stop()
    {
        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
            m_coroutine = null;
        }
    }

    private void Play()
    {
        m_coroutine = StartCoroutine(Play_Coroutine());
    }

    private IEnumerator Play_Coroutine()
    {
        bool loop = true;
        while (loop)
        {
            bool oozeComplete = false;
            m_ooze.Play(() => oozeComplete = true);

            bool bubbleComplete = false;
            m_bubble.Play(() => bubbleComplete = true);

            for (int i = 0; i < m_dripList.Count; ++i)
            {
                m_dripList[i].Play();
            }

            while (!(oozeComplete && bubbleComplete))
            {
                yield return null;
            }
            yield return new WaitForSeconds(m_loopInterval);

            loop = m_loop;
        }

        Stop();
    }
}
