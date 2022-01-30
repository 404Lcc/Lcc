namespace LccModel
{
    public class Binding<T>
    {
        //第一个参数是旧的 第二个参数是新的
        public delegate void ValueChangeHandler(T oldValue, T newValue);
        public event ValueChangeHandler ValueChange;
        private T _value = default;
        public Binding()
        {
        }
        public Binding(T value)
        {
            Value = value;
        }
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!Equals(_value, value))
                {
                    T old = _value;
                    _value = value;
                    OnValueChange(old, _value);
                }
            }
        }
        public void OnValueChange(T oldValue, T newValue)
        {
            ValueChange?.Invoke(oldValue, newValue);
        }
        public static implicit operator T(Binding<T> value)
        {
            return value.Value;
        }
    }
}