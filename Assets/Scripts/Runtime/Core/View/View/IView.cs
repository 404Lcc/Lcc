namespace LccModel
{
    public interface IView<T> where T : ViewModelBase
    {
        T ViewModel
        {
            get; set;
        }
    }
}