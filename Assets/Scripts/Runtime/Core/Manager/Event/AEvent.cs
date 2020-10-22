using System;

namespace LccModel
{
    [EventHandler]
    public abstract class AEvent<T> : IEvent
    {
        public Type EventType()
        {
            return typeof(T);
        }
        public abstract void Publish(T data);
    }
}