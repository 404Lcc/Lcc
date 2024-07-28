namespace LccModel
{
    public class FloatNumeric : Entity
    {
        private NumericEntity _numericEntity;

        private int _type;

        public float BaseValue
        {
            get
            {
                return _numericEntity.GetFloat(_type * 10 + 1);
            }
            set
            {
                _numericEntity.Set(_type * 10 + 1, value);
                EventSystem.Instance.Publish(new SyncModifyAttribute(Parent.InstanceId, _type, 1, value));
            }
        }
        public float Add
        {
            get
            {
                return _numericEntity.GetFloat(_type * 10 + 2);
            }
            set
            {
                _numericEntity.Set(_type * 10 + 2, value);
                EventSystem.Instance.Publish(new SyncModifyAttribute(Parent.InstanceId, _type, 2, value));
            }
        }
        public float PctAdd
        {
            get
            {
                return _numericEntity.GetFloat(_type * 10 + 3);
            }
            set
            {
                _numericEntity.Set(_type * 10 + 3, value);
                EventSystem.Instance.Publish(new SyncModifyAttribute(Parent.InstanceId, _type, 3, value));
            }
        }
        public float FinalAdd
        {
            get
            {
                return _numericEntity.GetFloat(_type * 10 + 4);
            }
            set
            {
                _numericEntity.Set(_type * 10 + 4, value);
                EventSystem.Instance.Publish(new SyncModifyAttribute(Parent.InstanceId, _type, 4, value));
            }
        }
        public float FinalPctAdd
        {
            get
            {
                return _numericEntity.GetFloat(_type * 10 + 5);
            }
            set
            {
                _numericEntity.Set(_type * 10 + 5, value);
                EventSystem.Instance.Publish(new SyncModifyAttribute(Parent.InstanceId, _type, 5, value));
            }
        }
        public float Value => _numericEntity.GetFloat(_type);

        public override void Awake<P1, P2>(P1 p1, P2 p2)
        {
            base.Awake(p1, p2);

            _numericEntity = (NumericEntity)(object)p1;
            _type = (int)(AttributeType)(object)p2;
        }
    }
}