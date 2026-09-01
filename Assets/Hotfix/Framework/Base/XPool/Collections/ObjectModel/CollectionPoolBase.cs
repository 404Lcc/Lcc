using System;
using System.Collections.Generic;
using MackySoft.XPool.Internal;

namespace MackySoft.XPool.Collections.ObjectModel {

	/// <summary>
	/// Base of pool for collections.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class CollectionPoolBase<T> : IPool<T> where T : class {

		readonly Stack<T> m_Pool;
		readonly int m_Capacity;

		// The new() constraint of generics use Activator.CreateInstance, which incurs overhead during object creation.
		// Therefore, explicit constructor call by factory method are used to avoid the overhead.
		readonly Func<T> m_Factory;

		// Stack<T> and Queue<T> do not implement ICollection<T>, so need to use a delegate to call the Clear method instead.
		readonly Action<T> m_Clear;

		public int Capacity => m_Capacity;

		public int Count => m_Pool.Count;

		/// <summary>
		/// Constructor to initialize a collection pool. You must call this in the constructor of the inherited class.
		/// </summary>
		/// <param name="capacity"> The pool capacity. If less than 0, <see cref="ArgumentOutOfRangeException"/> will be thrown. </param>
		/// <param name="factory"> Method that create new instance. If is null, <see cref="ArgumentNullException"/> will be thrown. </param>
		/// <param name="clear"> Method that clear collection. If is null, <see cref="ArgumentNullException"/> will be thrown. </param>
		protected CollectionPoolBase (int capacity, Func<T> factory,Action<T> clear) {
			if (capacity < 0) {
				throw Error.RequiredNonNegative(nameof(capacity));
			}
			if (factory == null) {
				throw Error.ArgumentNullException(nameof(factory));
			}
			if (clear == null) {
				throw Error.ArgumentNullException(nameof(clear));
			}
			m_Pool = new Stack<T>(capacity);
			m_Capacity = capacity;
			m_Factory = factory;
			m_Clear = clear;
		}

		public T Rent () {
			lock (m_Pool) {
				return (m_Pool.Count == 0) ? m_Factory() : m_Pool.Pop();
			}
		}

		public void Return (T collection) {
			if (collection == null) {
				return;
			}

			m_Clear(collection);

			lock (m_Pool) {
				if (m_Pool.Count < m_Capacity) {
					m_Pool.Push(collection);
				}
			}
		}

		public void ReleaseInstances (int keep) {
			if ((keep < 0) || (keep > m_Capacity)) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(keep));
			}

			lock (m_Pool) {
				if (keep != 0) {
					for (int i = m_Pool.Count - keep;i > 0;i--) {
						m_Pool.Pop();
					}
				}
				else {
					m_Pool.Clear();
				}
			}
		}
	}
}