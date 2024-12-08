using UnityEngine;

namespace LccHotfix
{
    public class ActorView : IViewWrapper
    {
        private bool _isPoolRes;

        private GameObject _gameObject;
        public GameObject GameObject => _gameObject;

        private Transform _transform;
        public Transform Transform => _transform;

        private Animator _animator;
        public Animator Animator => _animator;

        public void Init(GameObject go, bool isPoolRes)
        {
            _isPoolRes = isPoolRes;
            _gameObject = go;
            _transform = go.transform;
            _animator = _gameObject.GetComponent<Animator>();
        }

        public virtual void SyncTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            _transform.position = position;
            _transform.rotation = rotation;
            _transform.localScale = scale;
        }

        public virtual void Dispose()
        {
            if (_isPoolRes)
            {
            }

            EnableAnimator(true);

            _gameObject = null;
            _animator = null;
        }

        public void EnableAnimator(bool enable)
        {
            if (_animator == null)
                return;
            _animator.enabled = enable;
        }
    }
}