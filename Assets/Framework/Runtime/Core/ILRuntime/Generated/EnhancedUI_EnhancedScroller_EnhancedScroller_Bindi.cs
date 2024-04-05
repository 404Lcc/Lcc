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
    unsafe class EnhancedUI_EnhancedScroller_EnhancedScroller_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(EnhancedUI.EnhancedScroller.EnhancedScroller);
            args = new Type[]{typeof(System.Int32), typeof(EnhancedUI.EnhancedScroller.EnhancedScroller.CellViewPositionEnum)};
            method = type.GetMethod("GetScrollPositionForCellViewIndex", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, GetScrollPositionForCellViewIndex_0);
            args = new Type[]{};
            method = type.GetMethod("get_ScrollPosition", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_ScrollPosition_1);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("IgnoreLoopJump", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, IgnoreLoopJump_2);
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("ReloadData", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ReloadData_3);
            args = new Type[]{typeof(System.Single)};
            method = type.GetMethod("SetScrollPositionImmediately", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetScrollPositionImmediately_4);
            args = new Type[]{};
            method = type.GetMethod("ClearAll", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ClearAll_5);
            args = new Type[]{typeof(System.Int32), typeof(System.Single), typeof(System.Single), typeof(System.Boolean), typeof(EnhancedUI.EnhancedScroller.EnhancedScroller.TweenType), typeof(System.Single), typeof(System.Action), typeof(EnhancedUI.EnhancedScroller.EnhancedScroller.LoopJumpDirectionEnum), typeof(System.Boolean)};
            method = type.GetMethod("JumpToDataIndex", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, JumpToDataIndex_6);

            field = type.GetField("spacing", flag);
            app.RegisterCLRFieldGetter(field, get_spacing_0);
            app.RegisterCLRFieldSetter(field, set_spacing_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_spacing_0, AssignFromStack_spacing_0);
            field = type.GetField("scrollDirection", flag);
            app.RegisterCLRFieldGetter(field, get_scrollDirection_1);
            app.RegisterCLRFieldSetter(field, set_scrollDirection_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_scrollDirection_1, AssignFromStack_scrollDirection_1);


        }


        static StackObject* GetScrollPositionForCellViewIndex_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            EnhancedUI.EnhancedScroller.EnhancedScroller.CellViewPositionEnum @insertPosition = (EnhancedUI.EnhancedScroller.EnhancedScroller.CellViewPositionEnum)typeof(EnhancedUI.EnhancedScroller.EnhancedScroller.CellViewPositionEnum).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)20);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 @cellViewIndex = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            EnhancedUI.EnhancedScroller.EnhancedScroller instance_of_this_method = (EnhancedUI.EnhancedScroller.EnhancedScroller)typeof(EnhancedUI.EnhancedScroller.EnhancedScroller).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.GetScrollPositionForCellViewIndex(@cellViewIndex, @insertPosition);

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* get_ScrollPosition_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            EnhancedUI.EnhancedScroller.EnhancedScroller instance_of_this_method = (EnhancedUI.EnhancedScroller.EnhancedScroller)typeof(EnhancedUI.EnhancedScroller.EnhancedScroller).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method.ScrollPosition;

            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* IgnoreLoopJump_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @ignore = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            EnhancedUI.EnhancedScroller.EnhancedScroller instance_of_this_method = (EnhancedUI.EnhancedScroller.EnhancedScroller)typeof(EnhancedUI.EnhancedScroller.EnhancedScroller).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.IgnoreLoopJump(@ignore);

            return __ret;
        }

        static StackObject* ReloadData_3(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @scrollPositionFactor = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            EnhancedUI.EnhancedScroller.EnhancedScroller instance_of_this_method = (EnhancedUI.EnhancedScroller.EnhancedScroller)typeof(EnhancedUI.EnhancedScroller.EnhancedScroller).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.ReloadData(@scrollPositionFactor);

            return __ret;
        }

        static StackObject* SetScrollPositionImmediately_4(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Single @scrollPosition = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            EnhancedUI.EnhancedScroller.EnhancedScroller instance_of_this_method = (EnhancedUI.EnhancedScroller.EnhancedScroller)typeof(EnhancedUI.EnhancedScroller.EnhancedScroller).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetScrollPositionImmediately(@scrollPosition);

            return __ret;
        }

        static StackObject* ClearAll_5(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            EnhancedUI.EnhancedScroller.EnhancedScroller instance_of_this_method = (EnhancedUI.EnhancedScroller.EnhancedScroller)typeof(EnhancedUI.EnhancedScroller.EnhancedScroller).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.ClearAll();

            return __ret;
        }

        static StackObject* JumpToDataIndex_6(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 10);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @forceCalculateRange = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            EnhancedUI.EnhancedScroller.EnhancedScroller.LoopJumpDirectionEnum @loopJumpDirection = (EnhancedUI.EnhancedScroller.EnhancedScroller.LoopJumpDirectionEnum)typeof(EnhancedUI.EnhancedScroller.EnhancedScroller.LoopJumpDirectionEnum).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)20);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Action @jumpComplete = (System.Action)typeof(System.Action).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Single @tweenTime = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            EnhancedUI.EnhancedScroller.EnhancedScroller.TweenType @tweenType = (EnhancedUI.EnhancedScroller.EnhancedScroller.TweenType)typeof(EnhancedUI.EnhancedScroller.EnhancedScroller.TweenType).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)20);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 6);
            System.Boolean @useSpacing = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 7);
            System.Single @cellOffset = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 8);
            System.Single @scrollerOffset = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 9);
            System.Int32 @dataIndex = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 10);
            EnhancedUI.EnhancedScroller.EnhancedScroller instance_of_this_method = (EnhancedUI.EnhancedScroller.EnhancedScroller)typeof(EnhancedUI.EnhancedScroller.EnhancedScroller).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.JumpToDataIndex(@dataIndex, @scrollerOffset, @cellOffset, @useSpacing, @tweenType, @tweenTime, @jumpComplete, @loopJumpDirection, @forceCalculateRange);

            return __ret;
        }


        static object get_spacing_0(ref object o)
        {
            return ((EnhancedUI.EnhancedScroller.EnhancedScroller)o).spacing;
        }

        static StackObject* CopyToStack_spacing_0(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((EnhancedUI.EnhancedScroller.EnhancedScroller)o).spacing;
            __ret->ObjectType = ObjectTypes.Float;
            *(float*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_spacing_0(ref object o, object v)
        {
            ((EnhancedUI.EnhancedScroller.EnhancedScroller)o).spacing = (System.Single)v;
        }

        static StackObject* AssignFromStack_spacing_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Single @spacing = *(float*)&ptr_of_this_method->Value;
            ((EnhancedUI.EnhancedScroller.EnhancedScroller)o).spacing = @spacing;
            return ptr_of_this_method;
        }

        static object get_scrollDirection_1(ref object o)
        {
            return ((EnhancedUI.EnhancedScroller.EnhancedScroller)o).scrollDirection;
        }

        static StackObject* CopyToStack_scrollDirection_1(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((EnhancedUI.EnhancedScroller.EnhancedScroller)o).scrollDirection;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_scrollDirection_1(ref object o, object v)
        {
            ((EnhancedUI.EnhancedScroller.EnhancedScroller)o).scrollDirection = (EnhancedUI.EnhancedScroller.EnhancedScroller.ScrollDirectionEnum)v;
        }

        static StackObject* AssignFromStack_scrollDirection_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            EnhancedUI.EnhancedScroller.EnhancedScroller.ScrollDirectionEnum @scrollDirection = (EnhancedUI.EnhancedScroller.EnhancedScroller.ScrollDirectionEnum)typeof(EnhancedUI.EnhancedScroller.EnhancedScroller.ScrollDirectionEnum).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)20);
            ((EnhancedUI.EnhancedScroller.EnhancedScroller)o).scrollDirection = @scrollDirection;
            return ptr_of_this_method;
        }



    }
}
