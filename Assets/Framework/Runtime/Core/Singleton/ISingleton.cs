namespace LccModel
{
    public interface ISingleton
    {
        bool IsDisposed();
        void Register();
        void Destroy();
    }
}