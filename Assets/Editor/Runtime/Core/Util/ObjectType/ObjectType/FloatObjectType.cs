using System;
using UnityEditor;

namespace LccEditor
{
    [ObjectType]
    public class FloatObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(float);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.FloatField(memberName, (float)value);
        }
    }
}