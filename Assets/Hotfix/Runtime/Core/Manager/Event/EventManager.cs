using ET;
using LccModel;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class EventManager : Singleton<EventManager>
    {
        public Dictionary<Type, IEvent> eventDict = new Dictionary<Type, IEvent>();
        public void InitManager()
        {
            foreach (Type item in Manager.Instance.typeDict.Values)
            {
                if (item.IsAbstract) continue;
                object[] eventHandlerAttributes = item.GetCustomAttributes(typeof(EventHandlerAttribute), false);
                if (eventHandlerAttributes.Length > 0)
                {
                    IEvent iEvent = (IEvent)Activator.CreateInstance(item);
                    eventDict.Add(iEvent.EventType(), iEvent);
                }
            }
        }
        public async ETTask Publish<T>(T data)
        {
            Type type = typeof(T);
            if (eventDict.ContainsKey(type))
            {
                AEvent<T> aEvent = (AEvent<T>)eventDict[type];
                try
                {
                    await aEvent.Publish(data);
                }
                catch (Exception e)
                {
                    LogUtil.LogError(e.ToString());
                }
            }
            else
            {
                LogUtil.Log($"事件不存在{type.Name}");
            }
        }
    }
}