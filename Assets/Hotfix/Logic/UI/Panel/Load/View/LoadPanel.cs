namespace Hotfix
{
    public class LoadPanel : ViewBase<LoadModel>
    {
        public override void Start()
        {
            InitPanel();
        }
        public void InitPanel()
        {
            ViewModel = new LoadModel();
        }
        public void OnHidePanel()
        {
            PanelManager.Instance.ClearPanel(PanelType.Load);
        }
    }
}