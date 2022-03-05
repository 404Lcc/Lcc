using System;
using System.Collections.Generic;
using System.Linq;

namespace LccHotfix
{
    public class Manager : Singleton<Manager>
    {
        public Dictionary<string, Type> typeDict = new Dictionary<string, Type>();
        public void InitManager()
        {
            if (LccModel.MonoManager.Instance.typeList.Count != 0)
            {
                foreach (Type item in LccModel.MonoManager.Instance.typeList)
                {
                    if (!typeDict.ContainsKey(item.Name))
                    {
                        typeDict.Add(item.Name, item);
                    }
                }
            }
            else
            {
                foreach (Type item in LccModel.ILRuntimeManager.Instance.typeList)
                {
                    if (!typeDict.ContainsKey(item.Name))
                    {
                        typeDict.Add(item.Name, item);
                    }
                }
            }
        }
        public Type GetType(string name)
        {
            return typeDict[name];
        }
        public Type[] GetTypes()
        {
            return typeDict.Values.ToArray();
        }
    }
}