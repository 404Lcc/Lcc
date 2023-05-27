namespace LccModel
{
    public interface IEventListener
    {
        void HandleEvent(EventType eventType, IEventArgs args1 = null, IEventArgs args2 = null, IEventArgs args3 = null, IEventArgs args4 = null);
    }
}