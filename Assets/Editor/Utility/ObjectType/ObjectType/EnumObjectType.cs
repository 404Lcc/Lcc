using System;
using UnityEditor;

namespace LccEditor
{
    [ObjectType]
    public class EnumObjectType : IObjectType
    {
        public bool IsType(Type type)
        {
            return type == typeof(Enum);
        }
        public object Draw(Type memberType, string memberName, object value, object target)
        {
            if (memberType.IsDefined(typeof(FlagsAttribute), false))
            {
                return EditorGUILayout.EnumFlagsField(memberName, (Enum)value);
            }
            else
            {
                return EditorGUILayout.EnumPopup(memberName, (Enum)value);
            }
        }
    }
}