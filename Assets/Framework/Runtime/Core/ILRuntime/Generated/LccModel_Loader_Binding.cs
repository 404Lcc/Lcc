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
    unsafe class LccModel_Loader_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(LccModel.Loader);
            args = new Type[]{};
            method = type.GetMethod("GetHotfixTypeDict", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetHotfixTypeDict_0);

            field = type.GetField("FixedUpdate", flag);
            app.RegisterCLRFieldGetter(field, get_FixedUpdate_0);
            app.RegisterCLRFieldSetter(field, set_FixedUpdate_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_FixedUpdate_0, AssignFromStack_FixedUpdate_0);
            field = type.GetField("Update", flag);
            app.RegisterCLRFieldGetter(field, get_Update_1);
            app.RegisterCLRFieldSetter(field, set_Update_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_Update_1, AssignFromStack_Update_1);
            field = type.GetField("LateUpdate", flag);
            app.RegisterCLRFieldGetter(field, get_LateUpdate_2);
            app.RegisterCLRFieldSetter(field, set_LateUpdate_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_LateUpdate_2, AssignFromStack_LateUpdate_2);
            field = type.GetField("OnApplicationQuit", flag);
            app.RegisterCLRFieldGetter(field, get_OnApplicationQuit_3);
            app.RegisterCLRFieldSetter(field, set_OnApplicationQuit_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_OnApplicationQuit_3, AssignFromStack_OnApplicationQuit_3);


        }


        static StackObject* GetHotfixTypeDict_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            LccModel.Loader instance_of_this_method = (LccModel.Loader)typeof(LccModel.Loader).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetHotfixTypeDict();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


        static object get_FixedUpdate_0(ref object o)
        {
            return ((LccModel.Loader)o).FixedUpdate;
        }

        static StackObject* CopyToStack_FixedUpdate_0(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((LccModel.Loader)o).FixedUpdate;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_FixedUpdate_0(ref object o, object v)
        {
            ((LccModel.Loader)o).FixedUpdate = (System.Action)v;
        }

        static StackObject* AssignFromStack_FixedUpdate_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action @FixedUpdate = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((LccModel.Loader)o).FixedUpdate = @FixedUpdate;
            return ptr_of_this_method;
        }

        static object get_Update_1(ref object o)
        {
            return ((LccModel.Loader)o).Update;
        }

        static StackObject* CopyToStack_Update_1(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((LccModel.Loader)o).Update;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Update_1(ref object o, object v)
        {
            ((LccModel.Loader)o).Update = (System.Action)v;
        }

        static StackObject* AssignFromStack_Update_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action @Update = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((LccModel.Loader)o).Update = @Update;
            return ptr_of_this_method;
        }

        static object get_LateUpdate_2(ref object o)
        {
            return ((LccModel.Loader)o).LateUpdate;
        }

        static StackObject* CopyToStack_LateUpdate_2(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((LccModel.Loader)o).LateUpdate;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_LateUpdate_2(ref object o, object v)
        {
            ((LccModel.Loader)o).LateUpdate = (System.Action)v;
        }

        static StackObject* AssignFromStack_LateUpdate_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action @LateUpdate = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((LccModel.Loader)o).LateUpdate = @LateUpdate;
            return ptr_of_this_method;
        }

        static object get_OnApplicationQuit_3(ref object o)
        {
            return ((LccModel.Loader)o).OnApplicationQuit;
        }

        static StackObject* CopyToStack_OnApplicationQuit_3(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((LccModel.Loader)o).OnApplicationQuit;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_OnApplicationQuit_3(ref object o, object v)
        {
            ((LccModel.Loader)o).OnApplicationQuit = (System.Action)v;
        }

        static StackObject* AssignFromStack_OnApplicationQuit_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action @OnApplicationQuit = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((LccModel.Loader)o).OnApplicationQuit = @OnApplicationQuit;
            return ptr_of_this_method;
        }



    }
}
