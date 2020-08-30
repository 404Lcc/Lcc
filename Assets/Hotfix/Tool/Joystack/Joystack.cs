using Model;
using UnityEngine;

namespace Hotfix
{
    public class Joystack : MonoBehaviour
    {
        public static Joystack instance;
        //最大拖动距离
        public float maxDistance;
        //激活移动的最低距离
        public float activeDistance;
        public Vector3 origin;
        public float distance;
        //标准化移动的距离
        public Vector2 normalDistance;
        public bool drag;
        public float angle;
        void Awake()
        {
            instance = this;
            maxDistance = 120;
            activeDistance = 5;
            //设置原点
            origin = transform.localPosition;
            Drag.GetDrag(gameObject).beginDrag = OnBeginDrag;
            Drag.GetDrag(gameObject).drag = OnDrag;
            Drag.GetDrag(gameObject).endDrag = OnEndDrag;
        }
        void Start()
        {
        }
        void Update()
        {
            //根据触摸位置算出来的拖动距离
            distance = Vector3.Distance(transform.localPosition, origin);
            //距离大于最大拖动距离
            if (distance >= maxDistance)
            {
                //求圆上的一点 (目标点-原点) * 半径 / 原点到目标点的距离
                transform.localPosition = origin + (transform.localPosition - origin) * maxDistance / distance;
            }
            //距离大于激活移动的最低距离
            if (distance >= activeDistance)
            {
                normalDistance = (transform.localPosition - origin).normalized;
            }
            else
            {
                normalDistance = Vector2.zero;
            }
            angle = Mathf.Atan2(normalDistance.x, normalDistance.y) * Mathf.Rad2Deg;
        }
        public void OnBeginDrag()
        {
        }
        public void OnDrag(Vector2 delta)
        {
            if (!drag)
            {
                drag = true;
            }
            transform.localPosition += new Vector3(delta.x, delta.y, 0);
        }
        public void OnEndDrag()
        {
            drag = false;
            transform.localPosition = origin;
        }
    }
}