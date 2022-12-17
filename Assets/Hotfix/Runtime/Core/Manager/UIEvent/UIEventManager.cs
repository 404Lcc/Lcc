using LccModel;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public class UIEventManager : AObjectBase
    {
        public static UIEventManager Instance { get; set; }

        public Dictionary<string, UIEvent> uiEventDict = new Dictionary<string, UIEvent>();

        public override void Awake()
        {
            base.Awake();

            foreach (Type item in Manager.Instance.GetTypesByAttribute(typeof(UIEventHandlerAttribute)))
            {
                object[] atts = item.GetCustomAttributes(typeof(UIEventHandlerAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    UIEvent uiEvent = (UIEvent)Activator.CreateInstance(item);
                    uiEventDict.Add(((UIEventHandlerAttribute)atts[0]).uiEventType, uiEvent);
                }
            }

            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            uiEventDict.Clear();

            Instance = null;
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
                LogUtil.Debug($"事件不存在{uiEventType}");
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
                LogUtil.Debug($"事件不存在{uiEventType}");
            }
        }
    }
}