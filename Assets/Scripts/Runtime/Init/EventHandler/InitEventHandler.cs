using UnityEngine;

namespace LccModel
{
    [EventHandler]
    public class InitEventHandler : AEvent<Start>
    {
        public override async void Publish(Start data)
        {
            Object.DontDestroyOnLoad(await AssetManager.Instance.InstantiateAsset("Canvas", false, false, AssetType.Game));
            Object.DontDestroyOnLoad(await AssetManager.Instance.InstantiateAsset("AudioSource", false, false, AssetType.Game));
            Object.DontDestroyOnLoad(await AssetManager.Instance.InstantiateAsset("VideoPlayer", false, false, AssetType.Game));

            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(false, false, AssetType.UI));
            ItemManager.Instance.InitManager(new ItemObjectBaseHandler(false, false, AssetType.Item));
            //TipsManager.Instance.InitManager(new TipsPool(10));
            //TipsWindowManager.Instance.InitManager(new TipsWindowPool(10));

            UIEventManager.Instance.Publish(UIEventType.Launch);
        }
    }
}