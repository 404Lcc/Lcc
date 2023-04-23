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
    unsafe class BM_LoadHandler_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(BM.LoadHandler);

            field = type.GetField("Asset", flag);
            app.RegisterCLRFieldGetter(field, get_Asset_0);
            app.RegisterCLRFieldSetter(field, set_Asset_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Asset_0, AssignFromStack_Asset_0);


        }



        static object get_Asset_0(ref object o)
        {
            return ((BM.LoadHandler)o).Asset;
        }

        static StackObject* CopyToStack_Asset_0(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((BM.LoadHandler)o).Asset;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Asset_0(ref object o, object v)
        {
            ((BM.LoadHandler)o).Asset = (UnityEngine.Object)v;
        }

        static StackObject* AssignFromStack_Asset_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            UnityEngine.Object @Asset = (UnityEngine.Object)typeof(UnityEngine.Object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ((BM.LoadHandler)o).Asset = @Asset;
            return ptr_of_this_method;
        }



    }
}
