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
    unsafe class LccModel_LccView_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(LccModel.LccView);

            field = type.GetField("className", flag);
            app.RegisterCLRFieldGetter(field, get_className_0);
            app.RegisterCLRFieldSetter(field, set_className_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_className_0, AssignFromStack_className_0);
            field = type.GetField("type", flag);
            app.RegisterCLRFieldGetter(field, get_type_1);
            app.RegisterCLRFieldSetter(field, set_type_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_type_1, AssignFromStack_type_1);


        }



        static object get_className_0(ref object o)
        {
            return ((LccModel.LccView)o).className;
        }

        static StackObject* CopyToStack_className_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((LccModel.LccView)o).className;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_className_0(ref object o, object v)
        {
            ((LccModel.LccView)o).className = (System.String)v;
        }

        static StackObject* AssignFromStack_className_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @className = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((LccModel.LccView)o).className = @className;
            return ptr_of_this_method;
        }

        static object get_type_1(ref object o)
        {
            return ((LccModel.LccView)o).type;
        }

        static StackObject* CopyToStack_type_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((LccModel.LccView)o).type;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance, true);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method, true);
        }

        static void set_type_1(ref object o, object v)
        {
            ((LccModel.LccView)o).type = (System.Object)v;
        }

        static StackObject* AssignFromStack_type_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Object @type = (System.Object)typeof(System.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((LccModel.LccView)o).type = @type;
            return ptr_of_this_method;
        }



    }
}
