using ILRuntime.Runtime.Intepreter;
using System;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace LccModel
{
    public static partial class ILRuntimeHelper
    {
        public static void RegisterMethodDelegate(AppDomain appdomain)
        {
            appdomain.DelegateManager.RegisterMethodDelegate<ILTypeInstance, ILTypeInstance>();
            appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction>((act) =>
            {
                return new UnityEngine.Events.UnityAction(() =>
                {
                    ((Action)act)();
                });
            });
            appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.EventSystems.PointerEventData>();

            appdomain.DelegateManager.RegisterMethodDelegate<ILRuntime.Runtime.Intepreter.ILTypeInstance>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ILRuntime.Runtime.Intepreter.ILTypeInstance>();
            appdomain.DelegateManager.RegisterFunctionDelegate<System.Int32, UnityEngine.GameObject>();
            appdomain.DelegateManager.RegisterMethodDelegate<UnityEngine.Transform, System.Int32>();

        }
    }
}