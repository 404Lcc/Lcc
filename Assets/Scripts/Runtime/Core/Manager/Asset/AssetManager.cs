using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace LccModel
{
    public class AssetManager : Singleton<AssetManager>
    {
        public Dictionary<string, AssetData> assetDic = new Dictionary<string, AssetData>();
        private async Task<AssetData> LoadAssetData<T>(string name, string suffix, bool isKeep, bool isAssetBundle, params string[] types) where T : Object
        {
            if (types.Length == 0) return null;
            string path = string.Empty;
            for (int i = 0; i < types.Length; i++)
            {
                path += types[i] + "/";
                if (i == types.Length - 1)
                {
                    path += name;
                }
            }
            if (assetDic.ContainsKey(path))
            {
                return assetDic[path];
            }
            else
            {
                AssetData assetData = new AssetData();
#if AssetBundle
                if (isAssetBundle)
                {
                    AsyncOperationHandle<T> handler = Addressables.LoadAssetAsync<T>("Assets/Resources/" + path);
                    await handler.Task;
                    Object asset = handler.Result;
                    assetData.asset = asset;
                    assetData.types = types;
                    assetData.name = name;
                    assetData.isKeep = isKeep;
                    assetData.isAssetBundle = isAssetBundle;
                    assetDic.Add(path, assetData);
                }
                else
                {
                    Object asset = Resources.Load<T>(path);
                    assetData.asset = asset;
                    assetData.types = types;
                    assetData.name = name;
                    assetData.isKeep = isKeep;
                    assetData.isAssetBundle = isAssetBundle;
                    assetDic.Add(path, assetData);
                }
#else
                Object asset = Resources.Load<T>(path);
                assetData.asset = asset;
                assetData.types = types;
                assetData.name = name;
                assetData.isKeep = isKeep;
                assetData.isAssetBundle = isAssetBundle;
                assetDic.Add(path, assetData);
#endif
                return assetData;
            }
        }
        public void UnloadAssetsData()
        {
            string all = string.Empty;
            foreach (string item in assetDic.Keys)
            {
                if (!assetDic[item].isKeep)
                {
                    all += item + ",";
                }
            }
            foreach (string item in all.Split(','))
            {
                if (string.IsNullOrEmpty(item)) continue;
                Object asset = assetDic[item].asset;
                if (item.Contains("."))
                {
                    if (asset)
                    {
                        if (asset.GetType() != typeof(GameObject) && asset.GetType() != typeof(Component))
                        {
                            //Resources.UnloadAsset仅能释放非GameObject和Component的资源 比如Texture Mesh等真正的资源 对于由Prefab加载出来的Object或Component,则不能通过该函数来进行释放
                            Resources.UnloadAsset(asset);
                            assetDic.Remove(item);
                        }
                    }
                    else
                    {
                        assetDic.Remove(item);
                    }
                }
                else
                {
                    assetDic.Remove(item);
                }
            }
        }
        public void UnloadAllAssetsData()
        {
            assetDic.Clear();
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
        public async Task<T> LoadAsset<T>(string name, string suffix, bool isKeep, bool isAssetBundle, params string[] types) where T : Object
        {
            AssetData assetData = await LoadAssetData<T>(name, suffix, isKeep, isAssetBundle, types);
            return (T)assetData.asset;
        }
        public async Task<GameObject> InstantiateAsset(string name, bool isKeep, bool isAssetBundle, params string[] types)
        {
            AssetData assetData = await LoadAssetData<GameObject>(name, ".prefab", isKeep, isAssetBundle, types);
            if (assetData.asset == null) return null;
            GameObject gameObject = (GameObject)Object.Instantiate(assetData.asset);
            gameObject.name = name;
            return gameObject;
        }
        public async Task<T> InstantiateAsset<T>(string name, bool isKeep, bool isAssetBundle, params string[] types) where T : Component
        {
            AssetData assetData = await LoadAssetData<GameObject>(name, ".prefab", isKeep, isAssetBundle, types);
            if (assetData.asset == null) return null;
            GameObject gameObject = (GameObject)Object.Instantiate(assetData.asset);
            gameObject.name = name;
            T component = gameObject.AddComponent<T>();
            return component;
        }
        public async Task<GameObject> InstantiateAsset(string name, bool isKeep, bool isAssetBundle, Transform parent, params string[] types)
        {
            AssetData assetData = await LoadAssetData<GameObject>(name, ".prefab", isKeep, isAssetBundle, types);
            if (assetData.asset == null) return null;
            GameObject gameObject = (GameObject)Object.Instantiate(assetData.asset);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            return gameObject;
        }
        public async Task<T> InstantiateAsset<T>(string name, bool isKeep, bool isAssetBundle, Transform parent, params string[] types) where T : Component
        {
            AssetData assetdata = await LoadAssetData<GameObject>(name, ".prefab", isKeep, isAssetBundle, types);
            if (assetdata.asset == null) return null;
            GameObject gameObject = (GameObject)Object.Instantiate(assetdata.asset);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            T component = gameObject.AddComponent<T>();
            return component;
        }
    }
}