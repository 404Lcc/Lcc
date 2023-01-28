namespace LccModel
{
    public class FloatNumeric : Entity
    {
        private NumericEntity _numericEntity;

        private int _type;

        public float BaseValue => _numericEntity.GetFloat(_type * 10 + 1);
        public float Add => _numericEntity.GetFloat(_type * 10 + 2);
        public float PctAdd => _numericEntity.GetFloat(_type * 10 + 3);
        public float FinalAdd => _numericEntity.GetFloat(_type * 10 + 4);
        public float FinalPctAdd => _numericEntity.GetFloat(_type * 10 + 5);
        public float Value => _numericEntity.GetFloat(_type);

        public override void Awake<P1, P2>(P1 p1, P2 p2)
        {
            base.Awake(p1, p2);

            _numericEntity = (NumericEntity)(object)p1;
            _type = (int)(object)p2;
        }

        public float SetBase(float value)
        {
            _numericEntity.Set(_type * 10 + 1, value);
            return BaseValue;
        }



        public float AddBase(float value)
        {
            float temp = BaseValue + value;
            _numericEntity.Set(_type * 10 + 1, temp);
            return BaseValue;
        }

        public float MinusBase(float value)
        {
            float temp = BaseValue - value;
            _numericEntity.Set(_type * 10 + 1, temp);
            return BaseValue;
        }




        public void AddAddModifier(float value)
        {
            float temp = Add + value;
            _numericEntity.Set(_type * 10 + 2, temp);
        }
        public void RemoveAddModifier(float value)
        {
            float temp = Add - value;
            _numericEntity.Set(_type * 10 + 2, temp);
        }





        public void AddPctAddModifier(float value)
        {
            float temp = PctAdd + value;
            _numericEntity.Set(_type * 10 + 3, temp);
        }
        public void RemovePctAddModifier(float value)
        {
            float temp = PctAdd - value;
            _numericEntity.Set(_type * 10 + 3, temp);
        }





        public void AddFinalAddModifier(float value)
        {
            float temp = FinalAdd + value;
            _numericEntity.Set(_type * 10 + 4, temp);
        }
        public void RemoveFinalAddModifier(float value)
        {
            float temp = FinalAdd - value;
            _numericEntity.Set(_type * 10 + 4, temp);
        }





        public void AddFinalPctAddModifier(float value)
        {
            float temp = FinalPctAdd + value;
            _numericEntity.Set(_type * 10 + 5, temp);
        }
        public void RemoveFinalPctAddModifier(float value)
        {
            float temp = FinalPctAdd - value;
            _numericEntity.Set(_type * 10 + 5, temp);
        }
    }
}