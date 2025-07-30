using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using cfg;
using LccModel;
using Luban;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

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