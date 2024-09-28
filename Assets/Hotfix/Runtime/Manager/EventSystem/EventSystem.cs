using LccModel;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    internal class EventSystem : Module
    {
        public static EventSystem Instance { get; } = Entry.GetModule<EventSystem>();

        private readonly Dictionary<string, Type> allTypeDict = new Dictionary<string, Type>();

        private readonly UnOrderMultiMapSet<Type, Type> attributeTypeDict = new UnOrderMultiMapSet<Type, Type>();

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }

        private List<Type> GetAttributeBase(Dictionary<string, Type> typeDict)
        {
            List<Type> list = new List<Type>();
            foreach (Type type in typeDict.Values)
            {
                if (type.IsAbstract)
                {
                    continue;
                }

                if (type.IsSubclassOf(typeof(AttributeBase)))
                {
                    list.Add(type);
                }
            }

            return list;
        }
        public void InitType(Dictionary<string, Type> dict)
        {
            allTypeDict.Clear();
            attributeTypeDict.Clear();

            foreach ((string fullName, Type type) in dict)
            {
                allTypeDict[fullName] = type;
            }
            foreach (Type attributeBase in GetAttributeBase(allTypeDict))
            {
                foreach (Type type in allTypeDict.Values)
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    object[] objects = type.GetCustomAttributes(attributeBase, false);
                    if (objects.Length == 0)
                    {
                        continue;
                    }
                    foreach (object item in objects)
                    {
                        Type attributeType = item.GetType();
                        attributeTypeDict.Add(attributeType, type);
                    }
                }
            }
        }

        public HashSet<Type> GetTypesByAttribute(Type attributeType)
        {
            if (!attributeTypeDict.ContainsKey(attributeType))
            {
                return new HashSet<Type>();
            }

            return attributeTypeDict[attributeType];
        }

        public Type GetTypeByName(string typeName)
        {
            return allTypeDict[typeName];
        }

        public Dictionary<string, Type> GetTypeDict()
        {
            return allTypeDict;
        }


    }
}