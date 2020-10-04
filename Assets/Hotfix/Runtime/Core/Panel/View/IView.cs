namespace Hotfix
{
    public interface IView<T> where T : ViewModelBase
    {
        T ViewModel
        {
            get; set;
        }
        void ClosePanel();
        void ClearPanel();
    }
}