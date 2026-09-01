using System;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace MackySoft.XPool.Unity {
	public static class DefaultFunctions {

		public static Action<T> Destroy<T> () where T : UnityObject {
			return CachedUnityObjectFunctions<T>.Destroy;
		}

		public static Action<T> DestroyGameObjectWithComponent<T> () where T : Component {
			return CachedComponentFunctions<T>.DestroyGameObjectWithComponent;
		}

		static class CachedUnityObjectFunctions<T> where T : UnityObject {

			public static readonly Action<T> Destroy = instance => {
#if UNITY_EDITOR
				if (Application.isPlaying) {
					UnityObject.Destroy(instance);
				}
				else {
					UnityObject.DestroyImmediate(instance);
				}
#else
				UnityObject.Destroy(instance);
#endif
			};
		}

		static class CachedComponentFunctions<T> where T : Component {

			public static readonly Action<T> DestroyGameObjectWithComponent = instance => {
#if UNITY_EDITOR
				if (Application.isPlaying) {
					UnityObject.Destroy(instance.gameObject);
				}
				else {
					UnityObject.DestroyImmediate(instance.gameObject);
				}
#else
				UnityObject.Destroy(instance.gameObject);
#endif
			};
		}

	}
}