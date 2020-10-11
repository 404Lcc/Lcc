using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

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
                object value = ((IObjectType)objectTypes[item.FieldType]).Draw(item.FieldType, item.Name.First().ToString().ToUpper() + item.Name.Substring(1), item.GetValue(obj));
                item.SetValue(obj, value);
            }
        }
        EditorGUILayout.EndVertical();
    }
}