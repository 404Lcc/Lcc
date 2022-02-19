using System;
using System.Collections.Generic;

namespace ProtoBuf
{
    public class PType
    {
        static PType m_Current;
        static PType Current
        {
            get
            {
                if (m_Current == null)
                {
                    m_Current = new PType();
                }
                return m_Current;
            }
        }
        Dictionary<string, Type> m_Types = new Dictionary<string, Type>();

        private PType() { }

        void RegisterTypeInternal(string metaName, Type type)
        {
            m_Types[metaName] = type;
        }

        Type FindTypeInternal(string metaName)
        {
            if (!m_Types.TryGetValue(metaName, out var type))
            {
                throw new SystemException(string.Format("PropertyMeta : {0} is not registered!", metaName));
            }
            return type;
        }

        public static void RegisterType(string metaName, Type type)
        {
            Current.RegisterTypeInternal(metaName, type);
        }

        public delegate object DelegateFunctionCreateInstance(string typeName);
        static DelegateFunctionCreateInstance CreateInstanceFunc;
        public static void RegisterFunctionCreateInstance(DelegateFunctionCreateInstance func)
        {
            CreateInstanceFunc = func;
        }
        public delegate Type DelegateFunctionGetRealType(object o);
        static DelegateFunctionGetRealType GetRealTypeFunc;
        public static void RegisterFunctionGetRealType(DelegateFunctionGetRealType func)
        {
            GetRealTypeFunc = func;
        }

        public static Type FindType(string metaName)
        {
            return Current.FindTypeInternal(metaName);
        }

        public static object CreateInstance(Type type)
        {
            if (Type.GetType(type.FullName) == null)
            {
                if (CreateInstanceFunc != null)
                {
                    return CreateInstanceFunc.Invoke(type.FullName);
                }
            }
            return Activator.CreateInstance(type
#if !(CF || SILVERLIGHT || WINRT || PORTABLE || NETSTANDARD1_3 || NETSTANDARD1_4)
                , nonPublic: true
#endif
            );
        }
        public static Type GetPType(object o)
        {
            if (GetRealTypeFunc != null)
            {
                return GetRealTypeFunc.Invoke(o);
            }
            return o.GetType();
        }
        public static void RegisterILRuntimeCLRRedirection(ILRuntime.Runtime.Enviorment.AppDomain appDomain)
        {
            foreach (ILRuntime.CLR.TypeSystem.IType item in appDomain.LoadedTypes.Values)
            {
                RegisterType(item.FullName, item.ReflectionType);
            }
            RegisterFunctionCreateInstance(typeName => appDomain.Instantiate(typeName));
            RegisterFunctionGetRealType((obj) =>
            {
                Type type = obj.GetType();
                if (type.FullName == "ILRuntime.Runtime.Intepreter.ILTypeInstance")
                {
                    ILRuntime.Runtime.Intepreter.ILTypeInstance instance = (ILRuntime.Runtime.Intepreter.ILTypeInstance)obj;
                    type = FindType(instance.Type.FullName);
                }
                return type;
            });
        }
    }
}
