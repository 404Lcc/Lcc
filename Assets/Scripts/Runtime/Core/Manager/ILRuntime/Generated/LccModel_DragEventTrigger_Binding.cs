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

namespace ILRuntime.Runtime.Generated
{
    unsafe class LccModel_DragEventTrigger_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(LccModel.DragEventTrigger);
            args = new Type[]{typeof(UnityEngine.GameObject)};
            method = type.GetMethod("GetDragEventTrigger", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetDragEventTrigger_0);
            args = new Type[]{typeof(System.Action<UnityEngine.EventSystems.PointerEventData>)};
            method = type.GetMethod("add_Down", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, add_Down_1);
            args = new Type[]{typeof(System.Action<UnityEngine.EventSystems.PointerEventData>)};
            method = type.GetMethod("add_UP", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, add_UP_2);
            args = new Type[]{typeof(System.Action<UnityEngine.EventSystems.PointerEventData>)};
            method = type.GetMethod("add_Drag", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, add_Drag_3);


        }


        static StackObject* GetDragEventTrigger_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.GameObject @gameObject = (UnityEngine.GameObject)typeof(UnityEngine.GameObject).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = LccModel.DragEventTrigger.GetDragEventTrigger(@gameObject);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* add_Down_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<UnityEngine.EventSystems.PointerEventData> @value = (System.Action<UnityEngine.EventSystems.PointerEventData>)typeof(System.Action<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            LccModel.DragEventTrigger instance_of_this_method = (LccModel.DragEventTrigger)typeof(LccModel.DragEventTrigger).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Down += value;

            return __ret;
        }

        static StackObject* add_UP_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<UnityEngine.EventSystems.PointerEventData> @value = (System.Action<UnityEngine.EventSystems.PointerEventData>)typeof(System.Action<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            LccModel.DragEventTrigger instance_of_this_method = (LccModel.DragEventTrigger)typeof(LccModel.DragEventTrigger).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.UP += value;

            return __ret;
        }

        static StackObject* add_Drag_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Action<UnityEngine.EventSystems.PointerEventData> @value = (System.Action<UnityEngine.EventSystems.PointerEventData>)typeof(System.Action<UnityEngine.EventSystems.PointerEventData>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            LccModel.DragEventTrigger instance_of_this_method = (LccModel.DragEventTrigger)typeof(LccModel.DragEventTrigger).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Drag += value;

            return __ret;
        }



    }
}
