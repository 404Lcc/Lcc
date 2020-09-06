using UnityEngine;

namespace Hotfix
{
    public class ViewBase<T> : MonoBehaviour, IView<T> where T : ViewModelBase
    {
        public bool init;
        public Binding<T> binding;
        protected DataBinder<T> dataBinder;
        public T ViewModel
        {
            get
            {
                return binding.Value;
            }
            set
            {
                if (!init)
                {
                    InitView();
                }
                binding.Value = value;
            }
        }
        public virtual void InitView()
        {
            init = true;
            binding = new Binding<T>();
            dataBinder = new DataBinder<T>();
            binding.OnValueChange += Binding;
        }
        public virtual void Binding(T oldValue, T newValue)
        {
            dataBinder.UnBind(oldValue);
            dataBinder.Bind(newValue);
        }
        public virtual void OpenPanel()
        {
        }
        public virtual void ClosePanel()
        {
        }
    }
}