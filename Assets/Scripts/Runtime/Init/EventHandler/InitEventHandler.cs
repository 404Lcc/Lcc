using ET;
using System.IO;
using UnityEngine;

namespace LccModel
{
    [EventHandler]
    public class InitEventHandler : AEvent<Start>
    {
        public override async ETTask Publish(Start data)
        {
            await LaunchPanel.Instance.ShowLaunch();



            DownloadManager.Instance.InitManager();
            await AssetBundleManager.Instance.InitManager();

            ConfigManager.Instance.InitManager();

        

            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(AssetType.Prefab, AssetType.Panel));
            TipsManager.Instance.InitManager(new TipsPool(10));
            TipsWindowManager.Instance.InitManager(new TipsWindowPool(10));

            GameDataManager.Instance.InitManager("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");

            GameSettingManager.Instance.InitManager();


            await LoadingPanel.Instance.UpdateLoadingPercent(41, 50);

#if ILRuntime
            ILRuntimeManager.Instance.InitManager();
#else
            MonoManager.Instance.InitManager();
#endif
        }
    }
}