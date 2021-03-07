using LccModel;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LccHotfix
{
    public class Joystack : AObjectBase
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

        public RectTransform joystack;
        public RectTransform joystackBG;
        public void InitJoystack(float maxDistance, float activeDistance, RectTransform joystack, RectTransform joystackBG)
        {
            this.maxDistance = maxDistance;
            this.activeDistance = activeDistance;
            this.joystack = joystack;
            this.joystackBG = joystackBG;
            //设置原点
            origin = joystack.localPosition;
            DragEventTrigger.GetDragEventTrigger(gameObject).Down += OnDown;
            DragEventTrigger.GetDragEventTrigger(gameObject).UP += OnUp;
            DragEventTrigger.GetDragEventTrigger(gameObject).Drag += OnDrag;
            joystack.gameObject.SetActive(false);
            joystackBG.gameObject.SetActive(false);
        }
        public void JoystackDistance()
        {
            //根据触摸位置算出来的拖动距离
            currentDistance = Vector3.Distance(joystack.localPosition, origin);
            //距离大于最大拖动距离
            if (currentDistance >= maxDistance)
            {
                //求圆上的一点 (目标点-原点) * 半径 / 原点到目标点的距离
                joystack.localPosition = origin + (joystack.localPosition - origin) * maxDistance / currentDistance;
            }
            //距离大于激活移动的最低距离
            if (currentDistance >= activeDistance)
            {
                normalDistance = (joystack.localPosition - origin).normalized;
            }
            else
            {
                normalDistance = Vector2.zero;
            }
            angle = Mathf.Atan2(normalDistance.x, normalDistance.y) * Mathf.Rad2Deg;
        }
        public void OnDown(PointerEventData eventData)
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
                if (Input.GetTouch(left).position == eventData.position)
                {
                    isStandard = true;
                }
            }
            joystack.gameObject.SetActive(true);
            joystackBG.gameObject.SetActive(true);
            joystackBG.localPosition = eventData.position.ScreenToUGUI((RectTransform)transform);
            JoystackDistance();
        }
        public void OnUp(PointerEventData eventData)
        {
            isStandard = false;
            joystack.gameObject.SetActive(false);
            joystackBG.gameObject.SetActive(false);
            joystack.localPosition = origin;
            JoystackDistance();
        }
        public void OnDrag(PointerEventData eventData)
        {
            joystack.localPosition = eventData.position.ScreenToUGUI((RectTransform)transform);
            JoystackDistance();
        }
    }
}