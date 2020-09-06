using System.Collections.Generic;
using System.Reflection;

namespace Model
{
    public class DataBinder<T> where T : ViewModelBase
    {
        public delegate void DataBinderHandler(T viewModel);
        public delegate void UnDataBinderHandler(T viewModel);

        public List<DataBinderHandler> dataBinderList;
        public List<UnDataBinderHandler> unDataBinderList;
        public DataBinder()
        {
            dataBinderList = new List<DataBinderHandler>();
            unDataBinderList = new List<UnDataBinderHandler>();
        }
        public void Add<TProperty>(string name, Binding<TProperty>.ValueChangeHandler valueChangeHandler)
        {
            FieldInfo fieldInfo = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.Public);
            dataBinderList.Add((viewModel) =>
            {
                GetPropertyValue<TProperty>(viewModel, fieldInfo).OnValueChange += valueChangeHandler;
            });
            unDataBinderList.Add((viewModel) =>
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
                for (int i = 0; i < dataBinderList.Count; i++)
                {
                    dataBinderList[i](viewModel);
                }
            }
        }
        public void UnBind(T viewModel)
        {
            if (viewModel != null)
            {
                for (int i = 0; i < unDataBinderList.Count; i++)
                {
                    unDataBinderList[i](viewModel);
                }
            }
        }
    }
}