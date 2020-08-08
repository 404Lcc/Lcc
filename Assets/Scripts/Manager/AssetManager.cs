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
        public Dictionary<string, AssetData> assetdic;
        //public Dictionary<string, AssetRequest> assetrequestdic;
        void Awake()
        {
            InitManager();
        }
        public void InitManager()
        {
            assetdic = new Dictionary<string, AssetData>();
            //assetrequestdic = new Dictionary<string, AssetRequest>();
        }
        private AssetData LoadAssetData(string name, string suffix, bool bkeep, Type type, bool assetbundlemodel, params AssetType[] types)
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
            if (assetdic.ContainsKey(path))
            {
                return assetdic[path];
            }
            else
            {
                AssetData assetdata = new AssetData();
#if AssetBundle
                //AssetBundle assetbundle = AssetBundle.LoadFromFile(path);
                //Object asset = assetbundle.LoadAsset(assetbundle.GetAllAssetNames()[0]);
                //assetbundle.Unload(false);
                if (assetbundlemodel)
                {
                    //AssetRequest request = LoadAsset("Assets/Resources/" + path + suffix, type);
                    //Object asset = request.asset;
                    //assetdata.asset = asset;
                    //assetdata.types = types;
                    //assetdata.name = name;
                    //assetdata.bkeep = bkeep;
                    //assetdic.Add(path, assetdata);
                    //assetrequestdic.Add(path, request);
                }
                else
                {
                    Object asset = Resources.Load(path, type);
                    assetdata.asset = asset;
                    assetdata.types = types;
                    assetdata.name = name;
                    assetdata.bkeep = bkeep;
                    assetdic.Add(path, assetdata);
                }
#else
                Object asset = Resources.Load(path, type);
                assetdata.asset = asset;
                assetdata.types = types;
                assetdata.name = name;
                assetdata.bkeep = bkeep;
                assetdic.Add(path, assetdata);
#endif
                return assetdata;
            }
        }
        public void UnloadAssetsData()
        {
            string all = string.Empty;
            foreach (string item in assetdic.Keys)
            {
                if (!assetdic[item].bkeep)
                {
                    all += item + ",";
                }
            }
            foreach (string item in all.Split(','))
            {
                if (string.IsNullOrEmpty(item)) continue;
                Object temp = assetdic[item].asset;
                if (item.Contains("."))
                {
                    if (temp)
                    {
                        if (temp.GetType() != typeof(GameObject) && temp.GetType() != typeof(Component))
                        {
                            //Resources.UnloadAsset仅能释放非GameObject和Component的资源 比如Texture Mesh等真正的资源 对于由Prefab加载出来的Object或Component,则不能通过该函数来进行释放
                            Resources.UnloadAsset(temp);
                            assetdic.Remove(item);
                        }
                    }
                    else
                    {
                        assetdic.Remove(item);
                    }
                }
                else
                {
                    //assetrequestdic[item].Release();
                    assetdic.Remove(item);
                    //assetrequestdic.Remove(item);
                }
            }
        }
        public void UnloadAllAssetsData()
        {
            //foreach (var item in assetrequestdic.Values)
            //{
            //    item.Release();
            //}
            assetdic.Clear();
            //assetrequestdic.Clear();
            Resources.UnloadUnusedAssets();
            //Assets.RemoveUnusedAssets();
            GC.Collect();
        }
        public T LoadAssetData<T>(string name, string suffix, bool bkeep, bool assetbundlemodel, params AssetType[] types) where T : Object
        {
            AssetData asset = LoadAssetData(name, suffix, bkeep, typeof(T), assetbundlemodel, types);
            return (T)asset.asset;
        }
        public GameObject LoadGameObject(string name, bool bkeep, bool assetbundlemodel, params AssetType[] types)
        {
            AssetData assetdata = LoadAssetData(name, "*.prefab", bkeep, typeof(Object), assetbundlemodel, types);
            if (assetdata.asset == null) return null;
            GameObject obj = Instantiate(assetdata.asset) as GameObject;
            return obj;
        }
        public T LoadGameObject<T>(string name, bool bkeep, bool assetbundlemodel, params AssetType[] types) where T : Component
        {
            AssetData assetdata = LoadAssetData(name, "*.prefab", bkeep, typeof(Object), assetbundlemodel, types);
            if (assetdata.asset == null) return null;
            GameObject obj = Instantiate(assetdata.asset) as GameObject;
            T component = GameUtil.GetComponent<T>(obj);
            return component;
        }
        public GameObject LoadGameObject(string name, bool bkeep, Transform parent, bool assetbundlemodel, params AssetType[] types)
        {
            AssetData assetdata = LoadAssetData(name, "*.prefab", bkeep, typeof(Object), assetbundlemodel, types);
            if (assetdata.asset == null) return null;
            GameObject obj = Instantiate(assetdata.asset) as GameObject;
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            return obj;
        }
        public T LoadGameObject<T>(string name, bool bkeep, Transform parent, bool assetbundlemodel, params AssetType[] types) where T : Component
        {
            AssetData assetdata = LoadAssetData(name, "*.prefab", bkeep, typeof(Object), assetbundlemodel, types);
            if (assetdata.asset == null) return null;
            GameObject obj = Instantiate(assetdata.asset) as GameObject;
            obj.transform.SetParent(parent);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            T component = GameUtil.GetComponent<T>(obj);
            return component;
        }
        public byte[] LoadBytes(string path, string name)
        {
            Stream stream;
            FileInfo info = new FileInfo(path + name);
            if (info.Exists)
            {
                stream = info.Open(FileMode.Open);
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, (int)stream.Length);
                stream.Close();
                stream.Dispose();
                return bytes;
            }
            return null;
        }
        //public AssetRequest LoadAsset(string path, Type type)
        //{
        //    var request = Assets.LoadAsset(path, type);
        //    return request;
        //}
        //public IEnumerator LoadAssetAsync(string path, Type type)
        //{
        //    yield return Assets.LoadAssetAsync(path, type);
        //}
    }
}