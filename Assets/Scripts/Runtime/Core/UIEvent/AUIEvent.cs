namespace Model
{
    public class AUIEvent : IUIEvent
    {
        public virtual void Publish()
        {
        }
        public virtual void Publish<T>(T data)
        {
        }
    }
}