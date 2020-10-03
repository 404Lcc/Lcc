using System;

namespace Hotfix
{
    public class UIEventHandlerAttribute : Attribute
    {
        public string uiEventType;
        public UIEventHandlerAttribute(string uiEventType)
        {
            this.uiEventType = uiEventType;
        }
    }
}