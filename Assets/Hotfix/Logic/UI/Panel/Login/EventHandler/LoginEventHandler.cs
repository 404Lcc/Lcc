namespace Hotfix
{
    [UIEventHandler(UIEventType.Login)]
    public class LoginEventHandler : AUIEvent
    {
        public override void Publish()
        {
            PanelManager.Instance.OpenPanel(PanelType.Login);
        }
    }
}