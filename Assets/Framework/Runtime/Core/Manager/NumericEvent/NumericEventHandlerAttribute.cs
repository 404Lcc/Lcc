using System;

namespace LccModel
{
    public class NumericEventHandlerAttribute : Attribute
    {
        public int numericType;
        public NumericEventHandlerAttribute(int numericType)
        {
            this.numericType = numericType;
        }
    }
}