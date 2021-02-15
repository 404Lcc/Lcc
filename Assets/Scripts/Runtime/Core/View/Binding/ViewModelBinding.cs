using System;
using System.Collections.Generic;
using System.Reflection;

namespace LccModel
{
    public class ViewModelBinding<T> where T : ViewModelBase
    {
        //绑定ViewModel
        public List<Action<T>> viewModelBindingList;
        //解绑ViewModel
        public List<Action<T>> unViewModelBindingList;
        public ViewModelBinding()
        {
            viewModelBindingList = new List<Action<T>>();
            unViewModelBindingList = new List<Action<T>>();
        }
        public void Add<TProperty>(string name, Action<TProperty, TProperty> valueChange)
        {
            FieldInfo fieldInfo = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (fieldInfo == null) return;
            viewModelBindingList.Add((viewModel) =>
            {
                GetPropertyValue<TProperty>(viewModel, fieldInfo).ValueChange += valueChange;
            });
            unViewModelBindingList.Add((viewModel) =>
            {
                GetPropertyValue<TProperty>(viewModel, fieldInfo).ValueChange -= valueChange;
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