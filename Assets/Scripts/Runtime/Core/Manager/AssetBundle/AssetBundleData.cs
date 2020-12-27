namespace LccModel
{
    public class AssetBundleData
    {
        public string assetBundleName;
        public string assetBundleHash;
        public string[] assetNames;
        public AssetBundleData()
        {
        }
        public AssetBundleData(string assetBundleName, string assetBundleHash, string[] assetNames)
        {
            this.assetBundleName = assetBundleName;
            this.assetBundleHash = assetBundleHash;
            this.assetNames = assetNames;
        }
    }
}