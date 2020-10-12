using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

[ObjectType(typeof(Enum))]
public class EnumObjectType : IObjectType
{
    public void Draw(object obj, FieldInfo field)
    {
        string name = field.Name.First().ToString().ToUpper() + field.Name.Substring(1);
        object value = field.GetValue(obj);
        if (field.FieldType.IsDefined(typeof(FlagsAttribute), false))
        {
            field.SetValue(obj, EditorGUILayout.EnumFlagsField(name, (Enum)value));
        }
        else
        {
            field.SetValue(obj, EditorGUILayout.EnumPopup(name, (Enum)value));
        }
    }
}