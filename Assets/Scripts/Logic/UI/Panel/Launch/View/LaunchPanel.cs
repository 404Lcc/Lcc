namespace LccModel
{
    public class LaunchPanel : APanelView<LaunchModel>
    {
        public override void Start()
        {
#if AssetBundle
            //更新界面
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