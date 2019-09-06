using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutoCameraInput
{
    public class InputData
    {
        protected Vector3 DefaultScreenPoint
        {
            get { return Vector3.one * 99999; }
        }

        protected AutoCamera m_autoCamera;

        protected bool m_isPointerOverUI;

        private Vector3 m_lastViewportPoint;

        public LayerMask TargetLayer;

        public bool CanSelectTarget;

        public InputData(AutoCamera camera)
        {
            m_autoCamera = camera;

            m_lastViewportPoint = DefaultScreenPoint;
        }

        public virtual void Update()
        {

        }

        protected void CheckPointerOverUI()
        {
            UnityEngine.EventSystems.EventSystem system = UnityEngine.EventSystems.EventSystem.current;
            if (system == null)
            {
                m_isPointerOverUI = false;
                return;
            }
            m_isPointerOverUI = system.IsPointerOverGameObject();
            for (int i = 0; i < Input.touchCount; ++i)
            {
                UnityEngine.EventSystems.PointerEventData eventData = new UnityEngine.EventSystems.PointerEventData(system);
                eventData.position = Input.GetTouch(i).position;
                List<UnityEngine.EventSystems.RaycastResult> resultList = new List<UnityEngine.EventSystems.RaycastResult>();
                system.RaycastAll(eventData, resultList);
                m_isPointerOverUI |= resultList.Count != 0;
            }
        }

        protected void CheckTarget(Vector3 position)
        {
            if (!CanSelectTarget)
            {
                return;
            }
            if (m_isPointerOverUI)
            {
                return;
            }
            Ray ray = m_autoCamera.GetCamera().ScreenPointToRay(position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 999, TargetLayer))
            {
                m_autoCamera.SetTarget(hit.transform);
            }

            if (WeatherSystem.WeatherManager.MoveControl != null)
            {
                WeatherSystem.WeatherManager.MoveControl.MoveByScreenPoint(position);
            }
        }

        protected void Rotate(Vector3 viewportPoint)
        {
            if (m_lastViewportPoint != DefaultScreenPoint)
            {
                m_autoCamera.Rotate(m_lastViewportPoint - viewportPoint);
            }
            m_lastViewportPoint = viewportPoint;
        }

        protected void Zoom(float deltaDistance)
        {
            m_autoCamera.Zoom(deltaDistance);
        }

        protected void ResetRotate()
        {
            m_lastViewportPoint = DefaultScreenPoint;
        }
    }

    public class InputData_PC : InputData
    {
        private eButtonType m_buttonType;

        public InputData_PC(AutoCamera camera)
            : base(camera)
        {
            m_buttonType = eButtonType.None;
        }

        public override void Update()
        {
            base.Update();

            CheckMouseButton();

            if (m_isPointerOverUI)
            {
                return;
            }

            CheckScrollButton();

            switch (m_buttonType)
            {
                case eButtonType.Button0:
                    Rotate(m_autoCamera.GetCamera().ScreenToViewportPoint(Input.mousePosition));
                    break;
                case eButtonType.Button1:
                    Rotate(m_autoCamera.GetCamera().ScreenToViewportPoint(Input.mousePosition));
                    break;
            }
        }

        private void CheckScrollButton()
        {
            float deltaDistance = Input.GetAxis("Mouse ScrollWheel");
            Zoom(deltaDistance);
        }

        private float m_lastTouchTime;
        private void CheckMouseButton()
        {
            if (Input.GetMouseButton(1))
            {
                ChangeButtonType(eButtonType.Button1);
            }
            else if (Input.GetMouseButton(0))
            {
                ChangeButtonType(eButtonType.Button0);
            }
            else
            {
                ChangeButtonType(eButtonType.None);
            }

            if (Input.GetMouseButtonDown(0))
            {
                m_lastTouchTime = Time.realtimeSinceStartup;
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (Mathf.Abs(Time.realtimeSinceStartup - m_lastTouchTime) < 0.2f)
                {
                    CheckTarget(Input.mousePosition);
                    m_lastTouchTime = 0;
                }
            }
        }

        private void ChangeButtonType(eButtonType buttonType)
        {
            if (m_buttonType == buttonType)
            {
                return;
            }

            m_buttonType = buttonType;

            CheckPointerOverUI();

            ResetRotate();
        }

        private enum eButtonType
        {
            None,
            Button0,
            Button1,
        }
    }

    public class InputData_Mobile : InputData
    {
        private eTouchType m_touchType;
        private float m_lastFingerDistance;
        private const float DefaultFingerDistance = 0;

        public InputData_Mobile(AutoCamera camera)
            : base(camera)
        {
            ResetFingerDistance();
            m_touchType = eTouchType.None;
        }

        public override void Update()
        {
            base.Update();

            CheckTouch();

            if (m_isPointerOverUI)
            {
                return;
            }

            switch (m_touchType)
            {
                case eTouchType.Rotate:
                    Rotate(m_autoCamera.GetCamera().ScreenToViewportPoint(Input.GetTouch(0).position));
                    break;
                case eTouchType.Zoom:
                    Zoom();
                    break;
            }
        }

        private void Zoom()
        {
            Vector3 point1 = m_autoCamera.GetCamera().ScreenToViewportPoint(Input.GetTouch(0).position);
            Vector3 point2 = m_autoCamera.GetCamera().ScreenToViewportPoint(Input.GetTouch(1).position);

            float fingerDistance = Vector3.Distance(point1, point2) * 5;
            if (m_lastFingerDistance != DefaultFingerDistance)
            {
                m_autoCamera.Zoom(fingerDistance - m_lastFingerDistance);
            }
            m_lastFingerDistance = fingerDistance;

            Rotate(Vector3.Lerp(point1, point2, 0.5f));
        }

        private float m_lastTouchTime;
        private void CheckTouch()
        {
            if (Input.touchCount == 1)
            {
                ChangeTouchType(eTouchType.Rotate);
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    m_lastTouchTime = Time.realtimeSinceStartup;
                }
                if (touch.phase == TouchPhase.Ended)
                {
                    if (Mathf.Abs(Time.realtimeSinceStartup - m_lastTouchTime) < 0.2f)
                    {
                        CheckTarget(touch.position);
                        m_lastTouchTime = 0;
                    }
                }
            }
            else if (Input.touchCount == 2)
            {
                ChangeTouchType(eTouchType.Zoom);
            }
            else
            {
                ChangeTouchType(eTouchType.None);
            }
        }

        private void ChangeTouchType(eTouchType touchPhase)
        {
            if (m_touchType == touchPhase)
            {
                return;
            }

            m_touchType = touchPhase;

            CheckPointerOverUI();

            ResetRotate();
            ResetFingerDistance();
        }

        private void ResetFingerDistance()
        {
            m_lastFingerDistance = DefaultFingerDistance;
        }

        private enum eTouchType
        {
            None,
            Rotate,
            Zoom,
        }
    }
}