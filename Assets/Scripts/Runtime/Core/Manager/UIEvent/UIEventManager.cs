using System;
using System.Collections.Generic;

namespace LccModel
{
    public class UIEventManager : Singleton<UIEventManager>
    {
        public Dictionary<string, UIEvent> uiEventDict = new Dictionary<string, UIEvent>();
        public void InitManager()
        {
            foreach (Type item in Manager.Instance.typeDict.Values)
            {
                if (item.IsAbstract) continue;
                UIEventHandlerAttribute[] uiEventHandlerAttributes = (UIEventHandlerAttribute[])item.GetCustomAttributes(typeof(UIEventHandlerAttribute), false);
                if (uiEventHandlerAttributes.Length > 0)
                {
                    UIEvent uiEvent = (UIEvent)Activator.CreateInstance(item);
                    uiEventDict.Add(uiEventHandlerAttributes[0].uiEventType, uiEvent);
                }
            }
        }
        public void Publish(string uiEventType)
        {
            if (uiEventDict.ContainsKey(uiEventType))
            {
                UIEvent uiEvent = uiEventDict[uiEventType];
                uiEvent.Publish();
            }
            else
            {
                LogUtil.Log($"事件不存在{uiEventType}");
            }
        }
        public void Publish<T>(string uiEventType, T data)
        {
            if (uiEventDict.ContainsKey(uiEventType))
            {
                UIEvent uiEvent = uiEventDict[uiEventType];
                uiEvent.Publish(data);
            }
            else
            {
                LogUtil.Log($"事件不存在{uiEventType}");
            }
        }
    }
}