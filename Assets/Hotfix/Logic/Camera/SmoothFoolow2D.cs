using UnityEngine;

namespace LccHotfix
{
    public class SmoothFoolow2D : AObjectBase, IUpdate, ILateUpdate
    {
        public int smooth = 1;
        public bool isFollow = true;
        public Vector2 margin;
        public BoxCollider2D box;
        public Vector3 min;
        public Vector3 max;
        public Vector2 half;

        public Transform target;
        public Camera camera;
        public override void InitData(object[] datas)
        {
            smooth = (int)datas[0];
            isFollow = (bool)datas[1];
            margin = (Vector2)datas[2];
            //边界左下角
            min = (Vector3)datas[3];
            //边界右上角
            max = (Vector3)datas[4];
            target = (Transform)datas[5];
            camera = (Camera)datas[6];
        }
        public void Update()
        {
            half.x = Camera.main.orthographicSize * Screen.width / Screen.height;
            half.y = Camera.main.orthographicSize;
        }
        public void LateUpdate()
        {
            if (isFollow)
            {
                Vector3 localPosition = camera.gameObject.transform.localPosition;
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
                camera.gameObject.transform.localPosition = localPosition;
            }
        }
        public void ChangeTarget(Transform target)
        {
            this.target = target;
        }
    }
}