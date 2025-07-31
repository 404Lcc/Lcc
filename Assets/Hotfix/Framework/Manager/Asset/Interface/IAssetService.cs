using System;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace LccHotfix
{
    public interface IAssetService : IService
    {
        void UnloadAllAssetsAsync();

        void UnloadUnusedAssetsAsync();

        bool CheckLocationValid(string location);

        AssetHandle LoadAssetSync(string location, Type type);

        AssetHandle LoadAssetAsync(string location, Type type);

        AllAssetsHandle LoadAllAssetsSync(string location, Type type);

        AllAssetsHandle LoadAllAssetsAsync(string location, Type type);


        //同步加载
        T LoadRes<T>(GameObject loader, string location) where T : Object;

        //同步加载
        GameObject LoadGameObject(string location, bool keepHierar = false);
    }
}