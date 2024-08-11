using System;
using UnityEditor;

namespace LccEditor
{
    [ObjectType]
    public class IntObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(int);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            return EditorGUILayout.IntField(memberName, (int)value);
        }
    }
}