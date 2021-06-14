using System;
using System.Collections;
using System.Collections.Generic;

namespace LccModel
{
    public class NumericEventManager : Singleton<NumericEventManager>
    {
        public Hashtable numericEvents = new Hashtable();
        public void InitManager()
        {
            foreach (Type item in Manager.Instance.types.Values)
            {
                if (item.IsAbstract) continue;
                NumericEventHandlerAttribute[] numericEventHandlerAttributes = (NumericEventHandlerAttribute[])item.GetCustomAttributes(typeof(NumericEventHandlerAttribute), false);
                foreach (NumericEventHandlerAttribute numericEventHandlerAttributeItem in numericEventHandlerAttributes)
                {
                    INumericEvent iNumericEvent = (INumericEvent)Activator.CreateInstance(item);
                    if (!numericEvents.ContainsKey(numericEventHandlerAttributeItem.numericType))
                    {
                        numericEvents.Add(numericEventHandlerAttributeItem.numericType, new List<INumericEvent>());
                    }
                    ((List<INumericEvent>)numericEvents[numericEventHandlerAttributeItem.numericType]).Add(iNumericEvent);
                }
            }
        }
        public void Publish(NumericType type, long value)
        {
            if (numericEvents.ContainsKey(type))
            {
                List<INumericEvent> list = (List<INumericEvent>)numericEvents[type];
                foreach (INumericEvent item in list)
                {
                    item.Publish(value);
                }
            }
            else
            {
                LogUtil.Log($"事件不存在{type}");
            }
        }
    }
}