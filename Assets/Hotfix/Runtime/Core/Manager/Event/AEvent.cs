using System;

namespace Hotfix
{
    [EventHandler]
    public abstract class AEvent<T> : IEvent
    {
        public Type GetEventType()
        {
            return typeof(T);
        }
        public abstract void Publish(T data);
    }
}