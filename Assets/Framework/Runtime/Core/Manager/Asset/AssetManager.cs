using ET;
using UnityEngine;
using UnityEngine.SceneManagement;
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


        public void UnLoadAsset(AssetHandle handle)
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
        


        public T AutoLoadAsset<T>(Transform transform, string name, string suffix, params string[] types) where T : Object
        {
            string path = GetAssetPath(name, suffix, types);

            GameObject gameObject = new GameObject();
            AssetObject assetObject = gameObject.AddComponent<AssetObject>();
            gameObject.name = "resPath" + path;
            gameObject.transform.SetParent(transform);

            AssetHandle handle = Package.LoadAssetSync<T>(path);
            assetObject.handle = handle;
            return handle.AssetObject as T;
        }
        public async ETTask<T> AutoLoadAssetAsync<T>(Transform transform, string name, string suffix, params string[] types) where T : Object
        {
            string path = GetAssetPath(name, suffix, types);

            GameObject gameObject = new GameObject();
            AssetObject assetObject = gameObject.AddComponent<AssetObject>();
            gameObject.name = "resPath" + path;
            gameObject.transform.SetParent(transform);

            AssetHandle handle = Package.LoadAssetAsync<T>(path);
            assetObject.handle = handle;
            await handle.Task;
            return handle.AssetObject as T;
        }

        public T LoadAsset<T>(out AssetHandle handle, string name, string suffix, params string[] types) where T : Object
        {
            string path = GetAssetPath(name, suffix, types);
            handle = Package.LoadAssetSync<T>(path);
            return handle.AssetObject as T;
        }
        public async ETTask<AssetHandle> LoadAssetAsync<T>(string name, string suffix, params string[] types) where T : Object
        {
            string path = GetAssetPath(name, suffix, types);
            AssetHandle handle = Package.LoadAssetAsync<T>(path);
            await handle.Task;
            return handle;
        }

        public Object[] LoadAllAssets(out AllAssetsHandle handle, string name, string suffix, params string[] types)
        {
            string path = GetAssetPath(name, suffix, types);
            handle = Package.LoadAllAssetsAsync(path);
            return handle.AllAssetObjects;
        }

        public async ETTask<UnityEngine.SceneManagement.Scene> LoadSceneAsync(string name, LoadSceneMode sceneMode, bool activateOnLoad, params string[] types)
        {
            string path = GetAssetPath(name, "", types);
            SceneHandle handle = Package.LoadSceneAsync(path, sceneMode, activateOnLoad);
            await handle.Task;
            return handle.SceneObject;
        }
    }
}