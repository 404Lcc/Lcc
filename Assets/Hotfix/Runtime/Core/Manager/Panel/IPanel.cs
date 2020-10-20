namespace Hotfix
{
    public interface IPanel
    {
        PanelType Type
        {
            get; set;
        }
        PanelState State
        {
            get; set;
        }
        AObjectBase AObjectBase
        {
            get; set;
        }
        bool IsExist
        {
            get;
        }
        void OpenPanel();
        void ClosePanel();
    }
}