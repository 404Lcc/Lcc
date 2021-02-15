namespace LccModel
{
    public class LaunchPanel : APanelView<LaunchModel>
    {
        public override void Start()
        {
            ConfigManager.Instance.InitManager();
#if UNITY_EDITOR
#if ILRuntime
            ILRuntimeManager.Instance.InitManager();
#else
            MonoManager.Instance.InitManager();
#endif
#else
#if AssetBundle
            PanelManager.Instance.OpenPanel(PanelType.Updater);
#else
#if ILRuntime
            ILRuntimeManager.Instance.InitManager();
#else
            MonoManager.Instance.InitManager();
#endif
#endif
#endif
            ClearPanel();
        }
    }
}