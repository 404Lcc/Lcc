using BM;
using ET;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class AssetBundleManager : Singleton<AssetBundleManager>
    {
        public const string DefaultBundlePackageName = "ALL";
        private Dictionary<string, bool> _updatePackageBundle = new Dictionary<string, bool>();
        public async ETTask InitManager()
        {
            AssetComponentConfig.DefaultBundlePackageName = DefaultBundlePackageName;
            _updatePackageBundle.Add(AssetComponentConfig.DefaultBundlePackageName, false);
            await LoadingPanel.Instance.UpdateLoadingPercent(0, 10);
            UpdateBundleDataInfo updateBundleDataInfo = await AssetComponent.CheckAllBundlePackageUpdate(_updatePackageBundle);
            await LoadingPanel.Instance.UpdateLoadingPercent(11, 20);
            if (updateBundleDataInfo.NeedUpdate)
            {
                Debug.LogError("需要更新, 大小: " + updateBundleDataInfo.NeedUpdateSize);
                await AssetComponent.DownLoadUpdate(updateBundleDataInfo);
            }
            await LoadingPanel.Instance.UpdateLoadingPercent(21, 30);
            await AssetComponent.Initialize(AssetComponentConfig.DefaultBundlePackageName);
            await LoadingPanel.Instance.UpdateLoadingPercent(31, 40);
        }
    }
}