using ILRuntime.Reflection;
using LccModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LccEditor
{
    public static class ObjectTypeUtil
    {
        public static List<IObjectType> objectList = new List<IObjectType>();
        public static Dictionary<(object, FieldInfo), ObjectObjectType> objectObjectTypeDict = new Dictionary<(object, FieldInfo), ObjectObjectType>();
        public static Dictionary<(object, FieldInfo), ListObjectType> listObjectTypeDict = new Dictionary<(object, FieldInfo), ListObjectType>();
        static ObjectTypeUtil()
        {
            foreach (Type item in typeof(ObjectTypeUtil).Assembly.GetTypes())
            {
                ObjectTypeAttribute[] objectTypeAttributes = (ObjectTypeAttribute[])item.GetCustomAttributes(typeof(ObjectTypeAttribute), false);
                if (objectTypeAttributes.Length > 0)
                {
                    IObjectType iObjectType = (IObjectType)Activator.CreateInstance(item);
                    objectList.Add(iObjectType);
                }
            }
        }
        public static void Draw(object obj, int indentLevel)
        {
            EditorGUILayout.BeginVertical();
            EditorGUI.indentLevel = indentLevel;
            string assemblyName = string.Empty;
            switch (Path.GetFileNameWithoutExtension(obj.GetType().Assembly.ManifestModule.Name))
            {
                case "Unity.Model":
                    assemblyName = "Unity.Model";
                    break;
                case "Unity.Hotfix":
                    assemblyName = "Unity.Hotfix";
                    break;
                case "ILRuntime":
                    assemblyName = "Unity.Hotfix";
                    break;
            }
            if (assemblyName == "Unity.Model")
            {
                FieldInfo[] fieldInfos = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                foreach (FieldInfo item in fieldInfos)
                {
                    object value = item.GetValue(obj);
                    Type type = item.FieldType;
                    if (item.IsDefined(typeof(HideInInspector), false))
                    {
                        continue;
                    }
                    if (type.IsDefined(typeof(HideInInspector), false))
                    {
                        continue;
                    }
                    if (objectObjectTypeDict.ContainsKey((obj, item)))
                    {
                        ObjectObjectType objectObjectType = objectObjectTypeDict[(obj, item)];
                        objectObjectType.Draw(type, item.Name, value, null, indentLevel);
                        continue;
                    }
                    if ((item.IsDefined(typeof(SerializeField), false) || type.IsDefined(typeof(SerializeField), false)) && type.Assembly.ManifestModule.Name == "Unity.Model.dll")
                    {
                        ObjectObjectType objectObjectType = new ObjectObjectType();
                        if (value == null)
                        {
                            object instance = Activator.CreateInstance(type);
                            objectObjectType.Draw(type, item.Name, instance, null, indentLevel);
                            item.SetValue(obj, instance);
                        }
                        else
                        {
                            objectObjectType.Draw(type, item.Name, value, null, indentLevel);
                        }
                        objectObjectTypeDict.Add((obj, item), objectObjectType);
                        continue;
                    }
                    if (listObjectTypeDict.ContainsKey((obj, item)))
                    {
                        ListObjectType listObjectType = listObjectTypeDict[(obj, item)];
                        listObjectType.Draw(type, item.Name, value, null, indentLevel);
                        continue;
                    }
                    if (type.GetInterface("IList") != null)
                    {
                        ListObjectType listObjectType = new ListObjectType();
                        if (value == null)
                        {
                            continue;
                        }
                        listObjectType.Draw(type, item.Name, value, null, indentLevel);
                        listObjectTypeDict.Add((obj, item), listObjectType);
                        continue;
                    }
                    foreach (IObjectType objectTypeItem in objectList)
                    {
                        if (!objectTypeItem.IsType(type))
                        {
                            continue;
                        }
                        string fieldName = item.Name;
                        if (fieldName.Contains("clrInstance") || fieldName.Contains("Boxed"))
                        {
                            continue;
                        }
                        if (fieldName.Length > 17 && fieldName.Contains("k__BackingField"))
                        {
                            fieldName = fieldName.Substring(1, fieldName.Length - 17);
                        }
                        value = objectTypeItem.Draw(type, fieldName, value, null);
                        item.SetValue(obj, value);
                    }
                }
            }
            else
            {
                if (EditorDefine.HotfixMode == HotfixMode.ILRuntime)
                {
                    FieldInfo[] fieldInfos = Loader.Instance.appDomain.LoadedTypes[obj.ToString()].ReflectionType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                    foreach (FieldInfo item in fieldInfos)
                    {
                        object value = item.GetValue(obj);
                        if (item.FieldType is ILRuntimeWrapperType)
                        {
                            //基础类型绘制
                            Type type = ((ILRuntimeWrapperType)item.FieldType).RealType;
                            if (item.IsDefined(typeof(HideInInspector), false))
                            {
                                continue;
                            }
                            if (type.IsDefined(typeof(HideInInspector), false))
                            {
                                continue;
                            }
                            if (listObjectTypeDict.ContainsKey((obj, item)))
                            {
                                ListObjectType listObjectType = (ListObjectType)listObjectTypeDict[(obj, item)];
                                listObjectType.Draw(type, item.Name, value, null, indentLevel);
                                continue;
                            }
                            if (type.GetInterface("IList") != null)
                            {
                                ListObjectType listObjectType = new ListObjectType();
                                if (value == null)
                                {
                                    continue;
                                }
                                listObjectType.Draw(type, item.Name, value, null, indentLevel);
                                listObjectTypeDict.Add((obj, item), listObjectType);
                                continue;
                            }
                            foreach (IObjectType objectTypeItem in objectList)
                            {
                                if (!objectTypeItem.IsType(type))
                                {
                                    continue;
                                }
                                string fieldName = item.Name;
                                if (fieldName.Contains("clrInstance") || fieldName.Contains("Boxed"))
                                {
                                    continue;
                                }
                                if (fieldName.Length > 17 && fieldName.Contains("k__BackingField"))
                                {
                                    fieldName = fieldName.Substring(1, fieldName.Length - 17);
                                }
                                value = objectTypeItem.Draw(type, fieldName, value, null);
                                item.SetValue(obj, value);
                            }
                        }
                        else
                        {
                            //自定义类型绘制
                            Type type = item.FieldType;
                            if (item.IsDefined(typeof(HideInInspector), false))
                            {
                                continue;
                            }
                            if (type.IsDefined(typeof(HideInInspector), false))
                            {
                                continue;
                            }
                            if (objectObjectTypeDict.ContainsKey((obj, item)))
                            {
                                ObjectObjectType objectObjectType = objectObjectTypeDict[(obj, item)];
                                objectObjectType.Draw(type, item.Name, value, null, indentLevel);
                                continue;
                            }
                            if ((item.IsDefined(typeof(SerializeField), false) || type.IsDefined(typeof(SerializeField), false)) && type.Assembly.ManifestModule.Name == "ILRuntime.dll")
                            {
                                ObjectObjectType objectObjectType = new ObjectObjectType();
                                if (value == null)
                                {
                                    object instance = Loader.Instance.appDomain.Instantiate(type.ToString());
                                    objectObjectType.Draw(type, item.Name, instance, null, indentLevel);
                                    item.SetValue(obj, instance);
                                }
                                else
                                {
                                    objectObjectType.Draw(type, item.Name, value, null, indentLevel);
                                }
                                objectObjectTypeDict.Add((obj, item), objectObjectType);
                                continue;
                            }
                        }
                    }
                }
                else
                {
                    FieldInfo[] fieldInfos = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                    foreach (FieldInfo item in fieldInfos)
                    {
                        object value = item.GetValue(obj);
                        Type type = item.FieldType;
                        if (item.IsDefined(typeof(HideInInspector), false))
                        {
                            continue;
                        }
                        if (type.IsDefined(typeof(HideInInspector), false))
                        {
                            continue;
                        }
                        if (objectObjectTypeDict.ContainsKey((obj, item)))
                        {
                            ObjectObjectType objectObjectType = objectObjectTypeDict[(obj, item)];
                            objectObjectType.Draw(type, item.Name, value, null, indentLevel);
                            continue;
                        }
                        if ((item.IsDefined(typeof(SerializeField), false) || type.IsDefined(typeof(SerializeField), false)) && type.Assembly.ManifestModule.Name == "Unity.Hotfix.dll")
                        {
                            ObjectObjectType objectObjectType = new ObjectObjectType();
                            if (value == null)
                            {
                                object instance = Activator.CreateInstance(type);
                                objectObjectType.Draw(type, item.Name, instance, null, indentLevel);
                                item.SetValue(obj, instance);
                            }
                            else
                            {
                                objectObjectType.Draw(type, item.Name, value, null, indentLevel);
                            }
                            objectObjectTypeDict.Add((obj, item), objectObjectType);
                            continue;
                        }
                        if (listObjectTypeDict.ContainsKey((obj, item)))
                        {
                            ListObjectType listObjectType = listObjectTypeDict[(obj, item)];
                            listObjectType.Draw(type, item.Name, value, null, indentLevel);
                            continue;
                        }
                        if (type.GetInterface("IList") != null)
                        {
                            ListObjectType listObjectType = new ListObjectType();
                            if (value == null)
                            {
                                continue;
                            }
                            listObjectType.Draw(type, item.Name, value, null, indentLevel);
                            listObjectTypeDict.Add((obj, item), listObjectType);
                            continue;
                        }
                        foreach (IObjectType objectTypeItem in objectList)
                        {
                            if (!objectTypeItem.IsType(type))
                            {
                                continue;
                            }
                            string fieldName = item.Name;
                            if (fieldName.Contains("clrInstance") || fieldName.Contains("Boxed"))
                            {
                                continue;
                            }
                            if (fieldName.Length > 17 && fieldName.Contains("k__BackingField"))
                            {
                                fieldName = fieldName.Substring(1, fieldName.Length - 17);
                            }
                            value = objectTypeItem.Draw(type, fieldName, value, null);
                            item.SetValue(obj, value);
                        }
                    }
                }
            }
            EditorGUI.indentLevel = indentLevel;
            EditorGUILayout.EndVertical();
        }
    }
}