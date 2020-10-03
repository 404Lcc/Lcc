using System;
using System.Collections;
using UnityEngine;

namespace Hotfix
{
    public class EventManager : Singleton<EventManager>
    {
        public Hashtable events = new Hashtable();
        public void InitManager()
        {
            foreach (Type item in GetType().Assembly.GetTypes())
            {
                if (item.IsAbstract) continue;
                EventHandlerAttribute[] eventHandlerAttributes = (EventHandlerAttribute[])item.GetCustomAttributes(typeof(EventHandlerAttribute), true);
                if (eventHandlerAttributes.Length > 0)
                {
                    IEvent iEvent = (IEvent)Activator.CreateInstance(item);
                    events.Add(iEvent.GetEventType(), iEvent);
                }
            }
        }
        public void Publish<T>(T data)
        {
            Type type = typeof(T);
            if (events.ContainsKey(type))
            {
                AEvent<T> aEvent = (AEvent<T>)events[type];
                aEvent.Publish(data);
            }
            else
            {
                Debug.Log("事件不存在" + type.Name);
            }
        }
    }
}