using System;
using UnityEditor;

namespace LccEditor
{
    [ObjectType]
    public class BoolObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(bool);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.Toggle(memberName, (bool)value);
        }
    }
}