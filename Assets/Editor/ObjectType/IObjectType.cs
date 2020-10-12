using System.Reflection;

public interface IObjectType
{
    void Draw(object obj, FieldInfo field);
}