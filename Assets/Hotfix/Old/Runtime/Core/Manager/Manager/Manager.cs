using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class Manager : AObjectBase
    {
        public static Manager Instance { get; set; }
        public override void Awake()
        {
            base.Awake();


            Instance = this;

        }
        public override void OnDestroy()
        {
            base.OnDestroy();

            Instance = null;
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