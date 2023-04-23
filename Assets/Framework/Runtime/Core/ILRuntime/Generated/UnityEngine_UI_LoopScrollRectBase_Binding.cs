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
    unsafe class UnityEngine_UI_LoopScrollRectBase_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(UnityEngine.UI.LoopScrollRectBase);
            args = new Type[]{};
            method = type.GetMethod("get_content", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_content_0);
            args = new Type[]{};
            method = type.GetMethod("ClearCells", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ClearCells_1);
            args = new Type[]{typeof(System.Int32), typeof(System.Boolean), typeof(System.Single)};
            method = type.GetMethod("RefillCells", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, RefillCells_2);

            field = type.GetField("GetObjectHandler", flag);
            app.RegisterCLRFieldGetter(field, get_GetObjectHandler_0);
            app.RegisterCLRFieldSetter(field, set_GetObjectHandler_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_GetObjectHandler_0, AssignFromStack_GetObjectHandler_0);
            field = type.GetField("ReturnObjectHandler", flag);
            app.RegisterCLRFieldGetter(field, get_ReturnObjectHandler_1);
            app.RegisterCLRFieldSetter(field, set_ReturnObjectHandler_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_ReturnObjectHandler_1, AssignFromStack_ReturnObjectHandler_1);
            field = type.GetField("ProvideDataHandler", flag);
            app.RegisterCLRFieldGetter(field, get_ProvideDataHandler_2);
            app.RegisterCLRFieldSetter(field, set_ProvideDataHandler_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_ProvideDataHandler_2, AssignFromStack_ProvideDataHandler_2);
            field = type.GetField("totalCount", flag);
            app.RegisterCLRFieldGetter(field, get_totalCount_3);
            app.RegisterCLRFieldSetter(field, set_totalCount_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_totalCount_3, AssignFromStack_totalCount_3);


        }


        static StackObject* get_content_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.UI.LoopScrollRectBase instance_of_this_method = (UnityEngine.UI.LoopScrollRectBase)typeof(UnityEngine.UI.LoopScrollRectBase).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.content;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ClearCells_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.UI.LoopScrollRectBase instance_of_this_method = (UnityEngine.UI.LoopScrollRectBase)typeof(UnityEngine.UI.LoopScrollRectBase).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.ClearCells();

            return __ret;
        }

        static StackObject* RefillCells_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @contentOffset = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Boolean @fillViewRect = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Int32 @startItem = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            UnityEngine.UI.LoopScrollRectBase instance_of_this_method = (UnityEngine.UI.LoopScrollRectBase)typeof(UnityEngine.UI.LoopScrollRectBase).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.RefillCells(@startItem, @fillViewRect, @contentOffset);

            return __ret;
        }


        static object get_GetObjectHandler_0(ref object o)
        {
            return ((UnityEngine.UI.LoopScrollRectBase)o).GetObjectHandler;
        }

        static StackObject* CopyToStack_GetObjectHandler_0(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((UnityEngine.UI.LoopScrollRectBase)o).GetObjectHandler;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_GetObjectHandler_0(ref object o, object v)
        {
            ((UnityEngine.UI.LoopScrollRectBase)o).GetObjectHandler = (System.Func<System.Int32, UnityEngine.GameObject>)v;
        }

        static StackObject* AssignFromStack_GetObjectHandler_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Func<System.Int32, UnityEngine.GameObject> @GetObjectHandler = (System.Func<System.Int32, UnityEngine.GameObject>)typeof(System.Func<System.Int32, UnityEngine.GameObject>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((UnityEngine.UI.LoopScrollRectBase)o).GetObjectHandler = @GetObjectHandler;
            return ptr_of_this_method;
        }

        static object get_ReturnObjectHandler_1(ref object o)
        {
            return ((UnityEngine.UI.LoopScrollRectBase)o).ReturnObjectHandler;
        }

        static StackObject* CopyToStack_ReturnObjectHandler_1(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((UnityEngine.UI.LoopScrollRectBase)o).ReturnObjectHandler;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_ReturnObjectHandler_1(ref object o, object v)
        {
            ((UnityEngine.UI.LoopScrollRectBase)o).ReturnObjectHandler = (System.Action<UnityEngine.Transform, System.Int32>)v;
        }

        static StackObject* AssignFromStack_ReturnObjectHandler_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<UnityEngine.Transform, System.Int32> @ReturnObjectHandler = (System.Action<UnityEngine.Transform, System.Int32>)typeof(System.Action<UnityEngine.Transform, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((UnityEngine.UI.LoopScrollRectBase)o).ReturnObjectHandler = @ReturnObjectHandler;
            return ptr_of_this_method;
        }

        static object get_ProvideDataHandler_2(ref object o)
        {
            return ((UnityEngine.UI.LoopScrollRectBase)o).ProvideDataHandler;
        }

        static StackObject* CopyToStack_ProvideDataHandler_2(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((UnityEngine.UI.LoopScrollRectBase)o).ProvideDataHandler;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_ProvideDataHandler_2(ref object o, object v)
        {
            ((UnityEngine.UI.LoopScrollRectBase)o).ProvideDataHandler = (System.Action<UnityEngine.Transform, System.Int32>)v;
        }

        static StackObject* AssignFromStack_ProvideDataHandler_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<UnityEngine.Transform, System.Int32> @ProvideDataHandler = (System.Action<UnityEngine.Transform, System.Int32>)typeof(System.Action<UnityEngine.Transform, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((UnityEngine.UI.LoopScrollRectBase)o).ProvideDataHandler = @ProvideDataHandler;
            return ptr_of_this_method;
        }

        static object get_totalCount_3(ref object o)
        {
            return ((UnityEngine.UI.LoopScrollRectBase)o).totalCount;
        }

        static StackObject* CopyToStack_totalCount_3(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((UnityEngine.UI.LoopScrollRectBase)o).totalCount;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_totalCount_3(ref object o, object v)
        {
            ((UnityEngine.UI.LoopScrollRectBase)o).totalCount = (System.Int32)v;
        }

        static StackObject* AssignFromStack_totalCount_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Int32 @totalCount = ptr_of_this_method->Value;
            ((UnityEngine.UI.LoopScrollRectBase)o).totalCount = @totalCount;
            return ptr_of_this_method;
        }



    }
}
