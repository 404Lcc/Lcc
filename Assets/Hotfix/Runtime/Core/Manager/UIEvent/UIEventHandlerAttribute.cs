using System;

namespace LccHotfix
{
    public class UIEventHandlerAttribute : BaseAttribute
    {
        public string uiEventType;
        public UIEventHandlerAttribute(string uiEventType)
        {
            this.uiEventType = uiEventType;
        }
    }
}