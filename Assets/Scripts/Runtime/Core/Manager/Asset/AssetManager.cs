//using libx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Model
{
    public class AssetManager : Singleton<AssetManager>
    {
        public Dictionary<string, AssetData> assetDic = new Dictionary<string, AssetData>();
        //public Dictionary<string, AssetRequest> assetRequestDic = new Dictionary<string, AssetRequest>();
        private AssetData LoadAssetData(string name, string suffix, bool isKeep, Type type, bool isAssetBundle, params string[] types)
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
                    //AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
                    //Object asset = assetBundle.LoadAsset(assetBundle.GetAllAssetNames()[0]);
                    //assetBundle.Unload(false);

                    //AssetRequest request = LoadAsset("Assets/Resources/" + path + suffix, type);
                    //Object asset = request.asset;
                    //assetData.asset = asset;
                    //assetData.types = types;
                    //assetData.name = name;
                    //assetData.isKeep = isKeep;
                    //assetDic.Add(path, assetData);
                    //assetRequestDic.Add(path, request);
                }
                else
                {
                    Object asset = Resources.Load(path, type);
                    assetData.asset = asset;
                    assetData.types = types;
                    assetData.name = name;
                    assetData.isKeep = isKeep;
                    assetDic.Add(path, assetData);
                }
#else
                Object asset = Resources.Load(path, type);
                assetData.asset = asset;
                assetData.types = types;
                assetData.name = name;
                assetData.isKeep = isKeep;
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
                    //assetRequestDic[item].Release();
                    assetDic.Remove(item);
                    //assetRequestDic.Remove(item);
                }
            }
        }
        public void UnloadAllAssetsData()
        {
            //foreach (AssetRequest item in assetRequestDic.Values)
            //{
            //    item.Release();
            //}
            assetDic.Clear();
            //assetRequestDic.Clear();
            Resources.UnloadUnusedAssets();
            //Assets.RemoveUnusedAssets();
            GC.Collect();
        }
        public T LoadAssetData<T>(string name, string suffix, bool isKeep, bool isAssetBundle, params string[] types) where T : Object
        {
            AssetData assetData = LoadAssetData(name, suffix, isKeep, typeof(T), isAssetBundle, types);
            return (T)assetData.asset;
        }
        public GameObject LoadGameObject(string name, bool isKeep, bool isAssetBundle, params string[] types)
        {
            AssetData assetData = LoadAssetData(name, ".prefab", isKeep, typeof(Object), isAssetBundle, types);
            if (assetData.asset == null) return null;
            GameObject gameObject = (GameObject)Object.Instantiate(assetData.asset);
            gameObject.name = name;
            return gameObject;
        }
        public T LoadGameObject<T>(string name, bool isKeep, bool isAssetBundle, params string[] types) where T : Component
        {
            AssetData assetData = LoadAssetData(name, ".prefab", isKeep, typeof(Object), isAssetBundle, types);
            if (assetData.asset == null) return null;
            GameObject gameObject = (GameObject)Object.Instantiate(assetData.asset);
            gameObject.name = name;
            T component = gameObject.GetComponent<T>();
            return component;
        }
        public GameObject LoadGameObject(string name, bool isKeep, bool isAssetBundle, Transform parent, params string[] types)
        {
            AssetData assetData = LoadAssetData(name, ".prefab", isKeep, typeof(Object), isAssetBundle, types);
            if (assetData.asset == null) return null;
            GameObject gameObject = (GameObject)Object.Instantiate(assetData.asset);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            return gameObject;
        }
        public T LoadGameObject<T>(string name, bool isKeep, bool isAssetBundle, Transform parent, params string[] types) where T : Component
        {
            AssetData assetdata = LoadAssetData(name, ".prefab", isKeep, typeof(Object), isAssetBundle, types);
            if (assetdata.asset == null) return null;
            GameObject gameObject = (GameObject)Object.Instantiate(assetdata.asset);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            T component = gameObject.GetComponent<T>();
            return component;
        }
        //public AssetRequest LoadAsset(string path, Type type)
        //{
        //    AssetRequest request = Assets.LoadAsset(path, type);
        //    return request;
        //}
        //public IEnumerator LoadAssetAsync(string path, Type type)
        //{
        //    yield return Assets.LoadAssetAsync(path, type);
        //}
    }
}