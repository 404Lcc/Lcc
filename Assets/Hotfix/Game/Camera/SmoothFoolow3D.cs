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
        public bool needDamping;
        public bool touch;
        public bool mouse;
        public bool touchRightUI;
        public Vector3 offset;

        public Transform target;
        public Transform lockTarget;
        void Awake()
        {
            InitSmoothFoolow();
        }
        void Start()
        {
        }
        void Update()
        {
            if (touch)
            {
                PreventThroughWall();
                if (Input.touchCount == 1)
                {
                    //常规操作
                    if (Joystack.instance.standard)
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
                    //常规操作
                    if (Joystack.instance.standard)
                    {
                        //找到最右边的手指
                        int right;
                        float point1 = Input.GetTouch(0).position.x;
                        float point2 = Input.GetTouch(1).position.x;
                        if (point1 > point2)
                        {
                            right = 0;
                        }
                        else
                        {
                            right = 1;
                        }
                        //第二根手指第一次触摸屏幕时 判断最右边手指是否在UI上
                        if (Input.GetTouch(1).phase == TouchPhase.Began)
                        {
                            touchRightUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(right).fingerId);
                        }
                        //如果最右边的手指是第一个手指
                        if (right == 0)
                        {
                            if (touchRightUI)
                            {
                                //第一根手指按在UI上
                            }
                            else
                            {
                                //第一根手指按在屏幕上
                                if (Input.GetTouch(right).phase == TouchPhase.Moved)
                                {
                                    ComputeAngle(right);
                                }

                            }
                        }
                        //如果最右边的手指是第二个手指
                        if (right == 1)
                        {
                            if (touchRightUI)
                            {
                                //第二根手指按在UI上
                            }
                            else
                            {
                                //第二根手指按在屏幕上
                                if (Input.GetTouch(right).phase == TouchPhase.Moved)
                                {
                                    ComputeAngle(right);
                                }
                            }
                        }
                    }
                    else
                    {
                        //非常规操作
                        ComputeAngle(1);
                    }
                }
            }
            if (mouse)
            {
                PreventThroughWall();
                //常规操作
                if (Joystack.instance.standard)
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
                Quaternion localRotation;
                if (lockTarget != null)
                {
                    localRotation = Quaternion.LookRotation((lockTarget.localPosition - target.localPosition).normalized);
                    angleY = localRotation.eulerAngles.x;
                    angleX = localRotation.eulerAngles.y;
                }
                else
                {
                    localRotation = Quaternion.Euler(angleY, angleX, 0);
                }
                Vector3 localPosition = localRotation * new Vector3(0, 0, -distance * proportion) + target.localPosition + offset;
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
        public void InitSmoothFoolow()
        {
            distance = 3;
            damping = 45;
            limitYMin = -10;
            limitYMax = 40;
            proportion = 1;
            needDamping = true;
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                speedX = 200;
                speedY = 200;
                touch = false;
                mouse = true;
            }
            else
            {
                speedX = 30;
                speedY = 30;
                touch = true;
                mouse = false;
            }
            offset = new Vector3(0, 1.5f, 0);
        }
        public void InitSmoothFoolow(float distance, float damping, float limitYMin, float limitYMax, float proportion, bool needDamping, Vector3 offset, Transform target)
        {
            this.distance = distance;
            this.damping = damping;
            this.limitYMin = limitYMin;
            this.limitYMax = limitYMax;
            this.proportion = proportion;
            this.needDamping = needDamping;
            this.offset = offset;
            this.target = target;
        }
        public void SetTarget(Transform target)
        {
            this.target = target;
        }
        public void SetLockTarget(Transform lockTarget)
        {
            this.lockTarget = lockTarget;
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
                if (hit.collider != null)
                {
                    if (hit.collider.tag == "Wall")
                    {
                        proportion = Mathf.Min(1, Vector3.Distance(hit.point, target.localPosition) / distance);
                    }
                    else
                    {
                        proportion = 1;
                    }
                }
                else
                {
                    proportion = 1;
                }
            }
        }
    }
}