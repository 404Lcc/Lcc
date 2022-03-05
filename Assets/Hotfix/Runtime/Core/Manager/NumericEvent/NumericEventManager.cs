using LccModel;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class NumericEventManager : Singleton<NumericEventManager>
    {
        public Dictionary<NumericType, List<INumericEvent>> numericEventDitc = new Dictionary<NumericType, List<INumericEvent>>();
        public void InitManager()
        {
            foreach (Type item in Manager.Instance.typeDict.Values)
            {
                if (item.IsAbstract) continue;
                NumericEventHandlerAttribute[] numericEventHandlerAttributes = (NumericEventHandlerAttribute[])item.GetCustomAttributes(typeof(NumericEventHandlerAttribute), false);
                foreach (NumericEventHandlerAttribute numericEventHandlerAttributeItem in numericEventHandlerAttributes)
                {
                    INumericEvent iNumericEvent = (INumericEvent)Activator.CreateInstance(item);
                    if (!numericEventDitc.ContainsKey(numericEventHandlerAttributeItem.numericType))
                    {
                        numericEventDitc.Add(numericEventHandlerAttributeItem.numericType, new List<INumericEvent>());
                    }
                    numericEventDitc[numericEventHandlerAttributeItem.numericType].Add(iNumericEvent);
                }
            }
        }
        public void Publish(NumericType type, long value)
        {
            if (numericEventDitc.ContainsKey(type))
            {
                List<INumericEvent> list = numericEventDitc[type];
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