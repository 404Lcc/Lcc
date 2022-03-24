using ET;
using LccModel;
using System;

namespace LccHotfix
{
    [EventHandler]
    public class InitEventHandler : AEvent<Start>
    {
        public override async ETTask Publish(Start data)
        {
            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(AssetType.Prefab, AssetType.Panel));
            GameDataManager.Instance.InitManager("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            GameSettingManager.Instance.InitManager();

            UIEventManager.Instance.Publish(UIEventType.Load);
            await SceneLoadManager.Instance.LoadSceneAsync(SceneName.Login, AssetType.Scene);
            UIEventManager.Instance.Publish(UIEventType.Login);
        }
    }
}