using System.Collections.Generic;
using System.Reflection;

namespace LccModel
{
    public class DataBinding<T> where T : ViewModelBase
    {
        public delegate void DataBindingHandler(T viewModel);
        public delegate void UnDataBindingHandler(T viewModel);

        public List<DataBindingHandler> dataBindingList;
        public List<UnDataBindingHandler> unDataBindingList;
        public DataBinding()
        {
            dataBindingList = new List<DataBindingHandler>();
            unDataBindingList = new List<UnDataBindingHandler>();
        }
        public void Add<TProperty>(string name, Binding<TProperty>.ValueChangeHandler valueChangeHandler)
        {
            FieldInfo fieldInfo = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.Public);
            dataBindingList.Add((viewModel) =>
            {
                GetPropertyValue<TProperty>(viewModel, fieldInfo).OnValueChange += valueChangeHandler;
            });
            unDataBindingList.Add((viewModel) =>
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
                for (int i = 0; i < dataBindingList.Count; i++)
                {
                    dataBindingList[i](viewModel);
                }
            }
        }
        public void UnBind(T viewModel)
        {
            if (viewModel != null)
            {
                for (int i = 0; i < unDataBindingList.Count; i++)
                {
                    unDataBindingList[i](viewModel);
                }
            }
        }
    }
}