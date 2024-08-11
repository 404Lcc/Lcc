using System;
using UnityEditor;

namespace LccEditor
{
    public class DateTimeObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(DateTime);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            string oldData = value.ToString();
            string newData = EditorGUILayout.TextField(memberName, oldData);
            return newData != oldData ? DateTime.Parse(newData) : value;
        }
    }
}