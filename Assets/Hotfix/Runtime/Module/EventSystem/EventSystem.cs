using LccModel;
using System;
using System.Collections.Generic;

namespace LccHotfix
{
    public class EventSystem : Singleton<EventSystem>, ISingletonFixedUpdate, ISingletonUpdate, ISingletonLateUpdate
    {
        private readonly Dictionary<string, Type> allTypeDict = new Dictionary<string, Type>();

        private readonly UnOrderMultiMapSet<Type, Type> attributeTypeDict = new UnOrderMultiMapSet<Type, Type>();

        public Dictionary<long, AObjectBase> aObjectBaseDict = new Dictionary<long, AObjectBase>();
        public Queue<long> fixedUpdate = new Queue<long>();
        public Queue<long> fixedUpdateCopy = new Queue<long>();
        public Queue<long> update = new Queue<long>();
        public Queue<long> updateCopy = new Queue<long>();
        public Queue<long> lateUpdate = new Queue<long>();
        public Queue<long> lateUpdateCopy = new Queue<long>();

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

        public void Register(AObjectBase aObjectBase)
        {
            aObjectBaseDict.Add(aObjectBase.InstanceId, aObjectBase);
            if (aObjectBase is IFixedUpdate)
            {
                fixedUpdate.Enqueue(aObjectBase.InstanceId);
            }
            if (aObjectBase is IUpdate)
            {
                update.Enqueue(aObjectBase.InstanceId);
            }
            if (aObjectBase is ILateUpdate)
            {
                lateUpdate.Enqueue(aObjectBase.InstanceId);
            }
        }
        public void Remove(AObjectBase aObjectBase)
        {
            if (aObjectBaseDict.ContainsKey(aObjectBase.InstanceId))
            {
                aObjectBaseDict.Remove(aObjectBase.InstanceId);
            }
        }
        
        public void FixedUpdate()
        {
            while (fixedUpdate.Count > 0)
            {
                long id = fixedUpdate.Dequeue();
                if (aObjectBaseDict.ContainsKey(id))
                {
                    AObjectBase aObjectBase = aObjectBaseDict[id];
                    if (aObjectBase is IFixedUpdate fixedUpdate)
                    {
                        fixedUpdate.FixedUpdate();
                    }
                    fixedUpdateCopy.Enqueue(id);
                }
            }
            Swap(ref fixedUpdate, ref fixedUpdateCopy);
        }
        public void Update()
        {
            while (update.Count > 0)
            {
                long id = update.Dequeue();
                if (aObjectBaseDict.ContainsKey(id))
                {
                    AObjectBase aObjectBase = aObjectBaseDict[id];
                    if (aObjectBase is IUpdate update)
                    {
                        update.Update();
                    }
                    updateCopy.Enqueue(id);
                }
            }
            Swap(ref update, ref updateCopy);
        }
        public void LateUpdate()
        {
            while (lateUpdate.Count > 0)
            {
                long id = lateUpdate.Dequeue();
                if (aObjectBaseDict.ContainsKey(id))
                {
                    AObjectBase aObjectBase = aObjectBaseDict[id];
                    if (aObjectBase is ILateUpdate lateUpdate)
                    {
                        lateUpdate.LateUpdate();
                    }
                    lateUpdateCopy.Enqueue(id);
                }
            }
            Swap(ref lateUpdate, ref lateUpdateCopy);
        }

        public static void Swap<T>(ref T t1, ref T t2)
        {
            T t3 = t1;
            t1 = t2;
            t2 = t3;
        }
    }
}