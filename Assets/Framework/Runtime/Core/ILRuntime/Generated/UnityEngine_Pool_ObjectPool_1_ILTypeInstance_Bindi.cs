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
    unsafe class UnityEngine_Pool_ObjectPool_1_ILTypeInstance_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance>);
            args = new Type[]{};
            method = type.GetMethod("get_CountActive", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_CountActive_0);
            args = new Type[]{};
            method = type.GetMethod("Get", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Get_1);
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            method = type.GetMethod("Release", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Release_2);
            args = new Type[]{};
            method = type.GetMethod("Clear", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Clear_3);

            args = new Type[]{typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Boolean), typeof(System.Int32), typeof(System.Int32)};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }


        static StackObject* get_CountActive_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance> instance_of_this_method = (UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.CountActive;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* Get_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance> instance_of_this_method = (UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Get();

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Release_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            ILRuntime.Runtime.Intepreter.ILTypeInstance @element = (ILRuntime.Runtime.Intepreter.ILTypeInstance)typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance> instance_of_this_method = (UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Release(@element);

            return __ret;
        }

        static StackObject* Clear_3(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance> instance_of_this_method = (UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Clear();

            return __ret;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 7);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @maxSize = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 @defaultCapacity = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Boolean @collectionCheck = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance> @actionOnDestroy = (System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance> @actionOnRelease = (System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 6);
            System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance> @actionOnGet = (System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Action<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 7);
            System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance> @createFunc = (System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = new UnityEngine.Pool.ObjectPool<ILRuntime.Runtime.Intepreter.ILTypeInstance>(@createFunc, @actionOnGet, @actionOnRelease, @actionOnDestroy, @collectionCheck, @defaultCapacity, @maxSize);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
