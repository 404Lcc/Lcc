using System;
using System.Collections;
using UnityEngine;

namespace Model
{
    public class EventManager : Singleton<EventManager>
    {
        public Hashtable events = new Hashtable();
        public void InitManager()
        {
            foreach (Type item in GetType().Assembly.GetTypes())
            {
                if (item.IsAbstract) continue;
                object[] objects = item.GetCustomAttributes(typeof(EventHandlerAttribute), true);
                if (objects.Length > 0)
                {
                    IEvent iEvent = (IEvent)Activator.CreateInstance(item);
                    events.Add(iEvent.GetEventType(), iEvent);
                }
            }
        }
        public void Publish<T>(T data)
        {
            Type dataType = typeof(T);
            if (events.ContainsKey(dataType))
            {
                EventBase<T> eventBase = (EventBase<T>)events[dataType];
                eventBase.Publish(data);
            }
            else
            {
                Debug.Log("事件不存在" + dataType.Name);
            }
        }
    }
}