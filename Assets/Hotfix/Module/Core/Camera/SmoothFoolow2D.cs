using UnityEngine;

namespace LccHotfix
{
    public class SmoothFoolow2D : AObjectBase
    {
        public int smooth = 1;
        public bool isFollow = true;
        public Vector2 margin;
        public BoxCollider2D box;
        public Vector3 min;
        public Vector3 max;
        public Vector2 half;

        public Transform target;
        public void InitSmoothFoolow(int smooth, bool isFollow, Vector2 margin, BoxCollider2D box, Vector3 min, Vector3 max, Transform target)
        {
            this.smooth = smooth;
            this.isFollow = isFollow;
            this.margin = margin;
            this.box = box;
            //边界左下角
            this.min = min;
            //边界右上角
            this.max = max;
            this.target = target;
        }
        public override void Update()
        {
            half.x = Camera.main.orthographicSize * Screen.width / Screen.height;
            half.y = Camera.main.orthographicSize;
        }
        public override void LateUpdate()
        {
            if (isFollow)
            {
                Vector3 localPosition = transform.localPosition;
                if (Mathf.Abs(localPosition.x - target.localPosition.x) > margin.x)
                {
                    localPosition.x = Mathf.Lerp(localPosition.x, target.localPosition.x, smooth * Time.deltaTime);
                }
                if (Mathf.Abs(localPosition.y - target.localPosition.y) > margin.y)
                {
                    localPosition.y = Mathf.Lerp(localPosition.y, target.localPosition.y, smooth * Time.deltaTime);
                }
                localPosition.x = Mathf.Clamp(localPosition.x, min.x + half.x, max.x - half.x);
                localPosition.y = Mathf.Clamp(localPosition.y, min.y + half.y, max.y - half.y);
                transform.localPosition = localPosition;
            }
        }
        public void SetTarget(Transform target)
        {
            this.target = target;
        }
    }
}