using System;

namespace Hotfix
{
    [EventHandler]
    public abstract class EventBase<T> : IEvent
    {
        public Type GetEventType()
        {
            return typeof(T);
        }
        public abstract void Publish(T data);
    }
}