using System;

public interface IObjectType
{
    object Draw(Type type, string name, object value);
}