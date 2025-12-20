using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace LccModel
{
    public class ClassInfo
    {
        public string nameSpace;
        public string className;
        public string funcName;
    }

    public static class HotfixFunc
    {
        public static Func<ClassInfo, BindingFlags, object[], object, object> CrossDomainFunction;
        public static Func<ClassInfo, BindingFlags, object, object> CrossDomainProperty;
        public static Func<ClassInfo, BindingFlags, object, object> CrossDomainField;

        static readonly Queue<ClassInfo> ClassInfos = new Queue<ClassInfo>();
        static ClassInfo GetClass(string nameSpace, string className, string fName)
        {
            ClassInfo cls;
            if (ClassInfos.Count > 0)
            {
                cls = ClassInfos.Dequeue();
            }
            else
            {
                cls = new ClassInfo();
            }

            cls.nameSpace = nameSpace;
            cls.className = className;
            cls.funcName = fName;

            return cls;
        }
        static void PutClass(ClassInfo cls)
        {
            ClassInfos.Enqueue(cls);
        }

        public static object CallMethod(string nameSpace, string className, string funcName, BindingFlags flag, object[] param, object instance = null)
        {
            ClassInfo info = GetClass(nameSpace, className, funcName);
            if (CrossDomainFunction == null) return null;
            var ret = CrossDomainFunction(info, flag, param, instance);

            PutClass(info);
            return ret;
        }

        public static object CallPublicStaticMethod(string nameSpace, string className, string funcName, params object[] param)
        {
            ClassInfo info = GetClass(nameSpace, className, funcName);
            if (CrossDomainFunction == null) return null;
            var ret = CrossDomainFunction(info, BindingFlags.Public | BindingFlags.Static, param, null);

            PutClass(info);
            return ret;
        }

        public static object CallSingletonMethod(string nameSpace, string className, string instanceName, string funcName, params object[] param)
        {
            object singleton = CallProperty(nameSpace, className, instanceName, BindingFlags.Public | BindingFlags.Static, null);
            return CallMethod(nameSpace, className, funcName, BindingFlags.Public | BindingFlags.Instance, param, singleton);
        }

        public static object CallProperty(string nameSpace, string className, string propertyName, BindingFlags flag, object instance = null)
        {
            ClassInfo info = GetClass(nameSpace, className, propertyName);
            if (CrossDomainProperty == null) return null;
            var ret = CrossDomainProperty(info, flag, instance);

            PutClass(info);
            return ret;
        }

        public static object CallSingletonProperty(string nameSpace, string className, string instanceName, string propertyName, BindingFlags flag = BindingFlags.Public | BindingFlags.Instance)
        {
            object singleton = CallProperty(nameSpace, className, instanceName, BindingFlags.Public | BindingFlags.Static, null);
            return CallProperty(nameSpace, className, propertyName, flag, singleton);
        }

        public static object CallField(string nameSpace, string className, string fieldName, BindingFlags flag, object instance = null)
        {
            ClassInfo info = GetClass(nameSpace, className, fieldName);
            if (CrossDomainField == null) return null;
            var ret = CrossDomainField(info, flag, instance);

            PutClass(info);
            return ret;
        }

        public static object CallSingletonField(string nameSpace, string className, string instanceName, string fieldName, BindingFlags flag = BindingFlags.Public | BindingFlags.Instance)
        {
            object singleton = CallProperty(nameSpace, className, instanceName, BindingFlags.Public | BindingFlags.Static, null);
            return CallField(nameSpace, className, fieldName, flag, singleton);
        }
    }
}