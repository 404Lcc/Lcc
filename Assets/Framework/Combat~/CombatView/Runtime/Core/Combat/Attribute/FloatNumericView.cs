namespace LccModel
{
    public class FloatNumericView : Entity
    {
        private NumericEntity _numericEntity;

        private int _type;

        public float BaseValue
        {
            get
            {
                return _numericEntity.GetFloat(_type * 10 + 1);
            }
        }
        public float Add
        {
            get
            {
                return _numericEntity.GetFloat(_type * 10 + 2);
            }
        }
        public float PctAdd
        {
            get
            {
                return _numericEntity.GetFloat(_type * 10 + 3);
            }
        }
        public float FinalAdd
        {
            get
            {
                return _numericEntity.GetFloat(_type * 10 + 4);
            }
        }
        public float FinalPctAdd
        {
            get
            {
                return _numericEntity.GetFloat(_type * 10 + 5);
            }
        }
        public float Value => _numericEntity.GetFloat(_type);

        public override void Awake<P1, P2>(P1 p1, P2 p2)
        {
            base.Awake(p1, p2);

            _numericEntity = (NumericEntity)(object)p1;
            _type = (int)(AttributeType)(object)p2;
        }

        public void ModifyAttribute(int temp, float value)
        {
            _numericEntity.Set(_type * 10 + temp, value);
        }
    }
}