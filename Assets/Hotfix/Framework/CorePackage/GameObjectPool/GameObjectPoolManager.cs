using System;
using System.Collections.Generic;
using System.Linq;
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

    public class GameObjectPoolAsyncOperation
    {
        public string Location { get; internal set; }
        public bool IsDone { get; internal set; }
        public bool IsCancelled { get; internal set; }
        public Action<GameObjectPoolObject> Callback { get; internal set; }
        public List<GameObjectPoolAsyncOperation> OperationList { get; set; }

        internal void Complete(GameObjectPoolObject result)
        {
            if (IsDone)
                return;

            IsDone = true;

            Callback?.Invoke(result);

            RemoveSelf();

        }

        internal void Cancel()
        {
            if (IsDone)
                return;

            IsDone = true;

            IsCancelled = true;

            RemoveSelf();
        }

        public void RemoveSelf()
        {
            //从操作列表中移除
            OperationList?.Remove(this);
        }
    }

    internal class GameObjectPoolManager : Module, IGameObjectPoolService
    {
        private GameObjectPoolSetting _poolSetting;
        private Func<string, GameObject, GameObject> _loaderHandle;
        private Action<string, GameObject, Action<string, Object>> _asyncLoaderHandle;
        private Dictionary<string, IGameObjectPool> _poolDict;
        private Dictionary<string, List<GameObjectPoolAsyncOperation>> _pendingOperations;
        private Transform _root;

        public GameObjectPoolSetting PoolSetting => _poolSetting;
        public Transform Root => _root;
        public int PoolCount => _poolDict.Count;

        public GameObjectPoolManager()
        {
            _poolSetting = new GameObjectPoolSetting();
            _poolDict = new Dictionary<string, IGameObjectPool>();
            _pendingOperations = new Dictionary<string, List<GameObjectPoolAsyncOperation>>();
            _root = new GameObject("GameObjectPoolRoot").transform;
            GameObject.DontDestroyOnLoad(_root);

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
            _pendingOperations.Clear();

            GameObject.Destroy(_root.gameObject);
            _root = null;
        }

        public void SetLoader(Func<string, GameObject, GameObject> loader)
        {
            _loaderHandle = loader;
        }

        public void SetAsyncLoader(Action<string, GameObject, Action<string, Object>> asyncLoader)
        {
            _asyncLoaderHandle = asyncLoader;
        }

        /// <summary>
        /// 同步获取对象
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public GameObjectPoolObject GetObject(string location)
        {
            if (_poolDict.TryGetValue(location, out var pool))
            {
                return pool.Get();
            }
            else
            {
                if (_loaderHandle == null)
                {
                    Debug.LogError("对象池没有设置同步加载器");
                    return null;
                }

                var root = new GameObject(location + "Pool");
                root.transform.SetParent(Root);

                var original = _loaderHandle(location, root);
                if (original == null)
                {
                    Debug.LogError($"加载资源失败 {location}");
                    GameObject.Destroy(root);
                    return null;
                }

                pool = CreateDecoratedPool(original, root, location);
                _poolDict.Add(location, pool);
                return pool.Get();
            }
        }

        /// <summary>
        /// 异步获取对象
        /// </summary>
        /// <param name="location"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        public GameObjectPoolAsyncOperation GetObjectAsync(string location, Action<GameObjectPoolObject> onComplete)
        {
            if (_asyncLoaderHandle == null)
            {
                Debug.LogError("对象池没有设置异步加载器");
                return null;
            }

            var operation = new GameObjectPoolAsyncOperation();
            operation.Location = location;
            operation.Callback = onComplete;

            if (_poolDict.TryGetValue(location, out var pool))
            {
                //池已经存在，直接完成操作
                operation.Complete(pool.Get());
                return operation;
            }

            //添加到等待列表
            if (!_pendingOperations.TryGetValue(location, out var operationList))
            {
                operationList = new List<GameObjectPoolAsyncOperation>();
                operationList.Add(operation);
                operation.OperationList = operationList;
                _pendingOperations.Add(location, operationList);

                //开始异步加载池
                var root = new GameObject(location + "Pool");
                root.transform.SetParent(Root);
                _asyncLoaderHandle(location, root, (assetName, obj) => { OnAsyncLoadComplete(assetName, obj as GameObject, root); });
            }
            else
            {
                operationList.Add(operation);
                operation.OperationList = operationList;
            }

            return operation;
        }

        /// <summary>
        /// 异步加载对象完成
        /// </summary>
        /// <param name="location"></param>
        /// <param name="original"></param>
        /// <param name="root"></param>
        private void OnAsyncLoadComplete(string location, GameObject original, GameObject root)
        {
            //管理器要是被删除了_pendingOperations就会没有location
            if (!_pendingOperations.ContainsKey(location))
            {
                return;
            }

            if (_pendingOperations.TryGetValue(location, out var list))
            {
                //如果没有其他等待的操作，从等待列表中移除出去
                bool hasPending = list.Any(op => !op.IsDone);
                if (!hasPending)
                {
                    _pendingOperations.Remove(location);
                    //直接把root删除资源就会被卸载
                    GameObject.Destroy(root);
                    return;
                }
            }

            if (original == null)
            {
                Debug.LogError($"加载资源失败 {location}");
                GameObject.Destroy(root);
                AsyncLoadEnd(location, null);
                return;
            }

            //创建对象池
            var pool = CreateDecoratedPool(original, root, location);
            _poolDict.Add(location, pool);
            AsyncLoadEnd(location, pool);
        }

        /// <summary>
        /// 异步加载资源结束
        /// </summary>
        /// <param name="location"></param>
        /// <param name="pool"></param>
        private void AsyncLoadEnd(string location, IGameObjectPool pool)
        {
            //复制列表以避免在迭代时修改
            var list = new List<GameObjectPoolAsyncOperation>(_pendingOperations[location]);

            _pendingOperations.Remove(location);

            foreach (var item in list)
            {
                if (item.IsCancelled)
                    continue;

                if (pool != null)
                {
                    item.Complete(pool.Get());
                }
                else
                {
                    item.Complete(null);
                }
            }
        }

        /// <summary>
        /// 取消异步操作
        /// </summary>
        /// <param name="operation"></param>
        public void CancelAsyncOperation(GameObjectPoolAsyncOperation operation)
        {
            if (operation.IsDone)
                return;

            operation.Cancel();
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

        public void ReleaseObject(GameObjectPoolObject poolObject)
        {
            if (poolObject == null)
                return;
            poolObject.Release();
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