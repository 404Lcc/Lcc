using ET;
using LccModel;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class EventManager : AObjectBase
    {
        public static EventManager Instance { get; set; }

        public Dictionary<Type, IEvent> eventDict = new Dictionary<Type, IEvent>();

        public override void Awake()
        {
            base.Awake();

            Instance = this;
            foreach (Type item in Manager.Instance.GetTypesByAttribute(typeof(EventHandlerAttribute)))
            {
                IEvent iEvent = (IEvent)Activator.CreateInstance(item);
                eventDict.Add(iEvent.EventType(), iEvent);
            }
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            eventDict.Clear();

            Instance = null;
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