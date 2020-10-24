using System.Collections.Generic;
using System.Reflection;

namespace LccHotfix
{
    public class ViewModelBinding<T> where T : ViewModelBase
    {
        public delegate void ViewModelBindingHandler(T viewModel);
        public delegate void UnViewModelBindingHandler(T viewModel);

        public List<ViewModelBindingHandler> viewModelBindingList;
        public List<UnViewModelBindingHandler> unViewModelBindingList;
        public ViewModelBinding()
        {
            viewModelBindingList = new List<ViewModelBindingHandler>();
            unViewModelBindingList = new List<UnViewModelBindingHandler>();
        }
        public void Add<TProperty>(string name, Binding<TProperty>.ValueChangeHandler valueChangeHandler)
        {
            FieldInfo fieldInfo = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.Public);
            viewModelBindingList.Add((viewModel) =>
            {
                GetPropertyValue<TProperty>(viewModel, fieldInfo).OnValueChange += valueChangeHandler;
            });
            unViewModelBindingList.Add((viewModel) =>
            {
                GetPropertyValue<TProperty>(viewModel, fieldInfo).OnValueChange -= valueChangeHandler;
            });
        }
        public Binding<TProperty> GetPropertyValue<TProperty>(T viewModel, FieldInfo fieldInfo)
        {
            return (Binding<TProperty>)fieldInfo.GetValue(viewModel);
        }
        public void Bind(T viewModel)
        {
            if (viewModel != null)
            {
                for (int i = 0; i < viewModelBindingList.Count; i++)
                {
                    viewModelBindingList[i](viewModel);
                }
            }
        }
        public void UnBind(T viewModel)
        {
            if (viewModel != null)
            {
                for (int i = 0; i < unViewModelBindingList.Count; i++)
                {
                    unViewModelBindingList[i](viewModel);
                }
            }
        }
    }
}