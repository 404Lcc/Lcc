using System;
using System.Collections;

namespace LccModel
{
    public class Manager : Singleton<Manager>
    {
        public Hashtable types = new Hashtable();
        public void InitManager()
        {
            foreach (Type item in GetType().Assembly.GetTypes())
            {
                if (!types.ContainsKey(item.Name))
                {
                    types.Add(item.Name, item);
                }
            }
        }
        public Type GetType(string name)
        {
            return (Type)types[name];
        }
        public Type[] GetTypes()
        {
            return (Type[])types.Values;
        }
    }
}