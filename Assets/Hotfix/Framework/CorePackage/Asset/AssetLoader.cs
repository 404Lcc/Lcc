using System;
using System.Collections;
using System.Collections.Generic;
using YooAsset;

namespace LccHotfix
{
    public abstract class AssetLoaderComponent : IAssetLoader, IDisposable
    {
        private IAssetLoader _loader = new AssetLoader();

        public virtual void Dispose() => _loader.Release();
        public void Release() => _loader.Release();
        public void Release(string location) => _loader.Release(location);
        public AssetHandle TryGetAsset(string location) => _loader.TryGetAsset(location);
        public void LoadAssetAsync(string location, System.Action<AssetHandle> callback, uint priority = 0) => _loader.LoadAssetAsync(location, callback, priority);
        public void LoadAssetAsync<T>(string location, System.Action<AssetHandle> callback, uint priority = 0) where T : UnityEngine.Object => _loader.LoadAssetAsync<T>(location, callback, priority);
        public void LoadAssetRawFileAsync(string location, System.Action<RawFileHandle> onCompleted, uint priority = 0) => _loader.LoadAssetRawFileAsync(location, onCompleted, priority);
        public IEnumerator LoadAssetCoro(string location, System.Action<AssetHandle> onBegin = null, uint priority = 0) => _loader.LoadAssetCoro(location, onBegin, priority);
        public IEnumerator LoadAssetCoro<T>(string location, System.Action<AssetHandle> onBegin = null, uint priority = 0) where T : UnityEngine.Object => _loader.LoadAssetCoro<T>(location, onBegin, priority);
        public AssetHandle LoadAssetSync(string location) => _loader.LoadAssetSync(location);
        public AssetHandle LoadAssetSync<T>(string location) => _loader.LoadAssetSync<T>(location);
        public RawFileHandle LoadAssetRawFileSync(string location) => _loader.LoadAssetRawFileSync(location);
    }

    public class AssetLoader : IAssetLoader
    {
        private Dictionary<string, AssetHandle> _assetHandles = new Dictionary<string, AssetHandle>();
        private Dictionary<string, RawFileHandle> _rawFileHandles = new Dictionary<string, RawFileHandle>();

        public static IAssetLoader Create()
        {
            return new AssetLoader();
        }

        public void Release()
        {
            if (_assetHandles != null)
            {
                foreach (var handle in _assetHandles)
                {
                    handle.Value.Release();
                }

                _assetHandles.Clear();
            }

            if (_rawFileHandles != null)
            {
                foreach (var handle in _rawFileHandles)
                {
                    handle.Value.Release();
                }

                _rawFileHandles.Clear();
            }
        }

        public void Release(string location)
        {
            if (_assetHandles != null && _assetHandles.TryGetValue(location, out var handle))
            {
                handle.Release();
                _assetHandles.Remove(location);
            }
            else
            {
                if (_rawFileHandles != null && _rawFileHandles.TryGetValue(location, out var rawFileHandle))
                {
                    rawFileHandle.Release();
                    _rawFileHandles.Remove(location);
                }
            }
        }

        public AssetHandle TryGetAsset(string location)
        {
            return _assetHandles.TryGetValue(location, out var handle) ? handle : null;
        }

        public void LoadAssetAsync(string location, System.Action<AssetHandle> callback, uint priority = 0)
        {
            if (_assetHandles.TryGetValue(location, out var handle))
            {
                if (handle.IsDone)
                {
                    callback.Invoke(handle);
                    return;
                }

                handle.Completed += callback;
            }
            else
            {
                handle = Main.AssetService.DefaultPackage.LoadAssetAsync(location, priority);
                _assetHandles.Add(location, handle);
                handle.Completed += callback;
            }
        }

        public void LoadAssetAsync<T>(string location, System.Action<AssetHandle> onCompleted, uint priority = 0) where T : UnityEngine.Object
        {
            if (_assetHandles.TryGetValue(location, out var handle))
            {
                if (handle.IsDone)
                {
                    onCompleted.Invoke(handle);
                    return;
                }

                handle.Completed += onCompleted;
            }
            else
            {
                handle = Main.AssetService.DefaultPackage.LoadAssetAsync(location, typeof(T), priority);
                _assetHandles.Add(location, handle);
                handle.Completed += onCompleted;
            }
        }

        public void LoadAssetRawFileAsync(string location, System.Action<RawFileHandle> onCompleted, uint priority = 0)
        {
            if (_rawFileHandles.TryGetValue(location, out var handle))
            {
                if (handle.IsDone)
                {
                    onCompleted.Invoke(handle);
                    return;
                }

                handle.Completed += onCompleted;
            }
            else
            {
                handle = Main.AssetService.RawFilePackage.LoadRawFileAsync(location, priority);
                _rawFileHandles.Add(location, handle);
                handle.Completed += onCompleted;
            }
        }

        public IEnumerator LoadAssetCoro(string location, System.Action<AssetHandle> onBegin = null, uint priority = 0)
        {
            if (_assetHandles.TryGetValue(location, out var handle))
            {
                onBegin?.Invoke(handle);
                while (!handle.IsDone)
                    yield return null;
            }
            else
            {
                handle = Main.AssetService.DefaultPackage.LoadAssetAsync(location, priority);
                _assetHandles.Add(location, handle);
                onBegin?.Invoke(handle);
            }

            yield return handle;
        }

        public IEnumerator LoadAssetCoro<T>(string location, System.Action<AssetHandle> onBegin = null, uint priority = 0) where T : UnityEngine.Object
        {
            if (_assetHandles.TryGetValue(location, out var handle))
            {
                onBegin?.Invoke(handle);
                while (!handle.IsDone)
                    yield return null;
            }
            else
            {
                handle = Main.AssetService.DefaultPackage.LoadAssetAsync(location, typeof(T), priority);
                _assetHandles.Add(location, handle);
                onBegin?.Invoke(handle);
            }

            yield return handle;
        }

        // 只用于加载小资产，Web平台此接口内部会异步接口并同步等待
        public AssetHandle LoadAssetSync(string location)
        {
            if (_assetHandles.TryGetValue(location, out var handle))
            {
                return handle;
            }

            handle = Main.AssetService.DefaultPackage.LoadAssetSync(location);
            _assetHandles.Add(location, handle);
            return handle;
        }

        // 只用于加载小资产，Web平台此接口内部会异步接口并同步等待
        public AssetHandle LoadAssetSync<T>(string location)
        {
            if (_assetHandles.TryGetValue(location, out var handle))
            {
                return handle;
            }

            handle = Main.AssetService.DefaultPackage.LoadAssetSync(location, typeof(T));
            _assetHandles.Add(location, handle);
            return handle;
        }

        public RawFileHandle LoadAssetRawFileSync(string location)
        {
            if (_rawFileHandles.TryGetValue(location, out var handle))
            {
                return handle;
            }

            handle = Main.AssetService.RawFilePackage.LoadRawFileSync(location);
            _rawFileHandles.Add(location, handle);
            return handle;
        }
    }
}