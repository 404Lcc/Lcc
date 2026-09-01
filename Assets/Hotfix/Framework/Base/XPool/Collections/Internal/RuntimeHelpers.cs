using System;

namespace MackySoft.XPool.Collections.Internal {
	internal static class RuntimeHelpers {

		public static bool IsWellKnownNoReferenceContainsType<T> () {
			// BCL结果缓存 + 外部Register扩展
			return WellKnownNoReferenceContainsType<T>.IsWellKnownType ||
			       NoReferenceTypeRegistry.IsRegistered<T>();
		}

		static bool WellKnownNoReferenceContainsTypeInitialize (Type t) {
			if (t.IsPrimitive) {
				return true;
			}
			if (t.IsEnum) { return true; }
			if (t == typeof(DateTime)) { return true; }
			if (t == typeof(DateTimeOffset)) { return true; }
			if (t == typeof(Guid)) { return true; }
			if (t == typeof(decimal)) { return true; }

			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) {
				return WellKnownNoReferenceContainsTypeInitialize(t.GetGenericArguments()[0]);
			}

			return false;
		}

		static class WellKnownNoReferenceContainsType<T> {

			public static readonly bool IsWellKnownType;

			static WellKnownNoReferenceContainsType () {
				IsWellKnownType = WellKnownNoReferenceContainsTypeInitialize(typeof(T));
			}
		}
	}
}