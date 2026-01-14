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

        // void SetLoader(Func<string, GameObject, GameObject> loader);
        void SetAsyncLoader(Action<string, AssetLoader, Action<string, Object>> asyncLoader);

        // GameObjectPoolObject GetObject(string poolName);
        GameObjectHandle GetObjectAsync(string poolName, Action<GameObjectHandle> onComplete);
        void CancelLoad(GameObjectHandle handle);
        void CancelComplete(GameObjectHandle handle);
        void ReleasePool(string poolName);
    }
}