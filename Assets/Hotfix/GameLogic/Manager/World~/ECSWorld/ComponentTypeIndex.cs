using System;

namespace LccHotfix
{
    public class ComponentTypeIndex
    {
        public Type componentType;
        public int index;

        public ComponentTypeIndex(Type type)
        {
            componentType = type;
            index = -1;
        }
    }
}