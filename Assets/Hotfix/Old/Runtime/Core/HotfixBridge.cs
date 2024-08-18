//using LccModel;
//using System;
//using System.Reflection;
//using UnityEngine;

//namespace LccHotfix
//{
//    public class HotfixBridge
//    {
//        public static void Init()
//        {
//            HotfixFunc.CrossDomainFunction = CrossDomainCallMethod;
//            HotfixFunc.CrossDomainProperty = CrossDomainCallProperty;
//            HotfixFunc.CrossDomainField = CrossDomainCallField;
//        }
//        public static void Dispose()
//        {
//            HotfixFunc.CrossDomainFunction = null;
//            HotfixFunc.CrossDomainProperty = null;
//            HotfixFunc.CrossDomainField = null;
//        }

//        public static object CrossDomainCallMethod(ClassInfo clsInfo, BindingFlags flag, object[] param, object instance = null)
//        {
//            string nameSpace = clsInfo.nameSpace;
//            string className = clsInfo.className;
//            string funcName = clsInfo.funcName;

//            Type type = ClientTools.GetTypeByName(nameSpace + "." + className);
//            if (type == null)
//            {
//                Debug.LogError($"cant find class {className}");
//                return null;
//            }


//            MethodInfo method = type.GetMethod(funcName, flag);
//            if (method == null)
//            {
//                type = type.BaseType;
//                method = type?.GetMethod(funcName, flag);
//                if (method == null)
//                {
//                    Debug.LogError($"cant find method {className}.{funcName} flag:{flag}");
//                    return null;
//                }
//            }

//            ParameterInfo[] parameter = method.GetParameters();

//            object[] objects = new object[parameter.Length];
//            if (param.Length != parameter.Length)
//            {
//                Debug.LogError($"{className}.{funcName} 参数个数不一致！请确认!");
//            }

//            for (int i = 0; i < parameter.Length; i++)
//            {
//                if (i < param.Length)
//                {
//                    objects[i] = param[i];
//                }
//                else
//                {
//                    objects[i] = Activator.CreateInstance(parameter[i].ParameterType);
//                }
//            }

//            return method.Invoke(instance, objects);
//        }

//        public static object CrossDomainCallProperty(ClassInfo clsInfo, BindingFlags flag, object instance = null)
//        {
//            string nameSpace = clsInfo.nameSpace;
//            string className = clsInfo.className;
//            string propertyName = clsInfo.funcName;

//            Type type = ClientTools.GetTypeByName(nameSpace + "." + className);
//            if (type == null)
//            {
//                Debug.LogError($"cant find class {className}");
//                return null;
//            }

//            PropertyInfo property = type.GetProperty(propertyName, flag);
//            if (property == null)
//            {
//                type = type.BaseType;
//                property = type?.GetProperty(propertyName, flag);

//                if (property == null)
//                {
//                    Debug.LogError($"cant find field {className}.{propertyName} flag:{flag}");
//                    return null;
//                }
//            }

//            return property.GetValue(instance);
//        }

//        public static object CrossDomainCallField(ClassInfo clsInfo, BindingFlags flag, object instance = null)
//        {
//            string nameSpace = clsInfo.nameSpace;
//            string className = clsInfo.className;
//            string fieldName = clsInfo.funcName;

//            Type type = ClientTools.GetTypeByName(nameSpace + "." + className);
//            if (type == null)
//            {
//                Debug.LogError($"cant find class {className}");
//                return null;
//            }

//            FieldInfo field = type.GetField(fieldName, flag);
//            if (field == null)
//            {
//                type = type.BaseType;
//                field = type?.GetField(fieldName, flag);

//                if (field == null)
//                {
//                    Debug.LogError($"cant find field {className}.{fieldName} flag:{flag}");
//                    return null;
//                }
//            }

//            return field.GetValue(instance);
//        }
//    }
//}