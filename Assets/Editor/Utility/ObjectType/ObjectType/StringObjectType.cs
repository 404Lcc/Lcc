using System;
using UnityEditor;

namespace LccEditor
{
    [ObjectType]
    public class StringObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(string);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.TextField(memberName, (string)value);
        }
    }
}