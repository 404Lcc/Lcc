using ET;
using System;

namespace LccModel
{
    public abstract class AEvent<T> : IEvent
    {
        public Type EventType()
        {
            return typeof(T);
        }
        public abstract ETTask Publish(T data);
    }
}