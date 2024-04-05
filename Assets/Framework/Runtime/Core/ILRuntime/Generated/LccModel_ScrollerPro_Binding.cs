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
    unsafe class LccModel_ScrollerPro_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(LccModel.ScrollerPro);
            args = new Type[]{};
            method = type.GetMethod("get_NumberOfCellsPerRow", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_NumberOfCellsPerRow_0);
            args = new Type[]{};
            method = type.GetMethod("get_Scroller", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Scroller_1);
            args = new Type[]{};
            method = type.GetMethod("RefershData", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, RefershData_2);

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
            field = type.GetField("GetGroupSizeHandler", flag);
            app.RegisterCLRFieldGetter(field, get_GetGroupSizeHandler_3);
            app.RegisterCLRFieldSetter(field, set_GetGroupSizeHandler_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_GetGroupSizeHandler_3, AssignFromStack_GetGroupSizeHandler_3);
            field = type.GetField("GetDataCountHandler", flag);
            app.RegisterCLRFieldGetter(field, get_GetDataCountHandler_4);
            app.RegisterCLRFieldSetter(field, set_GetDataCountHandler_4);
            app.RegisterCLRFieldBinding(field, CopyToStack_GetDataCountHandler_4, AssignFromStack_GetDataCountHandler_4);
            field = type.GetField("isGrid", flag);
            app.RegisterCLRFieldGetter(field, get_isGrid_5);
            app.RegisterCLRFieldSetter(field, set_isGrid_5);
            app.RegisterCLRFieldBinding(field, CopyToStack_isGrid_5, AssignFromStack_isGrid_5);
            field = type.GetField("groupPrefab", flag);
            app.RegisterCLRFieldGetter(field, get_groupPrefab_6);
            app.RegisterCLRFieldSetter(field, set_groupPrefab_6);
            app.RegisterCLRFieldBinding(field, CopyToStack_groupPrefab_6, AssignFromStack_groupPrefab_6);


        }


        static StackObject* get_NumberOfCellsPerRow_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            LccModel.ScrollerPro instance_of_this_method = (LccModel.ScrollerPro)typeof(LccModel.ScrollerPro).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.NumberOfCellsPerRow;

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_Scroller_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            LccModel.ScrollerPro instance_of_this_method = (LccModel.ScrollerPro)typeof(LccModel.ScrollerPro).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.Scroller;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* RefershData_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            LccModel.ScrollerPro instance_of_this_method = (LccModel.ScrollerPro)typeof(LccModel.ScrollerPro).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.RefershData();

            return __ret;
        }


        static object get_GetObjectHandler_0(ref object o)
        {
            return ((LccModel.ScrollerPro)o).GetObjectHandler;
        }

        static StackObject* CopyToStack_GetObjectHandler_0(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((LccModel.ScrollerPro)o).GetObjectHandler;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_GetObjectHandler_0(ref object o, object v)
        {
            ((LccModel.ScrollerPro)o).GetObjectHandler = (System.Action<LccModel.GroupBase, System.Int32>)v;
        }

        static StackObject* AssignFromStack_GetObjectHandler_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<LccModel.GroupBase, System.Int32> @GetObjectHandler = (System.Action<LccModel.GroupBase, System.Int32>)typeof(System.Action<LccModel.GroupBase, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((LccModel.ScrollerPro)o).GetObjectHandler = @GetObjectHandler;
            return ptr_of_this_method;
        }

        static object get_ReturnObjectHandler_1(ref object o)
        {
            return ((LccModel.ScrollerPro)o).ReturnObjectHandler;
        }

        static StackObject* CopyToStack_ReturnObjectHandler_1(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((LccModel.ScrollerPro)o).ReturnObjectHandler;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_ReturnObjectHandler_1(ref object o, object v)
        {
            ((LccModel.ScrollerPro)o).ReturnObjectHandler = (System.Action<System.Int32>)v;
        }

        static StackObject* AssignFromStack_ReturnObjectHandler_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<System.Int32> @ReturnObjectHandler = (System.Action<System.Int32>)typeof(System.Action<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((LccModel.ScrollerPro)o).ReturnObjectHandler = @ReturnObjectHandler;
            return ptr_of_this_method;
        }

        static object get_ProvideDataHandler_2(ref object o)
        {
            return ((LccModel.ScrollerPro)o).ProvideDataHandler;
        }

        static StackObject* CopyToStack_ProvideDataHandler_2(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((LccModel.ScrollerPro)o).ProvideDataHandler;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_ProvideDataHandler_2(ref object o, object v)
        {
            ((LccModel.ScrollerPro)o).ProvideDataHandler = (System.Action<System.Int32>)v;
        }

        static StackObject* AssignFromStack_ProvideDataHandler_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<System.Int32> @ProvideDataHandler = (System.Action<System.Int32>)typeof(System.Action<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((LccModel.ScrollerPro)o).ProvideDataHandler = @ProvideDataHandler;
            return ptr_of_this_method;
        }

        static object get_GetGroupSizeHandler_3(ref object o)
        {
            return ((LccModel.ScrollerPro)o).GetGroupSizeHandler;
        }

        static StackObject* CopyToStack_GetGroupSizeHandler_3(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((LccModel.ScrollerPro)o).GetGroupSizeHandler;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_GetGroupSizeHandler_3(ref object o, object v)
        {
            ((LccModel.ScrollerPro)o).GetGroupSizeHandler = (System.Func<System.Int32, System.Int32>)v;
        }

        static StackObject* AssignFromStack_GetGroupSizeHandler_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Func<System.Int32, System.Int32> @GetGroupSizeHandler = (System.Func<System.Int32, System.Int32>)typeof(System.Func<System.Int32, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((LccModel.ScrollerPro)o).GetGroupSizeHandler = @GetGroupSizeHandler;
            return ptr_of_this_method;
        }

        static object get_GetDataCountHandler_4(ref object o)
        {
            return ((LccModel.ScrollerPro)o).GetDataCountHandler;
        }

        static StackObject* CopyToStack_GetDataCountHandler_4(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((LccModel.ScrollerPro)o).GetDataCountHandler;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_GetDataCountHandler_4(ref object o, object v)
        {
            ((LccModel.ScrollerPro)o).GetDataCountHandler = (System.Func<System.Int32>)v;
        }

        static StackObject* AssignFromStack_GetDataCountHandler_4(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Func<System.Int32> @GetDataCountHandler = (System.Func<System.Int32>)typeof(System.Func<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((LccModel.ScrollerPro)o).GetDataCountHandler = @GetDataCountHandler;
            return ptr_of_this_method;
        }

        static object get_isGrid_5(ref object o)
        {
            return ((LccModel.ScrollerPro)o).isGrid;
        }

        static StackObject* CopyToStack_isGrid_5(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((LccModel.ScrollerPro)o).isGrid;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static void set_isGrid_5(ref object o, object v)
        {
            ((LccModel.ScrollerPro)o).isGrid = (System.Boolean)v;
        }

        static StackObject* AssignFromStack_isGrid_5(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Boolean @isGrid = ptr_of_this_method->Value == 1;
            ((LccModel.ScrollerPro)o).isGrid = @isGrid;
            return ptr_of_this_method;
        }

        static object get_groupPrefab_6(ref object o)
        {
            return ((LccModel.ScrollerPro)o).groupPrefab;
        }

        static StackObject* CopyToStack_groupPrefab_6(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((LccModel.ScrollerPro)o).groupPrefab;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_groupPrefab_6(ref object o, object v)
        {
            ((LccModel.ScrollerPro)o).groupPrefab = (LccModel.GroupBase)v;
        }

        static StackObject* AssignFromStack_groupPrefab_6(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            LccModel.GroupBase @groupPrefab = (LccModel.GroupBase)typeof(LccModel.GroupBase).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            ((LccModel.ScrollerPro)o).groupPrefab = @groupPrefab;
            return ptr_of_this_method;
        }



    }
}
