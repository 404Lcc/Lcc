using LccModel;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

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
        public float angle;
        //标准操作
        public bool isStandard;

        public GameObject joystick;
        public RectTransform joystickCenter;
        public RectTransform joystickBG;

        public Action<Vector2, float> dragHandler;
        public void InitJoystick(params object[] datas)
        {
            maxDistance = (float)datas[0];
            activeDistance = (float)datas[1];
            joystick = (GameObject)datas[2];
            joystickCenter = (RectTransform)joystick.transform.GetChild(1);
            joystickBG = (RectTransform)joystick.transform.GetChild(0);

            EventTriggerListener.Get(joystick).onDown += OnDown;
            EventTriggerListener.Get(joystick).onUP += OnUP;
            EventTriggerListener.Get(joystick).onDrag += OnDrag;
            joystickCenter.gameObject.SetActive(false);
            joystickBG.gameObject.SetActive(false);
        }
        public void Bind(Action<Vector2, float> dragHandler)
        {
            this.dragHandler = dragHandler;
        }
        public void JoystackDistance()
        {
            //根据触摸位置算出来的拖动距离
            currentDistance = Vector3.Distance(joystickCenter.localPosition, origin);
            //距离大于最大拖动距离
            if (currentDistance >= maxDistance)
            {
                //求圆上的一点 (目标点-原点) * 半径 / 原点到目标点的距离
                joystickCenter.localPosition = origin + (joystickCenter.localPosition - origin) * maxDistance / currentDistance;
            }
            //距离大于激活移动的最低距离
            if (currentDistance >= activeDistance)
            {
                normalDistance = (joystickCenter.localPosition - origin).normalized;
            }
            else
            {
                normalDistance = Vector2.zero;
            }
            angle = Mathf.Atan2(normalDistance.x, normalDistance.y) * Mathf.Rad2Deg;
        }
        public void OnDown(GameObject obj)
        {
            if (Input.touchCount == 1)
            {
                isStandard = true;
            }
            if (Input.touchCount == 2)
            {
                //找到最左边的手指
                int left;
                float point1 = Input.GetTouch(0).position.x;
                float point2 = Input.GetTouch(1).position.x;
                if (point1 < point2)
                {
                    left = 0;
                }
                else
                {
                    left = 1;
                }
                //如果最左边的手指等于摇杆触发位置的话就是常规操作 否则是非常规操作
                if (Input.GetTouch(left).position == (Vector2)Input.mousePosition)
                {
                    isStandard = true;
                }
            }
            joystickCenter.gameObject.SetActive(true);
            joystickBG.gameObject.SetActive(true);
            joystickCenter.localPosition = Input.mousePosition.ScreenToUGUI((RectTransform)joystick.transform);
            joystickBG.localPosition = Input.mousePosition.ScreenToUGUI((RectTransform)joystick.transform);
            origin = joystickCenter.localPosition;
            JoystackDistance();
            dragHandler?.Invoke(normalDistance, angle);
        }
        public void OnUP(GameObject obj)
        {
            isStandard = false;
            joystickCenter.gameObject.SetActive(false);
            joystickBG.gameObject.SetActive(false);
            JoystackDistance();
            normalDistance = Vector2.zero;
            angle = 0;
            dragHandler?.Invoke(normalDistance, angle);
        }
        public void OnDrag(GameObject obj)
        {
            joystickCenter.localPosition = Input.mousePosition.ScreenToUGUI((RectTransform)joystick.transform);
            JoystackDistance();
            dragHandler?.Invoke(normalDistance, angle);
        }

        public void Destroy()
        {
            EventTriggerListener.Get(joystick).onDown -= OnDown;
            EventTriggerListener.Get(joystick).onUP -= OnUP;
            EventTriggerListener.Get(joystick).onDrag -= OnDrag;
        }
    }
}