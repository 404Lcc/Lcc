using UnityEngine;

namespace LccHotfix
{
    public class ActorView : IViewWrapper
    {
        private GameObjectPoolObject _poolObject;
        protected GameObject _gameObject;
        public GameObject GameObject => _gameObject;

        protected Transform _transform;
        public Transform Transform => _transform;

        protected Animator _animator;
        public Animator Animator => _animator;

        public virtual void Init(GameObjectPoolObject poolObject)
        {
            _poolObject = poolObject;
            _gameObject = _poolObject.GameObject;
            _transform = _poolObject.Transform;
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
            EnableAnimator(true);

            _gameObject = null;
            _animator = null;
            
            GameUtility.PutObj(ref _poolObject);
        }

        public void EnableAnimator(bool enable)
        {
            if (_animator == null)
                return;
            _animator.enabled = enable;
        }
    }
}