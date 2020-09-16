namespace Model
{
    public interface IEvent
    {
        void Run();
        void Run<T>(T data);
    }
}