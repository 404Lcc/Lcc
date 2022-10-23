using BM;
using ET;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccModel
{
    public class AssetManager : Singleton<AssetManager>
    {
        private Dictionary<string, LoadHandler> _loadedDict = new Dictionary<string, LoadHandler>();
        public string GetAssetPath(string name, string suffix, params string[] types)
        {
            if (types.Length == 0) return name;
            string path = string.Empty;
            for (int i = 0; i < types.Length; i++)
            {
                path = $"{path}{types[i]}/";
                if (i == types.Length - 1)
                {
                    path = $"{path}{name}";
                }
            }
            return $"Assets/Bundles/{path}{suffix}";
        }





        public async ETTask<LoadHandler> LoadAssetAsync<T>(string name, string suffix, params string[] types) where T : Object
        {
            string path = GetAssetPath(name, suffix, types);
            if (_loadedDict.ContainsKey(path))
            {
                return _loadedDict[path];
            }
            await AssetComponent.LoadAsync<GameObject>(out LoadHandler handler, path);
            if (!_loadedDict.ContainsKey(path))
            {
                _loadedDict.Add(path, handler);
            }
            return handler;
        }
        public T LoadAsset<T>(out LoadHandler handler, string name, string suffix, params string[] types) where T : Object
        {
            string path = GetAssetPath(name, suffix, types);
            if (_loadedDict.ContainsKey(path))
            {
                handler = _loadedDict[path];
                return (T)handler.Asset;
            }
            T asset = AssetComponent.Load<T>(out handler, path);
            if (!_loadedDict.ContainsKey(path))
            {
                _loadedDict.Add(path, handler);
            }
            return asset;
        }
        public T LoadAsset<T>(string name, string suffix, params string[] types) where T : Object
        {
            return LoadAsset<T>(out LoadHandler handler, name, suffix, types);
        }





        public void UnLoadAsset(string name, string suffix, params string[] types)
        {
            //Resources.UnloadAsset仅能释放非GameObject和Component的资源 比如Texture Mesh等真正的资源 对于由Prefab加载出来的Object或Component,则不能通过该函数来进行释放
            string path = GetAssetPath(name, suffix, types);
            if (_loadedDict.ContainsKey(path))
            {
                AssetComponent.UnLoad(_loadedDict[path]);
            }

        }



        public void UnLoadAllAssets()
        {
            AssetComponent.UnLoadAllAssets();
        }





        public GameObject InstantiateAsset(string name, params string[] types)
        {
            GameObject asset = LoadAsset<GameObject>(name, AssetSuffix.Prefab, types);
            if (asset == null) return null;
            GameObject gameObject = Object.Instantiate(asset);
            gameObject.name = name;
            return gameObject;
        }
        public GameObject InstantiateAsset(string name, Transform parent, params string[] types)
        {
            GameObject asset = LoadAsset<GameObject>(name, AssetSuffix.Prefab, types);
            if (asset == null) return null;
            GameObject gameObject = Object.Instantiate(asset);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            return gameObject;
        }
    }
}