using System;
using UnityEditor;

namespace LccEditor
{
    [ObjectType]
    public class CharObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(char);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            string info = EditorGUILayout.TextField(memberName, ((char)value).ToString());
            return info.Length > 0 ? info[0] : default;
        }
    }
}