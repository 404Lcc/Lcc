using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using System;
using System.Collections.Generic;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace LccModel
{
    public static partial class ILRuntimeHelper
    {
        public unsafe static void RegisterCLRMethodRedirction(AppDomain appdomain)
        {
            Type logUtilType = typeof(LogUtil);
            var logMethod = logUtilType.GetMethod("Log", new[] { typeof(object) });
            appdomain.RegisterCLRMethodRedirection(logMethod, Log);
            var logWarningMethod = logUtilType.GetMethod("LogWarning", new[] { typeof(object) });
            appdomain.RegisterCLRMethodRedirection(logWarningMethod, LogWarning);
            var logErrorMethod = logUtilType.GetMethod("LogError", new[] { typeof(object) });
            appdomain.RegisterCLRMethodRedirection(logErrorMethod, LogError);
        }
        public unsafe static StackObject* Log(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            //ILRuntime�ĵ���Լ��Ϊ�������������ջ�����ִ�������������Ҫ�������Ӷ�ջ����ɾ������ѷ���ֵ����ջ���������뿴ILRuntimeʵ��ԭ���ĵ�
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            //�������󷽷����غ�espջָ���ֵ��Ӧ�÷��������������ָ�򷵻�ֵ��������ֻ��Ҫ���������������ֵ����
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            //ȡLog�����Ĳ�������������������Ļ�����һ��������esp - 2,�ڶ���������esp -1, ��ΪMono��bug��ֱ��-2ֵ���������Ҫ����ILIntepreter.Minus
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);

            //�����ǽ�ջָ���ϵ�ֵת����object������ǻ������Ϳ�ֱ��ͨ��ptr->Value��ptr->ValueLow���ʵ�ֵ�������뿴ILRuntimeʵ��ԭ���ĵ�
            object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            //���зǻ������Ͷ��õ���Free���ͷ��йܶ�ջ
            __intp.Free(ptr_of_this_method);

            //����ʵ����Debug.Logǰ�������Ȼ�ȡDLL�ڵĶ�ջ
            var stacktrace = __domain.DebugService.GetStackTrace(__intp);

            //�����������Ϣ�������DLL��ջ
            LogUtil.Log(message + "\n" + stacktrace);

            return __ret;
        }
        public unsafe static StackObject* LogWarning(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            var stacktrace = __domain.DebugService.GetStackTrace(__intp);
            LogUtil.LogWarning(message + "\n" + stacktrace);
            return __ret;
        }
        public unsafe static StackObject* LogError(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            object message = typeof(object).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);
            var stacktrace = __domain.DebugService.GetStackTrace(__intp);
            LogUtil.LogError(message + "\n" + stacktrace);
            return __ret;
        }
    }
}