using System;
using System.Collections;
using System.Collections.Generic;
using MackySoft.XPool.Collections.Internal;
using MackySoft.XPool.Internal;

namespace MackySoft.XPool.Collections {

	/// <summary>
	/// <para> Temporary array without allocation. </para>
	/// <para> This struct use <see cref="ArrayPool{T}"/> internally to avoid allocation and can be used just like a normal array. </para>
	/// <para> After using it, please call the Dispose(). </para>
	/// </summary>
	public partial struct TemporaryArray<T> : IEnumerable<T>, IDisposable {

		T[] m_Array;
		int m_Length;
		ArrayPool<T> m_Pool;

		public int Length => m_Length;

		/// <summary>
		/// Length of internal array.
		/// </summary>
		public int Capacity => m_Array.Length;

		/// <summary>
		/// <para> Internal array. </para>
		/// <para> The length of internal array is always greater than or equal to <see cref="Length"/> property. </para>
		/// </summary>
		public T[] Array => m_Array;

		public T this[int index] {
			get {
				return index >= 0 && index < m_Length ? m_Array[index] : throw Error.ArgumentOutOfRangeOfCollection(nameof(index));
			}
			set {
				if (index < 0 || index >= m_Length) {
					throw Error.ArgumentOutOfRangeOfCollection(nameof(index));
				}
				m_Array[index] = value;
			}
		}

		public TemporaryArray (ArrayPool<T> pool,int length) {
			m_Pool = pool ?? throw Error.ArgumentNullException(nameof(pool));
			m_Array = pool.Rent(length);
			m_Length = length;
		}

		internal TemporaryArray (ArrayPool<T> pool,T[] array,int length) {
			m_Pool = pool ?? throw Error.ArgumentNullException(nameof(pool));
			m_Array = array;
			m_Length = length;
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
			m_Length = 0;
			m_Pool = null;
		}

		public IEnumerator<T> GetEnumerator () {
			for (int i = 0;m_Length > i;i++) {
				yield return m_Array[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator () {
			return GetEnumerator();
		}
	}
	
}