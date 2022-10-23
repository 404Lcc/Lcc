namespace LccHotfix
{
    public abstract class APanelView<T> : AViewBase<T>, IPanelHandler where T : ViewModelBase
    {
        public void BeforeUnload(Panel uiBaseWindow)
        {
        }

        public void OnHideWindow(Panel uiBaseWindow)
        {
        }

        public void OnInitComponent(Panel uiBaseWindow)
        {
        }

        public void OnInitWindowCoreData(Panel uiBaseWindow)
        {
        }

        public void OnRegisterUIEvent(Panel uiBaseWindow)
        {
        }

        public void OnShowWindow(Panel uiBaseWindow, AObjectBase contextData = null)
        {
        }
    }
}