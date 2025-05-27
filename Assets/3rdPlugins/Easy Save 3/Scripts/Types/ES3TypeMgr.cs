using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ES3Types;

namespace ES3Internal
{
    [UnityEngine.Scripting.Preserve]
    public static class ES3TypeMgr
    {
        private static object _lock = new object();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static Dictionary<Type, ES3Type> types = null;

        // We cache the last accessed type as we quite often use the same type multiple times,
        // so this improves performance as another lookup is not required.
        private static ES3Type lastAccessedType = null;

        public static ES3Type GetOrCreateES3Type(Type type, bool throwException = true)
        {
            if (types == null)
                Init();

            if (type != typeof(object) && lastAccessedType != null && lastAccessedType.type == type)
                return lastAccessedType;

            // If type doesn't exist, create one.
            if (types.TryGetValue(type, out lastAccessedType))
                return lastAccessedType;
            return (lastAccessedType = CreateES3Type(type, throwException));
        }

        public static ES3Type GetES3Type(Type type)
        {
            if (types == null)
                Init();

            if (types.TryGetValue(type, out lastAccessedType))
                return lastAccessedType;
            return null;
        }

        internal static void Add(Type type, ES3Type es3Type)
        {
            if (types == null)
                Init();

            var existingType = GetES3Type(type);
            if (existingType != null && existingType.priority > es3Type.priority)
                return;

            lock (_lock)
            {
                types[type] = es3Type;
            }
        }

        internal static ES3Type CreateES3Type(Type type, bool throwException = true)
        {
            ES3Type es3Type = null;

            // If we're saving a generic collection, see if it has an explicit implementation.
            // If there isn't fall through to the default way of finding ES3 types.
            if (ES3Reflection.IsGenericType(type) && ES3Reflection.ImplementsInterface(type, typeof(IEnumerable)))
            {
                Type genericType = ES3Reflection.GetGenericTypeDefinition(type);
                if (typeof(List<>).IsAssignableFrom(genericType))
                    es3Type = new ES3ListType(type);
                else if (typeof(Dictionary<,>).IsAssignableFrom(genericType))
                    es3Type = new ES3DictionaryType(type);
                else if (genericType == typeof(Queue<>))
                    es3Type = new ES3QueueType(type);
                else if (genericType == typeof(Stack<>))
                    es3Type = new ES3StackType(type);
                else if (genericType == typeof(HashSet<>))
                    es3Type = new ES3HashSetType(type);
                else if (genericType == typeof(Unity.Collections.NativeArray<>))
                    es3Type = new ES3NativeArrayType(type);

                if (es3Type != null)
                {
                    Add(type, es3Type);
                    return es3Type;
                }
            }

            if (ES3Reflection.IsEnum(type))
                return new ES3Type_enum(type);
            else if (ES3Reflection.TypeIsArray(type))
            {
                int rank = ES3Reflection.GetArrayRank(type);
                if (rank == 1)
                    es3Type = new ES3ArrayType(type);
                else if (rank == 2)
                    es3Type = new ES32DArrayType(type);
                else if (rank == 3)
                    es3Type = new ES33DArrayType(type);
                else if (throwException)
                    throw new NotSupportedException("Only arrays with up to three dimensions are supported by Easy Save.");
                else
                    return null;
            }
            else if (ES3Reflection.IsPrimitive(type)) // ERROR: We should not have to create an ES3Type for a primitive.
            {
                if (types == null || types.Count == 0)  // If the type list is not initialised, it is most likely an initialisation error.
                    throw new TypeLoadException("ES3Type for primitive could not be found, and the type list is empty. Please contact Easy Save developers at https://www.moodkie.com/contact");
                else // Else it's a different error, possibly an error in the specific ES3Type for that type.
                    throw new TypeLoadException("ES3Type for primitive could not be found, but the type list has been initialised and is not empty. Please contact Easy Save developers using the form at https://www.moodkie.com/contact.");
            }
            else
            {
                // See if there's a generic ES3Type. If not, let it fall through to the reflected ES3Types.
                if((es3Type = GetGenericES3Type(type, false)) != null)
                {
                }
                else if (ES3Reflection.IsAssignableFrom(typeof(Component), type))
                    es3Type = new ES3ReflectedComponentType(type);
                else if (ES3Reflection.IsAssignableFrom(typeof(ScriptableObject), type))
                    es3Type = new ES3ReflectedScriptableObjectType(type);
                else if (ES3Reflection.IsAssignableFrom(typeof(UnityEngine.Object), type))
                    es3Type = new ES3ReflectedUnityObjectType(type);
                else if (ES3Reflection.IsValueType(type))
                    es3Type = new ES3ReflectedValueType(type);
                /*else if (ES3Reflection.HasParameterlessConstructor(type) || ES3Reflection.IsAbstract(type) || ES3Reflection.IsInterface(type))
                    es3Type = new ES3ReflectedObjectType(type);*/
                /*else if (throwException)
                    throw new NotSupportedException("Type of " + type + " is not supported as it does not have a parameterless constructor. Only value types, Components or ScriptableObjects are supportable without a parameterless constructor. However, you may be able to create an ES3Type script to add support for it.");*/
                else
                    es3Type = new ES3ReflectedObjectType(type);
            }

            if (es3Type == null || es3Type.type == null || es3Type.isUnsupported)
			{
				if(throwException)
					throw new NotSupportedException($"ES3Type.type is null when trying to create an ES3Type for {type}, possibly because the element type is not supported or there is no generic ES3Type for this class.");
				return null;
			}

            Add(type, es3Type);
			return es3Type;
		}

		static ES3Type GetGenericES3Type(Type type, bool throwException)
		{
            if (!ES3Reflection.IsGenericType(type))
                return null;

            if (type.Name.StartsWith("Tuple`"))
                return new ES3TupleType(type);

            Type genericType = ES3Reflection.GetGenericTypeDefinition(type);

            var es3Type = GetES3Type(genericType);

            if(es3Type != null)
            {
                // Get the constructor which accepts a Type as a parameter.
                var constructor = ES3Reflection.GetConstructor(es3Type.GetType(), new Type[] { typeof(Type) });

                if (constructor == null)
                {
                    if (throwException)
                        throw new NotSupportedException($"Generic type {type} is not supported by Easy Save as it's generic type definition {genericType} does not have a constructor which accepts a Type as a parameter.");
                    return null;
                }
                return (ES3Type)constructor.Invoke(new object[] { type });
            }
            
			if (throwException)
                throw new NotSupportedException("Generic type \"" + type.ToString() + "\" is not supported by Easy Save.");
            
			return null;
        }

        internal static void Init()
        {
            lock (_lock)
            {
                types = new Dictionary<Type, ES3Type>();
                
                var instances = ES3Reflection.GetInstances<ES3Type>(); // ES3Types add themselves to the manager when instantiated to ensure they don't cause cyclic references if they contain a field which is the same type as themselves.

                /*foreach(var instance in instances)
                    ES3TypeMgr.Add(instance.type, instance);*/

                // Check that the type list was initialised correctly.
                if (types == null || types.Count == 0)
                    throw new TypeLoadException("Type list could not be initialised. Please contact Easy Save developers on mail@moodkie.com.");
            }
        }
	}
}
