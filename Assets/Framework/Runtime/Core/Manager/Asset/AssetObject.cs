using System;
using UnityEngine;
using YooAsset;

namespace LccModel
{
    public class AssetObject : MonoBehaviour
    {
        public string path;
        public Type type;
        public Action<string, UnityEngine.Object> onComplete;


        public AssetHandle handle;

        protected UnityEngine.Object res;

        public UnityEngine.Object Asset
        {
            get
            {
                return res;
            }
        }
        public void OnDestroy()
        {
            if (handle != null)
            {
                AssetManager.Instance?.UnLoadAsset(handle);
            }
        }

        public void SetInfo<T>(string path, Action<string, UnityEngine.Object> onComplete)
        {
            this.path = path;
            type = typeof(T);
            this.onComplete = onComplete;
        }
        public void SetInfo<T>(string path)
        {
            this.path = path;
            type = typeof(T);
        }

        public virtual void StartLoad()
        {
            handle = AssetManager.Instance.Package.LoadAssetAsync(path, type);
            handle.Completed += LoadEnd;
        }
        public virtual void Load()
        {
            handle = AssetManager.Instance.Package.LoadAssetSync(path, type);
            res = handle.AssetObject;
        }
        public virtual void LoadEnd(AssetHandle callback)
        {
            res = callback.AssetObject;
            this.onComplete?.Invoke(path, res);
        }
    }

    public class AssetGameObject : AssetObject
    {
        public GameObject resGameObject;

        public new Action<string, GameObject> onComplete;
        public void SetInfo(string path, Action<string, GameObject> onComplete)
        {
            this.path = path;
            type = typeof(GameObject);
            this.onComplete = onComplete;
        }

        public override void StartLoad()
        {
            handle = AssetManager.Instance.Package.LoadAssetAsync(path, type);
            handle.Completed += LoadEnd;
        }
        public override void Load()
        {
            handle = AssetManager.Instance.Package.LoadAssetSync(path, type);
            resGameObject = handle.AssetObject as GameObject;
        }
        public override void LoadEnd(AssetHandle callback)
        {
            resGameObject = callback.AssetObject as GameObject;
            this.onComplete?.Invoke(path, resGameObject);
        }
    }

    public class ALLAssetObject : MonoBehaviour
    {
        public string path;
        public Type type;
        public Action<string, UnityEngine.Object[]> onComplete;


        public AllAssetsHandle handle;

        protected UnityEngine.Object[] res;

        public UnityEngine.Object[] Assets
        {
            get
            {
                return res;
            }
        }
        public void OnDestroy()
        {
            if (handle != null)
            {
                AssetManager.Instance?.UnLoadAsset(handle);
            }
        }

        public void SetInfo<T>(string path, Action<string, UnityEngine.Object[]> onComplete)
        {
            this.path = path;
            type = typeof(T);
            this.onComplete = onComplete;
        }
        public void SetInfo<T>(string path)
        {
            this.path = path;
            type = typeof(T);
        }

        public virtual void StartLoad()
        {
            handle = AssetManager.Instance.Package.LoadAllAssetsAsync(path, type);
            handle.Completed += LoadEnd;
        }
        public virtual void Load()
        {
            handle = AssetManager.Instance.Package.LoadAllAssetsSync(path, type);
            res = handle.AllAssetObjects;
        }
        public virtual void LoadEnd(AllAssetsHandle callback)
        {
            res = callback.AllAssetObjects;
            this.onComplete?.Invoke(path, res);
        }
    }
}