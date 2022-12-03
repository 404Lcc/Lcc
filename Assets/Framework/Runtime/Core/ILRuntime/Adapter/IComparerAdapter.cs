using System;
using System.Collections.Generic;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace LccModel
{   
    public class IComparerAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(IComparer<ILTypeInstance>);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter : IComparer<ILTypeInstance>, CrossBindingAdaptorType
        {
            private AppDomain _appdomain;
            private ILTypeInstance _instance;
            private CrossBindingFunctionInfo<ILTypeInstance, ILTypeInstance, int> _compare = new CrossBindingFunctionInfo<ILTypeInstance, ILTypeInstance, int>("Compare");
            private bool _isInvokingToString;
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

            public Adapter(AppDomain appdomain, ILTypeInstance instance)
            {
                _appdomain = appdomain;
                _instance = instance;
            }
            public int Compare(ILTypeInstance x, ILTypeInstance y)
            {
                return _compare.Invoke(_instance, x, y);
            }

            public override string ToString()
            {
                IMethod m = _appdomain.ObjectType.GetMethod("ToString", 0);
                m = _instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    if (!_isInvokingToString)
                    {
                        _isInvokingToString = true;
                        string res = _instance.ToString();
                        _isInvokingToString = false;
                        return res;
                    }
                    else
                        return _instance.Type.FullName;
                }
                else
                    return _instance.Type.FullName;
            }
        }
    }
}
