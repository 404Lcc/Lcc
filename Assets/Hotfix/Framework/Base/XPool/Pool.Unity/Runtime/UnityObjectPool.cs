using System;
using System.Collections.Generic;
using MackySoft.XPool.Internal;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace MackySoft.XPool.Unity {

	/// <summary>
	/// A pool for object that inherit from <see cref="UnityObject"/>.
	/// </summary>
	[Serializable]
	public class UnityObjectPool<T> : IUnityObjectPool<T> where T : UnityObject {

		[Tooltip("The original object from which the pool will instantiate a new instance.")]
		[SerializeField]
		protected T m_Original;

		[Tooltip("Capacity to store instances in the pool.")]
		[SerializeField]
		int m_Capacity;

		readonly Queue<T> m_Pool = new Queue<T>();

		protected Action<T> m_OnCreate;
		protected Action<T> m_OnRent;
		protected Action<T> m_OnReturn;
		protected Action<T> m_OnRelease;

		/// <summary>
		/// The original object from which the pool will instantiate a new instance.
		/// </summary>
		public T Original => m_Original;

		public int Capacity => m_Capacity;

		public int Count => m_Pool.Count;

		/// <summary>
		/// Called when called <see cref="Rent"/> if pool is empty and new instance is instantiated by the pool.
		/// </summary>
		public Action<T> OnCreate { set => SetCallback(ref m_OnCreate,value); }

		/// <summary>
		/// Called when rent an instance from the pool.
		/// </summary>
		public Action<T> OnRent { set => SetCallback(ref m_OnRent,value); }

		/// <summary>
		/// Called when return an instance to the pool.
		/// </summary>
		public Action<T> OnReturn { set => SetCallback(ref m_OnReturn,value); }

		/// <summary>
		/// Called when the capacity of the pool is exceeded and the instance cannot be returned. The process to release the object must be performed, such as Dispose.
		/// </summary>
		public Action<T> OnRelease { set => SetCallback(ref m_OnRelease,value); }

		public UnityObjectPool () {
		}

		/// <param name="original"> The original object from which the pool will instantiate a new instance. </param>
		/// <param name="capacity"> The pool capacity. If less than 0, <see cref="ArgumentOutOfRangeException"/> will be thrown. </param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public UnityObjectPool (T original,int capacity) {
			m_Original = (original != null) ? original : throw Error.ArgumentNullException(nameof(original));
			m_Capacity = (capacity >= 0) ? capacity : throw Error.RequiredNonNegative(nameof(capacity));
		}

		public T Rent () {
			T instance = GetPooledInstance();
			if (instance == null) {
				instance = UnityObject.Instantiate(m_Original);
				m_OnCreate?.Invoke(instance);
			}
			m_OnRent?.Invoke(instance);
			return instance;
		}

		public void Return (T instance) {
			if (instance == null) {
				throw Error.ArgumentNullException(nameof(instance));
			}
			if (m_Pool.Count == m_Capacity) {
				m_OnRelease.Invoke(instance);
				return;
			}
			m_Pool.Enqueue(instance);
			m_OnReturn?.Invoke(instance);
		}

		public void ReleaseInstances (int keep) {
			if ((keep < 0) || (keep > m_Capacity)) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(keep));
			}

			if (keep != 0) {
				for (int i = m_Pool.Count - keep;i > 0;i--) {
					T instance = m_Pool.Dequeue();
					if (instance != null) {
						m_OnRelease.Invoke(instance);
					}
				}
			}
			else {
				while (m_Pool.Count > 0) {
					T instance = m_Pool.Dequeue();
					if (instance != null) {
						m_OnRelease.Invoke(instance);
					}
				}
			}
		}

		/// <summary>
		/// Try to get an instance until the pool is empty or an instance can be retrieved.
		/// This is because <see cref="UnityObject"/> can become null externally due to the <see cref="UnityObject.Destroy(UnityObject)"/> method.
		/// </summary>
		protected T GetPooledInstance () {
			T instance = null;
			while ((m_Pool.Count > 0) && (instance == null)) {
				instance = m_Pool.Dequeue();
			}
			return instance;
		}

		/// <summary>
		/// Set the callback. If used when the pool is not empty, i.e. when the pool is active, throw <see cref="InvalidOperationException"/>.
		/// </summary>
		void SetCallback (ref Action<T> target,Action<T> call) {
			target = (m_Pool.Count == 0) ? call : throw Error.CannotSetCallback();
		}
		
	}
}