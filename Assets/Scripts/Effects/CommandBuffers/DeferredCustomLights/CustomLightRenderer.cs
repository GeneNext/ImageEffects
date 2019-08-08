using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

// See _ReadMe.txt

public class CustomLightSystem
{
    static CustomLightSystem m_Instance;
    static public CustomLightSystem instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = new CustomLightSystem();
            return m_Instance;
        }
    }

    internal HashSet<CustomLight> Lights = new HashSet<CustomLight>();

    public void Add(CustomLight o)
    {
        Remove(o);
        Lights.Add(o);
    }
    public void Remove(CustomLight o)
    {
        Lights.Remove(o);
    }
}


[ExecuteInEditMode]
public class CustomLightRenderer : MonoBehaviour
{
    [SerializeField]
    private Shader m_lightShader;
    [SerializeField]
    private Mesh m_cubeMesh;
    [SerializeField]
    private Mesh m_sphereMesh;
    [SerializeField]
    private Mesh m_capsuleMesh;

    private Material m_lightMaterial;

    // We'll be adding two command buffers to each camera:
    // - one to calculate illumination from lights (after regular lighting)
    // - another to draw light "objects" themselves (before transparencies are rendered)
    private struct CommanddBufferEntry
    {
        public CommandBuffer BeforeImageEffectsOpaque;
        public CommandBuffer BeforeForwardAlpha;
    }

    private Dictionary<Camera, CommanddBufferEntry> m_commandBuffersDict = new Dictionary<Camera, CommanddBufferEntry>();


    public void OnDisable()
    {
        foreach (var pair in m_commandBuffersDict)
        {
            Camera camera = pair.Key;
            CommanddBufferEntry buffer = pair.Value;
            if (camera)
            {
                camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, buffer.BeforeImageEffectsOpaque);
                camera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, buffer.BeforeForwardAlpha);
            }
        }
        Object.DestroyImmediate(m_lightMaterial);
    }


    public void OnWillRenderObject()
    {
        var act = gameObject.activeInHierarchy && enabled;
        if (!act)
        {
            OnDisable();
            return;
        }

        var cam = Camera.current;
        if (!cam)
            return;

        // create material used to render lights
        if (!m_lightMaterial)
        {
            m_lightMaterial = new Material(m_lightShader);
            m_lightMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        CommanddBufferEntry buf = new CommanddBufferEntry();
        if (m_commandBuffersDict.ContainsKey(cam))
        {
            // use existing command buffers: clear them
            buf = m_commandBuffersDict[cam];
            buf.BeforeImageEffectsOpaque.Clear();
            buf.BeforeForwardAlpha.Clear();
        }
        else
        {
            // create new command buffers
            buf.BeforeImageEffectsOpaque = new CommandBuffer();
            buf.BeforeImageEffectsOpaque.name = "Deferred custom lights";
            buf.BeforeForwardAlpha = new CommandBuffer();
            buf.BeforeForwardAlpha.name = "Draw light shapes";
            m_commandBuffersDict[cam] = buf;

            cam.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, buf.BeforeImageEffectsOpaque);
            cam.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, buf.BeforeForwardAlpha);
        }

        //@TODO: in a real system should cull lights, and possibly only
        // recreate the command buffer when something has changed.

        var system = CustomLightSystem.instance;

        var propParams = Shader.PropertyToID("_CustomLightParams");
        var propColor = Shader.PropertyToID("_CustomLightColor");
        Vector4 param = Vector4.zero;
        Matrix4x4 trs = Matrix4x4.identity;

        // construct command buffer to draw lights and compute illumination on the scene
        foreach (var light in system.Lights)
        {
            // light parameters we'll use in the shader
            param.x = light.TubeLength;
            param.y = light.Size;
            param.z = 1.0f / (light.Range * light.Range);
            param.w = (float)light.LightType;
            buf.BeforeImageEffectsOpaque.SetGlobalVector(propParams, param);
            // light color
            buf.BeforeImageEffectsOpaque.SetGlobalColor(propColor, light.GetLinearColor());

            // draw sphere that covers light area, with shader
            // pass that computes illumination on the scene
            trs = Matrix4x4.TRS(light.transform.position, light.transform.rotation, Vector3.one * light.Range * 2);
            buf.BeforeImageEffectsOpaque.DrawMesh(m_sphereMesh, trs, m_lightMaterial, 0, 0);
        }

        // construct buffer to draw light shapes themselves as simple objects in the scene
        foreach (var light in system.Lights)
        {
            // light color
            buf.BeforeForwardAlpha.SetGlobalColor(propColor, light.GetLinearColor());

            // draw light "shape" itself as a small sphere/tube
            if (light.LightType == CustomLight.eLightType.Sphere)
            {
                trs = Matrix4x4.TRS(light.transform.position, light.transform.rotation, Vector3.one * light.Size * 2);
                buf.BeforeForwardAlpha.DrawMesh(m_sphereMesh, trs, m_lightMaterial, 0, 1);
            }
            else if (light.LightType == CustomLight.eLightType.Tube)
            {
                trs = Matrix4x4.TRS(light.transform.position, light.transform.rotation, new Vector3(light.TubeLength * 2, light.Size * 2, light.Size * 2));
                buf.BeforeForwardAlpha.DrawMesh(m_cubeMesh, trs, m_lightMaterial, 0, 1);
            }
            else if (light.LightType == CustomLight.eLightType.Capsule)
            {
                trs = Matrix4x4.TRS(light.transform.position, light.transform.rotation, new Vector3(light.Size * 2, light.TubeLength * 2, light.Size * 2));
                buf.BeforeForwardAlpha.DrawMesh(m_capsuleMesh, trs, m_lightMaterial, 0, 1);
            }
        }
    }
}
