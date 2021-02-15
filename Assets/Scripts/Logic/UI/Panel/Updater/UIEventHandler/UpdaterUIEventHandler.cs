namespace LccModel
{
    [UIEventHandler(UIEventType.Updater)]
    public class UpdaterUIEventHandler : UIEvent
    {
        public override void Publish()
        {
            PanelManager.Instance.OpenPanel(PanelType.Updater);
        }
    }
}