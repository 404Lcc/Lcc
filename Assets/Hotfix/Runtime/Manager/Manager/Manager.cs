using System;
using System.Collections.Generic;

namespace LccHotfix
{
    internal class Manager : Module
    {
        public static Manager Instance { get; } = Entry.GetModule<Manager>();


        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }

        public HashSet<Type> GetTypesByAttribute(Type attributeType)
        {
            return EventSystem.Instance.GetTypesByAttribute(attributeType);
        }
        public Type GetTypeByName(string typeName)
        {
            return EventSystem.Instance.GetTypeByName(typeName);
        }
        public Dictionary<string, Type> GetTypeDict()
        {
            return EventSystem.Instance.GetTypeDict();
        }


    }
}