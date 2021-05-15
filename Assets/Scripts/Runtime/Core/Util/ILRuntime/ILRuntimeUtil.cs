using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace LccModel
{
    public static class ILRuntimeUtil
    {
        public static void LccFrameworkRegisterCrossBindingAdaptor(AppDomain appDomain)
        {
            appDomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
            appDomain.RegisterCrossBindingAdaptor(new IAsyncStateMachineAdapter());
        }
        public static void LccFrameworkRegisterMethodDelegate(AppDomain appDomain)
        {
            appDomain.DelegateManager.RegisterMethodDelegate<ILTypeInstance, ILTypeInstance>();
        }
    }
}