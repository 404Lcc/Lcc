using ET;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace LccModel
{
    //Resources.UnloadAsset仅能释放非GameObject和Component的资源 比如Texture Mesh等真正的资源 对于由Prefab加载出来的Object或Component,则不能通过该函数来进行释放
    public class AssetManager : AObjectBase
    {
        public static AssetManager Instance { get; set; }

        public const string DefaultPackage = "DefaultPackage";

        public ResourcePackage Package => YooAssets.GetPackage(DefaultPackage);

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

            ForceUnloadAllAssets();
            Instance = null;
        }


        public void UnLoadAsset(AssetOperationHandle handle)
        {
            handle.Release();
        }
        public void ForceUnloadAllAssets()
        {
            Package.ForceUnloadAllAssets();
        }
        public void UnloadUnusedAssets()
        {
            Package.UnloadUnusedAssets();
        }
        public GameObject InstantiateAsset(out AssetOperationHandle handle, string name, params string[] types)
        {
            GameObject asset = LoadAsset<GameObject>(out handle, name, AssetSuffix.Prefab, types);
            if (asset == null) return null;
            GameObject gameObject = Object.Instantiate(asset);
            gameObject.name = name;
            return gameObject;
        }
        public GameObject InstantiateAsset(out AssetOperationHandle handle, string name, Transform parent, params string[] types)
        {
            GameObject asset = LoadAsset<GameObject>(out handle, name, AssetSuffix.Prefab, types);
            if (asset == null) return null;
            GameObject gameObject = Object.Instantiate(asset);
            gameObject.name = name;
            gameObject.transform.SetParent(parent);
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;
            return gameObject;
        }



        public T LoadAsset<T>(out AssetOperationHandle handle, string name, string suffix, params string[] types) where T : Object
        {
            string path = GetAssetPath(name, suffix, types);
            handle = Package.LoadAssetSync<T>(path);
            return handle.AssetObject as T;
        }
        public async ETTask<AssetOperationHandle> LoadAssetAsync<T>(string name, string suffix, params string[] types) where T : Object
        {
            string path = GetAssetPath(name, suffix, types);
            AssetOperationHandle handle = Package.LoadAssetAsync<T>(path);
            await handle.Task;
            return handle;
        }
    }
}