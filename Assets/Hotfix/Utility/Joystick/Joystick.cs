using LccModel;
using System;
using UnityEngine;

namespace LccHotfix
{
    public class Joystick
    {
        //原始坐标
        public Vector3 origin;

        //最大拖动距离
        public float maxDistance;

        //激活移动的最低距离
        public float activeDistance;

        //当前距离
        public float currentDistance;

        //标准化移动的距离
        public Vector2 normalDistance;

        //角度
        public float angle;

        //追踪控制手指ID
        public int? activeFingerId;

        public Camera uiCamera;
        public GameObject joystick;
        public RectTransform joystickCenter;
        public RectTransform joystickBG;

        public Action<Vector2, float, float> dragHandler;

        public void InitJoystick(float maxDistance, float activeDistance, Camera uiCamera, GameObject joystick)
        {
            this.maxDistance = maxDistance;
            this.activeDistance = activeDistance;
            this.uiCamera = uiCamera;
            this.joystick = joystick;
            joystickCenter = (RectTransform)joystick.transform.GetChild(1);
            joystickBG = (RectTransform)joystick.transform.GetChild(0);

            EventTriggerListener.Get(joystick).onDown += OnDown;
            EventTriggerListener.Get(joystick).onUP += OnUP;
            EventTriggerListener.Get(joystick).onDrag += OnDrag;
            joystickCenter.gameObject.SetActive(false);
            joystickBG.gameObject.SetActive(false);
        }

        public void Bind(Action<Vector2, float, float> dragHandler)
        {
            //参数1 当前方向
            //参数2 当前角度
            //参数3 当前距离
            this.dragHandler = dragHandler;
        }

        public void OnDown(GameObject obj)
        {
            // 检查是否已有活动触摸
            if (activeFingerId != null)
                return;

            // 寻找在摇杆区域内的首个触摸点
            foreach (Touch touch in Input.touches)
            {
                // 检查触摸点是否在摇杆区域内
                Vector2 localPoint;
                RectTransform rect = (RectTransform)joystick.transform;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, touch.position, uiCamera, out localPoint))
                {
                    if (!rect.rect.Contains(localPoint))
                        continue;

                    // 绑定触摸点
                    activeFingerId = touch.fingerId;
                    InitializeJoystickPosition(localPoint);
                    return;
                }
            }

            // 处理鼠标事件（当没有触摸时）
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 localPoint;
                RectTransform rect = (RectTransform)joystick.transform;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, uiCamera, out localPoint))
                {
                    if (rect.rect.Contains(localPoint))
                    {
                        // 使用特殊ID标记鼠标
                        activeFingerId = -1;
                        InitializeJoystickPosition(localPoint);
                    }
                }
            }
        }

        public void OnUP(GameObject obj)
        {
            if (activeFingerId == null)
                return;

            // 鼠标释放需要额外检查按钮状态
            if (activeFingerId == -1 && !Input.GetMouseButtonUp(0))
                return;

            ResetJoystick();
        }

        public void OnDrag(GameObject obj)
        {
            if (activeFingerId == null)
                return;

            // 处理触摸拖动
            if (activeFingerId >= 0)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touch.fingerId == activeFingerId)
                    {
                        UpdateJoystickPosition(touch.position);
                        return;
                    }
                }
            }
            // 处理鼠标拖动
            else if (activeFingerId == -1 && Input.GetMouseButton(0))
            {
                UpdateJoystickPosition(Input.mousePosition);
            }
            else
            {
                // 输入丢失时释放摇杆
                OnUP(obj);
            }
        }

        /// <summary>
        /// 初始化摇杆位置
        /// </summary>
        /// <param name="localPoint"></param>
        private void InitializeJoystickPosition(Vector2 localPoint)
        {
            joystickCenter.gameObject.SetActive(true);
            joystickBG.gameObject.SetActive(true);
            joystickCenter.localPosition = localPoint;
            joystickBG.localPosition = localPoint;
            origin = joystickCenter.localPosition;

            UpdateJoystickDistance();
            dragHandler?.Invoke(normalDistance, angle, currentDistance);
        }

        /// <summary>
        /// 更新摇杆位置
        /// </summary>
        /// <param name="screenPosition"></param>
        private void UpdateJoystickPosition(Vector2 screenPosition)
        {
            Vector2 localPoint;
            RectTransform rect = (RectTransform)joystick.transform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPosition, uiCamera, out localPoint))
            {
                joystickCenter.localPosition = localPoint;
                UpdateJoystickDistance();
                dragHandler?.Invoke(normalDistance, angle, currentDistance);
            }
        }

        /// <summary>
        /// 更新摇杆方向
        /// </summary>
        private void UpdateJoystickDistance()
        {
            //根据触摸位置算出来的拖动距离
            currentDistance = Vector3.Distance(joystickCenter.localPosition, origin);
            //距离大于最大拖动距离
            if (currentDistance >= maxDistance)
            {
                //求圆上的一点 (目标点-原点) * 半径 / 原点到目标点的距离
                joystickCenter.localPosition = origin + (joystickCenter.localPosition - origin) * maxDistance / currentDistance;
                currentDistance = maxDistance;
            }

            //距离大于激活移动的最低距离
            if (currentDistance >= activeDistance)
            {
                Vector2 delta = joystickCenter.localPosition - origin;
                normalDistance = delta.sqrMagnitude > 0.001f ? delta.normalized : Vector2.zero;
            }
            else
            {
                normalDistance = Vector2.zero;
            }

            if (normalDistance != Vector2.zero)
            {
                angle = -(Mathf.Atan2(normalDistance.y, normalDistance.x) * Mathf.Rad2Deg - 90);
            }
            else
            {
                angle = 0;
            }
        }

        /// <summary>
        /// 重置摇杆位置
        /// </summary>
        private void ResetJoystick()
        {
            joystickCenter.gameObject.SetActive(false);
            joystickBG.gameObject.SetActive(false);
            activeFingerId = null;
            currentDistance = 0;
            normalDistance = Vector2.zero;
            angle = 0;
            dragHandler?.Invoke(normalDistance, angle, currentDistance);
        }

        public void Destroy()
        {
            activeFingerId = null;
            if (joystick == null)
                return;
            EventTriggerListener.Get(joystick).onDown -= OnDown;
            EventTriggerListener.Get(joystick).onUP -= OnUP;
            EventTriggerListener.Get(joystick).onDrag -= OnDrag;
        }
    }
}