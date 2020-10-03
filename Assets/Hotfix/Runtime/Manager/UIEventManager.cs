using System;
using System.Collections;
using UnityEngine;

namespace Hotfix
{
    public class UIEventManager : Singleton<UIEventManager>
    {
        public Hashtable events = new Hashtable();
        public void InitManager()
        {
            foreach (Type item in GetType().Assembly.GetTypes())
            {
                if (item.IsAbstract) continue;
                UIEventHandlerAttribute[] uiEventHandlerAttributes = (UIEventHandlerAttribute[])item.GetCustomAttributes(typeof(UIEventHandlerAttribute), false);
                if (uiEventHandlerAttributes.Length > 0)
                {
                    AUIEvent aUIEvent = (AUIEvent)Activator.CreateInstance(item);
                    events.Add(uiEventHandlerAttributes[0].uiEventType, aUIEvent);
                }
            }
        }
        public void Publish(string uiEventType)
        {
            if (events.ContainsKey(uiEventType))
            {
                AUIEvent aUIEvent = (AUIEvent)events[uiEventType];
                aUIEvent.Publish();
            }
            else
            {
                Debug.Log("事件不存在" + uiEventType);
            }
        }
        public void Publish<T>(string uiEventType, T data)
        {
            if (events.ContainsKey(uiEventType))
            {
                AUIEvent aUIEvent = (AUIEvent)events[uiEventType];
                aUIEvent.Publish(data);
            }
            else
            {
                Debug.Log("事件不存在" + uiEventType);
            }
        }
    }
}