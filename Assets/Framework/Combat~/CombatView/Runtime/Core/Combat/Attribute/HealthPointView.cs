namespace LccModel
{
    public class HealthPointView : Entity
    {
        public FloatNumericView Current => Parent.GetComponent<AttributeViewComponent>().HealthPoint;
        public FloatNumericView Max => Parent.GetComponent<AttributeViewComponent>().HealthPointMax;
        public int Value => (int)Current.Value;
        public int MaxValue => (int)Max.Value;


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