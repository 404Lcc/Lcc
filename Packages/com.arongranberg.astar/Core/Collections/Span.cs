using Unity.Mathematics;
using Unity.Profiling;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace Pathfinding.Collections {
	/// <summary>
	/// Replacement for System.Span which is compatible with earlier versions of C#.
	///
	/// Warning: These spans do not in any way guarantee that the memory they refer to is valid. It is up to the user to make sure
	/// the memory is not deallocated before usage. It should never be used to refer to managed heap memory without pinning it, since unpinned managed memory can be moved by some runtimes.
	///
	/// This has several benefits over e.g. UnsafeList:
	/// - It is faster to index into a span than into an UnsafeList, especially from C#. In fact, indexing into an UnsafeSpan is as fast as indexing into a native C# array.
	///    - As a comparison, indexing into a NativeArray can easily be 10x slower, and indexing into an UnsafeList is at least a few times slower.
	/// - You can create a UnsafeSpan from a C# array by pinning it.
	/// - It can be sliced efficiently.
	/// - It supports ref returns for the indexing operations.
	/// </summary>
	public readonly struct UnsafeSpan<T> where T : unmanaged {
		[NativeDisableUnsafePtrRestriction]
		internal readonly unsafe T* ptr;
		internal readonly uint length;

		/// <summary>Number of elements in this span</summary>
		public int Length => (int)length;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe UnsafeSpan(void* ptr, int length) {
			if (length < 0) throw new System.ArgumentOutOfRangeException();
			if (length > 0 && ptr == null) throw new System.ArgumentNullException();
			this.ptr = (T*)ptr;
			this.length = (uint)length;
		}

		/// <summary>
		/// Creates a new UnsafeSpan from a C# array.
		/// The array is pinned to ensure it does not move while the span is in use.
		///
		/// You must unpin the pinned memory using UnsafeUtility.ReleaseGCObject when you are done with the span.
		/// </summary>
		public unsafe UnsafeSpan(T[] data, out ulong gcHandle) {
			unsafe {
				this.ptr = (T*)UnsafeUtility.PinGCArrayAndGetDataAddress(data, out gcHandle);
			}
			this.length = (uint)data.Length;
		}

		/// <summary>
		/// Creates a new UnsafeSpan from a 2D C# array.
		/// The array is pinned to ensure it does not move while the span is in use.
		///
		/// You must unpin the pinned memory using UnsafeUtility.ReleaseGCObject when you are done with the span.
		/// </summary>
		public unsafe UnsafeSpan(T[,] data, out ulong gcHandle) {
			unsafe {
				this.ptr = (T*)UnsafeUtility.PinGCArrayAndGetDataAddress(data, out gcHandle);
			}
			this.length = (uint)data.Length;
		}

		/// <summary>
		/// Allocates a new UnsafeSpan with the specified length.
		/// The memory is not initialized.
		///
		/// You are responsible for freeing the memory using the same allocator when you are done with it.
		/// </summary>
		public UnsafeSpan(Allocator allocator, int length) {
			unsafe {
				if (length < 0) throw new System.ArgumentOutOfRangeException();
				if (length > 0) this.ptr = (T*)UnsafeUtility.MallocTracked(length * (long)UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), allocator, 1);
				else this.ptr = null;
				this.length = (uint)length;
			}
		}

		public ref T this[int index] {
			// With aggressive inlining the performance of indexing is essentially the same as indexing into a native C# array
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[IgnoredByDeepProfiler]
			get {
				unsafe {
					if ((uint)index >= length) throw new System.IndexOutOfRangeException();
					Unity.Burst.CompilerServices.Hint.Assume(ptr != null);
					return ref *(ptr + index);
				}
			}
		}

		public ref T this[uint index] {
			// With aggressive inlining the performance of indexing is essentially the same as indexing into a native C# array
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			[IgnoredByDeepProfiler]
			get {
				unsafe {
					if (index >= length) throw new System.IndexOutOfRangeException();
					Unity.Burst.CompilerServices.Hint.Assume(ptr != null);
					Unity.Burst.CompilerServices.Hint.Assume(ptr + index != null);
					return ref *(ptr + index);
				}
			}
		}

		/// <summary>
		/// Returns a copy of this span, but with a different data-type.
		/// The new data-type must have the same size as the old one.
		///
		/// In burst, this should effectively be a no-op, except possibly a branch.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UnsafeSpan<U> Reinterpret<U> () where U : unmanaged {
			unsafe {
				if (sizeof(T) != sizeof(U)) throw new System.InvalidOperationException("Cannot reinterpret span because the size of the types do not match");
				return new UnsafeSpan<U>(ptr, (int)length);
			}
		}

		/// <summary>
		/// Returns a copy of this span, but with a different data-type.
		/// The new data-type does not need to have the same size as the old one.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UnsafeSpan<U> Reinterpret<U>(int expectedOriginalTypeSize) where U : unmanaged {
			unsafe {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (sizeof(T) != expectedOriginalTypeSize) throw new System.InvalidOperationException("Cannot reinterpret span because sizeof(T) != expectedOriginalTypeSize");
#endif
				return new UnsafeSpan<U>(ptr, (int)length * sizeof(T) / sizeof(U));
			}
		}

		/// <summary>
		/// Creates a new span which is a slice of this span.
		/// The new span will start at the specified index and have the specified length.
		/// </summary>
		public UnsafeSpan<T> Slice (int start, int length) {
			if (start < 0 || length < 0 || start + length > this.length) throw new System.ArgumentOutOfRangeException();
			unsafe {
				return new UnsafeSpan<T>(ptr + start, length);
			}
		}

		/// <summary>
		/// Creates a new span which is a slice of this span.
		/// The new span will start at the specified index and continue to the end of this span.
		/// </summary>
		public UnsafeSpan<T> Slice (int start) {
			return Slice(start, (int)this.length - start);
		}

		/// <summary>Copy the range [startIndex,startIndex+count) to [toIndex,toIndex+count)</summary>
		public void Move (int startIndex, int toIndex, int count) {
			unsafe {
				if (count < 0) throw new System.ArgumentOutOfRangeException();
				if (startIndex < 0 || startIndex + count > length) throw new System.ArgumentOutOfRangeException();
				if (toIndex < 0 || toIndex + count > length) throw new System.ArgumentOutOfRangeException();
				// If length is zero, the pointers may be null, which is technically undefined behavior (but in practice usually fine)
				if (count == 0) return;
				UnsafeUtility.MemMove(ptr + toIndex, ptr + startIndex, (long)sizeof(T) * (long)count);
			}
		}

		/// <summary>
		/// Removes an element from a span, reducing its length by one.
		/// This is done by moving all elements after the index one step back.
		///
		/// The underlaying allocation is not resized, only the length field is changed.
		/// </summary>
		public static void RemoveAt (ref UnsafeSpan<T> span, int index) {
			if (index < 0 || index >= span.length) throw new System.ArgumentOutOfRangeException();
			span.Move(index + 1, index, (int)span.length - index - 1);
			span = span.Slice(0, (int)span.length - 1);
		}

		/// <summary>
		/// Copies the memory of this span to another span.
		/// The other span must be large enough to hold the contents of this span.
		///
		/// Note: Assumes the other span does not alias this one.
		/// </summary>
		public void CopyTo (UnsafeSpan<T> other) {
			if (other.length < length) throw new System.ArgumentException();
			unsafe {
				// If length is zero, the pointers may be null, which is technically undefined behavior (but in practice usually fine)
				if (length > 0) UnsafeUtility.MemCpy(other.ptr, ptr, (long)sizeof(T) * (long)length);
			}
		}

		/// <summary>Appends all elements in this span to the given list</summary>
		public void CopyTo (List<T> buffer) {
			if (buffer.Capacity < buffer.Count + Length) buffer.Capacity = buffer.Count + Length;
			for (int i = 0; i < Length; i++) buffer.Add(this[i]);
		}

		/// <summary>
		/// Creates a new copy of the span allocated using the given allocator.
		///
		/// You are responsible for freeing this memory using the same allocator when you are done with it.
		/// </summary>
		public UnsafeSpan<T> Clone (Allocator allocator) {
			unsafe {
				var clone = new UnsafeSpan<T>(allocator, (int)length);
				CopyTo(clone);
				return clone;
			}
		}

		/// <summary>Converts the span to a managed array</summary>
		public T[] ToArray () {
			var arr = new T[length];
			if (length > 0) {
				unsafe {
					fixed (T* ptr = arr) {
						UnsafeUtility.MemCpy(ptr, this.ptr, (long)sizeof(T) * (long)length);
					}
				}
			}
			return arr;
		}

		/// <summary>
		/// Moves this data to a new NativeArray.
		///
		/// This transfers ownership of the memory to the NativeArray, without any copying.
		/// The NativeArray must be disposed when you are done with it.
		///
		/// Warning: This span must have been allocated using the specified allocator.
		/// </summary>
		public unsafe NativeArray<T> MoveToNativeArray (Allocator allocator) {
			var arr = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(ptr, Length, allocator);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref arr, AtomicSafetyHandle.Create());
#endif
			return arr;
		}

		/// <summary>
		/// Frees the underlaying memory.
		///
		/// Warning: The span must have been allocated using the specified allocator.
		///
		/// Warning: You must never use this span (or any other span referencing the same memory) again after calling this method.
		/// </summary>
		public unsafe void Free (Allocator allocator) {
			if (length > 0) UnsafeUtility.FreeTracked(ptr, allocator);
		}

		/// <summary>
		/// Returns a new span with a different size, copies the current data over to it, and frees this span.
		///
		/// The new span may be larger or smaller than the current span. If it is larger, the new elements will be uninitialized.
		///
		/// Warning: The span must have been allocated using the specified allocator.
		///
		/// Warning: You must never use the old span (or any other span referencing the same memory) again after calling this method.
		///
		/// Returns: The new span.
		/// </summary>
		public unsafe UnsafeSpan<T> Reallocate (Allocator allocator, int newSize) {
			var newSpan = new UnsafeSpan<T>(allocator, newSize);
			Slice(0, System.Math.Min(newSize, Length)).CopyTo(newSpan);
			Free(allocator);
			return newSpan;
		}
	}

	public static class SpanExtensions {
		public static void FillZeros<T>(this UnsafeSpan<T> span) where T : unmanaged {
			unsafe {
				if (span.length > 0) UnsafeUtility.MemSet(span.ptr, 0, (long)sizeof(T) * (long)span.length);
			}
		}

		public static void Fill<T>(this UnsafeSpan<T> span, T value) where T : unmanaged {
			unsafe {
				// This is wayy faster than a C# for loop (easily 10x faster).
				// It is also faster than a burst loop (at least as long as the span is reasonably large).
				// It also generates a lot less code than a burst for loop.
				if (span.length > 0) {
					// If this is too big, unity seems to overflow and crash internally
					if ((long)sizeof(T) * (long)span.length > (long)int.MaxValue) throw new System.ArgumentException("Span is too large to fill");
					UnsafeUtility.MemCpyReplicate(span.ptr, &value, sizeof(T), (int)span.length);
				}
			}
		}

		/// <summary>
		/// Copies the contents of a NativeArray to this span.
		/// The span must be large enough to hold the contents of the array.
		/// </summary>
		public static void CopyFrom<T>(this UnsafeSpan<T> span, NativeArray<T> array) where T : unmanaged {
			array.AsUnsafeReadOnlySpan().CopyTo(span);
		}

		/// <summary>
		/// Copies the contents of another span to this span.
		/// The span must be large enough to hold the contents of the array.
		/// </summary>
		public static void CopyFrom<T>(this UnsafeSpan<T> span, UnsafeSpan<T> other) where T : unmanaged {
			other.CopyTo(span);
		}

		/// <summary>
		/// Copies the contents of an array to this span.
		/// The span must be large enough to hold the contents of the array.
		/// </summary>
		public static void CopyFrom<T>(this UnsafeSpan<T> span, T[] array) where T : unmanaged {
			if (array.Length > span.Length) throw new System.InvalidOperationException();
			if (array.Length == 0) return;
			unsafe {
				var ptr = UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var gcHandle);
				UnsafeUtility.MemCpy(span.ptr, ptr, (long)sizeof(T) * (long)array.Length);
				UnsafeUtility.ReleaseGCObject(gcHandle);
			}
		}

		/// <summary>
		/// Converts an UnsafeAppendBuffer to a span.
		/// The buffer must be a multiple of the element size.
		///
		/// The span is a view of the buffer memory, so do not dispose the buffer while the span is in use.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnsafeSpan<T> AsUnsafeSpan<T>(this UnsafeAppendBuffer buffer) where T : unmanaged {
			unsafe {
				var items = buffer.Length / UnsafeUtility.SizeOf<T>();
				if (items * UnsafeUtility.SizeOf<T>() != buffer.Length) throw new System.ArgumentException("Buffer length is not a multiple of the element size");
				return new UnsafeSpan<T>(buffer.Ptr, items);
			}
		}

		/// <summary>
		/// Converts a NativeList to a span.
		///
		/// The span is a view of the list memory, so do not dispose the list while the span is in use.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnsafeSpan<T> AsUnsafeSpan<T>(this NativeList<T> list) where T : unmanaged {
			unsafe {
				return new UnsafeSpan<T>(list.GetUnsafePtr(), list.Length);
			}
		}

		/// <summary>
		/// Converts a NativeArray to a span.
		///
		/// The span is a view of the array memory, so do not dispose the array while the span is in use.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnsafeSpan<T> AsUnsafeSpan<T>(this NativeArray<T> arr) where T : unmanaged {
			unsafe {
				return new UnsafeSpan<T>(arr.GetUnsafePtr(), arr.Length);
			}
		}

		/// <summary>
		/// Converts a NativeArray to a span without performing any checks.
		///
		/// The span is a view of the array memory, so do not dispose the array while the span is in use.
		/// This method does not perform any checks to ensure that the array is safe to write to or read from.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnsafeSpan<T> AsUnsafeSpanNoChecks<T>(this NativeArray<T> arr) where T : unmanaged {
			unsafe {
				return new UnsafeSpan<T>(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(arr), arr.Length);
			}
		}

		/// <summary>
		/// Converts a NativeArray to a span, assuming it will only be read.
		///
		/// The span is a view of the array memory, so do not dispose the array while the span is in use.
		///
		/// Warning: No checks are done to ensure that you only read from the array. You are responsible for ensuring that you do not write to the span.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnsafeSpan<T> AsUnsafeReadOnlySpan<T>(this NativeArray<T> arr) where T : unmanaged {
			unsafe {
				return new UnsafeSpan<T>(arr.GetUnsafeReadOnlyPtr(), arr.Length);
			}
		}

		/// <summary>
		/// Converts an UnsafeList to a span.
		///
		/// The span is a view of the list memory, so do not dispose the list while the span is in use.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnsafeSpan<T> AsUnsafeSpan<T>(this UnsafeList<T> arr) where T : unmanaged {
			unsafe {
				return new UnsafeSpan<T>(arr.Ptr, arr.Length);
			}
		}

		/// <summary>
		/// Converts a NativeSlice to a span.
		///
		/// The span is a view of the slice memory, so do not dispose the underlaying memory allocation while the span is in use.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnsafeSpan<T> AsUnsafeSpan<T>(this NativeSlice<T> slice) where T : unmanaged {
			unsafe {
				return new UnsafeSpan<T>(slice.GetUnsafePtr(), slice.Length);
			}
		}

		/// <summary>Returns true if the value exists in the span</summary>
		public static bool Contains<T>(this UnsafeSpan<T> span, T value) where T : unmanaged, System.IEquatable<T> {
			return IndexOf(span, value) != -1;
		}

		/// <summary>
		/// Returns the index of the first occurrence of a value in the span.
		/// If the value is not found, -1 is returned.
		/// </summary>
		public static int IndexOf<T>(this UnsafeSpan<T> span, T value) where T : unmanaged, System.IEquatable<T> {
			unsafe {
				return System.MemoryExtensions.IndexOf(new System.ReadOnlySpan<T>(span.ptr, (int)span.length), value);
			}
		}

		/// <summary>Sorts the span in ascending order</summary>
		public static void Sort<T>(this UnsafeSpan<T> span) where T : unmanaged, System.IComparable<T> {
			unsafe {
				NativeSortExtension.Sort<T>(span.ptr, span.Length);
			}
		}

		/// <summary>Sorts the span in ascending order</summary>
		public static void Sort<T, U>(this UnsafeSpan<T> span, U comp) where T : unmanaged where U : System.Collections.Generic.IComparer<T> {
			unsafe {
				NativeSortExtension.Sort<T, U>(span.ptr, span.Length, comp);
			}
		}

#if !MODULE_COLLECTIONS_2_4_0_OR_NEWER
		/// <summary>Shifts elements toward the end of this list, increasing its length</summary>
		public static void InsertRange<T>(this NativeList<T> list, int index, int count) where T : unmanaged {
			list.ResizeUninitialized(list.Length + count);
			list.AsUnsafeSpan().Move(index, index + count, list.Length - (index + count));
		}
#endif

#if !MODULE_COLLECTIONS_2_1_0_OR_NEWER
		/// <summary>Appends value count times to the end of this list</summary>
		public static void AddReplicate<T>(this NativeList<T> list, T value, int count) where T : unmanaged {
			var origLength = list.Length;
			list.ResizeUninitialized(origLength + count);
			list.AsUnsafeSpan().Slice(origLength).Fill(value);
		}

		/// <summary>Appends value count times to the end of this list</summary>
		public static void AddReplicate<T>(this UnsafeList<T> list, T value, int count) where T : unmanaged {
			var origLength = list.Length;
			list.Resize(origLength + count, NativeArrayOptions.UninitializedMemory);
			list.AsUnsafeSpan().Slice(origLength).Fill(value);
		}
#endif
	}
}
