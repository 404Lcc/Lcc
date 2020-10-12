using System.Linq;
using System.Reflection;
using UnityEditor;

[ObjectType(typeof(char))]
public class CharObjectType : IObjectType
{
    public void Draw(object obj, FieldInfo field)
    {
        string name = field.Name.First().ToString().ToUpper() + field.Name.Substring(1);
        object value = field.GetValue(obj);
        string info = EditorGUILayout.TextField(name, ((char)value).ToString());
        field.SetValue(obj, info.Length > 0 ? info[0] : default);
    }
}