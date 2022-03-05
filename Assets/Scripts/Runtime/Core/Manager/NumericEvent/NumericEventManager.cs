using System;
using System.Collections.Generic;

namespace LccModel
{
    public class NumericEventManager : Singleton<NumericEventManager>
    {
        public Dictionary<NumericType, List<INumericEvent>> numericEventDict = new Dictionary<NumericType, List<INumericEvent>>();
        public void InitManager()
        {
            foreach (Type item in Manager.Instance.typeDict.Values)
            {
                if (item.IsAbstract) continue;
                NumericEventHandlerAttribute[] numericEventHandlerAttributes = (NumericEventHandlerAttribute[])item.GetCustomAttributes(typeof(NumericEventHandlerAttribute), false);
                foreach (NumericEventHandlerAttribute numericEventHandlerAttributeItem in numericEventHandlerAttributes)
                {
                    INumericEvent iNumericEvent = (INumericEvent)Activator.CreateInstance(item);
                    if (!numericEventDict.ContainsKey(numericEventHandlerAttributeItem.numericType))
                    {
                        numericEventDict.Add(numericEventHandlerAttributeItem.numericType, new List<INumericEvent>());
                    }
                    numericEventDict[numericEventHandlerAttributeItem.numericType].Add(iNumericEvent);
                }
            }
        }
        public void Publish(NumericType type, long value)
        {
            if (numericEventDict.ContainsKey(type))
            {
                List<INumericEvent> list = numericEventDict[type];
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