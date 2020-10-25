using System;

namespace LccHotfix
{
    public abstract class AViewBase<T> : AObjectBase, IView<T> where T : ViewModelBase
    {
        public bool isInit;
        public ViewModelBinding<T> viewModelBinding;
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
            viewModelBinding = new ViewModelBinding<T>();
            binding = new Binding<T>();
            binding.ValueChange += Binding;
        }
        public virtual void Binding(T oldValue, T newValue)
        {
            viewModelBinding.UnBind(oldValue);
            viewModelBinding.Bind(newValue);
        }
        public void Binding<TProperty>(string name, Binding<TProperty>.ValueChangeHandler valueChange)
        {
            viewModelBinding.Add(name, valueChange);
        }
        public void Binding<TProperty>(Binding<TProperty> binding, Binding<TProperty>.ValueChangeHandler valueChange)
        {
            viewModelBinding.Add(nameof(binding), valueChange);
        }
    }
}