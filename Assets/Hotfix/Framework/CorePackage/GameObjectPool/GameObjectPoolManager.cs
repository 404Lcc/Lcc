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
        public string PoolName { get; internal set; }
        public bool IsDone { get; internal set; }
        public bool IsCancelled { get; internal set; }
        public Action<GameObjectPoolObject> Callback { get; internal set; }
        internal List<GameObjectPoolAsyncOperation> OperationList { get; set; }

        internal void Complete(GameObjectPoolObject result)
        {
            if (IsDone)
                return;

            IsDone = true;

            try
            {
                Callback?.Invoke(result);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in async operation callback: {e}");
            }

            // 从操作列表中移除
            OperationList?.Remove(this);
        }

        internal void Cancel()
        {
            if (IsDone)
                return;

            IsCancelled = true;
            IsDone = true;

            // 从操作列表中移除
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
            // 取消所有异步操作
            foreach (var poolName in new List<string>(_pendingOperations.Keys))
            {
                CancelAllAsyncOperations(poolName);
            }

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

        public GameObjectPoolObject GetObject(string poolName)
        {
            if (_poolDict.TryGetValue(poolName, out var pool))
            {
                return pool.Get();
            }
            else
            {
                return CreatePoolSync(poolName);
            }
        }

        public GameObjectPoolAsyncOperation GetObjectAsync(string poolName, Action<GameObjectPoolObject> onComplete)
        {
            var operation = new GameObjectPoolAsyncOperation
            {
                PoolName = poolName,
                Callback = onComplete
            };

            if (_poolDict.TryGetValue(poolName, out var pool))
            {
                // 如果池已经存在，直接完成操作
                operation.Complete(pool.Get());
                return operation;
            }

            // 添加到等待列表
            if (!_pendingOperations.TryGetValue(poolName, out var operationList))
            {
                operationList = new List<GameObjectPoolAsyncOperation>();
                operationList.Add(operation);
                operation.OperationList = operationList;
                _pendingOperations.Add(poolName, operationList);

                // 开始异步加载池
                StartAsyncLoadPool(poolName);
            }
            else
            {
                operationList.Add(operation);
                operation.OperationList = operationList;
            }

            return operation;
        }

        public void CancelAsyncOperation(GameObjectPoolAsyncOperation operation)
        {
            if (operation.IsDone)
                return;

            operation.Cancel();

            // 如果该池没有其他等待的操作，取消资源加载
            if (_pendingOperations.TryGetValue(operation.PoolName, out var operationList))
            {
                bool hasPendingOperations = operationList.Any(op => !op.IsDone);
                if (!hasPendingOperations)
                {
                    CancelPoolLoading(operation.PoolName);
                }
            }
        }

        private void CancelAllAsyncOperations(string poolName)
        {
            if (_pendingOperations.TryGetValue(poolName, out var operationList))
            {
                CancelPoolLoading(poolName);
            }
        }

        private void CancelPoolLoading(string poolName)
        {
            _pendingOperations.Remove(poolName);

            // 清理临时创建的根节点
            var rootName = poolName + "Pool";
            var rootObj = GameObject.Find(rootName);
            if (rootObj != null && rootObj.transform.parent == _root)
            {
                GameObject.Destroy(rootObj);
            }
        }

        private GameObjectPoolObject CreatePoolSync(string poolName)
        {
            if (_loaderHandle == null)
            {
                Debug.LogError("Sync loader is not set!");
                return null;
            }

            var root = new GameObject(poolName + "Pool");
            root.transform.SetParent(Root);
            var original = _loaderHandle(poolName, root);

            if (original == null)
            {
                Debug.LogError($"Failed to load GameObject: {poolName}");
                GameObject.Destroy(root);
                return null;
            }

            var pool = CreateDecoratedPool(original, root, poolName);
            _poolDict.Add(poolName, pool);
            return pool.Get();
        }

        private void StartAsyncLoadPool(string poolName)
        {
            if (_asyncLoaderHandle == null)
            {
                Debug.LogError("Async loader is not set!");
                CompleteAsyncLoad(poolName, null);
                return;
            }

            var root = new GameObject(poolName + "Pool");
            root.transform.SetParent(Root);

            // 开始异步加载
            _asyncLoaderHandle(poolName, root, (assetName, loadedObject) => { OnAsyncLoadComplete(poolName, loadedObject as GameObject, root); });
        }

        private void OnAsyncLoadComplete(string poolName, GameObject loadedObject, GameObject root)
        {
            if (!_pendingOperations.TryGetValue(poolName, out var operationList))
            {
                //todo
                // 所有操作都已被取消
                GameObject.Destroy(root);
                return;
            }

            if (loadedObject == null)
            {
                Debug.LogError($"Async load failed for: {poolName}");
                GameObject.Destroy(root);
                CompleteAsyncLoad(poolName, null);
                return;
            }

            // 创建对象池
            var pool = CreateDecoratedPool(loadedObject, root, poolName);
            _poolDict.Add(poolName, pool);

            CompleteAsyncLoad(poolName, pool);
        }

        private void CompleteAsyncLoad(string poolName, IGameObjectPool pool)
        {
            if (!_pendingOperations.TryGetValue(poolName, out var operationList))
                return;

            // 复制列表以避免在迭代时修改
            var operationsToComplete = new List<GameObjectPoolAsyncOperation>(operationList);
            _pendingOperations.Remove(poolName);

            foreach (var operation in operationsToComplete)
            {
                if (operation.IsCancelled)
                    continue;

                if (pool != null)
                {
                    var poolObject = pool?.Get();
                    operation.Complete(poolObject);
                }
                else
                {
                    operation.Complete(null);
                }
            }
        }

        private IGameObjectPool CreateDecoratedPool(GameObject original, GameObject root, string poolName)
        {
            IGameObjectPool pool = new GameObjectPool(original, root, PoolSetting, poolName);
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