using System;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace LccModel
{
    public class AssetObject : MonoBehaviour
    {
        public string path;
        public object userData;
        public Type type;
        public Action<string, Object, AssetObject> onComplete;

        public AssetHandle handle;

        protected Object res;

        public Object Asset
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

        public void SetInfo<T>(string path, object userData, Action<string, Object, AssetObject> onComplete)
        {
            this.path = path;
            this.userData = userData;
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
            this.onComplete?.Invoke(path, res, this);
        }
    }

    //allunity资源
    public class ALLAssetObject : MonoBehaviour
    {
        public string path;
        public Type type;
        public Action<string, Object[]> onComplete;

        public AllAssetsHandle handle;

        protected Object[] res;

        public Object[] Assets
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

        public void SetInfo<T>(string path, Action<string, Object[]> onComplete)
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
            res = (Object[])handle.AllAssetObjects;
        }
        public virtual void LoadEnd(AllAssetsHandle callback)
        {
            res = (Object[])callback.AllAssetObjects;
            this.onComplete?.Invoke(path, res);
        }
    }


    //原生资源
    public class RawObject : MonoBehaviour
    {
        public string path;
        public Action<string, byte[]> onComplete;

        public RawFileHandle handle;

        protected byte[] res;

        public byte[] Asset
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

        public void SetInfo(string path, Action<string, byte[]> onComplete)
        {
            this.path = path;
            this.onComplete = onComplete;
        }
        public void SetInfo(string path)
        {
            this.path = path;
        }

        public virtual void StartLoad()
        {
            handle = AssetManager.Instance.Package.LoadRawFileAsync(path);
            handle.Completed += LoadEnd;
        }
        public virtual void Load()
        {
            handle = AssetManager.Instance.Package.LoadRawFileSync(path);
            res = handle.GetRawFileData();
        }
        public virtual void LoadEnd(RawFileHandle callback)
        {
            res = callback.GetRawFileData();
            this.onComplete?.Invoke(path, res);
        }
    }
}