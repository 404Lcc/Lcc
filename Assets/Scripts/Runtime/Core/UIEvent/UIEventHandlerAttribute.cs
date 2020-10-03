using System;

namespace Model
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