using BM;
using ET;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LccModel
{
    //Resources.UnloadAsset仅能释放非GameObject和Component的资源 比如Texture Mesh等真正的资源 对于由Prefab加载出来的Object或Component,则不能通过该函数来进行释放
    public class AssetManager : AObjectBase
    {
        public static AssetManager Instance { get; set; }
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

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            UnLoadAllAssets();
            Instance = null;
        }
        public async ETTask<LoadHandler> LoadAssetAsync<T>(string name, string suffix, params string[] types) where T : Object
        {
            string path = GetAssetPath(name, suffix, types);
            await AssetComponent.LoadAsync<GameObject>(out LoadHandler handler, path);
            return handler;
        }
        public T LoadAsset<T>(out LoadHandler handler, string name, string suffix, params string[] types) where T : Object
        {
            string path = GetAssetPath(name, suffix, types);
            T asset = AssetComponent.Load<T>(out handler, path);
            return asset;
        }
        public void UnLoadAsset(LoadHandler handler)
        {
            AssetComponent.UnLoad(handler);
        }
        public void UnLoadAllAssets()
        {
            AssetComponent.UnLoadAllAssets();
        }
        public GameObject InstantiateAsset(out LoadHandler handler, string name, params string[] types)
        {
            GameObject asset = LoadAsset<GameObject>(out handler, name, AssetSuffix.Prefab, types);
            if (asset == null) return null;
            GameObject gameObject = Object.Instantiate(asset);
            gameObject.name = name;
            return gameObject;
        }
        public GameObject InstantiateAsset(out LoadHandler handler, string name, Transform parent, params string[] types)
        {
            GameObject asset = LoadAsset<GameObject>(out handler, name, AssetSuffix.Prefab, types);
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