using LccModel;
using System;

namespace LccHotfix
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