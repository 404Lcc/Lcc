using System;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace LccModel
{
    //Resources.UnloadAsset仅能释放非GameObject和Component的资源 比如Texture Mesh等真正的资源 对于由Prefab加载出来的Object或Component,则不能通过该函数来进行释放
    public class AssetManager : SingletonMono<AssetManager>
    {
        public const string DefaultPackage = "DefaultPackage";

        public ResourcePackage Package => YooAssets.GetPackage(DefaultPackage);

        public void OnDestroy()
        {
            UnloadAllAssetsAsync();
        }

        public void UnLoadAsset(AssetHandle handle)
        {
            handle.Release();
        }
        public void UnLoadAsset(AllAssetsHandle handle)
        {
            handle.Release();
        }
        public void UnLoadAsset(RawFileHandle handle)
        {
            handle.Release();
        }
        public void UnloadAllAssetsAsync()
        {
            if (Package == null) return;
            Package.UnloadAllAssetsAsync();
        }
        public void UnloadUnusedAssetsAsync()
        {
            if (Package == null) return;
            Package.UnloadUnusedAssetsAsync();
        }

        #region 异步加载
        //异步加载
        public AssetObject StartLoadRes<T>(GameObject loader, string location, Action<string, Object> onComplete = null) where T : Object
        {
            if (loader != null && !string.IsNullOrEmpty(location))
            {
                GameObject subLoader = new GameObject("loader_" + location + "_" + typeof(T).Name);
                subLoader.transform.SetParent(loader.transform);
                AssetObject res = subLoader.AddComponent<AssetObject>();
                res.SetInfo<T>(location, onComplete);
                res.StartLoad();
                return res;
            }
            return null;
        }

        //异步加载
        public AssetObject StartLoadGameObject(GameObject loader, string location, Action<string, Object> onComplete = null)
        {
            if (loader != null && !string.IsNullOrEmpty(location))
            {
                GameObject subLoader = new GameObject("loader_" + location + "_" + typeof(GameObject).Name);
                subLoader.transform.SetParent(loader.transform);
                AssetObject res = subLoader.AddComponent<AssetObject>();
                res.SetInfo<GameObject>(location, onComplete);
                res.StartLoad();
                return res;
            }
            return null;
        }

        //异步加载
        public ALLAssetObject StartLoadALLRes<T>(GameObject loader, string location, Action<string, Object[]> onComplete = null) where T : Object
        {
            if (loader != null && !string.IsNullOrEmpty(location))
            {
                GameObject subLoader = new GameObject("loader_" + location + "_" + typeof(T).Name);
                subLoader.transform.SetParent(loader.transform);
                ALLAssetObject res = subLoader.AddComponent<ALLAssetObject>();
                res.SetInfo<T>(location, onComplete);
                res.StartLoad();
                return res;
            }
            return null;
        }

        //异步加载
        public byte[] StartLoadRawRes(GameObject loader, string location, Action<string, byte[]> onComplete = null)
        {
            if (loader != null && !string.IsNullOrEmpty(location))
            {
                GameObject subLoader = new GameObject("loader_" + location + "_" + typeof(byte[]).Name);
                subLoader.transform.SetParent(loader.transform);
                RawObject res = subLoader.AddComponent<RawObject>();
                res.SetInfo(location, onComplete);
                res.Load();
                return res.Asset;
            }
            return null;
        }
        #endregion

        #region 同步加载

        //同步加载
        public T LoadRes<T>(GameObject loader, string location) where T : Object
        {
            if (loader != null && !string.IsNullOrEmpty(location))
            {
                GameObject subLoader = new GameObject("loader_" + location + "_" + typeof(T).Name);
                subLoader.transform.SetParent(loader.transform);
                AssetObject res = subLoader.AddComponent<AssetObject>();
                res.SetInfo<T>(location);
                res.Load();
                return res.Asset as T;
            }
            return null;
        }

        //同步加载
        public GameObject LoadGameObject(GameObject loader, string location)
        {
            if (loader != null && !string.IsNullOrEmpty(location))
            {
                GameObject subLoader = new GameObject("loader_" + location + "_" + typeof(GameObject).Name);
                subLoader.transform.SetParent(loader.transform);
                AssetObject res = subLoader.AddComponent<AssetObject>();
                res.SetInfo<GameObject>(location);
                res.Load();
                return res.Asset as GameObject;
            }
            return null;
        }

        //同步加载
        public T[] LoadALLRes<T>(GameObject loader, string location) where T : Object
        {
            if (loader != null && !string.IsNullOrEmpty(location))
            {
                GameObject subLoader = new GameObject("loader_" + location + "_" + typeof(T).Name);
                subLoader.transform.SetParent(loader.transform);
                ALLAssetObject res = subLoader.AddComponent<ALLAssetObject>();
                res.SetInfo<T>(location);
                res.Load();
                return res.Assets as T[];
            }
            return null;
        }


        //同步加载
        public byte[] LoadRawRes(GameObject loader, string location)
        {
            if (loader != null && !string.IsNullOrEmpty(location))
            {
                GameObject subLoader = new GameObject("loader_" + location + "_" + typeof(byte[]).Name);
                subLoader.transform.SetParent(loader.transform);
                RawObject res = subLoader.AddComponent<RawObject>();
                res.SetInfo(location);
                res.Load();
                return res.Asset;
            }
            return null;
        }
        #endregion

    }
}