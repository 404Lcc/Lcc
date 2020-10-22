using System.Linq;
using System.Reflection;
using UnityEditor;

namespace LccEditor
{
    [ObjectType(typeof(string))]
    public class StringObjectType : IObjectType
    {
        public void Draw(object obj, FieldInfo field)
        {
            string name = field.Name.First().ToString().ToUpper() + field.Name.Substring(1);
            object value = field.GetValue(obj);
            field.SetValue(obj, EditorGUILayout.TextField(name, (string)value));
        }
    }
}