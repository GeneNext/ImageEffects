using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reflection : MonoBehaviour
{
    [SerializeField]
    private Camera m_mainCamera;

    [SerializeField]
    private Camera m_reflectCamera;

    [SerializeField]
    private LayerMask m_layerMask;

    [SerializeField]
    private Renderer m_renderer;

    [SerializeField]
    private float m_clipPlaneOffset = 0f;

    private RenderTexture m_reflectTexture;

    void OnEnable()
    {
        if (m_reflectTexture == null)
        {
            m_reflectTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
        }

        m_reflectCamera.CopyFrom(m_mainCamera);

        m_reflectCamera.clearFlags = CameraClearFlags.SolidColor;
        m_reflectCamera.backgroundColor = Color.black;

        m_reflectCamera.targetTexture = m_reflectTexture;
        m_reflectCamera.cullingMask = m_layerMask;

        m_renderer.material.SetTexture("_ReflectTex", m_reflectTexture);
    }

    void OnDisable()
    {
        if (m_reflectTexture != null)
        {
            m_reflectCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(m_reflectTexture);
            m_reflectTexture = null;
        }
    }

    void Update()
    {
        GL.invertCulling = true;

        Transform reflectiveSurface = this.transform; 

        Vector3 eulerA = m_mainCamera.transform.eulerAngles;

        m_reflectCamera.transform.eulerAngles = new Vector3(-eulerA.x, eulerA.y, eulerA.z);
        m_reflectCamera.transform.position = m_mainCamera.transform.position;

        Vector3 pos = reflectiveSurface.transform.position;
        pos.y = reflectiveSurface.position.y;
        Vector3 normal = reflectiveSurface.transform.up;
        float d = -Vector3.Dot(normal, pos) - m_clipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        reflection = CalculateReflectionMatrix(reflection, reflectionPlane);
        Vector3 newpos = reflection.MultiplyPoint(m_mainCamera.transform.position);

        m_reflectCamera.worldToCameraMatrix = m_mainCamera.worldToCameraMatrix * reflection;

        Vector4 clipPlane = CameraSpacePlane(m_reflectCamera, pos, normal, 1.0f);

        Matrix4x4 projection = m_mainCamera.projectionMatrix;
        projection = CalculateObliqueMatrix(projection, clipPlane);
        m_reflectCamera.projectionMatrix = projection;

        m_reflectCamera.transform.position = newpos;
        Vector3 euler = m_mainCamera.transform.eulerAngles;
        m_reflectCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);

        m_reflectCamera.Render();

        GL.invertCulling = false;
    }

    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * m_clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;

        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    private float Sgn(float a)
    {
        if (a > 0.0f)
        {
            return 1.0f;
        }
        if (a < 0.0f)
        {
            return -1.0f;
        }
        return 0.0f;
    }

    private Matrix4x4 CalculateObliqueMatrix(Matrix4x4 projection, Vector4 clipPlane)
    {
        Vector4 q = projection.inverse * new Vector4(Sgn(clipPlane.x), Sgn(clipPlane.y), 1.0f, 1.0f);
        Vector4 c = clipPlane * (2.0f / (Vector4.Dot(clipPlane, q)));
        projection[2] = c.x - projection[3];
        projection[6] = c.y - projection[7];
        projection[10] = c.z - projection[11];
        projection[14] = c.w - projection[15];

        return projection;
    }

    private Matrix4x4 CalculateReflectionMatrix(Matrix4x4 reflectionMatrix, Vector4 plane)
    {
        reflectionMatrix.m00 = (1f - 2f * plane[0] * plane[0]);
        reflectionMatrix.m01 = (-2f * plane[0] * plane[1]);
        reflectionMatrix.m02 = (-2f * plane[0] * plane[2]);
        reflectionMatrix.m03 = (-2f * plane[0] * plane[3]);

        reflectionMatrix.m10 = (-2f * plane[1] * plane[0]);
        reflectionMatrix.m11 = (1f - 2f * plane[1] * plane[1]);
        reflectionMatrix.m12 = (-2f * plane[1] * plane[2]);
        reflectionMatrix.m13 = (-2f * plane[1] * plane[3]);

        reflectionMatrix.m20 = (-2f * plane[2] * plane[0]);
        reflectionMatrix.m21 = (-2f * plane[2] * plane[1]);
        reflectionMatrix.m22 = (1f - 2f * plane[2] * plane[2]);
        reflectionMatrix.m23 = (-2f * plane[2] * plane[3]);

        reflectionMatrix.m30 = 0f;
        reflectionMatrix.m31 = 0f;
        reflectionMatrix.m32 = 0f;
        reflectionMatrix.m33 = 1f;

        return reflectionMatrix;
    }
}
