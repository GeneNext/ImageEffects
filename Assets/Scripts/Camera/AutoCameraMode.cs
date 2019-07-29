using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutoCameraMode
{
    public class ModeData
    {
        private static Dictionary<eModeType, ModeData> m_modeDataDict = new Dictionary<eModeType, ModeData>();

        public static ModeData GetModeData(eModeType modeType)
        {
            ModeData mode = null;
            if (!m_modeDataDict.TryGetValue(modeType, out mode))
            {
                mode = new ModeData(modeType);
                m_modeDataDict.Add(modeType, mode);
            }
            return mode;
        }

        /// <summary>
        /// 相机模式
        /// </summary>
        public eModeType ModeType { get; set; }
        /// <summary>
        /// 遮挡检测层
        /// </summary>
        public LayerMask OcclusionMask { get; set; }
        /// <summary>
        /// 相机碰撞层
        /// </summary>
        public LayerMask CameraCollisionMask { get; set; }
        /// <summary>
        /// 可以旋转相机X轴
        /// </summary>
        public bool EnableAxisX { get; set; }
        /// <summary>
        /// 可以旋转相机Y轴
        /// </summary>
        public bool EnableAxisY { get; set; }

        public ModeData(eModeType modeType)
        {
            ModeType = modeType;

            switch (modeType)
            {
                case eModeType.Fixed:
                    InitModeData_Fixed();
                    break;
                case eModeType.SemiFixed:
                    InitModeData_SemiFixed();
                    break;
                case eModeType.Free:
                    InitModeData_Free();
                    break;
            }
        }

        /// <summary>
        /// 初始化相机数据——固定视角
        /// </summary>
        private void InitModeData_Fixed()
        {
            OcclusionMask = 0;
            CameraCollisionMask = 1 << LayerMask.NameToLayer("Ground");
            EnableAxisX = false;
            EnableAxisY = false;
        }

        /// <summary>
        /// 初始化相机数据——半固定视角
        /// </summary>
        private void InitModeData_SemiFixed()
        {
            OcclusionMask = 0;
            CameraCollisionMask = 1 << LayerMask.NameToLayer("Ground");
            EnableAxisX = false;
            EnableAxisY = true;
        }

        /// <summary>
        /// 初始化相机数据——自由视角
        /// </summary>
        private void InitModeData_Free()
        {
            OcclusionMask = 0;
            CameraCollisionMask = 1 << LayerMask.NameToLayer("Ground");
            EnableAxisX = true;
            EnableAxisY = true;
        }

        public enum eModeType
        {
            /// <summary>
            /// 固定视角，2D
            /// </summary>
            Fixed = 0,
            /// <summary>
            /// 半固定视角，2.5D
            /// </summary>
            SemiFixed,
            /// <summary>
            /// 自由视角，3D
            /// </summary>
            Free,
        }
    }
}