// #define DEBUG_ALLOCATOR
namespace Pathfinding.Collections {
	using Unity.Mathematics;
	using Unity.Collections;
	using Unity.Collections.LowLevel.Unsafe;

	/// <summary>
	/// A tiny slab allocator.
	/// Allocates spans of type T in power-of-two sized blocks.
	///
	/// Note: This allocator has no support for merging adjacent freed blocks.
	/// Therefore it is best suited for similarly sized allocations which are relatively small.
	///
	/// Can be used in burst jobs.
	///
	/// This is faster than allocating NativeArrays using the Temp allocator, and significantly faster
	/// than allocating them using the Persistent allocator.
	/// </summary>
	public struct SlabAllocator<T> where T : unmanaged {
		/// <summary>Allocation which is always invalid</summary>
		public const int InvalidAllocation = -2;
		/// <summary>Allocation representing a zero-length array</summary>
		public const int ZeroLengthArray = -1;

		// The max number of items we are likely to need to allocate comes from the connections array of each hierarchical node.
		// If you have a ton (thousands) of off-mesh links next to each other, then that array can get large.
		public const int MaxAllocationSizeIndex = 12;
		public const int MaxAllocationSize = 1 << MaxAllocationSizeIndex;

		internal static int SizeIndexToElements (int sizeIndex) {
			return 1 << sizeIndex;
		}

		internal static int ElementsToSizeIndex (int nElements) {
			if (nElements < 0) throw new System.Exception("SlabAllocator cannot allocate less than 1 element");
			if (nElements == 0) return 0;
			int sizeIndex = CollectionHelper.Log2Ceil(nElements);
			if (sizeIndex > MaxAllocationSizeIndex) throw new System.Exception("SlabAllocator cannot allocate more than MaxAllocationSize elements.");
			return sizeIndex;
		}

		const uint UsedBit = 1u << 31;
		const uint AllocatedBit = 1u << 30;
		const uint LengthMask = AllocatedBit - 1;
		public bool IsDebugAllocator => false;

		[NativeDisableUnsafePtrRestriction]
		unsafe AllocatorData* data;

		struct AllocatorData {
			public UnsafeList<byte> mem;
			public unsafe fixed int freeHeads[MaxAllocationSizeIndex+1];
		}

		struct Header {
			public uint length;
		}

		struct NextBlock {
			public int next;
		}

		public bool IsCreated {
			get {
				unsafe {
					return data != null;
				}
			}
		}

		public int ByteSize {
			get {
				unsafe {
					return data->mem.Length;
				}
			}
		}

		public SlabAllocator(int initialCapacityBytes, AllocatorManager.AllocatorHandle allocator) {
			unsafe {
				data = AllocatorManager.Allocate<AllocatorData>(allocator);
				data->mem = new UnsafeList<byte>(initialCapacityBytes, allocator);
				Clear();
			}
		}

		/// <summary>
		/// Frees all existing allocations.
		/// Does not free the underlaying unmanaged memory. Use <see cref="Dispose"/> for that.
		/// </summary>
		public void Clear () {
			CheckDisposed();
			unsafe {
				data->mem.Clear();
				for (int i = 0; i < MaxAllocationSizeIndex + 1; i++) {
					data->freeHeads[i] = -1;
				}
			}
		}


		/// <summary>
		/// Get the span representing the given allocation.
		/// The returned array does not need to be disposed.
		/// It is only valid until the next call to <see cref="Allocate"/>, <see cref="Free"/> or <see cref="Dispose"/>.
		/// </summary>
		public UnsafeSpan<T> GetSpan (int allocatedIndex) {
			CheckDisposed();
			unsafe {
				if (allocatedIndex == ZeroLengthArray) return new UnsafeSpan<T>(null, 0);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (allocatedIndex < sizeof(Header) || allocatedIndex >= data->mem.Length) throw new System.IndexOutOfRangeException($"Invalid allocation {allocatedIndex}");
#endif
				var ptr = data->mem.Ptr + allocatedIndex;
				var header = (Header*)(ptr - sizeof(Header));
				var length = header->length & LengthMask;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (length > SizeIndexToElements(MaxAllocationSizeIndex)) throw new System.Exception($"Invalid allocation {allocatedIndex}");
				if ((header->length & AllocatedBit) == 0) throw new System.Exception("Trying to get a span for an unallocated index");
#endif
				return new UnsafeSpan<T>(ptr, (int)length);
			}
		}

		public void Realloc (ref int allocatedIndex, int nElements) {
			CheckDisposed();
			if (allocatedIndex == ZeroLengthArray) {
				allocatedIndex = Allocate(nElements);
				return;
			}

			unsafe {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (allocatedIndex < sizeof(Header) || allocatedIndex >= data->mem.Length) throw new System.IndexOutOfRangeException();
#endif
				var ptr = data->mem.Ptr + allocatedIndex;
				var header = (Header*)(ptr - sizeof(Header));
				var length = header->length & LengthMask;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (length > SizeIndexToElements(MaxAllocationSizeIndex)) throw new System.Exception("Invalid index");
				if ((header->length & AllocatedBit) == 0) throw new System.Exception("Trying to get a span for an unallocated index");
#endif
				var capacityIndex = ElementsToSizeIndex((int)length);
				var newCapacityIndex = ElementsToSizeIndex((int)nElements);
				if (capacityIndex == newCapacityIndex) {
					header->length = (uint)nElements | AllocatedBit | UsedBit;
				} else {
					int newAllocation = Allocate(nElements);
					var oldSpan = GetSpan(allocatedIndex);
					var newSpan = GetSpan(newAllocation);
					oldSpan.Slice(0, math.min((int)length, nElements)).CopyTo(newSpan);
					Free(allocatedIndex);
					allocatedIndex = newAllocation;
				}
			}
		}

		/// <summary>
		/// Allocates an array big enough to fit the given values and copies them to the new allocation.
		/// Returns: An ID for the new allocation.
		/// </summary>
		public int Allocate (System.Collections.Generic.List<T> values) {
			var index = Allocate(values.Count);
			var span = GetSpan(index);
			for (int i = 0; i < span.Length; i++) span[i] = values[i];
			return index;
		}

		/// <summary>
		/// Allocates an array big enough to fit the given values and copies them to the new allocation.
		/// Returns: An ID for the new allocation.
		/// </summary>
		public int Allocate (NativeList<T> values) {
			var index = Allocate(values.Length);
			GetSpan(index).CopyFrom(values.AsArray());
			return index;
		}

		/// <summary>
		/// Allocates an array of type T with length nElements.
		/// Must later be freed using <see cref="Free"/> (or <see cref="Dispose)"/>.
		///
		/// Returns: An ID for the new allocation.
		/// </summary>
		public int Allocate (int nElements) {
			CheckDisposed();
			if (nElements == 0) return ZeroLengthArray;
			var sizeIndex = ElementsToSizeIndex(nElements);
			unsafe {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (sizeIndex < 0 || sizeIndex > MaxAllocationSizeIndex) throw new System.Exception("Invalid size index " + sizeIndex);
#endif
				int head = data->freeHeads[sizeIndex];
				if (head != -1) {
					var ptr = data->mem.Ptr;
					data->freeHeads[sizeIndex] = ((NextBlock*)(ptr + head))->next;
					*(Header*)(ptr + head - sizeof(Header)) = new Header { length = (uint)nElements | UsedBit | AllocatedBit };
					return head;
				}

				int headerStart = data->mem.Length;
				int requiredSize = headerStart + sizeof(Header) + SizeIndexToElements(sizeIndex)*sizeof(T);
				if (Unity.Burst.CompilerServices.Hint.Unlikely(requiredSize > data->mem.Capacity)) {
					data->mem.SetCapacity(math.max(data->mem.Capacity*2, requiredSize));
				}

				// Set the length field directly because we know we don't have to resize the list,
				// and we do not care about zeroing the memory.
				data->mem.m_length = requiredSize;
				*(Header*)(data->mem.Ptr + headerStart) = new Header { length = (uint)nElements | UsedBit | AllocatedBit };
				return headerStart + sizeof(Header);
			}
		}

		/// <summary>Frees a single allocation</summary>
		public void Free (int allocatedIndex) {
			CheckDisposed();
			if (allocatedIndex == ZeroLengthArray) return;
			unsafe {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (allocatedIndex < sizeof(Header) || allocatedIndex >= data->mem.Length) throw new System.IndexOutOfRangeException();
#endif
				var ptr = data->mem.Ptr;
				var header = (Header*)(ptr + allocatedIndex - sizeof(Header));
				var length = (int)(header->length & LengthMask);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (length < 0 || length > SizeIndexToElements(MaxAllocationSizeIndex)) throw new System.Exception("Invalid index");
				if ((header->length & AllocatedBit) == 0) throw new System.Exception("Trying to free an already freed index");
#endif

				var sizeIndex = ElementsToSizeIndex(length);

				*(NextBlock*)(ptr + allocatedIndex) = new NextBlock {
					next = data->freeHeads[sizeIndex]
				};
				data->freeHeads[sizeIndex] = allocatedIndex;
				// Mark as not allocated
				header->length &= ~(AllocatedBit | UsedBit);
			}
		}

		public void CopyTo (SlabAllocator<T> other) {
			CheckDisposed();
			other.CheckDisposed();
			unsafe {
				other.data->mem.CopyFrom(data->mem);
				for (int i = 0; i < MaxAllocationSizeIndex + 1; i++) {
					other.data->freeHeads[i] = data->freeHeads[i];
				}
			}
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		void CheckDisposed () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			unsafe {
				if (data == null) throw new System.InvalidOperationException("SlabAllocator is already disposed or not initialized");
			}
#endif
		}

		/// <summary>Frees all unmanaged memory associated with this container</summary>
		public void Dispose () {
			unsafe {
				if (data == null) return;
				var allocator = data->mem.Allocator;
				data->mem.Dispose();
				AllocatorManager.Free(allocator, data);
				data = null;
			}
		}

		public List GetList (int allocatedIndex) {
			return new List(this, allocatedIndex);
		}

		public ref struct List {
			public UnsafeSpan<T> span;
			SlabAllocator<T> allocator;
			// TODO: Can be derived from span
			public int allocationIndex;

			public List(SlabAllocator<T> allocator, int allocationIndex) {
				this.span = allocator.GetSpan(allocationIndex);
				this.allocator = allocator;
				this.allocationIndex = allocationIndex;
			}

			public void Add (T value) {
				allocator.Realloc(ref allocationIndex, span.Length + 1);
				span = allocator.GetSpan(allocationIndex);
				span[span.Length - 1] = value;
			}

			public void RemoveAt (int index) {
				span.Slice(index + 1).CopyTo(span.Slice(index, span.Length - index - 1));
				allocator.Realloc(ref allocationIndex, span.Length - 1);
				span = allocator.GetSpan(allocationIndex);
			}

			public void Clear () {
				allocator.Realloc(ref allocationIndex, 0);
				span = allocator.GetSpan(allocationIndex);
			}

			public int Length => span.Length;

			public ref T this[int index] {
				[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
				get {
					return ref span[index];
				}
			}
		}
	}

	public static class SlabListExtensions {
		public static void Remove<T>(ref this SlabAllocator<T>.List list, T value) where T : unmanaged, System.IEquatable<T> {
			int idx = list.span.IndexOf(value);
			if (idx != -1) list.RemoveAt(idx);
		}
	}
}
