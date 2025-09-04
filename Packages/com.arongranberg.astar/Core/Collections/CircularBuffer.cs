using System.Collections;
using System.Collections.Generic;
using Pathfinding.Pooling;
using Unity.Profiling;
using System.Runtime.CompilerServices;

namespace Pathfinding.Collections {
	/// <summary>
	/// Implements an efficient circular buffer that can be appended to in both directions.
	///
	/// See: <see cref="NativeCircularBuffer"/>
	/// </summary>
	public struct CircularBuffer<T> : IReadOnlyList<T>, IReadOnlyCollection<T> {
		internal T[] data;
		internal int head;
		int length;

		/// <summary>Number of items in the buffer</summary>
		public readonly int Length {
			[IgnoredByDeepProfiler]
			get {
				return length;
			}
		}
		/// <summary>Absolute index of the first item in the buffer, may be negative or greater than <see cref="Length"/></summary>
		public readonly int AbsoluteStartIndex => head;
		/// <summary>Absolute index of the last item in the buffer, may be negative or greater than <see cref="Length"/></summary>
		public readonly int AbsoluteEndIndex => head + length - 1;

		/// <summary>First item in the buffer, throws if the buffer is empty</summary>
		public readonly ref T First {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[IgnoredByDeepProfiler]
			get {
				return ref data[head & (data.Length-1)];
			}
		}

		/// <summary>Last item in the buffer, throws if the buffer is empty</summary>
		public readonly ref T Last {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[IgnoredByDeepProfiler]
			get {
				return ref data[(head+length-1) & (data.Length-1)];
			}
		}

		readonly int IReadOnlyCollection<T>.Count {
			[IgnoredByDeepProfiler]
			get {
				return length;
			}
		}

		/// <summary>Create a new buffer with the given capacity</summary>
		public CircularBuffer(int initialCapacity) {
			data = ArrayPool<T>.Claim(initialCapacity);
			head = 0;
			length = 0;
		}

		/// <summary>
		/// Create a new buffer using the given array as an internal store.
		/// This will take ownership of the given array.
		/// </summary>
		public CircularBuffer(T[] backingArray) {
			data = backingArray;
			head = 0;
			length = 0;
		}

		/// <summary>Resets the buffer's length to zero. Does not clear the current allocation</summary>
		public void Clear () {
			length = 0;
			head = 0;
		}

		/// <summary>Appends a list of items to the end of the buffer</summary>
		public void AddRange (List<T> items) {
			// TODO: Can be optimized
			for (int i = 0; i < items.Count; i++) PushEnd(items[i]);
		}

		/// <summary>Pushes a new item to the start of the buffer</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[IgnoredByDeepProfiler]
		public void PushStart (T item) {
			if (data == null || length >= data.Length) Grow();
			length += 1;
			head -= 1;
			this[0] = item;
		}

		/// <summary>Pushes a new item to the end of the buffer</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[IgnoredByDeepProfiler]
		public void PushEnd (T item) {
			if (data == null || length >= data.Length) Grow();
			length += 1;
			this[length-1] = item;
		}

		/// <summary>Pushes a new item to the start or the end of the buffer</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[IgnoredByDeepProfiler]
		public void Push (bool toStart, T item) {
			if (toStart) PushStart(item);
			else PushEnd(item);
		}

		/// <summary>Removes and returns the first element</summary>
		[IgnoredByDeepProfiler]
		public T PopStart () {
			if (length == 0) throw new System.InvalidOperationException();
			var r = this[0];
			head++;
			length--;
			return r;
		}

		/// <summary>Removes and returns the last element</summary>
		[IgnoredByDeepProfiler]
		public T PopEnd () {
			if (length == 0) throw new System.InvalidOperationException();
			var r = this[length-1];
			length--;
			return r;
		}

		/// <summary>Pops either from the start or from the end of the buffer</summary>
		[IgnoredByDeepProfiler]
		public T Pop (bool fromStart) {
			if (fromStart) return PopStart();
			else return PopEnd();
		}

		/// <summary>Return either the first element or the last element</summary>
		public readonly T GetBoundaryValue (bool start) {
			return GetAbsolute(start ? AbsoluteStartIndex : AbsoluteEndIndex);
		}

		/// <summary>Inserts an item at the given absolute index</summary>
		public void InsertAbsolute (int index, T item) {
			SpliceUninitializedAbsolute(index, 0, 1);
			data[index & (data.Length - 1)] = item;
		}

		/// <summary>Removes toRemove items from the buffer, starting at startIndex, and then inserts the toInsert items at startIndex</summary>
		public void Splice (int startIndex, int toRemove, List<T> toInsert) {
			SpliceAbsolute(startIndex + head, toRemove, toInsert);
		}

		/// <summary>Like <see cref="Splice"/>, but startIndex is an absolute index</summary>
		public void SpliceAbsolute (int startIndex, int toRemove, List<T> toInsert) {
			if (toInsert == null) {
				SpliceUninitializedAbsolute(startIndex, toRemove, 0);
			} else {
				SpliceUninitializedAbsolute(startIndex, toRemove, toInsert.Count);
				for (int i = 0; i < toInsert.Count; i++) data[(startIndex + i) & (data.Length - 1)] = toInsert[i];
			}
		}

		/// <summary>Like <see cref="Splice"/>, but the newly inserted items are left in an uninitialized state</summary>
		public void SpliceUninitialized (int startIndex, int toRemove, int toInsert) {
			SpliceUninitializedAbsolute(startIndex + head, toRemove, toInsert);
		}

		/// <summary>Like <see cref="SpliceUninitialized"/>, but startIndex is an absolute index</summary>
		public void SpliceUninitializedAbsolute (int startIndex, int toRemove, int toInsert) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			if (startIndex - head < 0 || startIndex + toRemove - head > length) throw new System.ArgumentOutOfRangeException();
#endif
			var itemsToAdd = toInsert - toRemove;
			while (this.length + itemsToAdd > this.data.Length) Grow();

			// move items [startIndex+length .. end] itemsToAdd steps forward in the array
			MoveAbsolute(startIndex + toRemove, AbsoluteEndIndex, itemsToAdd);
			this.length += itemsToAdd;
		}

		void MoveAbsolute (int startIndex, int endIndex, int deltaIndex) {
			if (deltaIndex > 0) {
				for (int i = endIndex; i >= startIndex; i--) data[(i+deltaIndex) & (data.Length-1)] = data[i & (data.Length-1)];
			} else if (deltaIndex < 0) {
				for (int i = startIndex; i <= endIndex; i++) data[(i+deltaIndex) & (data.Length-1)] = data[i & (data.Length-1)];
			}
		}

		/// <summary>Indexes the buffer, with index 0 being the first element</summary>
		public T this[int index] {
			[MethodImpl(MethodImplOptions.AggressiveInlining)][IgnoredByDeepProfiler]
			readonly get {
#if UNITY_EDITOR
				if ((uint)index >= length) throw new System.ArgumentOutOfRangeException();
#endif
				return data[(index+head) & (data.Length-1)];
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)][IgnoredByDeepProfiler]
			set {
#if UNITY_EDITOR
				if ((uint)index >= length) throw new System.ArgumentOutOfRangeException();
#endif
				data[(index+head) & (data.Length-1)] = value;
			}
		}

		/// <summary>
		/// Indexes the buffer using absolute indices.
		/// When pushing to and popping from the buffer, the absolute indices do not change.
		/// So e.g. after doing PushStart(x) on an empty buffer, GetAbsolute(-1) will get the newly pushed element.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)][IgnoredByDeepProfiler]
		public readonly T GetAbsolute (int index) {
#if UNITY_EDITOR
			if ((uint)(index - head) >= length) throw new System.ArgumentOutOfRangeException();
#endif
			return data[index & (data.Length-1)];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)][IgnoredByDeepProfiler]
		public readonly void SetAbsolute (int index, T value) {
#if UNITY_EDITOR
			if ((uint)(index - head) >= length) throw new System.ArgumentOutOfRangeException();
#endif
			data[index & (data.Length-1)] = value;
		}

		void Grow () {
			var newData = ArrayPool<T>.Claim(System.Math.Max(4, data != null ? data.Length*2 : 0));
			if (data != null) {
				var inOrderItems = data.Length - (head & (data.Length-1));
				System.Array.Copy(data, head & (data.Length-1), newData, head & (newData.Length - 1), inOrderItems);
				var wraparoundItems = length - inOrderItems;
				if (wraparoundItems > 0) System.Array.Copy(data, 0, newData, (head + inOrderItems) & (newData.Length - 1), wraparoundItems);

				// If T is a class, we need to clear the old array to avoid leaking references that prevent the GC from working
				System.Array.Fill(data, default(T));

				ArrayPool<T>.Release(ref data);
			}
			data = newData;
		}

		/// <summary>Release the backing array of this buffer back into an array pool</summary>
		public void Pool () {
			System.Array.Fill(data, default(T));
			ArrayPool<T>.Release(ref data);
			length = 0;
			head = 0;
		}

		public IEnumerator<T> GetEnumerator () {
			for (int i = 0; i < length; i++) {
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator () {
			for (int i = 0; i < length; i++) {
				yield return this[i];
			}
		}

		public CircularBuffer<T> Clone () {
			return new CircularBuffer<T> {
					   data = data != null ? (T[])data.Clone() : null,
					   length = length,
					   head = head
			};
		}
	}
}
