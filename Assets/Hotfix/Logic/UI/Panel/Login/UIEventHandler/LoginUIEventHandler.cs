namespace LccHotfix
{
    [LccModel.UIEventHandler(UIEventType.Login)]
    public class LoginUIEventHandler : UIEvent
    {
        public override void Publish()
        {
            PanelManager.Instance.OpenPanelAsync(PanelType.Login).Continue();
        }
    }
}