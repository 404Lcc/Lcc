using System.Linq;
using System.Reflection;
using UnityEditor;

namespace LccEditor
{
    [ObjectType(typeof(bool))]
    public class BoolObjectType : IObjectType
    {
        public void Draw(object obj, FieldInfo field)
        {
            string name = field.Name.First().ToString().ToUpper() + field.Name.Substring(1);
            if (name == "<Boxed>k__BackingField") return;
            object value = field.GetValue(obj);
            field.SetValue(obj, EditorGUILayout.Toggle(name, (bool)value));
        }
    }
}