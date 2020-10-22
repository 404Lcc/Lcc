using System;
using System.Collections;
using UnityEngine;

namespace LccHotfix
{
    public class EventManager : Singleton<EventManager>
    {
        public Hashtable events = new Hashtable();
        public void InitManager()
        {
            foreach (Type item in LccModel.ILRuntimeManager.Instance.typeList)
            {
                if (item.IsAbstract) continue;
                object[] objects = item.GetCustomAttributes(typeof(LccModel.EventHandlerAttribute), true);
                if (objects.Length > 0)
                {
                    Debug.Log(10000);
                }
                EventHandlerAttribute[] eventHandlerAttributes = (EventHandlerAttribute[])item.GetCustomAttributes(typeof(EventHandlerAttribute), true);
                if (eventHandlerAttributes.Length > 0)
                {
                    Debug.Log(1);
                    IEvent iEvent = (IEvent)Activator.CreateInstance(item);
                    events.Add(iEvent.EventType(), iEvent);
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