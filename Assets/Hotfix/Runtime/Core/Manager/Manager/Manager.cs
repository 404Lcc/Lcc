using System;
using System.Collections;

namespace LccHotfix
{
    public class Manager : Singleton<Manager>
    {
        public Hashtable types = new Hashtable();
        public void InitManager()
        {
            if (LccModel.MonoManager.Instance.typeList.Count != 0)
            {
                foreach (Type item in LccModel.MonoManager.Instance.typeList)
                {
                    if (!types.ContainsKey(item.Name))
                    {
                        types.Add(item.Name, item);
                    }
                }
            }
            else
            {
                foreach (Type item in LccModel.ILRuntimeManager.Instance.typeList)
                {
                    if (!types.ContainsKey(item.Name))
                    {
                        types.Add(item.Name, item);
                    }
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