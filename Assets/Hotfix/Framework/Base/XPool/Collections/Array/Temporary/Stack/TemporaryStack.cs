using System;
using System.Collections;
using System.Collections.Generic;
using MackySoft.XPool.Collections.Internal;
using MackySoft.XPool.Internal;

namespace MackySoft.XPool.Collections {
	public partial struct TemporaryStack<T> : IEnumerable<T>, IDisposable {

		T[] m_Array;
		ArrayPool<T> m_Pool;
		int m_Count;

		public int Count => m_Count;
		public int Capacity => m_Array.Length;
		public T[] Array => m_Array;

		public TemporaryStack (ArrayPool<T> pool,int minimumCapacity) {
			m_Pool = pool ?? throw Error.ArgumentNullException(nameof(pool));
			m_Array = pool.Rent(minimumCapacity);
			m_Count = 0;
		}

		/// <summary>
		/// Add object to the head of the stack.
		/// </summary>
		/// <param name="item"> Object to add to the stack. </param>
		public void Push (T item) {
			ArrayPoolUtility.EnsureCapacity(ref m_Array,m_Count + 1,m_Pool);
			m_Array[m_Count] = item;
			m_Count++;
		}

		/// <summary>
		/// Remove object at the head of the stack and returns it. If the stack is empty, <see cref="InvalidOperationException"/> will be thrown.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public T Pop () {
			if (m_Count == 0) {
				throw Error.EmptyCollection();
			}
			T item = m_Array[m_Count - 1];
			m_Array[m_Count - 1] = default;
			m_Count--;
			return item;
		}

		/// <summary>
		/// Return object at the head of the stack. If the stack is empty, <see cref="InvalidOperationException"/> will be thrown.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public T Peek () {
			if (m_Count == 0) {
				throw Error.EmptyCollection();
			}
			return m_Array[m_Count - 1];
		}

		/// <summary>
		/// Remove all objects from the stack.
		/// </summary>
		public void Clear () {
			m_Pool.Return(m_Array,!RuntimeHelpers.IsWellKnownNoReferenceContainsType<T>());
			m_Array = m_Pool.Rent(0);
			m_Count = 0;
		}

		/// <summary>
		/// Whether the specified object exists in the stack.
		/// </summary>
		public bool Contains (T item) {
			int count = m_Count;
			if (item == null) {
				while (count-- > 0) {
					if (m_Array[count] == null) {
						return true;
					}
				}
			}
			else {
				var c = EqualityComparer<T>.Default;
				while (count-- > 0) {
					T e = m_Array[count];
					if ((e != null) && c.Equals(e,item)) {
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Copy objects to array in the stack.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public void CopyTo (T[] array,int arrayIndex) {
			if (array == null) {
				throw Error.ArgumentNullException(nameof(array));
			}
			if ((arrayIndex < 0) || (arrayIndex > array.Length)) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(arrayIndex));
			}
			if ((array.Length - arrayIndex) < m_Count) {
				throw Error.InvalidOffLength();
			}
			System.Array.Copy(m_Array,0,array,arrayIndex,m_Count);
			System.Array.Reverse(array,arrayIndex,m_Count);
		}

		/// <summary>
		/// Return the internal array to the pool.
		/// </summary>
		public void Dispose () {
			Dispose(!RuntimeHelpers.IsWellKnownNoReferenceContainsType<T>());
		}

		/// <summary>
		/// Return the internal array to the pool.
		/// </summary>
		public void Dispose (bool clearArray) {
			m_Pool.Return(ref m_Array,clearArray);
			m_Pool = null;
			m_Count = 0;
		}

		public IEnumerator<T> GetEnumerator () {
			for (int i = m_Count - 1;i >= 0;i--) {
				yield return m_Array[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();

	}
}