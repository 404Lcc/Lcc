using UnityEngine;
using UnityEngine.EventSystems;

namespace Hotfix
{
    public class SmoothFoolow3D : MonoBehaviour
    {
        public float distance;
        public float damping;
        public float limitYMin;
        public float limitYMax;
        public float speedX;
        public float speedY;
        public float angleX;
        public float angleY;
        public bool touch;
        public bool mouse;
        public bool needdamping;
        public bool touch1ui;
        public bool touch2ui;
        public Vector3 offset;
        public Transform target;
        void Awake()
        {
            distance = 3;
            damping = 6;
            limitYMin = -10;
            limitYMax = 35;
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                speedX = 1.5f;
                speedY = 1.5f;
                touch = false;
                mouse = true;
            }
            else
            {
                speedX = 0.3f;
                speedY = 0.3f;
                touch = true;
                mouse = false;
            }
        }
        void Start()
        {
        }
        void Update()
        {
            if (touch)
            {
                if (Input.touchCount == 1)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        touch1ui = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                    }
                    if (touch1ui)
                    {
                        //第一根手指按在摇杆上
                    }
                    else
                    {
                        //第一根手指按在屏幕上
                        if (Input.GetTouch(0).phase == TouchPhase.Moved)
                        {
                            ComputeAngle(0);
                        }
                    }
                }
                if (Input.touchCount == 2)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        touch1ui = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                    }
                    if (Input.GetTouch(1).phase == TouchPhase.Began)
                    {
                        touch2ui = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(1).fingerId);
                    }
                    if (touch1ui)
                    {
                        //第一根手指按在摇杆上
                    }
                    else
                    {
                        //第一根手指按在屏幕上
                        if (Input.GetTouch(0).phase == TouchPhase.Moved)
                        {
                            ComputeAngle(0);
                        }

                    }
                    if (touch2ui)
                    {
                        //第二根手指按在摇杆上
                    }
                    else
                    {
                        //第二根手指按在屏幕上
                        if (Input.GetTouch(1).phase == TouchPhase.Moved)
                        {
                            ComputeAngle(1);
                        }
                    }
                }
            }
            if (mouse)
            {
                ComputeAngle();
            }
        }
        void LateUpdate()
        {
            if (target != null)
            {
                Quaternion localRotation = Quaternion.Euler(angleY, angleX, 0);
                Vector3 localPosition = localRotation * new Vector3(0, 0, -distance) + target.localPosition + offset;
                if (needdamping)
                {
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, localRotation, Time.deltaTime * damping);
                    transform.localPosition = Vector3.Lerp(transform.localPosition, localPosition, Time.deltaTime * damping);
                }
                else
                {
                    transform.localRotation = localRotation;
                    transform.localPosition = localPosition;
                }
            }
        }
        public void ComputeAngle()
        {
            if (mouse)
            {
                if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
                {
                    angleX += Input.GetAxis("Mouse X") * speedX * Screen.dpi;
                    angleY -= Input.GetAxis("Mouse Y") * speedY * Screen.dpi;
                    angleX = ClampAngle(angleX);
                    angleY = Mathf.Clamp(angleY, limitYMin, limitYMax);
                }
            }
        }
        public void ComputeAngle(int index)
        {
            if (touch)
            {
                angleX += Input.GetTouch(index).deltaPosition.x * speedX * Screen.dpi;
                angleY -= Input.GetTouch(index).deltaPosition.y * speedY * Screen.dpi;
                angleX = ClampAngle(angleX);
                angleY = Mathf.Clamp(angleY, limitYMin, limitYMax);
            }
        }
        public float ClampAngle(float angle)
        {
            if (angle < -360)
            {
                angle += 360;
            }
            if (angle > 360)
            {
                angle -= 360;
            }
            return angle;
        }
    }
}