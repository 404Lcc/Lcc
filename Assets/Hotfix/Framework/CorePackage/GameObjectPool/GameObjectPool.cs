using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public interface IGameObjectPool
    {
        GameObject Root { get; }
        GameObjectPoolSetting PoolSetting { get; }
        string Name { get; }
        int Count { get; }

        void Update();
        GameObjectPoolObject Get();
        void Release(GameObjectPoolObject poolObject);
        void ReleaseAll();
        GameObjectPoolObject ForceSpawm();
        void ForceRelease();
    }

    public class GameObjectPool : IGameObjectPool
    {
        private GameObject _original;
        private GameObject _root;
        private GameObjectPoolSetting _poolSetting;
        private Stack<GameObjectPoolObject> _cachedStack;

        public GameObject Root => _root;
        public GameObjectPoolSetting PoolSetting => _poolSetting;
        public string Name { get; private set; }
        public int Count => _cachedStack.Count;

        public GameObjectPool(GameObject original, GameObject root, GameObjectPoolSetting poolSetting, string poolName)
        {
            Debug.Assert(original != null);
            _original = original;
            _root = root;
            _poolSetting = poolSetting;
            Name = poolName;
            _cachedStack = new Stack<GameObjectPoolObject>();
        }

        public virtual GameObjectPoolObject Get()
        {
            GameObjectPoolObject poolObject = null;
            if (_cachedStack.Count > 0)
            {
                poolObject = _cachedStack.Pop();
                poolObject.GameObject.transform.SetParent(null);
                poolObject.GameObject.SetActive(true);

                poolObject.OnReset();
                poolObject.Pool = this;
            }
            else
            {
                poolObject = ForceSpawm();
            }

            return poolObject;
        }

        public void Release(GameObjectPoolObject poolObject)
        {
            if (poolObject != null)
            {
                poolObject.GameObject.SetActive(false);
                _cachedStack.Push(poolObject);
            }
        }

        public void ReleaseAll()
        {
            while (_cachedStack.Count > 0)
            {
                GameObject.Destroy(_cachedStack.Pop().GameObject);
            }

            GameObject.Destroy(_root);
        }

        public void Update()
        {
        }

        public GameObjectPoolObject ForceSpawm()
        {
            var obj = Object.Instantiate(_original.gameObject, _root.transform);
            var poolObject = new GameObjectPoolObject(obj);
            poolObject.GameObject.name = Name;
            poolObject.GameObject.transform.SetParent(null);
            poolObject.GameObject.SetActive(true);

            poolObject.OnReset();
            poolObject.Pool = this;
            return poolObject;
        }

        public void ForceRelease()
        {
            if (_cachedStack.Count > 0)
            {
                GameObject.Destroy(_cachedStack.Pop().GameObject);
            }
        }
    }
}