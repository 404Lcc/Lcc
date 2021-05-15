using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public override Type[] BaseCLRTypes
        {
            get
            {
                //跨域继承只能有1个Adapter，因此应该尽量避免一个类同时实现多个外部接口，对于coroutine来说是IEnumerator<object>,IEnumerator和IDisposable
                //ILRuntime虽然支持，但是一定要小心这种用法，使用不当很容易造成不可预期的问题
                //日常开发如果需要实现多个DLL外部接口，请在Unity这边先做一个基类实现那些个接口，然后继承那个基类
                return new Type[] { typeof(IEnumerator<object>), typeof(IEnumerator), typeof(IDisposable) };
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
        public class Adapter : CrossBindingAdaptorType
        {
            private AppDomain _appDomain;
            private ILTypeInstance _instance;
            private IMethod _currentMethod;
            private IMethod _disposeMethod;
            private IMethod _moveNextMethod;
            private IMethod _resetMethod;
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
            public object Current
            {
                get
                {
                    if (_currentMethod == null)
                    {
                        _currentMethod = _instance.Type.GetMethod("get_Current", 0);
                        if (_currentMethod == null)
                        {
                            _currentMethod = _instance.Type.GetMethod("System.Collections.IEnumerator.get_Current", 0);
                        }
                    }
                    return _appDomain.Invoke(_currentMethod, _instance, null);
                }
            }
            public void Dispose()
            {
                if (_disposeMethod == null)
                {
                    _disposeMethod = _instance.Type.GetMethod("Dispose", 0);
                    if (_disposeMethod == null)
                    {
                        _disposeMethod = _instance.Type.GetMethod("System.IDisposable.Dispose", 0);
                    }
                }
                _appDomain.Invoke(_disposeMethod, _instance, null);
            }
            public bool MoveNext()
            {
                if (_moveNextMethod == null)
                {
                    _moveNextMethod = _instance.Type.GetMethod("MoveNext", 0);
                }
                return (bool)_appDomain.Invoke(_moveNextMethod, _instance, null);
            }
            public void Reset()
            {
                if (_resetMethod == null)
                {
                    _resetMethod = _instance.Type.GetMethod("Reset", 0);
                }
                _appDomain.Invoke(_resetMethod, _instance, null);
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