using System;

namespace Hotfix
{
    public class EventHandlerAttribute : Attribute
    {
        public string eventName;
        public EventHandlerAttribute(string eventName)
        {
            this.eventName = eventName;
        }
    }
}