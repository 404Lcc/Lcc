using UnityEngine;

namespace LccModel
{
    public class TransformComponent : Component
    {
        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _localScale;
        public Vector3 position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                SyncTransform();
            }
        }
        public Quaternion rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
                SyncTransform();
            }
        }
        public Vector3 localScale
        {
            get
            {
                return _localScale;
            }
            set
            {
                _localScale = value;
                SyncTransform();
            }
        }
        public override void Awake()
        {
            base.Awake();

            position = Vector3.zero;
            rotation = Quaternion.identity;
            localScale = Vector3.one;
        }
        public void Translate(Vector3 translation)
        {
            position += translation;
        }
        public void Move(float angle, float speed)
        {
            Quaternion quaternion = Quaternion.Euler(0, 0, -angle);
            Vector3 normalDistance = (quaternion * Vector3.up).normalized;
            if (angle < 0)
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                rotation = Quaternion.Euler(0, 0, 0);
            }
            Translate(normalDistance * UnityEngine.Time.deltaTime * speed);
        }
        public void MoveToTarget(TransformComponent target, float speed)
        {
            Vector3 normalDistance = (target.position - position).normalized;
            if (normalDistance.x < 0)
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                rotation = Quaternion.Euler(0, 0, 0);
            }
            Translate(normalDistance * UnityEngine.Time.deltaTime * speed);
        }
        public void MoveToTarget(Vector3 target, float speed)
        {
            Vector3 normalDistance = target.normalized;
            if (normalDistance.x < 0)
            {
                rotation = Quaternion.Euler(0, 180, 0);
            }
            else
            {
                rotation = Quaternion.Euler(0, 0, 0);
            }
            Translate(normalDistance * UnityEngine.Time.deltaTime * speed);
        }

        private void SyncTransform()
        {
            EventSystem.Instance.Publish(new SyncTransform(GetParent<Combat>().InstanceId, _position, _rotation, _localScale));
        }
    }
}