using System;
using System.Collections.Generic;
using MackySoft.XPool.Internal;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace MackySoft.XPool.Unity.ObjectModel {

	/// <summary>
	/// Base of pool for <see cref="UnityObject"/>.
	/// </summary>
	public abstract class UnityObjectPoolBase<T> : IUnityObjectPool<T> where T : UnityObject {

		[Tooltip("The original object from which the pool will instantiate a new instance.")]
		[SerializeField]
		protected T m_Original;

		[Tooltip("Capacity to store instances in the pool.")]
		[SerializeField]
		int m_Capacity;

		readonly Queue<T> m_Pool = new Queue<T>();

		public int Capacity => m_Capacity;

		public int Count => m_Pool.Count;

		protected UnityObjectPoolBase () {
		}

		/// <param name="original"> The original object from which the pool will instantiate a new instance. </param>
		/// <param name="capacity"> The pool capacity. If less than 0, <see cref="ArgumentOutOfRangeException"/> will be thrown. </param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		protected UnityObjectPoolBase (T original,int capacity) {
			m_Original = (original != null) ? original : throw Error.ArgumentNullException(nameof(original));
			m_Capacity = (capacity >= 0) ? capacity : throw Error.RequiredNonNegative(nameof(capacity));
		}

		public T Rent () {
			T instance = GetPooledInstance();
			if (instance == null) {
				instance = UnityObject.Instantiate(m_Original);
				OnCreate(instance);
			}
			OnRent(instance);

			return (instance != null) ? instance : throw Error.InstanceDestroyed();
		}

		public void Return (T instance) {
			if (instance == null) {
				throw Error.ArgumentNullException(nameof(instance));
			}
			if (m_Pool.Count == m_Capacity) {
				OnRelease(instance);
				return;
			}
			m_Pool.Enqueue(instance);
			OnReturn(instance);
		}

		public void ReleaseInstances (int keep) {
			if ((keep < 0) || (keep > m_Capacity)) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(keep));
			}

			if (keep != 0) {
				for (int i = m_Pool.Count - keep;i > 0;i--) {
					T instance = m_Pool.Dequeue();
					if (instance != null) {
						OnRelease(instance);
					}
				}
			}
			else {
				while (m_Pool.Count > 0) {
					T instance = m_Pool.Dequeue();
					if (instance != null) {
						OnRelease(instance);
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
		/// Called when called <see cref="Rent"/> if pool is empty and new instance is instantiated by the pool.
		/// </summary>
		protected abstract void OnCreate (T instance);

		/// <summary>
		/// Called when rent an instance from the pool.
		/// </summary>
		protected abstract void OnRent (T instance);

		/// <summary>
		/// Called when return an instance to the pool.
		/// </summary>
		protected abstract void OnReturn (T instance);

		/// <summary>
		/// Called when the capacity of the pool is exceeded and the instance cannot be returned. The process to release the object must be performed, such as Dispose.
		/// </summary>
		protected abstract void OnRelease (T instance);
	}
}
