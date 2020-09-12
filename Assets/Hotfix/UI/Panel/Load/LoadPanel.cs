namespace Hotfix
{
    public class LoadPanel : ObjectBase
    {
        public override void Start()
        {
            InitPanel();
        }
        public void InitPanel()
        {
        }
        public void OnHidePanel()
        {
            PanelManager.Instance.ClearPanel(PanelType.Load);
        }
    }
}