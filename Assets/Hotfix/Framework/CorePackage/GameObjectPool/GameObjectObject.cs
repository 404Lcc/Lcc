using UnityEngine;

namespace LccHotfix
{
    public class GameObjectObject
    {
        private IGameObjectPool _pool;
        private GameObject _gameObject;

        public IGameObjectPool Pool
        {
            get { return _pool; }
            set { _pool = value; }
        }

        public GameObject GameObject => _gameObject;
        public Transform Transform => GameObject.transform;

        public GameObjectObject(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public void Release(ref GameObjectObject obj)
        {
            _pool.Release(this);
            obj = null;
        }

        public void OnReset()
        {
            Transform.position = new Vector3(30000, 0, 0);
        }
    }
}