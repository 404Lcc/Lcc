using System;

namespace LccModel
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