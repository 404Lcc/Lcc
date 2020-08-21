//using libx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Model
{
    public class AssetManager : MonoBehaviour
    {
        public Dictionary<string, AssetData> assetDic;
        //public Dictionary<string, AssetRequest> assetRequestDic;
        void Awake()
        {
            InitManager();
        }
        public void InitManager()
        {
            assetDic = new Dictionary<string, AssetData>();
            //assetRequestDic = new Dictionary<string, AssetRequest>();
        }
        private AssetData LoadAssetData(string name, string suffix, bool keep, Type type, bool assetBundleMode, params string[] types)
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
                if (assetBundleMode)
                {
                    //AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
                    //Object asset = assetBundle.LoadAsset(assetBundle.GetAllAssetNames()[0]);
                    //assetBundle.Unload(false);

                    //AssetRequest request = LoadAsset("Assets/Resources/" + path + suffix, type);
                    //Object asset = request.asset;
                    //assetData.asset = asset;
                    //assetData.types = types;
                    //assetData.name = name;
                    //assetData.keep = keep;
                    //assetDic.Add(path, assetData);
                    //assetRequestDic.Add(path, request);
                }
                else
                {
                    Object asset = Resources.Load(path, type);
                    assetData.asset = asset;
                    assetData.types = types;
                    assetData.name = name;
                    assetData.keep = keep;
                    assetDic.Add(path, assetData);
                }
#else
                Object asset = Resources.Load(path, type);
                assetData.asset = asset;
                assetData.types = types;
                assetData.name = name;
                assetData.keep = keep;
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
                if (!assetDic[item].keep)
                {
                    all += item + ",";
                }
            }
            foreach (string item in all.Split(','))
            {
                if (string.IsNullOrEmpty(item)) continue;
                Object obj = assetDic[item].asset;
                if (item.Contains("."))
                {
                    if (obj)
                    {
                        if (obj.GetType() != typeof(GameObject) && obj.GetType() != typeof(Component))
                        {
                            //Resources.UnloadAsset仅能释放非GameObject和Component的资源 比如Texture Mesh等真正的资源 对于由Prefab加载出来的Object或Component,则不能通过该函数来进行释放
                            Resources.UnloadAsset(obj);
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
        public T LoadAssetData<T>(string name, string suffix, bool keep, bool assetBundleMode, params string[] types) where T : Object
        {
            AssetData asset = LoadAssetData(name, suffix, keep, typeof(T), assetBundleMode, types);
            return (T)asset.asset;
        }
        public GameObject LoadGameObject(string name, bool keep, bool assetBundleMode, params string[] types)
        {
            AssetData assetData = LoadAssetData(name, "*.prefab", keep, typeof(Object), assetBundleMode, types);
            if (assetData.asset == null) return null;
            GameObject obj = Instantiate(assetData.asset) as GameObject;
            return obj;
        }
        public T LoadGameObject<T>(string name, bool keep, bool assetBundleMode, params string[] types) where T : Component
        {
            AssetData assetData = LoadAssetData(name, "*.prefab", keep, typeof(Object), assetBundleMode, types);
            if (assetData.asset == null) return null;
            GameObject obj = Instantiate(assetData.asset) as GameObject;
            T component = GameUtil.GetComponent<T>(obj);
            return component;
        }
        public GameObject LoadGameObject(string name, bool keep, bool assetBundleMode, Transform parent, params string[] types)
        {
            AssetData assetData = LoadAssetData(name, "*.prefab", keep, typeof(Object), assetBundleMode, types);
            if (assetData.asset == null) return null;
            GameObject obj = Instantiate(assetData.asset) as GameObject;
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            return obj;
        }
        public T LoadGameObject<T>(string name, bool keep, bool assetBundleMode, Transform parent, params string[] types) where T : Component
        {
            AssetData assetdata = LoadAssetData(name, "*.prefab", keep, typeof(Object), assetBundleMode, types);
            if (assetdata.asset == null) return null;
            GameObject obj = Instantiate(assetdata.asset) as GameObject;
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            T component = GameUtil.GetComponent<T>(obj);
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