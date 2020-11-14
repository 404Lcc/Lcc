namespace LccHotfix
{
    [LccModel.EventHandler]
    public class InitEventHandler : AEvent<Start>
    {
        public override void Publish(Start data)
        {
            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(false, true, AssetType.UI));
            ItemManager.Instance.InitManager(new ItemObjectBaseHandler(false, true, AssetType.Item));

            LccModel.SceneLoadManager.Instance.LoadScene(SceneName.Login, true, () =>
            {
                UIEventManager.Instance.Publish(UIEventType.Login);
            }, AssetType.Scene);
        }
    }
}