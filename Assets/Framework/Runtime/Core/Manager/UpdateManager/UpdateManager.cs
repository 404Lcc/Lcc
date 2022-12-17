using BM;
using ET;
using System.Collections.Generic;
using UnityEngine;

namespace LccModel
{
    public class UpdateManager : AObjectBase
    {
        public static UpdateManager Instance { get; set; }
        public const string DefaultBundlePackageName = "ALL";

        public override void Awake()
        {
            base.Awake();

            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;
        }


        public async ETTask StartUpdate()
        {
            AssetComponentConfig.DefaultBundlePackageName = DefaultBundlePackageName;
            Dictionary<string, bool> updatePackageBundle = new Dictionary<string, bool>();
            updatePackageBundle.Add(AssetComponentConfig.DefaultBundlePackageName, false);

            await UpdatePanel.Instance.UpdateLoadingPercent(0, 10);

            UpdateBundleDataInfo updateBundleDataInfo = await AssetComponent.CheckAllBundlePackageUpdate(updatePackageBundle);

            await UpdatePanel.Instance.UpdateLoadingPercent(11, 20);
            if (updateBundleDataInfo.NeedUpdate)
            {
                Debug.LogError("需要更新, 大小: " + updateBundleDataInfo.NeedUpdateSize);
                await AssetComponent.DownLoadUpdate(updateBundleDataInfo);
            }

            await UpdatePanel.Instance.UpdateLoadingPercent(21, 30);

            await AssetComponent.Initialize(AssetComponentConfig.DefaultBundlePackageName);

            await UpdatePanel.Instance.UpdateLoadingPercent(31, 40);
        }
    }
}