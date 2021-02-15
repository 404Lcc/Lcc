using LccModel;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace LccEditor
{
    [CreateAssetMenu(fileName = "AssetBundleSetting", menuName = "CreateAssetBundleSetting", order = 1)]
    public class AssetBundleSetting : ScriptableObject
    {
        [InfoBox("版本号")]
        public int buildId;
        [InfoBox("输出路径")]
        public string outputPath;
        [InfoBox("是否拷贝到StreamingAssets")]
        public bool isCopyStreamingAssets;
        [ShowInInspector]
        public List<AssetBundleRule> assetBundleRuleList;
        [ShowInInspector]
        public List<AssetBundleData> assetBundleDataList;
        [Button("清除AssetBundle配置")]
        public void Clear()
        {
            buildId = 0;
            outputPath = string.Empty;
            isCopyStreamingAssets = true;
            assetBundleRuleList = null;
            assetBundleDataList = null;
        }
    }
}