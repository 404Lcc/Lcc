using System.Collections.Generic;

namespace LccModel
{
    public class AssetBundleConfig
    {
        public int version;
        public Dictionary<string, AssetBundleData> assetBundleDataDict;
        public Dictionary<string, AssetBundleRuleType> assetBundleRuleTypeDict;
        public AssetBundleConfig()
        {
        }
        public AssetBundleConfig(int version, Dictionary<string, AssetBundleData> assetBundleDataDict, Dictionary<string, AssetBundleRuleType> assetBundleRuleTypeDict)
        {
            this.version = version;
            this.assetBundleDataDict = assetBundleDataDict;
            this.assetBundleRuleTypeDict = assetBundleRuleTypeDict;
        }
    }
}