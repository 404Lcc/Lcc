using System;

namespace LccEditor
{
    public interface IObjectType
    {
        bool IsType(Type type);
        object Draw(Type memberType, string memberName, object value, object target);
    }
}