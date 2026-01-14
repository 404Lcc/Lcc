using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

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

    /// <summary>
    /// 异步加载句柄
    /// </summary>
    public class GameObjectHandle : IReference
    {
        private IGameObjectPoolService _service;
        private Action<GameObjectHandle> _callback;
        private GameObjectObject _result;
        public string Location { get; private set; }
        public bool IsDone { get; private set; }

        public GameObject GameObject
        {
            get
            {
                if (IsDone)
                {
                    return _result.GameObject;
                }

                return null;
            }
        }

        public Transform Transform
        {
            get
            {
                if (IsDone)
                {
                    return _result.Transform;
                }

                return null;
            }
        }

        public void Init(IGameObjectPoolService service, Action<GameObjectHandle> callback, string location)
        {
            _service = service;
            _callback = callback;
            Location = location;
        }

        public void SetResult(GameObjectObject result)
        {
            _result = result;
        }

        public void Complete()
        {
            if (IsDone)
            {
                return;
            }

            IsDone = true;
            _callback?.Invoke(this);
        }

        public void Release(ref GameObjectHandle handle)
        {
            if (_result != null)
            {
                _result.Release(ref _result);
            }

            _service.CancelLoad(this);
            _service.CancelComplete(this);

            ReferencePool.Release(this);
            handle = null;
        }

        public void OnRecycle()
        {
            _service = null;
            _callback = null;
            _result = null;
            Location = null;
            IsDone = false;
        }
    }

    internal class GameObjectPoolManager : Module, IGameObjectPoolService
    {
        private GameObjectPoolSetting _poolSetting;

        // private Func<string, GameObject, GameObject> _loader;
        private Action<string, AssetLoader, Action<string, Object>> _asyncLoader;
        private Dictionary<string, IGameObjectPool> _poolDict; //对象池列表
        private Dictionary<string, List<GameObjectHandle>> _loadList; //加载列表
        private List<GameObjectHandle> _completeList; //完成列表
        private List<GameObjectHandle> _tempList; //完成缓存列表
        private GameObjectHandle _temp; //当前的完成句柄

        private Transform _root;
        private AssetLoader _assetLoader;

        public GameObjectPoolSetting PoolSetting => _poolSetting;
        public Transform Root => _root;
        public int PoolCount => _poolDict.Count;

        public GameObjectPoolManager()
        {
            _poolSetting = new GameObjectPoolSetting();
            _poolDict = new Dictionary<string, IGameObjectPool>();
            _loadList = new Dictionary<string, List<GameObjectHandle>>();
            _completeList = new List<GameObjectHandle>();
            _tempList = new List<GameObjectHandle>();
            _root = new GameObject("GameObjectPoolRoot").transform;
            GameObject.DontDestroyOnLoad(_root);
            _assetLoader = new AssetLoader();

            //预加载
            _poolSetting.preloadCount = 0;
            _poolSetting.preloadPerFrame = 10;

            //自动回收
            _poolSetting.autoRelease = -1;

            //最大数量
            _poolSetting.maxActiveObjects = -1;
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            // foreach (var item in _poolDict.Values)
            // {
            //     item.Update();
            // }

            if (_completeList.Count == 0)
                return;

            _tempList.Clear();
            _tempList.AddRange(_completeList);
            _completeList.Clear();
            foreach (var item in _tempList)
            {
                item.Complete();
            }
        }

        internal override void Shutdown()
        {
            foreach (var item in _poolDict.Values)
            {
                item.ReleaseAll();
            }

            _poolDict.Clear();
            _loadList.Clear();

            GameObject.Destroy(_root.gameObject);
            _root = null;
        }

        // public void SetLoader(Func<string, GameObject, GameObject> loader)
        // {
        //     _loader = loader;
        // }

        public void SetAsyncLoader(Action<string, AssetLoader, Action<string, Object>> asyncLoader)
        {
            _asyncLoader = asyncLoader;
        }

        /// <summary>
        /// 同步获取对象
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        // public GameObjectPoolObject GetObject(string location)
        // {
        //     if (_poolDict.TryGetValue(location, out var pool))
        //     {
        //         return pool.Get();
        //     }
        //     else
        //     {
        //         if (_loader == null)
        //         {
        //             Debug.LogError("对象池没有设置同步加载器");
        //             return null;
        //         }
        //
        //         var root = new GameObject(location + "Pool");
        //         root.transform.SetParent(Root);
        //
        //         var original = _loader(location, root);
        //         if (original == null)
        //         {
        //             Debug.LogError($"加载资源失败 {location}");
        //             GameObject.Destroy(root);
        //             return null;
        //         }
        //
        //         pool = CreateDecoratedPool(original, root, location);
        //         _poolDict.Add(location, pool);
        //         return pool.Get();
        //     }
        // }

        /// <summary>
        /// 异步获取对象
        /// </summary>
        /// <param name="location"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        public GameObjectHandle GetObjectAsync(string location, Action<GameObjectHandle> onComplete)
        {
            if (_asyncLoader == null)
            {
                UnityEngine.Debug.LogError("对象池没有设置异步加载器");
                return null;
            }

            var handle = ReferencePool.Acquire<GameObjectHandle>();
            handle.Init(this, onComplete, location);

            if (_poolDict.TryGetValue(location, out var pool))
            {
                //池已经存在，直接完成
                handle.SetResult(pool.Get());
                _completeList.Add(handle);
                return handle;
            }

            //添加到加载列表
            if (!_loadList.TryGetValue(location, out var list))
            {
                list = new List<GameObjectHandle>();
                list.Add(handle);
                _loadList.Add(location, list);

                _asyncLoader(location, _assetLoader, (assetName, obj) => { CreateObjectPool(assetName, obj as GameObject); });
            }
            else
            {
                list.Add(handle);
            }

            return handle;
        }

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <param name="location"></param>
        /// <param name="original"></param>
        private void CreateObjectPool(string location, GameObject original)
        {
            //检查是否还在加载列表里
            if (!_loadList.ContainsKey(location))
            {
                //卸载资源
                _assetLoader.Release(location);
                return;
            }

            if (original == null)
            {
                UnityEngine.Debug.LogError($"加载资源失败 {location}");
                CompleteAllLoad(location, null);
                return;
            }

            var root = new GameObject(location + "Pool");
            root.transform.SetParent(Root);

            //创建对象池
            var pool = CreateDecoratedPool(original, root, location);
            _poolDict.Add(location, pool);
            CompleteAllLoad(location, pool);
        }

        /// <summary>
        /// 完成所有加载
        /// </summary>
        /// <param name="location"></param>
        /// <param name="pool"></param>
        private void CompleteAllLoad(string location, IGameObjectPool pool)
        {
            //复制列表以避免在迭代时修改
            var list = new List<GameObjectHandle>(_loadList[location]);

            _loadList.Remove(location);

            foreach (var item in list)
            {
                if (pool != null)
                {
                    item.SetResult(pool.Get());
                    _completeList.Add(item);
                }
                else
                {
                    item.SetResult(null);
                    _completeList.Add(item);
                }
            }
        }

        /// <summary>
        /// 取消加载
        /// </summary>
        /// <param name="handle"></param>
        public void CancelLoad(GameObjectHandle handle)
        {
            //从加载列表中移除
            if (_loadList.TryGetValue(handle.Location, out var list))
            {
                if (list.Contains(handle))
                {
                    list.Remove(handle);
                }

                //如果列表为空，清理等待列表
                if (list.Count == 0)
                {
                    _loadList.Remove(handle.Location);
                }
            }
        }


        /// <summary>
        /// 取消完成
        /// </summary>
        /// <param name="handle"></param>
        public void CancelComplete(GameObjectHandle handle)
        {
            //从完成列表中移除
            if (_completeList.Contains(handle))
            {
                _completeList.Remove(handle);
            }
        }

        /// <summary>
        /// 创建修饰器
        /// </summary>
        /// <param name="original"></param>
        /// <param name="root"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        private IGameObjectPool CreateDecoratedPool(GameObject original, GameObject root, string location)
        {
            IGameObjectPool pool = new GameObjectPool(original, root, PoolSetting, location);
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

            return decorator;
        }

        public void ReleasePool(string location)
        {
            if (_poolDict.TryGetValue(location, out var pool))
            {
                pool.ReleaseAll();
                _poolDict.Remove(location);
            }
        }
    }
}