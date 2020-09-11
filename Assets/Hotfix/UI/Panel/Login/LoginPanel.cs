namespace Hotfix
{
    public class LoginPanel : ObjectBase
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