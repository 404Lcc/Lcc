using System;

namespace LccModel
{
    public abstract class AViewBase<T> : AObjectBase, IView<T> where T : ViewModelBase
    {
        public bool isInit;
        public DataBinding<T> dataBinding;
        public Binding<T> binding;
        public AViewBase()
        {
            ViewModel = Activator.CreateInstance<T>();
        }
        public T ViewModel
        {
            get
            {
                return binding.Value;
            }
            set
            {
                if (!isInit)
                {
                    InitView();
                }
                binding.Value = value;
            }
        }
        public virtual void InitView()
        {
            isInit = true;
            dataBinding = new DataBinding<T>();
            binding = new Binding<T>();
            binding.OnValueChange += Binding;
        }
        public virtual void Binding(T oldValue, T newValue)
        {
            dataBinding.UnBind(oldValue);
            dataBinding.Bind(newValue);
        }
    }
}