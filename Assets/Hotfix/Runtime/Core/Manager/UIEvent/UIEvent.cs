namespace LccHotfix
{
    public class UIEvent
    {
        public virtual void Publish()
        {
        }
        public virtual void Publish<T>(T data)
        {
        }
    }
}