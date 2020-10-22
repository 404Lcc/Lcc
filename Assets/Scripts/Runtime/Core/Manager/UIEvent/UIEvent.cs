namespace LccModel
{
    public class UIEvent : IUIEvent
    {
        public virtual void Publish()
        {
        }
        public virtual void Publish<T>(T data)
        {
        }
    }
}