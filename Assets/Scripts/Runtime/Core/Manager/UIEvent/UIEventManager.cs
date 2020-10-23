using System;
using System.Collections;
using UnityEngine;

namespace LccModel
{
    public class UIEventManager : Singleton<UIEventManager>
    {
        public Hashtable uiEvents = new Hashtable();
        public void InitManager()
        {
            foreach (Type item in Manager.Instance.types.Values)
            {
                if (item.IsAbstract) continue;
                UIEventHandlerAttribute[] uiEventHandlerAttributes = (UIEventHandlerAttribute[])item.GetCustomAttributes(typeof(UIEventHandlerAttribute), false);
                if (uiEventHandlerAttributes.Length > 0)
                {
                    UIEvent uiEvent = (UIEvent)Activator.CreateInstance(item);
                    uiEvents.Add(uiEventHandlerAttributes[0].uiEventType, uiEvent);
                }
            }
        }
        public void Publish(string uiEventType)
        {
            if (uiEvents.ContainsKey(uiEventType))
            {
                UIEvent uiEvent = (UIEvent)uiEvents[uiEventType];
                uiEvent.Publish();
            }
            else
            {
                Debug.Log("事件不存在" + uiEventType);
            }
        }
        public void Publish<T>(string uiEventType, T data)
        {
            if (uiEvents.ContainsKey(uiEventType))
            {
                UIEvent uiEvent = (UIEvent)uiEvents[uiEventType];
                uiEvent.Publish(data);
            }
            else
            {
                Debug.Log("事件不存在" + uiEventType);
            }
        }
    }
}