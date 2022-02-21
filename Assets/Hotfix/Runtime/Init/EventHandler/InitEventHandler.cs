using LccModel;
using System;

namespace LccHotfix
{
    [EventHandler]
    public class InitEventHandler : AEvent<Start>
    {
        public override async ETTask Publish(Start data)
        {
            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(false, true, AssetType.Prefab, AssetType.Panel));
            GameDataManager.Instance.InitManager("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            GameSettingManager.Instance.InitManager();

            UIEventManager.Instance.Publish(UIEventType.Load);
            await SceneLoadManager.Instance.LoadScene(SceneName.Login, true, AssetType.Scene);
            UIEventManager.Instance.Publish(UIEventType.Login);
        }
    }
}