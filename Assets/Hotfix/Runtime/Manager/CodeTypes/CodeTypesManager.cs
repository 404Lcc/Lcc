using System;
using System.Collections.Generic;
using System.Reflection;

namespace LccHotfix
{
    internal class CodeTypesManager : Module
    {
        public static CodeTypesManager Instance { get; } = Entry.GetModule<CodeTypesManager>();

        private readonly Dictionary<string, Type> allTypes = new Dictionary<string, Type>();
        private readonly UnOrderMultiMapSet<Type, Type> types = new UnOrderMultiMapSet<Type, Type>();

        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        internal override void Shutdown()
        {
        }



        public void LoadTypes(Assembly[] assemblies)
        {
            Dictionary<string, Type> addTypes = GetAssemblyTypes(assemblies);
            foreach ((string fullName, Type type) in addTypes)
            {
                this.allTypes[fullName] = type;

                if (type.IsAbstract)
                {
                    continue;
                }

                // 记录所有的有BaseAttribute标记的的类型
                object[] objects = type.GetCustomAttributes(typeof(AttributeBase), true);

                foreach (object o in objects)
                {
                    this.types.Add(o.GetType(), type);
                }
            }
        }
        public Dictionary<string, Type> GetAssemblyTypes(params Assembly[] args)
        {
            Dictionary<string, Type> types = new Dictionary<string, Type>();

            foreach (Assembly ass in args)
            {
                var ts = ass.GetTypes();
                foreach (Type type in ts)
                {
                    types[type.FullName] = type;
                }
            }

            return types;
        }

        public HashSet<Type> GetTypes(Type systemAttributeType)
        {
            if (!this.types.ContainsKey(systemAttributeType))
            {
                return new HashSet<Type>();
            }

            return this.types[systemAttributeType];
        }

        public Dictionary<string, Type> GetTypes()
        {
            return allTypes;
        }

        public Type GetType(string typeName)
        {
            Type type = null;
            this.allTypes.TryGetValue(typeName, out type);
            return type;
        }


    }
}