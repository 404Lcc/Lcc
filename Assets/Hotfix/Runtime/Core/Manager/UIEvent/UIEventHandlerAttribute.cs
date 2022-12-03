using System;

namespace LccHotfix
{
    public class UIEventHandlerAttribute : AttributeBase
    {
        public string uiEventType;
        public UIEventHandlerAttribute(string uiEventType)
        {
            this.uiEventType = uiEventType;
        }
    }
}