namespace LccModel
{
    public class LaunchPanel : APanelView<LaunchModel>
    {
        public override void Start()
        {
#if UNITY_EDITOR
#if ILRuntime
            ConfigManager.Instance.InitManager();
            ILRuntimeManager.Instance.InitManager();
#else
            ConfigManager.Instance.InitManager();
            MonoManager.Instance.InitManager();
#endif
#else
#if AssetBundle
            UIEventManager.Instance.Publish(UIEventType.Updater);
#else
#if ILRuntime
            ConfigManager.Instance.InitManager();
            ILRuntimeManager.Instance.InitManager();
#else
            ConfigManager.Instance.InitManager();
            MonoManager.Instance.InitManager();
#endif
#endif
#endif
            ClearPanel();
        }
    }
}