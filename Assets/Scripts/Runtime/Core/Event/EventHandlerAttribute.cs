using System;

namespace Model
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