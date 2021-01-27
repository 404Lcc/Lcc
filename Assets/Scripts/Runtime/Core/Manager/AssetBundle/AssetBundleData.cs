namespace LccModel
{
    public class AssetBundleData
    {
        public string assetBundleName;
        public string assetBundleHash;
        public uint assetBundleCRC;
        public string[] assetNames;
        public AssetBundleData()
        {
        }
        public AssetBundleData(string assetBundleName, string assetBundleHash, uint assetBundleCRC, string[] assetNames)
        {
            this.assetBundleName = assetBundleName;
            this.assetBundleHash = assetBundleHash;
            this.assetBundleCRC = assetBundleCRC;
            this.assetNames = assetNames;
        }
    }
}