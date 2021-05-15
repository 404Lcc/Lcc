using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using System;
using System.Runtime.CompilerServices;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace LccModel
{
    public class IAsyncStateMachineAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(IAsyncStateMachine);
            }
        }
        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }
        public override object CreateCLRInstance(AppDomain appDomain, ILTypeInstance instance)
        {
            return new Adapter(appDomain, instance);
        }
        public class Adapter : IAsyncStateMachine, CrossBindingAdaptorType
        {
            private AppDomain _appDomain;
            private ILTypeInstance _instance;
            private IMethod _moveNextMethod;
            private IMethod _setStateMachineMethod;
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
            public void MoveNext()
            {
                if (_moveNextMethod == null)
                {
                    _moveNextMethod = _instance.Type.GetMethod("MoveNext", 0);
                }
                _appDomain.Invoke(_moveNextMethod, _instance, null);
            }
            public void SetStateMachine(IAsyncStateMachine asyncStateMachine)
            {
                if (_setStateMachineMethod == null)
                {
                    _setStateMachineMethod = _instance.Type.GetMethod("SetStateMachine", 0);
                }
                _appDomain.Invoke(_setStateMachineMethod, _instance, asyncStateMachine);
            }
            public override string ToString()
            {
                IMethod method = _instance.Type.GetVirtualMethod(_appDomain.ObjectType.GetMethod("ToString", 0));
                if (method == null || method is ILMethod)
                {
                    return _instance.ToString();
                }
                return _instance.Type.FullName;
            }
        }
    }
}