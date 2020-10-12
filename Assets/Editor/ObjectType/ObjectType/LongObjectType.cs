using System.Linq;
using System.Reflection;
using UnityEditor;

[ObjectType(typeof(long))]
public class LongObjectType : IObjectType
{
    public void Draw(object obj, FieldInfo field)
    {
        string name = field.Name.First().ToString().ToUpper() + field.Name.Substring(1);
        object value = field.GetValue(obj);
        field.SetValue(obj, EditorGUILayout.LongField(name, (long)value));
    }
}