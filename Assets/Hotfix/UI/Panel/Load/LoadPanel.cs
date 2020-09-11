namespace Hotfix
{
    public class LoadPanel : ObjectBase
    {
        public override void Start()
        {
            InitPanel();
        }
        public override void Update()
        {
            if (Model.LoadSceneManager.Instance.process >= 100)
            {
                OnHidePanel();
            }
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