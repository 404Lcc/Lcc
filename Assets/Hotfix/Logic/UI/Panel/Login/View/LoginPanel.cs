namespace Hotfix
{
    public class LoginPanel : ViewBase<LoginModel>
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
            PanelManager.Instance.ClearPanel(PanelType.Login);
        }
    }
}