using LccModel;

namespace LccHotfix
{
    [EventHandler]
    public class InitEventHandler : AEvent<Start>
    {
        public override async ETTask Publish(Start data)
        {
            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(false, true, AssetType.Prefab, AssetType.Panel));
            ItemManager.Instance.InitManager(new ItemObjectBaseHandler(false, true, AssetType.Prefab, AssetType.Item));
            GameDataManager.Instance.InitManager("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            UserManager.Instance.InitManager();

            await SceneLoadManager.Instance.LoadScene(SceneName.Login, true, AssetType.Scene);
            UIEventManager.Instance.Publish(UIEventType.Login);
        }
    }
}