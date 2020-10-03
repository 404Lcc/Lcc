namespace Model
{
    [UIEventHandler(UIEventType.Launch)]
    public class LaunchEventHandler : AUIEvent
    {
        public override void Publish()
        {
            //开屏界面-资源更新界面-初始化IL-开始界面
            PanelManager.Instance.OpenPanel(PanelType.Launch);
        }
    }
}