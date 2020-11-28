using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace LccModel
{
    public class AssetManager : Singleton<AssetManager>
    {
        public Dictionary<string, AssetData> assetDict = new Dictionary<string, AssetData>();
        private string GetAssetPath(string name, params string[] types)
        {
            if (types.Length == 0) return name;
            string path = string.Empty;
            for (int i = 0; i < types.Length; i++)
            {
                path += $"{types[i]}/";
                if (i == types.Length - 1)
                {
                    path += name;
                }
            }
            return path;
        }
        private async Task<AssetData> LoadAssetDataAsync<T>(string name, string suffix, bool isKeep, bool isAssetBundle, params string[] types) where T : Object
        {
            string path = GetAssetPath(name, types);
            if (assetDict.ContainsKey(path))
            {
                return assetDict[path];
            }
            else
            {
                AssetData assetData = new AssetData();
#if AssetBundle
                if (isAssetBundle)
                {
#if !UNITY_EDITOR
                    AsyncOperationHandle<T> handler = Addressables.LoadAssetAsync<T>($"Assets/Bundles/{path}{suffix}");
                    await handler.Task;
                    Object asset = handler.Result;
                    assetData.asset = asset;
                    assetData.types = types;
                    assetData.name = name;
                    assetData.isKeep = isKeep;
                    assetData.isAssetBundle = isAssetBundle;
#else
                    Object asset = AssetDatabase.LoadAssetAtPath<T>(path);
                    assetData.asset = asset;
                    assetData.types = types;
                    assetData.name = name;
                    assetData.isKeep = isKeep;
                    assetData.isAssetBundle = isAssetBundle;
#endif
                }
                else
                {
                    Object asset = Resources.Load<T>(path);
                    assetData.asset = asset;
                    assetData.types = types;
                    assetData.name = name;
                    assetData.isKeep = isKeep;
                    assetData.isAssetBundle = isAssetBundle;
                }
#else
                Object asset = Resources.Load<T>(path);
                assetData.asset = asset;
                assetData.types = types;
                assetData.name = name;
                assetData.isKeep = isKeep;
                assetData.isAssetBundle = isAssetBundle;
#endif
                if (assetData.asset == null) return null;
                assetDict.Add(path, assetData);
                return assetData;
            }
        }
        private void UnloadAsset(AssetData assetData)
        {
            if (!assetData.isKeep)
            {
#if AssetBundle
                    if (assetData.isAssetBundle)
                    {
#if !UNITY_EDITOR
                        Addressables.Release(assetData.asset);
#endif
                    }
                    else
                    {
                        if (assetData.asset.GetType() != typeof(GameObject) && assetData.asset.GetType() != typeof(Component))
                        {
                            //Resources.UnloadAsset仅能释放非GameObject和Component的资源 比如Texture Mesh等真正的资源 对于由Prefab加载出来的Object或Component,则不能通过该函数来进行释放
                            Resources.UnloadAsset(assetData.asset);
                        }
                    }
#else
                if (assetData.asset.GetType() != typeof(GameObject) && assetData.asset.GetType() != typeof(Component))
                {
                    //Resources.UnloadAsset仅能释放非GameObject和Component的资源 比如Texture Mesh等真正的资源 对于由Prefab加载出来的Object或Component,则不能通过该函数来进行释放
                    Resources.UnloadAsset(assetData.asset);
                }
#endif
            }
        }
        public async Task<T> LoadAssetAsync<T>(string name, string suffix, bool isKeep, bool isAssetBundle, params string[] types) where T : Object
        {
            AssetData assetData = await LoadAssetDataAsync<T>(name, suffix, isKeep, isAssetBundle, types);
            if (assetData == null) return null;
            return (T)assetData.asset;
        }
        public async Task<GameObject> InstantiateAssetAsync(string name, bool isKeep, bool isAssetBundle, params string[] types)
        {
            AssetData assetData = await LoadAssetDataAsync<GameObject>(name, ".prefab", isKeep, isAssetBundle, types);
            if (assetData == null) return null;
            GameObject gameObject = (GameObject)Object.Instantiate(assetData.asset);
            gameObject.name = name;
            return gameObject;
        }
        public async Task<T> InstantiateAssetAsync<T>(string name, bool isKeep, bool isAssetBundle, params string[] types) where T : Component
        {
            AssetData assetData = await LoadAssetDataAsync<GameObject>(name, ".prefab", isKeep, isAssetBundle, types);
            if (assetData == null) return null;
            GameObject gameObject = (GameObject)Object.Instantiate(assetData.asset);
            gameObject.name = name;
            T component = gameObject.AddComponent<T>();
            return component;
        }
        public async Task<GameObject> InstantiateAssetAsync(string name, bool isKeep, bool isAssetBundle, Transform parent, params string[] types)
        {
            AssetData assetData = await LoadAssetDataAsync<GameObject>(name, ".prefab", isKeep, isAssetBundle, types);
            if (assetData == null) return null;
            GameObject gameObject = (GameObject)Object.Instantiate(assetData.asset);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            return gameObject;
        }
        public async Task<T> InstantiateAssetAsync<T>(string name, bool isKeep, bool isAssetBundle, Transform parent, params string[] types) where T : Component
        {
            AssetData assetData = await LoadAssetDataAsync<GameObject>(name, ".prefab", isKeep, isAssetBundle, types);
            if (assetData == null) return null;
            GameObject gameObject = (GameObject)Object.Instantiate(assetData.asset);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            T component = gameObject.AddComponent<T>();
            return component;
        }
        public void UnloadAsset(string name, params string[] types)
        {
            string path = GetAssetPath(name, types);
            if (assetDict.ContainsKey(path))
            {
                AssetData assetData = assetDict[path];
                UnloadAsset(assetData);
                assetDict.Remove(path);
            }
        }
        public void UnloadAssets()
        {
            string all = string.Empty;
            foreach (string item in assetDict.Keys)
            {
                if (!assetDict[item].isKeep)
                {
                    all += $"{item},";
                }
            }
            foreach (string item in all.Split(','))
            {
                if (string.IsNullOrEmpty(item)) continue;
                AssetData assetData = assetDict[item];
                UnloadAsset(assetData);
                assetDict.Remove(item);
            }
        }
        public void UnloadAllAssets()
        {
            foreach (AssetData item in assetDict.Values)
            {
                UnloadAsset(item);
            }
            Resources.UnloadUnusedAssets();
            assetDict.Clear();
            GC.Collect();
        }
    }
}