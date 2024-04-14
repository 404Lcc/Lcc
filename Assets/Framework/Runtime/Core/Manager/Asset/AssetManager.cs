using ET;
using System;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace LccModel
{
    //Resources.UnloadAsset仅能释放非GameObject和Component的资源 比如Texture Mesh等真正的资源 对于由Prefab加载出来的Object或Component,则不能通过该函数来进行释放
    public class AssetManager : Singleton<AssetManager>
    {
        public const string DefaultPackage = "DefaultPackage";

        public ResourcePackage Package => YooAssets.GetPackage(DefaultPackage);

        public override void Register()
        {
            base.Register();


        }
        public override void Destroy()
        {
            base.Destroy();

            ForceUnloadAllAssets();
        }

        public void UnLoadAsset(AssetHandle handle)
        {
            handle.Release();
        }
        public void UnLoadAsset(AllAssetsHandle handle)
        {
            handle.Release();
        }
        public void ForceUnloadAllAssets()
        {
            if (Package == null) return;
            Package.ForceUnloadAllAssets();
        }
        public void UnloadUnusedAssets()
        {
            if (Package == null) return;
            Package.UnloadUnusedAssets();
        }

        //异步加载
        public AssetObject StartLoadRes<T>(GameObject loader, string location, Action<string, Object> onComplete = null) where T : Object
        {
            if (loader != null && !string.IsNullOrEmpty(location))
            {
                AssetObject res = loader.AddComponent<AssetObject>();
                res.SetInfo<T>(location, onComplete);
                res.StartLoad();
                return res;
            }
            return null;
        }

        //异步加载
        public async ETTask<T> StartLoadRes<T>(GameObject loader, string location) where T : Object
        {
            ETTask task = ETTask.Create();
            Action<string, Object> onComplete = (location, asset) =>
            {
                task.SetResult();
            };

            if (loader != null && !string.IsNullOrEmpty(location))
            {
                AssetObject res = loader.AddComponent<AssetObject>();
                res.SetInfo<T>(location, onComplete);
                res.StartLoad();

                await task;
                return res.Asset as T;
            }
            return null;
        }

        //异步加载
        public AssetGameObject StartLoadGameObject(GameObject loader, string location, Action<string, Object> onComplete = null)
        {
            if (loader != null && !string.IsNullOrEmpty(location))
            {
                AssetGameObject res = loader.AddComponent<AssetGameObject>();
                res.SetInfo(location, onComplete);
                res.StartLoad();
                return res;
            }
            return null;
        }

        //异步加载
        public async ETTask<GameObject> StartLoadGameObject(GameObject loader, string location)
        {
            ETTask task = ETTask.Create();
            Action<string, Object> onComplete = (location, asset) =>
            {
                task.SetResult();
            };

            if (loader != null && !string.IsNullOrEmpty(location))
            {
                AssetGameObject res = loader.AddComponent<AssetGameObject>();
                res.SetInfo(location, onComplete);
                res.StartLoad();

                await task;
                return res.resGameObject;
            }
            return null;
        }

        //异步加载
        public ALLAssetObject StartLoadALLRes<T>(GameObject loader, string location, Action<string, Object[]> onComplete = null) where T : Object
        {
            if (loader != null && !string.IsNullOrEmpty(location))
            {
                ALLAssetObject res = loader.AddComponent<ALLAssetObject>();
                res.SetInfo<T>(location, onComplete);
                res.StartLoad();
                return res;
            }
            return null;
        }

        //异步加载
        public async ETTask<T[]> StartLoadALLRes<T>(GameObject loader, string location) where T : Object
        {
            ETTask task = ETTask.Create();
            Action<string, Object[]> onComplete = (location, asset) =>
            {
                task.SetResult();
            };

            if (loader != null && !string.IsNullOrEmpty(location))
            {
                ALLAssetObject res = loader.AddComponent<ALLAssetObject>();
                res.SetInfo<T>(location, onComplete);
                res.StartLoad();

                await task;
                return res.Assets as T[];
            }
            return null;
        }


        //同步加载
        public T LoadRes<T>(GameObject loader, string location) where T : Object
        {
            if (loader != null && !string.IsNullOrEmpty(location))
            {
                AssetObject res = loader.AddComponent<AssetObject>();
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
                AssetGameObject res = loader.AddComponent<AssetGameObject>();
                res.SetInfo<GameObject>(location);
                res.Load();
                return res.resGameObject;
            }
            return null;
        }

        //同步加载
        public T[] LoadALLRes<T>(GameObject loader, string location) where T : Object
        {
            if (loader != null && !string.IsNullOrEmpty(location))
            {
                ALLAssetObject res = loader.AddComponent<ALLAssetObject>();
                res.SetInfo<T>(location);
                res.Load();
                return res.Assets as T[];
            }
            return null;
        }

        public RawFileHandle LoadRawAssetAsync(string location, Action<RawFileHandle> callback = null)
        {
            RawFileHandle rawFileOperation = YooAssets.GetPackage(DefaultPackage).LoadRawFileAsync(location);
            rawFileOperation.Completed += callback;
            return rawFileOperation;
        }
    }
}