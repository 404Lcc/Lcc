namespace Model
{
    public class LaunchPanel : ViewBase<LaunchModel>
    {
        public override void Start()
        {
            InitPanel();
        }
        public void InitPanel()
        {
#if AssetBundle
            PanelManager.Instance.OpenPanel(PanelType.Updater);
#else
#if ILRuntime
            ILRuntimeManager.Instance.InitManager();
#else
            MonoManager.Instance.InitManager();
#endif
#endif
            OnHidePanel();
        }
        public void OnHidePanel()
        {
            PanelManager.Instance.ClearPanel(PanelType.Launch);
        }
    }
}