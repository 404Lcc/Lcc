using System;

namespace LccModel
{
    public abstract class AViewBase<T> : AObjectBase, IView<T> where T : ViewModelBase
    {
        public bool isInit;
        public ViewModelBinding<T> viewModelBinding = new ViewModelBinding<T>();
        public Binding<T> binding = new Binding<T>();
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
                    isInit = true;
                    binding.ValueChange += Binding;
                    InitView(value);
                }
                binding.Value = value;
            }
        }
        public virtual void InitView(T viewModel)
        {
        }
        public virtual void Binding(T oldValue, T newValue)
        {
            viewModelBinding.UnBind(oldValue);
            viewModelBinding.Bind(newValue);
        }
        public void Binding<TProperty>(string name, Action<TProperty, TProperty> valueChange)
        {
            viewModelBinding.Add(name, valueChange);
        }
    }
}