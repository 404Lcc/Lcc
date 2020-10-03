namespace Hotfix
{
    public class Init
    {
        public static void InitHotfix()
        {
            EventManager.Instance.InitManager();
            UIEventManager.Instance.InitManager();

            PanelManager.Instance.InitManager(new PanelObjectBaseHandler(false, true, AssetType.UI));

            EventManager.Instance.Publish(new Start());
        }
    }
}