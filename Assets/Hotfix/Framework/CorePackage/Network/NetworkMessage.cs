namespace LccHotfix
{
    public class NetworkMessage
    {
        public int code;
        public object message;

        public T GetMessage<T>() where T : class
        {
            return (T)message;
        }
    }
}