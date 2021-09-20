using ILRuntime.Runtime.Enviorment;
using System;
using System.Linq;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace LccModel
{
    public static partial class ILRuntimeHelper
    {
        public static void RegisterCrossBindingAdaptor(AppDomain appdomain)
        {
            foreach (Type item in typeof(Init).Assembly.GetTypes().ToList().FindAll(item => item.IsSubclassOf(typeof(CrossBindingAdaptor))))
            {
                object obj = Activator.CreateInstance(item);
                if (!(obj is CrossBindingAdaptor))
                {
                    continue;
                }
                appdomain.RegisterCrossBindingAdaptor((CrossBindingAdaptor)obj);
            }
        }
    }
}