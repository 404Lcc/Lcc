using System;

namespace LccHotfix
{
    public class NumericEventHandlerAttribute : Attribute
    {
        public NumericType numericType;
        public NumericEventHandlerAttribute(NumericType numericType)
        {
            this.numericType = numericType;
        }
    }
}