using LccModel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

namespace LccHotfix
{
    public class BaseAttribute : Attribute
    {
    }
    public class Manager : Singleton<Manager>
    {
        private Dictionary<string, Type> _typeDict = new Dictionary<string, Type>();
        private Dictionary<Type, List<Type>> _attDict = new Dictionary<Type, List<Type>>();
        public void InitManager()
        {
            List<Type> list = null;
            _typeDict.Clear();
            _attDict.Clear();
            if (MonoManager.Instance.typeList.Count > 0)
            {
                list = MonoManager.Instance.typeList;
            }
            else if (ILRuntimeManager.Instance.typeList.Count > 0)
            {
                list = ILRuntimeManager.Instance.typeList;
            }

            List<Type> baseAttributeTypeList = GetBaseAttributes(list);
            foreach (Type baseAttributeType in baseAttributeTypeList)
            {
                foreach (Type type in list)
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    object[] objects = type.GetCustomAttributes(baseAttributeType, true);
                    if (objects.Length == 0)
                    {
                        continue;
                    }
                    if (_attDict.TryGetValue(baseAttributeType, out var types))
                    {
                        types.Add(type);
                    }
                    else
                    {
                        types = new List<Type>();
                        types.Add(type);
                        _attDict.Add(baseAttributeType, types);
                    }
                }
            }


            foreach (Type item in list)
            {
                if (!_typeDict.ContainsKey(item.Name))
                {
                    _typeDict.Add(item.Name, item);
                }
            }
        }
        public Type GetTypeByName(string name)
        {
            if (_typeDict.ContainsKey(name))
            {
                return _typeDict[name];
            }
            return null;
        }
        public Type[] GetTypes()
        {
            return _typeDict.Values.ToArray();
        }

        public List<Type> GetTypesByAttribute(Type type)
        {
            if (_attDict.TryGetValue(type, out var list))
            {
                return list;
            }
            else
            {
                return null;
            }
        }


        private List<Type> GetBaseAttributes(List<Type> typeList)
        {
            List<Type> attributeTypeList = new List<Type>();
            foreach (Type type in typeList)
            {
                if (type.IsAbstract)
                {
                    continue;
                }

                if (type.IsSubclassOf(typeof(BaseAttribute)))
                {
                    attributeTypeList.Add(type);
                }
            }
            return attributeTypeList;
        }
    }
}