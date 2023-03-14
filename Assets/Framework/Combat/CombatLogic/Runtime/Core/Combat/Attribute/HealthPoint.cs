namespace LccModel
{
    public class HealthPoint : Entity
    {
        public FloatNumeric current;
        public FloatNumeric max;
        public int Value => (int)current.Value;
        public int MaxValue => (int)max.Value;


        public void Reset()
        {
            current.SetBase(MaxValue);
        }

        public void SetMaxValue(int value)
        {
            max.SetBase(value);
        }

        public void Minus(int value)
        {
            current.MinusBase(value);
        }

        public void Add(int value)
        {
            current.AddBase(value);
        }

        public float Percent()
        {
            return (float)Value / MaxValue;
        }

        public bool IsFull()
        {
            return Value == MaxValue;
        }
    }
}