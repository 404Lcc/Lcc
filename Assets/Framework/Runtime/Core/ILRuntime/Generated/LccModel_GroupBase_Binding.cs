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
    unsafe class LccModel_GroupBase_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(LccModel.GroupBase);
            args = new Type[]{typeof(LccModel.ScrollerPro), typeof(UnityEngine.Transform)};
            method = type.GetMethod("InitGroup", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, InitGroup_0);

            field = type.GetField("groupIndex", flag);
            app.RegisterCLRFieldGetter(field, get_groupIndex_0);
            app.RegisterCLRFieldSetter(field, set_groupIndex_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_groupIndex_0, AssignFromStack_groupIndex_0);
            field = type.GetField("groupStart", flag);
            app.RegisterCLRFieldGetter(field, get_groupStart_1);
            app.RegisterCLRFieldSetter(field, set_groupStart_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_groupStart_1, AssignFromStack_groupStart_1);


        }


        static StackObject* InitGroup_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            UnityEngine.Transform @itemPrefab = (UnityEngine.Transform)typeof(UnityEngine.Transform).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            LccModel.ScrollerPro @scrollerPro = (LccModel.ScrollerPro)typeof(LccModel.ScrollerPro).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            LccModel.GroupBase instance_of_this_method = (LccModel.GroupBase)typeof(LccModel.GroupBase).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.InitGroup(@scrollerPro, @itemPrefab);

            return __ret;
        }


        static object get_groupIndex_0(ref object o)
        {
            return ((LccModel.GroupBase)o).groupIndex;
        }

        static StackObject* CopyToStack_groupIndex_0(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((LccModel.GroupBase)o).groupIndex;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_groupIndex_0(ref object o, object v)
        {
            ((LccModel.GroupBase)o).groupIndex = (System.Int32)v;
        }

        static StackObject* AssignFromStack_groupIndex_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Int32 @groupIndex = ptr_of_this_method->Value;
            ((LccModel.GroupBase)o).groupIndex = @groupIndex;
            return ptr_of_this_method;
        }

        static object get_groupStart_1(ref object o)
        {
            return ((LccModel.GroupBase)o).groupStart;
        }

        static StackObject* CopyToStack_groupStart_1(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((LccModel.GroupBase)o).groupStart;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_groupStart_1(ref object o, object v)
        {
            ((LccModel.GroupBase)o).groupStart = (System.Int32)v;
        }

        static StackObject* AssignFromStack_groupStart_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Int32 @groupStart = ptr_of_this_method->Value;
            ((LccModel.GroupBase)o).groupStart = @groupStart;
            return ptr_of_this_method;
        }



    }
}
