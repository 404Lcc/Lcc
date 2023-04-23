using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif
namespace ILRuntime.Runtime.Generated
{
    unsafe class DG_Tweening_DOTween_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(DG.Tweening.DOTween);
            args = new Type[]{typeof(DG.Tweening.Core.DOGetter<System.Single>), typeof(DG.Tweening.Core.DOSetter<System.Single>), typeof(System.Single), typeof(System.Single)};
            method = type.GetMethod("To", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, To_0);


        }


        static StackObject* To_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @duration = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Single @endValue = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            DG.Tweening.Core.DOSetter<System.Single> @setter = (DG.Tweening.Core.DOSetter<System.Single>)typeof(DG.Tweening.Core.DOSetter<System.Single>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            DG.Tweening.Core.DOGetter<System.Single> @getter = (DG.Tweening.Core.DOGetter<System.Single>)typeof(DG.Tweening.Core.DOGetter<System.Single>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = DG.Tweening.DOTween.To(@getter, @setter, @endValue, @duration);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }



    }
}
