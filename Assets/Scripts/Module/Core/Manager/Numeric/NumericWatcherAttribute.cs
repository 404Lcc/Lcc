using System;

namespace LccModel
{
    public class NumericWatcherAttribute : Attribute
    {
        public NumericType type;
        public NumericWatcherAttribute(NumericType type)
        {
            this.type = type;
        }
    }
}