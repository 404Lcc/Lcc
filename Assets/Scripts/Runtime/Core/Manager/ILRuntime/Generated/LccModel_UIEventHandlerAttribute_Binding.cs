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
    unsafe class LccModel_UIEventHandlerAttribute_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(LccModel.UIEventHandlerAttribute);

            field = type.GetField("uiEventType", flag);
            app.RegisterCLRFieldGetter(field, get_uiEventType_0);
            app.RegisterCLRFieldSetter(field, set_uiEventType_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_uiEventType_0, AssignFromStack_uiEventType_0);


        }



        static object get_uiEventType_0(ref object o)
        {
            return ((LccModel.UIEventHandlerAttribute)o).uiEventType;
        }

        static StackObject* CopyToStack_uiEventType_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((LccModel.UIEventHandlerAttribute)o).uiEventType;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_uiEventType_0(ref object o, object v)
        {
            ((LccModel.UIEventHandlerAttribute)o).uiEventType = (System.String)v;
        }

        static StackObject* AssignFromStack_uiEventType_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @uiEventType = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((LccModel.UIEventHandlerAttribute)o).uiEventType = @uiEventType;
            return ptr_of_this_method;
        }



    }
}
