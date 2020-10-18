using Model;
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ObjectTypeUtil
{
    public static Hashtable objectTypes = new Hashtable();
    static ObjectTypeUtil()
    {
        foreach (Type item in typeof(ObjectTypeUtil).Assembly.GetTypes())
        {
            ObjectTypeAttribute[] objectTypeAttributes = (ObjectTypeAttribute[])item.GetCustomAttributes(typeof(ObjectTypeAttribute), false);
            if (objectTypeAttributes.Length > 0)
            {
                IObjectType iObjectType = (IObjectType)Activator.CreateInstance(item);
                objectTypes.Add(objectTypeAttributes[0].type, iObjectType);
            }
        }
    }
    public static void Draw(object obj)
    {
        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        EditorGUILayout.BeginVertical();
        foreach (FieldInfo item in fields)
        {
            if (item.FieldType.IsDefined(typeof(HideInInspector), false))
            {
                continue;
            }
            if (item.IsDefined(typeof(HideInInspector), false))
            {
                continue;
            }
            if (objectTypes.ContainsKey(item.FieldType))
            {
                ((IObjectType)objectTypes[item.FieldType]).Draw(obj, item);
                continue;
            }
            if (item.FieldType.IsSubclassOf(typeof(Object)))
            {
                ((IObjectType)objectTypes[typeof(Object)]).Draw(obj, item);
                continue;
            }
            if (item.IsDefined(typeof(ShowObjectInInspectorAttribute), false))
            {
                ((IObjectType)objectTypes[typeof(object)]).Draw(item.GetValue(obj), item);
                continue;
            }
        }
        EditorGUILayout.EndVertical();
    }
}