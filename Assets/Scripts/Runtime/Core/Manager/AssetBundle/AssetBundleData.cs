namespace LccModel
{
    public class AssetBundleData
    {
        public string assetBundleName;
        public string assetBundleHash;
        public uint assetBundleCRC;
        public long fileSize;
        public string[] assetNames;
        public AssetBundleData()
        {
        }
        public AssetBundleData(string assetBundleName, string assetBundleHash, uint assetBundleCRC, long fileSize, string[] assetNames)
        {
            this.assetBundleName = assetBundleName;
            this.assetBundleHash = assetBundleHash;
            this.assetBundleCRC = assetBundleCRC;
            this.fileSize = fileSize;
            this.assetNames = assetNames;
        }
    }
}