using System;

namespace Hotfix
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