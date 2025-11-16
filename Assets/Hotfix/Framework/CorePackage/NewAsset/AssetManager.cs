using System;
using System.Collections;
using System.Collections.Generic;
using YooAsset;

namespace LccHotfix
{
    public static class AssetExtension
    {
        public static AssetHandle AssetHandle(this IEnumerator proto)
        {
            if (proto == null)
                throw new ArgumentNullException(nameof(proto));
            if (proto.Current is not AssetHandle handle)
                throw new InvalidOperationException("No valid AssetHandle available.");
            return handle;
        }
    }

    public interface IAssetService : IService
    {
        public ResourcePackage DefaultPackage { get; }

        public ResourcePackage RawFilePackage { get; }
    }

    internal class AssetManager : Module, IAssetService
    {
        public const string DefaultPackageName = "DefaultPackage";
        public const string RawFilePackageName = "RawFilePackage";

        public ResourcePackage DefaultPackage { get; private set; }
        public ResourcePackage RawFilePackage { get; private set; }

        private readonly Dictionary<EAssetGroup, AssetLoader> _loader = new();

        public AssetManager()
        {
            DefaultPackage = YooAssets.GetPackage(DefaultPackageName);
            RawFilePackage = YooAssets.GetPackage(RawFilePackageName);
        }

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {

        }

        internal override void Shutdown()
        {
            foreach (var loader in _loader.Values)
                loader.Release();
            _loader.Clear();
        }

        public void Release(EAssetGroup group)
        {
            if (_loader.TryGetValue(group, out var loader))
                loader.Release();
            _loader.Remove(group);
        }

        public void Release(string location, EAssetGroup group = EAssetGroup.Default)
        {
            if (_loader.TryGetValue(group, out var loader))
                loader.Release(location);
        }

        public void LoadAssetAsync(string location, System.Action<AssetHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0)
        {
            GetOrCreateLoader(group).LoadAssetAsync(location, callback, priority);
        }

        public void LoadAssetAsync<T>(string location, System.Action<AssetHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0) where T : UnityEngine.Object
        {
            GetOrCreateLoader(group).LoadAssetAsync<T>(location, callback, priority);
        }

        public IEnumerator LoadAssetAsync(string location, EAssetGroup group = EAssetGroup.Default, System.Action<AssetHandle> onBegin = null, uint priority = 0)
        {
            return GetOrCreateLoader(group).LoadAssetCoro(location, onBegin, priority);
        }

        public IEnumerator LoadAssetAsync<T>(string location, EAssetGroup group = EAssetGroup.Default, System.Action<AssetHandle> onBegin = null, uint priority = 0) where T : UnityEngine.Object
        {
            return GetOrCreateLoader(group).LoadAssetCoro<T>(location, onBegin, priority);
        }

        public AssetHandle LoadAssetSync(string location, EAssetGroup group = EAssetGroup.Default)
        {
            return GetOrCreateLoader(group).LoadAssetSync(location);
        }

        public AssetHandle LoadAssetSync<T>(string location, EAssetGroup group = EAssetGroup.Default) where T : UnityEngine.Object
        {
            return GetOrCreateLoader(group).LoadAssetSync<T>(location);
        }

        public RawFileHandle LoadAssetRawFileSync(string location, EAssetGroup group = EAssetGroup.Default)
        {
            return GetOrCreateLoader(group).LoadAssetRawFileSync(location);
        }

        private AssetLoader GetOrCreateLoader(EAssetGroup group)
        {
            if (_loader.TryGetValue(group, out var loader))
                return loader;
            loader = new AssetLoader();
            _loader.Add(group, loader);
            return loader;
        }
    }
}