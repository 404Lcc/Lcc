using UnityEngine;
using UnityEngine.EventSystems;

namespace LccHotfix
{
    public class SmoothFoolow3D : AObjectBase
    {
        public GameObject gameObject => GetParent<GameObjectEntity>().gameObject;

        public float distance = 3;
        public float damping = 45;
        public float limitYMin = -10;
        public float limitYMax = 40;
        //相机的距离比例
        public float proportion = 1;
        public float speedX;
        public float speedY;
        public float angleX;
        public float angleY;
        public bool isNeedDamping = true;
        public bool isTouch;
        public bool isMouse;
        public bool isTouchRightUI;
        public Vector3 offset = new Vector3(0, 1.5f, 0);

        public Transform target;
        public Joystick joystick;
        public Transform lockTarget;
        public override void InitData(object[] datas)
        {
            distance = (float)datas[0];
            damping = (float)datas[1];
            limitYMin = (float)datas[2];
            limitYMax = (float)datas[3];
            proportion = (float)datas[4];
            isNeedDamping = (bool)datas[5];
            offset = (Vector3)datas[3];
            target = (Transform)datas[4];
            joystick = (Joystick)datas[5];
#if UNITY_EDITOR
            speedX = 200;
            speedY = 200;
            isTouch = false;
            isMouse = true;
#else
            speedX = 30;
            speedY = 30;
            isTouch = true;
            isMouse = false;
#endif
        }
        public override void Update()
        {
            if (isTouch)
            {
                PreventThroughWall();
                if (Input.touchCount == 1)
                {
                    //常规操作
                    if (joystick.isStandard)
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
                    if (joystick.isStandard)
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
                            isTouchRightUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(right).fingerId);
                        }
                        //如果最右边的手指是第一个手指
                        if (right == 0)
                        {
                            if (isTouchRightUI)
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
                            if (isTouchRightUI)
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
            if (isMouse)
            {
                PreventThroughWall();
                //常规操作
                if (joystick.isStandard)
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
        public override void LateUpdate()
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
                if (isNeedDamping)
                {
                    gameObject.transform.localRotation = Quaternion.Lerp(gameObject.transform.localRotation, localRotation, Time.deltaTime * damping);
                    gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, localPosition, Time.deltaTime * damping);
                }
                else
                {
                    gameObject.transform.localRotation = localRotation;
                    gameObject.transform.localPosition = localPosition;
                }
            }
        }


        public void SetTarget(Transform target)
        {
            this.target = target;
        }
        public void SetLockTarget(Transform lockTarget)
        {
            this.lockTarget = lockTarget;
        }


        private void ComputeAngle()
        {
            if (isMouse)
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
        private void ComputeAngle(int index)
        {
            if (isTouch)
            {
                angleX += Input.GetTouch(index).deltaPosition.x / Screen.dpi * speedX;
                angleY -= Input.GetTouch(index).deltaPosition.y / Screen.dpi * speedY;
                angleX = ClampAngle(angleX);
                angleY = Mathf.Clamp(angleY, limitYMin, limitYMax);
            }
        }
        private float ClampAngle(float angle)
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
        private void PreventThroughWall()
        {
            //从目标点到相机
            Vector3 direction = (gameObject.transform.localPosition - target.localPosition).normalized;
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