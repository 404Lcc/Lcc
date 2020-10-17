using System;

namespace Hotfix
{
    public interface IEvent
    {
        Type GetEventType();
    }
}