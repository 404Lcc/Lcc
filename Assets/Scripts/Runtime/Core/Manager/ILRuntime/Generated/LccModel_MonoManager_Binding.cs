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
    unsafe class LccModel_MonoManager_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(LccModel.MonoManager);

            field = type.GetField("typeList", flag);
            app.RegisterCLRFieldGetter(field, get_typeList_0);
            app.RegisterCLRFieldSetter(field, set_typeList_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_typeList_0, AssignFromStack_typeList_0);


        }



        static object get_typeList_0(ref object o)
        {
            return ((LccModel.MonoManager)o).typeList;
        }

        static StackObject* CopyToStack_typeList_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((LccModel.MonoManager)o).typeList;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_typeList_0(ref object o, object v)
        {
            ((LccModel.MonoManager)o).typeList = (System.Collections.Generic.List<System.Type>)v;
        }

        static StackObject* AssignFromStack_typeList_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Collections.Generic.List<System.Type> @typeList = (System.Collections.Generic.List<System.Type>)typeof(System.Collections.Generic.List<System.Type>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((LccModel.MonoManager)o).typeList = @typeList;
            return ptr_of_this_method;
        }



    }
}
