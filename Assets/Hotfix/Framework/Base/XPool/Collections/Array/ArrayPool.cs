using System;
using System.Threading;
using System.Collections.Generic;
using MackySoft.XPool.Internal;
using MackySoft.XPool.Collections.Internal;

namespace MackySoft.XPool.Collections {

	public class ArrayPool<T> : IPool<T[]> {

		const int kMaxBucketSize = 32;
		
		public static readonly ArrayPool<T> Shared = new ArrayPool<T>();

		readonly Stack<T[]>[] m_Pool;
		readonly SpinLock[] m_Locks;

		public ArrayPool () {
			m_Pool = new Stack<T[]>[18];
			m_Locks = new SpinLock[18];
			for (int i = 0;m_Pool.Length > i;i++) {
				m_Locks[i] = new SpinLock(false);
			}
		}

		/// <summary>
		/// The array length is not always accurate.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public T[] Rent (int minimumLength) {
			if (minimumLength < 0) {
				throw Error.RequiredNonNegative(nameof(minimumLength));
			}
			else if (minimumLength == 0) {
				return Array.Empty<T>();
			}

			int size = CalculateArraySize(minimumLength);
			int poolIndex = GetPoolIndex(size);

			if (poolIndex != -1) {
				var pool = m_Pool[poolIndex] ?? (m_Pool[poolIndex] = new Stack<T[]>());

				bool lockTaken = false;
				try {
					m_Locks[poolIndex].Enter(ref lockTaken);
					if (pool.Count != 0) {
						return pool.Pop();
					}
				}
				finally {
					if (lockTaken) {
						m_Locks[poolIndex].Exit(false);
					}
				}
			}

			return new T[size];
		}

		/// <summary>
		/// <para> Return the array to the pool. </para>
		/// <para> The length of the array must be greater than or equal to 8 and a power of 2. </para>
		/// </summary>
		/// <param name="array"> The length of the array must be greater than or equal to 8 and a power of 2. </param>
		public void Return (T[] array) {
			Return(array,!RuntimeHelpers.IsWellKnownNoReferenceContainsType<T>());
		}

		/// <summary>
		/// <para> Return the array to the pool. </para>
		/// <para> The length of the array must be greater than or equal to 8 and a power of 2. </para>
		/// </summary>
		/// <param name="array"> The length of the array must be greater than or equal to 8 and a power of 2. </param>
		public void Return (T[] array,bool clearArray = false) {
			if ((array == null) || (array.Length == 0)) {
				return;
			}

			int poolIndex = GetPoolIndex(array.Length);
			if (poolIndex == -1) {
				return;
			}

			var pool = m_Pool[poolIndex] ?? (m_Pool[poolIndex] = new Stack<T[]>());

			if (clearArray) {
				Array.Clear(array,0,array.Length);
			}

			bool lockTaken = false;
			try {
				m_Locks[poolIndex].Enter(ref lockTaken);

				if (pool.Count < kMaxBucketSize) {
					pool.Push(array);
				}
			}
			finally {
				if (lockTaken) {
					m_Locks[poolIndex].Exit(false);
				}
			}
		}

		/// <summary>
		/// <para> Return the array to the pool and set array reference to null. </para>
		/// <para> The length of the array must be greater than or equal to 8 and a power of 2. </para>
		/// </summary>
		/// <param name="array"> The length of the array must be greater than or equal to 8 and a power of 2. </param>
		public void Return (ref T[] array,bool clearArray = false) {
			Return(array,clearArray);
			array = null;
		}

		public void ReleaseInstances (int keep) {
			if ((keep < 0) || (keep > kMaxBucketSize)) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(keep));
			}

			if (keep != 0) {
				// Release instances from each buckets.
				for (int i = 0;i < m_Pool.Length;i++) {
					var bucket = m_Pool[i];
					if (bucket == null) {
						continue;
					}
					for (int k = bucket.Count - keep;k > 0;k--) {
						bucket.Pop();
					}
				}
			}
			else {
				// Release buckets.
				Array.Clear(m_Pool,0,m_Pool.Length);
			}
		}
		
		static int CalculateArraySize (int size) {
			size--;
			size |= size >> 1;
			size |= size >> 2;
			size |= size >> 4;
			size |= size >> 8;
			size |= size >> 16;
			size += 1;

			if (size < ArrayPoolUtility.kMinArraySize) {
				size = ArrayPoolUtility.kMinArraySize;
			}
			return size;
		}
		
		static int GetPoolIndex (int length) {
			switch (length) {
				case 8:
					return 0;
				case 16:
					return 1;
				case 32:
					return 2;
				case 64:
					return 3;
				case 128:
					return 4;
				case 256:
					return 5;
				case 512:
					return 6;
				case 1024:
					return 7;
				case 2048:
					return 8;
				case 4096:
					return 9;
				case 8192:
					return 10;
				case 16384:
					return 11;
				case 32768:
					return 12;
				case 65536:
					return 13;
				case 131072:
					return 14;
				case 262144:
					return 15;
				case 524288:
					return 16;
				case 1048576:
					return 17;
				default:
					return -1;
			}
		}

		int IPool.Capacity => throw Error.FunctionIsNotSupported();

		int IPool.Count => throw Error.FunctionIsNotSupported();

		T[] IPool<T[]>.Rent () {
			throw Error.FunctionIsNotSupported();
		}
	}
}