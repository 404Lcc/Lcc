using System;
using System.Collections.Generic;

namespace LccModel
{
    public class NumericEventManager : AObjectBase
    {
        public static NumericEventManager Instance { get; set; }
        public Dictionary<int, List<INumericEvent>> numericEventDict = new Dictionary<int, List<INumericEvent>>();

        public override void Awake()
        {
            base.Awake();

            foreach (Type item in Manager.Instance.GetTypesByAttribute(typeof(NumericEventHandlerAttribute)))
            {
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

            Instance = this;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            numericEventDict.Clear();

            Instance = null;
        }

        public void Publish(int type, long oldValue, long newValue)
        {
            if (numericEventDict.ContainsKey(type))
            {
                List<INumericEvent> list = numericEventDict[type];
                foreach (INumericEvent item in list)
                {
                    item.Publish(oldValue, newValue);
                }
            }
            else
            {
                LogHelper.Warning($"事件不存在{type}");
            }
        }
    }
}