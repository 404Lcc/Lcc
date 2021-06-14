using System;

namespace LccModel
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