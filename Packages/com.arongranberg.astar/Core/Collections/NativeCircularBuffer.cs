using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Assertions;
using Unity.Profiling;

namespace Pathfinding.Collections {
	/// <summary>
	/// Implements an efficient circular buffer that can be appended to in both directions.
	///
	/// See: <see cref="CircularBuffer"/>
	/// </summary>
	public struct NativeCircularBuffer<T> : IReadOnlyList<T>, IReadOnlyCollection<T> where T : unmanaged {
		[NativeDisableUnsafePtrRestriction]
		internal unsafe T* data;
		internal int head;
		int length;
		/// <summary>Capacity of the allocation minus 1. Invariant: (a power of two) minus 1</summary>
		int capacityMask;

		/// <summary>The allocator used to create the internal buffer.</summary>
		public AllocatorManager.AllocatorHandle Allocator;
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

		/// <summary>First item in the buffer throws if the buffer is empty</summary>
		public readonly ref T First {
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			[IgnoredByDeepProfiler]
			get {
				unsafe {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
					if (length == 0) throw new System.InvalidOperationException();
#endif
					return ref data[head & capacityMask];
				}
			}
		}

		/// <summary>Last item in the buffer, throws if the buffer is empty</summary>
		public readonly ref T Last {
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			[IgnoredByDeepProfiler]
			get {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (length == 0) throw new System.InvalidOperationException();
#endif
				unsafe { return ref data[(head+length-1) & capacityMask]; }
			}
		}

		readonly int IReadOnlyCollection<T>.Count => Length;

		public readonly bool IsCreated {
			get {
				unsafe {
					return data != null;
				}
			}
		}

		/// <summary>Create a new empty buffer</summary>

		public NativeCircularBuffer(AllocatorManager.AllocatorHandle allocator) {
			unsafe {
				data = null;
			}
			Allocator = allocator;
			capacityMask = -1;
			head = 0;
			length = 0;
		}

		/// <summary>Create a new buffer with the given capacity</summary>
		public NativeCircularBuffer(int initialCapacity, AllocatorManager.AllocatorHandle allocator) {
			initialCapacity = math.ceilpow2(initialCapacity);
			unsafe {
				data = AllocatorManager.Allocate<T>(allocator, initialCapacity);
				capacityMask = initialCapacity - 1;
			}
			Allocator = allocator;
			head = 0;
			length = 0;
		}

		unsafe public NativeCircularBuffer(CircularBuffer<T> buffer, out ulong gcHandle) : this(buffer.data, buffer.head, buffer.Length, out gcHandle) {}

		unsafe public NativeCircularBuffer(T[] data, int head, int length, out ulong gcHandle) {
			Assert.IsTrue((data.Length & (data.Length - 1)) == 0);
			Assert.IsTrue(length <= data.Length);
			unsafe {
				this.data = (T*)UnsafeUtility.PinGCArrayAndGetDataAddress(data, out gcHandle);
			}
			this.capacityMask = data.Length - 1;
			this.head = head;
			this.length = length;
			Allocator = Unity.Collections.Allocator.None;
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
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		[IgnoredByDeepProfiler]
		public void PushStart (T item) {
			if (length > capacityMask) Grow();
			length += 1;
			head -= 1;
			this[0] = item;
		}

		/// <summary>Pushes a new item to the end of the buffer</summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		[IgnoredByDeepProfiler]
		public void PushEnd (T item) {
			if (length > capacityMask) Grow();
			length += 1;
			this[length-1] = item;
		}

		/// <summary>Pushes a new item to the start or the end of the buffer</summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void Push (bool toStart, T item) {
			if (toStart) PushStart(item);
			else PushEnd(item);
		}

		/// <summary>Removes and returns the first element</summary>
		[IgnoredByDeepProfiler]
		public T PopStart () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			if (length == 0) throw new System.InvalidOperationException();
#endif
			var r = this[0];
			head++;
			length--;
			return r;
		}

		/// <summary>Removes and returns the last element</summary>
		[IgnoredByDeepProfiler]
		public T PopEnd () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			if (length == 0) throw new System.InvalidOperationException();
#endif
			var r = this[length-1];
			length--;
			return r;
		}

		/// <summary>Pops either from the start or from the end of the buffer</summary>
		public T Pop (bool fromStart) {
			if (fromStart) return PopStart();
			else return PopEnd();
		}

		/// <summary>Return either the first element or the last element</summary>
		public readonly T GetBoundaryValue (bool start) {
			return start ? GetAbsolute(AbsoluteStartIndex) : GetAbsolute(AbsoluteEndIndex);
		}

		/// <summary>Lowers the length of the buffer to the given value, and does nothing if the given value is greater or equal to the current length</summary>

		public void TrimTo (int length) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			if (length < 0) throw new System.ArgumentOutOfRangeException();
#endif
			this.length = math.min(this.length, length);
		}

		/// <summary>Removes toRemove items from the buffer, starting at startIndex, and then inserts the toInsert items at startIndex</summary>

		public void Splice (int startIndex, int toRemove, List<T> toInsert) {
			SpliceAbsolute(startIndex + head, toRemove, toInsert);
		}

		/// <summary>Like <see cref="Splice"/>, but startIndex is an absolute index</summary>

		public void SpliceAbsolute (int startIndex, int toRemove, List<T> toInsert) {
			SpliceUninitializedAbsolute(startIndex, toRemove, toInsert.Count);
			unsafe {
				for (int i = 0; i < toInsert.Count; i++) data[(startIndex + i) & capacityMask] = toInsert[i];
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
			while (this.length + itemsToAdd > capacityMask + 1) Grow();

			// move items [startIndex+length .. end] itemsToAdd steps forward in the array
			MoveAbsolute(startIndex + toRemove, AbsoluteEndIndex, itemsToAdd);
			this.length += itemsToAdd;
		}

		void MoveAbsolute (int startIndex, int endIndex, int deltaIndex) {
			unsafe {
				if (deltaIndex > 0) {
					for (int i = endIndex; i >= startIndex; i--) data[(i+deltaIndex) & capacityMask] = data[i & capacityMask];
				} else if (deltaIndex < 0) {
					for (int i = startIndex; i <= endIndex; i++) data[(i+deltaIndex) & capacityMask] = data[i & capacityMask];
				}
			}
		}

		/// <summary>Indexes the buffer, with index 0 being the first element</summary>
		public T this[int index] {
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			[IgnoredByDeepProfiler]
			readonly get {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if ((uint)index >= length) throw new System.ArgumentOutOfRangeException();
#endif
				unsafe {
					return data[(index+head) & capacityMask];
				}
			}
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			[IgnoredByDeepProfiler]
			set {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if ((uint)index >= length) throw new System.ArgumentOutOfRangeException();
#endif
				unsafe {
					data[(index+head) & capacityMask] = value;
				}
			}
		}

		/// <summary>
		/// Indexes the buffer using absolute indices.
		/// When pushing to and popping from the buffer, the absolute indices do not change.
		/// So e.g. after doing PushStart(x) on an empty buffer, GetAbsolute(-1) will get the newly pushed element.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		[IgnoredByDeepProfiler]
		public readonly T GetAbsolute (int index) {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			if ((uint)(index - head) >= length) throw new System.ArgumentOutOfRangeException();
#endif
			unsafe {
				return data[index & capacityMask];
			}
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		void Grow () {
			unsafe {
				// Note: Will always be a power of 2 since capacity is a power of 2
				var capacity = capacityMask + 1;
				var newCapacity = math.max(4, capacity*2);
				var newData = AllocatorManager.Allocate<T>(this.Allocator, newCapacity);
				if (data != null) {
					var inOrderItems = capacity - (head & capacityMask);
					UnsafeUtility.MemCpy(newData + (head & (newCapacity - 1)), data + (head & capacityMask), inOrderItems * sizeof(T));
					var wraparoundItems = length - inOrderItems;
					if (wraparoundItems > 0) {
						UnsafeUtility.MemCpy(newData + ((head + inOrderItems) & (newCapacity - 1)), data, wraparoundItems * sizeof(T));
					}
					AllocatorManager.Free(Allocator, data);
				}
				capacityMask = newCapacity - 1;
				data = newData;
			}
		}

		/// <summary>Releases the unmanaged memory held by this container</summary>
		public void Dispose () {
			capacityMask = -1;
			length = 0;
			head = 0;
			unsafe {
				AllocatorManager.Free(Allocator, data);
				data = null;
			}
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

		public NativeCircularBuffer<T> Clone () {
			unsafe {
				if (!IsCreated) return default;

				var newData = AllocatorManager.Allocate<T>(this.Allocator, capacityMask + 1);
				UnsafeUtility.MemCpy(newData, data, length * sizeof(T));
				return new NativeCircularBuffer<T> {
						   data = newData,
						   head = head,
						   length = length,
						   capacityMask = capacityMask,
						   Allocator = this.Allocator
				};
			}
		}
	}
}
