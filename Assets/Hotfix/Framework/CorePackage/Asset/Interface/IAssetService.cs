using System.Collections;
using YooAsset;

namespace LccHotfix
{
    public interface IAssetService : IService
    {
        public ResourcePackage DefaultPackage { get; }

        public ResourcePackage RawFilePackage { get; }

        public void LoadAssetAsync(string location, System.Action<AssetHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0);

        public void LoadAssetAsync<T>(string location, System.Action<AssetHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0) where T : UnityEngine.Object;

        public void LoadAssetRawFileAsync(string location, System.Action<RawFileHandle> callback, EAssetGroup group = EAssetGroup.Default, uint priority = 0);

        public IEnumerator LoadAssetAsync(string location, EAssetGroup group = EAssetGroup.Default, System.Action<AssetHandle> onBegin = null, uint priority = 0);

        public IEnumerator LoadAssetAsync<T>(string location, EAssetGroup group = EAssetGroup.Default, System.Action<AssetHandle> onBegin = null, uint priority = 0) where T : UnityEngine.Object;

        public AssetHandle LoadAssetSync(string location, EAssetGroup group = EAssetGroup.Default);

        public AssetHandle LoadAssetSync<T>(string location, EAssetGroup group = EAssetGroup.Default) where T : UnityEngine.Object;

        public RawFileHandle LoadAssetRawFileSync(string location, EAssetGroup group = EAssetGroup.Default);
    }
}