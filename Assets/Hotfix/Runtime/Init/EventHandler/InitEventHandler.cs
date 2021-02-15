namespace LccHotfix
{
    [LccModel.EventHandler]
    public class InitEventHandler : AEvent<Start>
    {
        public override void Publish(Start data)
        {
            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(false, true, AssetType.Prefab, AssetType.Panel));
            ItemManager.Instance.InitManager(new ItemObjectBaseHandler(false, true, AssetType.Prefab, AssetType.Item));
            GameDataManager.Instance.InitManager("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            UserManager.Instance.InitManager();

            LccModel.SceneLoadManager.Instance.LoadScene(SceneName.Login, true, () =>
            {
                UIEventManager.Instance.Publish(UIEventType.Login);
            }, AssetType.Scene);
        }
    }
}