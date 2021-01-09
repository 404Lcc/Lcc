using LccModel;
using System.Collections.Generic;
using UnityEngine;

namespace LccEditor
{
    [CreateAssetMenu(fileName = "AssetBundleSetting", menuName = "CreateAssetBundleSetting", order = 1)]
    public class AssetBundleSetting : ScriptableObject
    {
        public int buildId;
        public string outputPath;
        public Dictionary<string, AssetBundleRule> assetBundleRuleDict;
        public Dictionary<string, AssetBundleData> assetBundleDataDict;
        public Dictionary<string, AssetBundleRuleType> assetBundleRuleTypeDict;
    }
}