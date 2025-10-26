using UnityEngine;

namespace LccHotfix
{
    public class GameObjectPoolObject
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

        public GameObjectPoolObject(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public void Release()
        {
            _pool.Release(this);
        }

        public void OnReset()
        {
        }
    }
}