using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class GameObjectPoolSetting
    {
        //预加载
        public int preloadCount = 0;
        public int preloadPerFrame = 0;

        //自动回收
        public int autoRelease = 0;

        //最大数量
        public int maxActiveObjects = 0;

    }
    internal class GameObjectPoolManager : Module
    {
        public static GameObjectPoolManager Instance => Entry.GetModule<GameObjectPoolManager>();

        private GameObjectPoolSetting _poolSetting;
        private Func<string, GameObject, GameObject> _loaderHandle;
        private Dictionary<string, IGameObjectPool> _poolDict;
        private Transform _root;

        public GameObjectPoolSetting PoolSetting => _poolSetting;
        public Transform Root => _root;
        public int PoolCount => _poolDict.Count;

        public GameObjectPoolManager()
        {
            _poolSetting = new GameObjectPoolSetting();
            _poolDict = new Dictionary<string, IGameObjectPool>();
            _root = new GameObject("GameObjectPoolRoot").transform;
            GameObject.DontDestroyOnLoad(_root);

            //预加载
            _poolSetting.preloadCount = 0;
            _poolSetting.preloadPerFrame = 10;

            //自动回收
            _poolSetting.autoRelease = 100;

            //最大数量
            _poolSetting.maxActiveObjects = 100;
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            foreach (var item in _poolDict.Values)
            {
                item.Update();
            }
        }

        internal override void Shutdown()
        {
            foreach (var item in _poolDict.Values)
            {
                item.ReleaseAll();
            }
            _poolDict.Clear();

            GameObject.Destroy(_root.gameObject);
            _root = null;
        }

        public void SetLoader(Func<string, GameObject, GameObject> loader)
        {
            _loaderHandle = loader;
        }

        public GameObjectPoolObject GetObject(string poolName)
        {
            if (_poolDict.TryGetValue(poolName, out var pool))
            {
                return pool.Get();
            }
            else
            {
                var root = new GameObject(poolName + "Pool");
                root.transform.SetParent(Root);
                pool = new GameObjectPool(_loaderHandle(poolName, root), root, PoolSetting, poolName);

                IGameObjectPool decorator = new SetParentDecorator(pool);

                if (PoolSetting.preloadCount > 0)
                {
                    decorator = new PreloadDecorator(decorator);
                }
                if (PoolSetting.autoRelease > 0)
                {
                    decorator = new AutoReleaseDecorator(decorator);
                }
                if (PoolSetting.maxActiveObjects > 0)
                {
                    decorator = new MaxActiveDecorator(decorator);
                }

                _poolDict.Add(poolName, decorator);


                return decorator.Get();
            }
        }

        public void ReleaseObject(GameObjectPoolObject poolObject)
        {
            if (poolObject == null)
                return;
            poolObject.Release();
        }

        public void ReleasePool(string poolName)
        {
            if (_poolDict.TryGetValue(poolName, out var pool))
            {
                pool.ReleaseAll();
                _poolDict.Remove(poolName);
            }
        }
    }
}