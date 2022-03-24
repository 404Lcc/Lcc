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
            UpdateBundleDataInfo updateBundleDataInfo = await AssetComponent.CheckAllBundlePackageUpdate(_updatePackageBundle);
            if (updateBundleDataInfo.NeedUpdate)
            {
                Debug.LogError("需要更新, 大小: " + updateBundleDataInfo.NeedUpdateSize);
                await AssetComponent.DownLoadUpdate(updateBundleDataInfo);
            }
            await AssetComponent.Initialize(AssetComponentConfig.DefaultBundlePackageName);
        }
    }
}