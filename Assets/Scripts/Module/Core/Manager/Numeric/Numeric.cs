namespace LccModel
{
    public class Numeric
    {
        public NumericType type;
        public long oldValue;
        public long newValue;
        public Numeric()
        {
        }
        public Numeric(NumericType type, long oldValue, long newValue)
        {
            this.type = type;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }
    }
}