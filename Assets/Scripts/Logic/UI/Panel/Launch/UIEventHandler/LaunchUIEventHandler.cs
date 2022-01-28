namespace LccModel
{
    [UIEventHandler(UIEventType.Launch)]
    public class LaunchUIEventHandler : UIEvent
    {
        public override void Publish()
        {
            PanelManager.Instance.OpenPanel(PanelType.Launch);
        }
    }
}