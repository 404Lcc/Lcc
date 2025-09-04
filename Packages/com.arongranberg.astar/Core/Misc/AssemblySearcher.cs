using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pathfinding.Util {
	internal static class AssemblySearcher {
		public static List<System.Type> FindTypesInheritingFrom<T>() {
			var result = new List<System.Type>();
			foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
				// Skip some assemblies which are known to not contain any graph types, for performance
				var name = assembly.GetName().Name;
				if (name.StartsWith("Unity.") || name.StartsWith("UnityEngine.") || name == "UnityEngine" || name.StartsWith("UnityEditor.") || name == "UnityEditor" || name.StartsWith("Mono.") || name.StartsWith("System.") || name == "System" || name.StartsWith("mscorlib") || name.StartsWith("I18N") || name == "netstandard" || name == "nunit.framework") continue;

				System.Type[] types = null;
				try {
					types = assembly.GetTypes();
				} catch {
					// Ignore type load exceptions and things like that.
					// We might not be able to read all assemblies for some reason, but hopefully the relevant types exist in the assemblies that we can read
					continue;
				}

				foreach (var type in types) {
#if NETFX_CORE && !UNITY_EDITOR
					System.Type baseType = type.GetTypeInfo().BaseType;
#else
					var baseType = type.BaseType;
#endif
					while (baseType != null) {
						if (System.Type.Equals(baseType, typeof(T))) {
							result.Add(type);
							break;
						}

#if NETFX_CORE && !UNITY_EDITOR
						baseType = baseType.GetTypeInfo().BaseType;
#else
						baseType = baseType.BaseType;
#endif
					}
				}
			}
			return result;
		}
	}
}
