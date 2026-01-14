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
        GameObjectObject Get();
        void Release(GameObjectObject obj);
        void ReleaseAll();
        GameObjectObject ForceSpawm();
        void ForceRelease();
    }

    public class GameObjectPool : IGameObjectPool
    {
        private GameObject _original;
        private GameObject _root;
        private GameObjectPoolSetting _poolSetting;
        private Stack<GameObjectObject> _cachedStack;

        public GameObject Root => _root;
        public GameObjectPoolSetting PoolSetting => _poolSetting;
        public string Name { get; private set; }
        public int Count => _cachedStack.Count;

        public GameObjectPool(GameObject original, GameObject root, GameObjectPoolSetting poolSetting, string location)
        {
            UnityEngine.Debug.Assert(original != null);
            _original = original;
            _root = root;
            _poolSetting = poolSetting;
            Name = location;
            _cachedStack = new Stack<GameObjectObject>();
        }

        public virtual GameObjectObject Get()
        {
            GameObjectObject obj = null;
            if (_cachedStack.Count > 0)
            {
                obj = _cachedStack.Pop();
                obj.GameObject.transform.SetParent(null);
                obj.GameObject.SetActive(true);

                obj.OnReset();
                obj.Pool = this;
            }
            else
            {
                obj = ForceSpawm();
            }

            return obj;
        }

        public void Release(GameObjectObject obj)
        {
            if (obj != null)
            {
                obj.GameObject.SetActive(false);
                _cachedStack.Push(obj);
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

        public GameObjectObject ForceSpawm()
        {
            var go = Object.Instantiate(_original.gameObject, _root.transform);
            var obj = new GameObjectObject(go);
            obj.GameObject.name = Name;
            obj.GameObject.transform.SetParent(null);
            obj.GameObject.SetActive(true);

            obj.OnReset();
            obj.Pool = this;
            return obj;
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