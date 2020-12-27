namespace LccModel
{
    public class AssetBundleRule
    {
        public string path;
        public AssetBundleRuleType assetBundleRuleType;
        public AssetBundleRule()
        {
        }
        public AssetBundleRule(string path, AssetBundleRuleType assetBundleRuleType)
        {
            this.path = path;
            this.assetBundleRuleType = assetBundleRuleType;
        }
    }
}