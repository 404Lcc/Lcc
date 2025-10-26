using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public interface IGameObjectPoolService : IService
    {
        GameObjectPoolSetting PoolSetting { get; }
        Transform Root { get; }
        int PoolCount { get; }

        void SetLoader(Func<string, GameObject, GameObject> loader);
        void SetAsyncLoader(Action<string, GameObject, Action<string, Object>> asyncLoader);

        GameObjectPoolObject GetObject(string poolName);
        GameObjectPoolAsyncOperation GetObjectAsync(string poolName, Action<GameObjectPoolObject> onComplete);
        void CancelAsyncOperation(GameObjectPoolAsyncOperation operation);

        void ReleaseObject(GameObjectPoolObject poolObject);
        void ReleasePool(string poolName);
    }
}