using LccModel;
using System;
using System.Collections;

namespace LccHotfix
{
    public class EventManager : Singleton<EventManager>
    {
        public Hashtable events = new Hashtable();
        public void InitManager()
        {
            foreach (Type item in Manager.Instance.types.Values)
            {
                if (item.IsAbstract) continue;
                EventHandlerAttribute[] eventHandlerAttributes = (EventHandlerAttribute[])item.GetCustomAttributes(typeof(EventHandlerAttribute), false);
                if (eventHandlerAttributes.Length > 0)
                {
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
                LogUtil.Log($"事件不存在{type.Name}");
            }
        }
    }
}