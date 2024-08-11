using System;
using UnityEditor;

namespace LccEditor
{
    [ObjectType]
    public class DoubleObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(double);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.DoubleField(memberName, (double)value);
        }
    }
}