using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SceneAreaMgr : BaseInstance_Mono<SceneAreaMgr>
{
    [SerializeField]
    private Transform m_target;
    [SerializeField]
    private Transform m_cameraTrans;

    [SerializeField]
    private Transform m_ambientRoot;
    public Transform AmbientRoot
    {
        get { return m_ambientRoot; }
        set { m_ambientRoot = value; }
    }

    [SerializeField]
    private Transform m_pathRoot;
    public Transform PathRoot
    {
        get { return m_pathRoot; }
        set { m_pathRoot = value; }
    }

    [SerializeField]
    private SceneAreaAmbient m_defaultAmbient;

    [SerializeField]
    private SceneAreaAmbient[] m_areaAmbients = new SceneAreaAmbient[0];
    public SceneAreaAmbient[] AreaAmbients { get { return m_areaAmbients; } }

    [SerializeField]
    private SceneAreaPath[] m_areaPaths = new SceneAreaPath[0];
    public SceneAreaPath[] AreaPaths { get { return m_areaPaths; } }

    private Dictionary<string, SceneAreaAmbient> m_areaAmbientDict = new Dictionary<string, SceneAreaAmbient>();
    private Dictionary<string, SceneAreaPath> m_areaPathDict = new Dictionary<string, SceneAreaPath>();

    protected override void Awake()
    {
        base.Awake();

        InitAmbients();
        InitPaths();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            m_areaAmbients = this.transform.GetComponentsInChildren<SceneAreaAmbient>(true);
            for (int i = 0; i < m_areaAmbients.Length; ++i)
            {
                SceneAreaAmbient ambient = m_areaAmbients[i];
                ambient.Serialize();
            }

            m_areaPaths = this.transform.GetComponentsInChildren<SceneAreaPath>(true);
            for (int i = 0; i < m_areaPaths.Length; ++i)
            {
                SceneAreaPath path = m_areaPaths[i];
                path.Serialize();
            }
        }
        else
#endif
        {
            UpdateAmbients();
            UpdatePaths();
        }
    }

    public void SetTarget(Transform target)
    {
        m_target = target;
    }

    private void InitAmbients()
    {
        m_defaultAmbient.SetDataToScene();

        if (m_areaAmbientDict == null)
        {
            m_areaAmbientDict = new Dictionary<string, SceneAreaAmbient>();
        }
        else
        {
            m_areaAmbientDict.Clear();
        }

        for (int i = 0; i < m_areaAmbients.Length; ++i)
        {
            SceneAreaAmbient ambient = m_areaAmbients[i];
            ambient.Init();
            m_areaAmbientDict[ambient.name] = ambient;
        }
    }

    private void InitPaths()
    {
        if (m_areaPathDict == null)
        {
            m_areaPathDict = new Dictionary<string, SceneAreaPath>();
        }
        else
        {
            m_areaPathDict.Clear();
        }

        for (int i = 0; i < m_areaPaths.Length; ++i)
        {
            SceneAreaPath path = m_areaPaths[i];
            path.SetCameraTrans(m_cameraTrans);
            path.SetTargetTrans(m_target);
            m_areaPathDict[path.name] = path;
        }
    }

    private void UpdateAmbients()
    {
        if (m_target == null)
        {
            return;
        }

        for (int i = 0; i < m_areaAmbients.Length; ++i)
        {
            SceneAreaAmbient ambient = m_areaAmbients[i];
            ambient.Blend(m_target.position);
        }
    }

    private void UpdatePaths()
    {
        if (m_cameraTrans == null)
        {
            return;
        }

        Vector3 position = Vector3.zero;
        for (int i = 0; i < m_areaPaths.Length; ++i)
        {
            SceneAreaPath path = m_areaPaths[i];
            path.UpdateTargetTrans();
            //if(path.
            //Vector3 bestPosition = path.GetBestPoint(m_target.position);
            //path.UpdateTargetTrans(m_cameraTrans);

        }
    }

    public SceneAreaAmbient GetAreaAmbient(string name)
    {
        SceneAreaAmbient ambient = null;
        return m_areaAmbientDict.TryGetValue(name, out ambient) ? ambient : null;
    }

    public SceneAreaPath GetAreaPath(string name)
    {
        SceneAreaPath path = null;
        return m_areaPathDict.TryGetValue(name, out path) ? path : null;
    }
}
