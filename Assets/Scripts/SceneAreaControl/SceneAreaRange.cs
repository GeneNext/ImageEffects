using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneAreaRange
{
    public enum eAreaType
    {
        Sphere = 0,
        Cylinder = 1,
        Cube = 2,
    }

    public interface iAreaRange
    {
        float GetBlendWeight(Transform trans, Vector3 position);

        bool CheckInRange(Transform trans, Vector3 position);

        void DrawGizmos(Transform trans, Color color1, Color color2);
    }

    [System.Serializable]
    public class SphereRange : iAreaRange
    {
        public float AreaRadius;
        public float BlendDistance;

        public float GetBlendWeight(Transform trans, Vector3 position)
        {
            float distance = Vector3.Distance(trans.position, position);
            if (distance > AreaRadius)
            {
                return 0;
            }

            float effectRange = AreaRadius - BlendDistance;
            float blendWeight = 1 - Mathf.Clamp01((distance - effectRange) / BlendDistance);

            return blendWeight;
        }

        public bool CheckInRange(Transform trans, Vector3 position)
        {
            float distance = Vector3.Distance(trans.position, position);
            return distance <= AreaRadius;
        }

        public void DrawGizmos(Transform trans, Color color1, Color color2)
        {
            Gizmos.matrix = trans.localToWorldMatrix;
            Gizmos.color = color1;
            Gizmos.DrawWireSphere(Vector3.zero, AreaRadius);
            Gizmos.color = color2;
            Gizmos.DrawWireSphere(Vector3.zero, AreaRadius - BlendDistance);
        }
    }

    [System.Serializable]
    public class CylinderRange : iAreaRange
    {
        public float AreaRadius;
        public float AreaHeight;
        public float BlendDistance;
        public bool IgnoreHeight;
        public Mesh GizmosMesh;

        public float GetBlendWeight(Transform trans, Vector3 position)
        {
            float distance = CalcDistanceV2(trans.position, position);
            if (distance > AreaRadius)
            {
                return 0;
            }

            float effectRange = AreaRadius - BlendDistance;
            float blendWeight = 1 - Mathf.Clamp01((distance - effectRange) / BlendDistance);

            float heightDistance = CalcDistanceHeight(trans.position, position);
            if (!IgnoreHeight && heightDistance > AreaHeight)
            {
                effectRange = AreaHeight * 2 - BlendDistance;
                blendWeight *= 1 - Mathf.Clamp01((heightDistance - effectRange) / BlendDistance);
            }

            return blendWeight;
        }

        private Vector2 ConvertToVector2(Vector3 v3)
        {
            return new Vector2(v3.x, v3.z);
        }

        private float CalcDistanceV2(Vector3 from, Vector3 to)
        {
            return Vector2.Distance(ConvertToVector2(from), ConvertToVector2(to));
        }

        private float CalcDistanceHeight(Vector3 from, Vector3 to)
        {
            return Mathf.Abs(from.y - to.y);
        }

        public bool CheckInRange(Transform trans, Vector3 position)
        {
            float distance = CalcDistanceV2(trans.position, position);
            float height = CalcDistanceHeight(trans.position, position);

            return distance <= AreaRadius && (IgnoreHeight || height <= AreaHeight);
        }

        public void DrawGizmos(Transform trans, Color color1, Color color2)
        {
            if (GizmosMesh == null)
            {
                return;
            }

            Gizmos.matrix = trans.localToWorldMatrix;
            Gizmos.color = color1;
            Gizmos.DrawWireMesh(GizmosMesh, Vector3.zero, Quaternion.identity, new Vector3(AreaRadius, AreaHeight, AreaRadius) * 2);
            Gizmos.color = color2;
            Gizmos.DrawWireMesh(GizmosMesh, Vector3.zero, Quaternion.identity, new Vector3(AreaRadius - BlendDistance, AreaHeight - BlendDistance * 0.5f, AreaRadius - BlendDistance) * 2);
        }
    }

    [System.Serializable]
    public class CubeRange : iAreaRange
    {
        public Vector3 AreaSize;//应为正数
        public float BlendDistance;

        public float GetBlendWeight(Transform trans, Vector3 position)
        {
            Matrix4x4 matrix = trans.worldToLocalMatrix;
            Vector3 center = trans.localPosition;
            Vector3 size = AreaSize * 0.5f;
            position = matrix.MultiplyPoint(position) + center;

            float blendWeight = 0;
            if (CheckInRange(center, size, position))
            {
                size = (AreaSize - Vector3.one * BlendDistance) * 0.5f;
                if (CheckInRange(center, size, position))
                {
                    blendWeight = 1;
                }
                else
                {
                    //最近边缘距离
                    float distanceX = Mathf.Max(Mathf.Abs(position.x - center.x) - size.x, 0);
                    float distanceY = Mathf.Max(Mathf.Abs(position.y - center.y) - size.y, 0);
                    float distanceZ = Mathf.Max(Mathf.Abs(position.z - center.z) - size.z, 0);

                    //影响范围
                    float effectRangeX = AreaSize.x * 0.5f - size.x;
                    float effectRangeY = AreaSize.y * 0.5f - size.y;
                    float effectRangeZ = AreaSize.z * 0.5f - size.z;

                    //混合
                    float blendX = 1 - distanceX / effectRangeX;
                    float blendY = 1 - distanceY / effectRangeY;
                    float blendZ = 1 - distanceZ / effectRangeZ;

                    blendWeight = blendX * blendY * blendZ;
                }
            }
            else
            {
                blendWeight = 0;
            }

            //Debug.LogError(blendWeight);
            return blendWeight;
        }

        private bool CheckInRange(Vector3 center, Vector3 size, Vector3 position)
        {
            float right = center.x + size.x;
            float left = center.x - size.x;
            float top = center.y + size.y;
            float bottom = center.y - size.y;
            float forward = center.z + size.z;
            float back = center.z - size.z;

            //true:position在范围内，false:position在范围外
            return (position.x > left && position.x < right && position.y > bottom && position.y < top && position.z > back && position.z < forward);
        }

        public bool CheckInRange(Transform trans, Vector3 position)
        {
            Matrix4x4 matrix = trans.worldToLocalMatrix;
            Vector3 center = trans.localPosition;
            Vector3 size = AreaSize * 0.5f;
            position = matrix.MultiplyPoint(position) + center;

            return CheckInRange(center, size, position);
        }

        public void DrawGizmos(Transform trans, Color color1, Color color2)
        {
            Gizmos.matrix = trans.localToWorldMatrix;
            Gizmos.color = color1;
            Gizmos.DrawWireCube(Vector3.zero, AreaSize);
            Gizmos.color = color2;
            Gizmos.DrawWireCube(Vector3.zero, AreaSize - Vector3.one * BlendDistance);
        }
    }
}
