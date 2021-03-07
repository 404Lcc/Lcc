using System.Reflection;

namespace LccEditor
{
    public interface IObjectType
    {
        void Draw(object obj, FieldInfo field);
    }
}