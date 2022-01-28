using UnityEngine;

namespace LccModel
{
    [EventHandler]
    public class InitEventHandler : AEvent<Start>
    {
        public override async ETTask Publish(Start data)
        {
            Object.DontDestroyOnLoad(AssetManager.Instance.InstantiateAsset("Canvas", false, false, AssetType.Game));
            Object.DontDestroyOnLoad(AssetManager.Instance.InstantiateAsset("AudioSource", false, false, AssetType.Game));
            Object.DontDestroyOnLoad(AssetManager.Instance.InstantiateAsset("VideoPlayer", false, false, AssetType.Game));

            DownloadManager.Instance.InitManager();
            AssetBundleManager.Instance.InitManager(string.Empty);

            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(false, false, AssetType.Prefab, AssetType.Panel));
            ItemManager.Instance.InitManager(new ItemObjectBaseHandler(false, false, AssetType.Prefab, AssetType.Item));
            //TipsManager.Instance.InitManager(new TipsPool(10));
            //TipsWindowManager.Instance.InitManager(new TipsWindowPool(10));

            //步骤
            //打开开屏界面
            //如果是ab模式进入检测资源更新界面
            //初始化主工程并初始化热更层
            //打开登录界面
            UIEventManager.Instance.Publish(UIEventType.Launch);
            await ETTask.ETTaskCompleted;
        }
    }
}