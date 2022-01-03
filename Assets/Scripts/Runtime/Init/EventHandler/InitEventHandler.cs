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

            UIEventManager.Instance.Publish(UIEventType.Launch);
            await ETTask.ETTaskCompleted;
        }
    }
}