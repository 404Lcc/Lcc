using System;
using UnityEditor;

[ObjectType(typeof(Enum))]
public class EnumObjectType : IObjectType
{
    public object Draw(Type type, string name, object value)
    {
        if (type.IsDefined(typeof(FlagsAttribute), false))
        {
            return EditorGUILayout.EnumFlagsField(name, (Enum)value);
        }
        else
        {
            return EditorGUILayout.EnumPopup(name, (Enum)value);
        }
    }
}