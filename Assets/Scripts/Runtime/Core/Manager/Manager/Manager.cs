using System;
using System.Collections.Generic;
using System.Linq;

namespace LccModel
{
    public class Manager : Singleton<Manager>
    {
        public Dictionary<string, Type> typeDict = new Dictionary<string, Type>();
        public void InitManager()
        {
            foreach (Type item in GetType().Assembly.GetTypes())
            {
                if (!typeDict.ContainsKey(item.Name))
                {
                    typeDict.Add(item.Name, item);
                }
            }
        }
        public Type GetType(string name)
        {
            if (typeDict.ContainsKey(name))
            {
                return typeDict[name];
            }
            return null;
        }
        public Type[] GetTypes()
        {
            return typeDict.Values.ToArray();
        }
    }
}