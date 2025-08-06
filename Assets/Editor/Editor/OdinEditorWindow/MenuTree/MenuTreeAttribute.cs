using System;

namespace LccEditor
{
    public class MenuTreeAttribute : Attribute
    {
        public string name;
        public int order;
        public Type type;

        public MenuTreeAttribute()
        {
        }

        public MenuTreeAttribute(string name, int order)
        {
            this.name = name;
            this.order = order;
        }
    }
}