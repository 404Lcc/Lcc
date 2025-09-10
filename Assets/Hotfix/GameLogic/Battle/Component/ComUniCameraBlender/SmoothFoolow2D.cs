using UnityEngine;

namespace LccHotfix
{
    public class SmoothFoolow2D : ICameraBlender
    {
        public float smoothTime;
        public Bounds bounds;

        public Transform Target { get; set; }
        public Camera Camera => Main.CameraService.CurrentCamera;

        public virtual void PostInitialize()
        {
            smoothTime = 0.1f;
            bounds = new Bounds();
            bounds.center = Vector3.zero;
            bounds.size = new Vector3(50, 50, 0);
        }

        public virtual void Update()
        {
        }

        public virtual void LateUpdate()
        {
            MoveCamera();
        }

        public virtual void ChangeTarget(Transform target)
        {
            Target = target;
        }

        public virtual void Dispose()
        {
        }

        private void MoveCamera()
        {
            if (Target == null)
                return;

            Vector3 targetPos = Target.position;
            targetPos.z = Camera.transform.position.z;
            targetPos = GetClampedPosition(targetPos);
            Vector3 velocity = Vector3.zero;
            Camera.transform.position = Vector3.SmoothDamp(Camera.transform.position, targetPos, ref velocity, smoothTime);
        }


        private Vector3 GetClampedPosition(Vector3 targetPosition)
        {
            float cameraSize = Camera.orthographicSize;

            // 获取当前相机视口矩形
            Rect cameraRect = GetCameraRect(targetPosition, cameraSize);

            // 水平边界限制
            if (cameraRect.width < bounds.size.x)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, bounds.min.x + cameraRect.width / 2, bounds.max.x - cameraRect.width / 2);
            }
            else
            {
                targetPosition.x = bounds.center.x; // 边界过窄时居中
            }

            // 垂直边界限制
            if (cameraRect.height < bounds.size.y)
            {
                targetPosition.y = Mathf.Clamp(targetPosition.y, bounds.min.y + cameraRect.height / 2, bounds.max.y - cameraRect.height / 2);
            }
            else
            {
                targetPosition.y = bounds.center.y; // 边界过矮时居中
            }

            return new Vector3(targetPosition.x, targetPosition.y, Camera.transform.position.z);
        }



        private Rect GetCameraRect(Vector3 camPos, float size)
        {
            float height = size * 2; //高度 = 正交相机的size*2
            float width = height * Camera.aspect; //宽度 = 高度*宽高比

            return new Rect(camPos.x - width / 2, camPos.y - height / 2, width, height);
        }
    }
}