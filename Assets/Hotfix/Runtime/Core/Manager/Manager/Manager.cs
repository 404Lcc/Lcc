using LccModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LccHotfix
{
    public class Manager : AObjectBase
    {
        public static Manager Instance { get; set; }
        private Dictionary<string, Type> _typeDict = new Dictionary<string, Type>();
        private Dictionary<Type, List<Type>> _attDict = new Dictionary<Type, List<Type>>();

        public override void Awake()
        {
            base.Awake();


            Instance = this;

            List<Type> list = Loader.Instance.GetHotfixTypeALL();
            _typeDict.Clear();
            _attDict.Clear();

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
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;
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
                return new List<Type>();
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

                if (type.IsSubclassOf(typeof(AttributeBase)))
                {
                    attributeTypeList.Add(type);
                }
            }
            return attributeTypeList;
        }
    }
}