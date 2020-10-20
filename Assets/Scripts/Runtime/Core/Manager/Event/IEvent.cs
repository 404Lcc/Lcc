using System;

namespace Model
{
    public interface IEvent
    {
        Type EventType();
    }
}