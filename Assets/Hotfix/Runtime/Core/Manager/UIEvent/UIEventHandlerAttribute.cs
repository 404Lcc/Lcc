using System;

namespace LccHotfix
{
    public class UIEventHandlerAttribute : Attribute
    {
        public UIEventType uiEventType;
        public UIEventHandlerAttribute(UIEventType uiEventType)
        {
            this.uiEventType = uiEventType;
        }
    }
}