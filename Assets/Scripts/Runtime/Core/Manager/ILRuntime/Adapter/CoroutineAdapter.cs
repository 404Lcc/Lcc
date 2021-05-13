using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using System;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace LccModel
{
    public class CoroutineAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return null;
            }
        }
        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter();
        }
        public class Adapter : CrossBindingAdaptorType
        {
            private AppDomain _appDomain;
            private ILTypeInstance _instance;
            private IMethod method;
            public ILTypeInstance ILInstance
            {
                get
                {
                    return _instance;
                }
            }
            public Adapter()
            {
            }
            public Adapter(AppDomain appDomain, ILTypeInstance instance)
            {
                _appDomain = appDomain;
                _instance = instance;
            }
        }
    }
}