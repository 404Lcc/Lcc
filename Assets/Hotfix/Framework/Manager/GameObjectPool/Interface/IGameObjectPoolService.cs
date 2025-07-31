using System;
using UnityEngine;

namespace LccHotfix
{
    public interface IGameObjectPoolService : IService
    {
        GameObjectPoolSetting PoolSetting { get; }
        Transform Root { get; }
        int PoolCount { get; }

        void SetLoader(Func<string, GameObject, GameObject> loader);

        GameObjectPoolObject GetObject(string poolName);

        void ReleaseObject(GameObjectPoolObject poolObject);

        void ReleasePool(string poolName);
    }
}