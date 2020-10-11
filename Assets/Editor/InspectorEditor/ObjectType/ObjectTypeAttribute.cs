using System;

public class ObjectTypeAttribute : Attribute
{
    public Type type;
    public ObjectTypeAttribute(Type type)
    {
        this.type = type;
    }
}