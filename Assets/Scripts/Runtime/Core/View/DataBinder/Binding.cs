namespace Model
{
    public class Binding<T>
    {
        public delegate void ValueChangeHandler(T oldValue, T newValue);
        private T _value;
        public ValueChangeHandler OnValueChange;
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
                    ValueChange(old, _value);
                }
            }
        }
        public void ValueChange(T oldValue, T newValue)
        {
            OnValueChange?.Invoke(oldValue, newValue);
        }
    }
}