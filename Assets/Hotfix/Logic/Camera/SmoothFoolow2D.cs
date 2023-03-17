using LccModel;
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

        public TransformComponent target;
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
            target = (TransformComponent)datas[5];
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
                Vector3 position = camera.gameObject.transform.position;
                if (Mathf.Abs(position.x - target.position.x) > margin.x)
                {
                    position.x = Mathf.Lerp(position.x, target.position.x, smooth * UnityEngine.Time.deltaTime);
                }
                if (Mathf.Abs(position.y - target.position.y) > margin.y)
                {
                    position.y = Mathf.Lerp(position.y, target.position.y, smooth * UnityEngine.Time.deltaTime);
                }
                position.x = Mathf.Clamp(position.x, min.x + half.x, max.x - half.x);
                position.y = Mathf.Clamp(position.y, min.y + half.y, max.y - half.y);
                camera.gameObject.transform.position = position;
            }
        }
        public void ChangeTarget(TransformComponent target)
        {
            this.target = target;
        }
    }
}