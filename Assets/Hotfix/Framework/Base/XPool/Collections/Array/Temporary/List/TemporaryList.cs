using System;
using System.Collections;
using System.Collections.Generic;
using MackySoft.XPool.Collections.Internal;
using MackySoft.XPool.Internal;

namespace MackySoft.XPool.Collections {

	/// <summary>
	/// <para> Temporary list without allocation. </para>
	/// <para> This struct use <see cref="ArrayPool{T}"/> internally to avoid allocation and can be used just like a normal list. </para>
	/// <para> After using it, please call the Dispose(). </para>
	/// </summary>
	public partial struct TemporaryList<T> : IEnumerable<T>, IDisposable {

		T[] m_Array;
		int m_Count;
		ArrayPool<T> m_Pool;

		public int Count => m_Count;

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
				return index >= 0 && index < m_Count ? m_Array[index] : throw Error.ArgumentOutOfRangeOfCollection(nameof(index));
			}
			set {
				if (index < 0 || index >= m_Count) {
					throw Error.ArgumentOutOfRangeOfCollection(nameof(index));
				}
				m_Array[index] = value;
			}
		}

		public TemporaryList (ArrayPool<T> pool,int minimumCapacity) {
			m_Pool = pool ?? throw Error.ArgumentNullException(nameof(pool));
			m_Array = pool.Rent(minimumCapacity);
			m_Count = 0;
		}

		internal TemporaryList (ArrayPool<T> pool,T[] array,int count) {
			m_Pool = pool ?? throw Error.ArgumentNullException(nameof(pool));
			m_Array = array;
			m_Count = count;
		}

		/// <summary>
		/// Whether the specified object exists in the list.
		/// </summary>
		public bool Contains (T item) {
			if (item == null) {
				for (int i = 0;i < m_Count;i++) {
					if (m_Array[i] == null) {
						return true;
					}
				}
				return false;
			}
			else {
				var comparer = EqualityComparer<T>.Default;
				for (int i = 0;i < m_Count;i++) {
					if (comparer.Equals(m_Array[i],item)) {
						return true;
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Add object to the head of the list.
		/// </summary>
		public void Add (T item) {
			ArrayPoolUtility.EnsureCapacity(ref m_Array,m_Count + 1,m_Pool);
			m_Array[m_Count] = item;
			m_Count++;
		}

		/// <summary>
		/// Add elements of specified collection to the head of the list.
		/// </summary>
		/// <param name="collection"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public void AddRange (IEnumerable<T> collection) {
			InsertRange(m_Count,collection);
		}

		/// <summary>
		/// Insert object at the specified index in the list.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void Insert (int index,T item) {
			if (index > m_Count) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(index));
			}

			ArrayPoolUtility.EnsureCapacity(ref m_Array,m_Count + 1,m_Pool);

			if (index < m_Count) {
				System.Array.Copy(m_Array,index,m_Array,index + 1,m_Count - index);
			}

			m_Array[index] = item;
			m_Count++;
		}

		/// <summary>
		/// Insert elements of specified collection to the specified index in the list.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="collection"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public void InsertRange (int index,IEnumerable<T> collection) {
			if (collection == null) {
				throw Error.ArgumentNullException(nameof(collection));
			}
			if (collection is ICollection<T> c) {
				int count = c.Count;

				ArrayPoolUtility.EnsureCapacity(ref m_Array,m_Count + count,m_Pool);
				if (index < m_Count) {
					System.Array.Copy(m_Array,index,m_Array,index + count,m_Count - index);
				}

				c.CopyTo(m_Array,index);

				m_Count += count;
			}
			else {
				foreach (T item in collection) {
					Insert(index,item);
					index++;
				}
			}
		}

		/// <summary>
		/// Remove the first matching element when the specified object exists in the list.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove (T item) {
			int index = IndexOf(item);
			if (index >= 0) {
				RemoveAt(index);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Remove object at the specified index in the list.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public bool RemoveAt (int index) {
			if (index >= m_Count) {
				return false;
			}
			m_Count--;
			if (index < m_Count) {
				System.Array.Copy(m_Array,index + 1,m_Array,index,m_Count - index);
			}
			m_Array[m_Count] = default;
			return true;
		}

		/// <summary>
		/// Remove objects in the list within specified range from specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public void RemoveRange (int index,int count) {
			if (index < 0) {
				throw Error.RequiredNonNegative(nameof(index));
			}
			if (count < 0) {
				throw Error.RequiredNonNegative(nameof(count));
			}
			if ((m_Count - index) < count) {
				throw Error.InvalidOffLength();
			}

			if (count > 0) {
				m_Count -= count;
				if (index < m_Count) {
					System.Array.Copy(m_Array,index + count,m_Array,index,m_Count - index);;
				}
				System.Array.Clear(m_Array,m_Count,count);
			}
		}

		/// <summary>
		/// Remove all objects from the list that match the speficied condition.
		/// </summary>
		/// <param name="match"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public int RemoveAll (Predicate<T> match) {
			if (match == null) {
				throw Error.ArgumentNullException(nameof(match));
			}

			int freeIndex = 0;
			while (freeIndex < m_Count && !match(m_Array[freeIndex])) {
				freeIndex++;
			}
			if (freeIndex >= m_Count) {
				return 0;
			}

			int current = freeIndex + 1;
			while (current  < m_Count) {
				while (current < m_Count && match(m_Array[current])) {
					current++;
				}
				if (current < m_Count) {
					m_Array[freeIndex++] = m_Array[current++];
				}
			}

			System.Array.Clear(m_Array,freeIndex,m_Count - freeIndex);
			int result = m_Count - freeIndex;
			m_Count = freeIndex;
			return result;
		}

		/// <summary>
		/// Remove all objects from the list.
		/// </summary>
		public void Clear () {
			m_Pool.Return(m_Array,!RuntimeHelpers.IsWellKnownNoReferenceContainsType<T>());

			m_Array = m_Pool.Rent(0);
			m_Count = 0;
		}

		#region IndexOf

		/// <summary>
		/// Search for specified object in the list and return the index of the first matching element.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int IndexOf (T item) {
			return System.Array.IndexOf(m_Array,item,0,m_Count);
		}

		/// <summary>
		/// Search for specified object in the list at between specified index and the tail, and return the index of the first matching element.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public int IndexOf (T item,int index) {
			if (index > m_Count) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(index));
			}
			return System.Array.IndexOf(m_Array,item,index,m_Count - index);
		}

		/// <summary>
		/// Search for specified object in the list within specified range from specified index, and return the index of the first matching element.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public int IndexOf (T item,int index,int count) {
			if (index > m_Count) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(index));
			}
			if ((count < 0) || index > (m_Count - count)) {
				throw Error.ArgumentOutOfRangeCount(nameof(count));
			}
			return System.Array.IndexOf(m_Array,item,index,count);
		}

		#endregion

		#region LastIndexOf

		/// <summary>
		/// Search for specified object in the list and return the index of the last matching element.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int LastIndexOf (T item) {
			if (m_Count == 0) {
				return -1;
			}
			return LastIndexOf(item,m_Count - 1,m_Count);
		}

		/// <summary>
		/// Search for specified object in the list at between specified index and the tail, and return the index of the last matching element.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public int LastIndexOf (T item,int index) {
			if (index >= m_Count) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(index));
			}
			return LastIndexOf(item,index,index + 1);
		}

		/// <summary>
		/// Search for specified object in the list within specified range from specified index, and return the index of the first matching element.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public int LastIndexOf (T item,int index,int count) {
			if (m_Count == 0) {
				if (index < 0) {
					throw Error.ArgumentOutOfRangeOfCollection(nameof(index));
				}
				if (count < 0) {
					throw Error.RequiredNonNegative(nameof(count));
				}
				return -1;
			}
			if (index < 0 || index >= m_Count) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(index));
			}
			if ((count < 0) || count > index + 1) {
				throw Error.ArgumentOutOfRangeCount(nameof(count));
			}
			return System.Array.LastIndexOf(m_Array,item,index,count);
		}

		#endregion

		#region BinarySearch

		/// <summary>
		/// Search a sorted list using the default comparer and return the index starting from 0 for that element.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int BinarySearch (T item) {
			return BinarySearch(0,m_Count,item,null);
		}

		/// <summary>
		/// Search in the sorted list within specified range from specified index, using the specified comparer and return the index starting from 0 for that element.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="comparer"></param>
		/// <returns></returns>
		public int BinarySearch (T item,IComparer<T> comparer) {
			return BinarySearch(0,m_Count,item,comparer);
		}

		/// <summary>
		/// Search a sorted list using the specified comparer and return the index starting from 0 for that element.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <param name="item"></param>
		/// <param name="comparer"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public int BinarySearch (int index,int count,T item,IComparer<T> comparer) {
			if (index < 0) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(index));
			}
			if (count < 0) {
				throw Error.RequiredNonNegative(nameof(count));
			}
			if ((m_Count - index) < count) {
				throw Error.ArgumentOutOfRangeCount(nameof(count));
			}
			return System.Array.BinarySearch(m_Array,index,count,item,comparer);
		}

		#endregion

		#region CopyTo

		/// <summary>
		/// Copy objects to array in the list.
		/// </summary>
		/// <param name="array"></param>
		public void CopyTo (T[] array) {
			CopyTo(array,0);
		}

		/// <summary>
		/// Copy objects to array in the list.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		/// <exception cref="ArgumentException"></exception>
		public void CopyTo (T[] array,int arrayIndex) {
			System.Array.Copy(m_Array,0,array,arrayIndex,m_Count);
		}

		/// <summary>
		/// Copy objects to array in the list.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="array"></param>
		/// <param name="arrayIndex"></param>
		/// <param name="count"></param>
		/// <exception cref="ArgumentException"></exception>
		public void CopyTo (int index,T[] array,int arrayIndex,int count) {
			if ((m_Count - index) < count) {
				throw Error.InvalidOffLength();
			}
			System.Array.Copy(m_Array,index,array,arrayIndex,count);
		}

		#endregion

		#region Reverse

		/// <summary>
		/// Reverse the order of the elements in the list.
		/// </summary>
		public void Reverse () {
			Reverse(0,m_Count);
		}

		/// <summary>
		/// Reverse the order of the elements in the list.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public void Reverse (int index,int count) {
			if (index < 0) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(index));
			}
			if (count < 0) {
				throw Error.RequiredNonNegative(nameof(count));
			}
			if ((m_Count - index) < count) {
				throw Error.InvalidOffLength();
			}
			System.Array.Reverse(m_Array,index,count);
		}

		#endregion

		#region Sort

		/// <summary>
		/// Sort the elements of the list using the default comparer.
		/// </summary>
		public void Sort () {
			Sort(0,m_Count,null);
		}

		/// <summary>
		/// Sort the elements of the list using the specified comparer.
		/// </summary>
		/// <param name="comparer"></param>
		public void Sort (IComparer<T> comparer) {
			Sort(0,m_Count,comparer);
		}

		/// <summary>
		/// Sort the elements of the list using the specified comparer.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <param name="comparer"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public void Sort (int index,int count,IComparer<T> comparer) {
			if (index < 0) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(index));
			}
			if (count < 0) {
				throw Error.RequiredNonNegative(nameof(count));
			}
			if ((m_Count - index) < count) {
				throw Error.InvalidOffLength();
			}

			System.Array.Sort(m_Array,index,count,comparer);
		}

		#endregion

		#region FindIndex

		/// <summary>
		/// Search in the list with specified condition and return the index of the first matching element.
		/// </summary>
		/// <param name="match"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public int FindIndex (Predicate<T> match) {
			return FindIndex(0,match);
		}

		/// <summary>
		/// Search in the list at between specified index and the tail with specified condition, and return the index of the first matching element.
		/// </summary>
		/// <param name="startIndex"></param>
		/// <param name="match"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public int FindIndex (int startIndex,Predicate<T> match) {
			// 搜索区间为 [startIndex, m_Count)，count 必须是剩余长度而非总长度
			return FindIndex(startIndex,m_Count - startIndex,match);
		}

		/// <summary>
		/// Search in the list within specified range from specified index with specified condition, and return the index of the first matching element.
		/// </summary>
		/// <param name="startIndex"></param>
		/// <param name="count"></param>
		/// <param name="match"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public int FindIndex (int startIndex,int count,Predicate<T> match) {
			if (startIndex > m_Count) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(startIndex));
			}
			if ((count < 0) || startIndex > (m_Count - count)) {
				throw Error.ArgumentOutOfRangeCount(nameof(count));
			}
			if (match == null) {
				throw Error.ArgumentNullException(nameof(match));
			}

			int endIndex = startIndex + count;
			for (int i = startIndex;i < endIndex;i++) {
				if (match(m_Array[i])) {
					return i;
				}
			}
			return -1;
		}

		#endregion

		#region FindLastIndex

		/// <summary>
		/// Search in the list with specified condition and return the index of the last matching element.
		/// </summary>
		/// <param name="match"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public int FindLastIndex (Predicate<T> match) {
			return FindLastIndex(m_Count - 1,m_Count,match);
		}

		/// <summary>
		/// Search in the list at between specified index and the tail with specified condition, and return the index of the last matching element.
		/// </summary>
		/// <param name="startIndex"></param>
		/// <param name="match"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public int FindLastIndex (int startIndex,Predicate<T> match) {
			return FindLastIndex(startIndex,startIndex + 1,match);
		}

		/// <summary>
		/// Search in the list within specified range from specified index with specified condition, and return the index of the last matching element.
		/// </summary>
		/// <param name="startIndex"></param>
		/// <param name="count"></param>
		/// <param name="match"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public int FindLastIndex (int startIndex,int count,Predicate<T> match) {
			if (match == null) {
				throw Error.ArgumentNullException(nameof(match));
			}
			if (m_Count == 0) {
				// 空列表仅允许 startIndex == -1（与 List<T>.FindLastIndex 一致）
				if (startIndex != -1) {
					throw Error.ArgumentOutOfRangeOfCollection(nameof(startIndex));
				}
			}
			else if (startIndex >= m_Count) {
				throw Error.ArgumentOutOfRangeOfCollection(nameof(startIndex));
			}
			if ((count < 0) || (startIndex - count + 1) < 0) {
				throw Error.ArgumentOutOfRangeCount(nameof(count));
			}

			int endIndex = startIndex - count;
			for (int i = startIndex;i > endIndex;i--) {
				if (match(m_Array[i])) {
					return i;
				}
			}
			return -1;
		}

		#endregion

		/// <summary>
		/// Whether the object matching the specified condition exists in the list.
		/// </summary>
		/// <param name="match"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public bool Exists (Predicate<T> match) {
			return FindIndex(match) != -1;
		}

		/// <summary>
		/// Search in the list with specified condition and return the first matching element.
		/// </summary>
		/// <param name="match"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public T Find (Predicate<T> match) {
			if (match == null) {
				throw Error.ArgumentNullException(nameof(match));
			}

			for (int i = 0;i < m_Count;i++) {
				T item = m_Array[i];
				if (match(item)) {
					return item;
				}
			}
			return default;
		}

		/// <summary>
		/// Search in the list with specified condition and return the last matching element.
		/// </summary>
		/// <param name="match"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public T FindLast (Predicate<T> match) {
			if (match == null) {
				throw Error.ArgumentNullException(nameof(match));
			}

			for (int i = m_Count - 1;i >= 0;i--) {
				T item = m_Array[i];
				if (match(item)) {
					return item;
				}
			}
			return default;
		}

		/// <summary>
		/// Return a collection of objects in a list that match the specified condition.
		/// </summary>
		/// <param name="match"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public TemporaryList<T> FindAll (Predicate<T> match) {
			if (match == null) {
				throw Error.ArgumentNullException(nameof(match));
			}

			TemporaryList<T> result = Create(m_Pool);
			for (int i = 0;i < m_Count;i++) {
				T item = m_Array[i];
				if (match(item)) {
					result.Add(item);
				}
			}
			return result;
		}

		/// <summary>
		/// Whether all objects in the list match the specified condition.
		/// </summary>
		/// <param name="match"></param>
		/// <exception cref="ArgumentNullException"></exception>
		public bool TrueForAll (Predicate<T> match) {
			if (match == null) {
				throw Error.ArgumentNullException(nameof(match));
			}

			for (int i = 0;i < m_Count;i++) {
				if (!match(m_Array[i])) {
					return false;
				}
			}
			return true;
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
			m_Count = 0;
		}

		public IEnumerator<T> GetEnumerator () {
			for (int i = 0;i < m_Count;i++) {
				yield return m_Array[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();

	}

}