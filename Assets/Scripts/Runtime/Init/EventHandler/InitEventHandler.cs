using UnityEngine;

namespace LccModel
{
    [EventHandler]
    public class InitEventHandler : AEvent<Start>
    {
        public override void Publish(Start data)
        {
            Object.DontDestroyOnLoad(AssetManager.Instance.LoadGameObject("Canvas", false, false, AssetType.Game));
            Object.DontDestroyOnLoad(AssetManager.Instance.LoadGameObject("AudioSource", false, false, AssetType.Game));
            Object.DontDestroyOnLoad(AssetManager.Instance.LoadGameObject("VideoPlayer", false, false, AssetType.Game));

            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(false, false, AssetType.UI));
            ItemManager.Instance.InitManager(new ItemObjectBaseHandler(false, false, AssetType.Item));
            //TipsManager.Instance.InitManager(new TipsPool(10));
            //TipsWindowManager.Instance.InitManager(new TipsWindowPool(10));

            UIEventManager.Instance.Publish(UIEventType.Launch);
        }
    }
}