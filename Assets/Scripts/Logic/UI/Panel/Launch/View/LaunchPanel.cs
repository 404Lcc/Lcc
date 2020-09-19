namespace Model
{
    public class LaunchPanel : ViewBase<LaunchModel>
    {
        public override void Start()
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
            ClearPanel();
        }
    }
}