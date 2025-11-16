using YooAsset;

namespace LccHotfix
{
    public interface IAssetService : IService
    {
        public ResourcePackage DefaultPackage { get; }

        public ResourcePackage RawFilePackage { get; }
    }
}