using System;
using System.Collections;

namespace Model
{
    public class EventManager : Singleton<EventManager>
    {
        public Hashtable events = new Hashtable();
        public void InitManager()
        {
            foreach (Type item in GetType().Assembly.GetTypes())
            {
                object[] objects = item.GetCustomAttributes(typeof(EventHandlerAttribute), false);
                if (objects.Length > 0)
                {
                    EventHandlerAttribute eventHandlerAttribute = (EventHandlerAttribute)objects[0];
                    events.Add(eventHandlerAttribute.eventName, item);
                }
            }
        }
        public void Run(string eventName)
        {
            IEvent iEvent = (IEvent)Activator.CreateInstance((Type)events[eventName]);
            iEvent.Run();
        }
        public void Run<T>(string eventName, T data)
        {
            IEvent iEvent = (IEvent)Activator.CreateInstance((Type)events[eventName]);
            iEvent.Run(data);
        }
    }
}