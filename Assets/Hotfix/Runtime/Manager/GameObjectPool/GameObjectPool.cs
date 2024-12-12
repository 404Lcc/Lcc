using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public interface IGameObjectPool
    {
        GameObjectPoolSetting PoolSetting { get; }
        string Name { get; }
        GameObject Root { get; }
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
        private GameObjectPoolSetting _poolSetting;
        private GameObject _original;
        private Stack<GameObjectPoolObject> _cachedStack;
        private GameObject _root;

        public GameObjectPoolSetting PoolSetting => _poolSetting;
        public string Name { get; private set; }
        public GameObject Root => _root;
        public int Count => _cachedStack.Count;

        public GameObjectPool(GameObject original, GameObjectPoolSetting poolSetting, string poolName)
        {
            Debug.Assert(original != null);
            _cachedStack = new Stack<GameObjectPoolObject>();
            Name = poolName;
            _original = original;
            _poolSetting = poolSetting;
            _root = new GameObject(poolName + "Pool");
            _root.transform.SetParent(GameObjectPoolManager.Instance.Root);
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