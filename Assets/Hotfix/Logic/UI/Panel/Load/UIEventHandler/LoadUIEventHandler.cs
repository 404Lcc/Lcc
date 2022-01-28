namespace LccHotfix
{
    [LccModel.UIEventHandler(UIEventType.Load)]
    public class LoadUIEventHandler : UIEvent
    {
        public override void Publish()
        {
            PanelManager.Instance.OpenPanel(PanelType.Load);
        }
    }
}