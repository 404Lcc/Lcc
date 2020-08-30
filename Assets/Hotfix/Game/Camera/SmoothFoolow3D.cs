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
        //相机的距离比例
        public float proportion;
        public float speedX;
        public float speedY;
        public float angleX;
        public float angleY;
        public bool touch;
        public bool mouse;
        public bool needDamping;
        public bool touch1UI;
        public bool touch2UI;
        public Vector3 offset;
        public Transform target;
        void Awake()
        {
            distance = 3;
            damping = 6;
            limitYMin = -10;
            limitYMax = 35;
            proportion = 1;
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                speedX = 200;
                speedY = 200;
                touch = false;
                mouse = true;
            }
            else
            {
                speedX = 100;
                speedY = 100;
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
                    if (Joystack.instance.angle != 0)
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
                        touch1UI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                    }
                    if (Input.GetTouch(1).phase == TouchPhase.Began)
                    {
                        touch2UI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(1).fingerId);
                    }
                    if (touch1UI)
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
                    if (touch2UI)
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
                if (Joystack.instance.angle != 0)
                {
                    //第一根手指按在摇杆上
                }
                else
                {
                    //第一根手指按在屏幕上
                    if (Input.GetMouseButton(0))
                    {
                        ComputeAngle();
                    }
                }
            }
        }
        void LateUpdate()
        {
            if (target != null)
            {
                PreventThroughWall();
                Quaternion localRotation = Quaternion.Euler(angleY, angleX, 0);
                Vector3 localPosition = localRotation * new Vector3(0, 0, -distance) + target.localPosition + offset;
                if (needDamping)
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
                    angleX += Input.GetAxis("Mouse X") / Screen.dpi * speedX;
                    angleY -= Input.GetAxis("Mouse Y") / Screen.dpi * speedY;
                    angleX = ClampAngle(angleX);
                    angleY = Mathf.Clamp(angleY, limitYMin, limitYMax);
                }
            }
        }
        public void ComputeAngle(int index)
        {
            if (touch)
            {
                angleX += Input.GetTouch(index).deltaPosition.x / Screen.dpi * speedX;
                angleY -= Input.GetTouch(index).deltaPosition.y / Screen.dpi * speedY;
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
        public void PreventThroughWall()
        {
            //从目标点到相机
            Vector3 direction = (transform.localPosition - target.localPosition).normalized;
            Ray ray = new Ray(target.localPosition, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, distance, LayerMask.GetMask("Wall")))
            {
                if (hit.collider.tag.Equals("Wall"))
                {
                    proportion = Mathf.Min(1, Vector3.Distance(hit.point, target.localPosition) / distance);
                }
                else
                {
                    proportion = 1;
                }
            }
        }
    }
}