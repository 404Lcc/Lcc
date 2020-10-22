namespace LccHotfix
{
    public interface IView<T> where T : ViewModelBase
    {
        T ViewModel
        {
            get; set;
        }
    }
}