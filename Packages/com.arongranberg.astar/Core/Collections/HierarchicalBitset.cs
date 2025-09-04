using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Pathfinding.Collections {
	/// <summary>
	/// Thread-safe hierarchical bitset.
	///
	/// Stores an array of bits. Each bit can be set or cleared individually from any thread.
	///
	/// Note: Setting the capacity is not thread-safe, nor is iterating over the bitset while it is being modified.
	/// </summary>
	[BurstCompile]
	public struct HierarchicalBitset {
		UnsafeSpan<ulong> l1;
		UnsafeSpan<ulong> l2;
		UnsafeSpan<ulong> l3;
		Allocator allocator;

		const int Log64 = 6;

		public HierarchicalBitset (int size, Allocator allocator) {
			this.allocator = allocator;
			l1 = new UnsafeSpan<ulong>(allocator, (size + 64 - 1) >> Log64);
			l2 = new UnsafeSpan<ulong>(allocator, (size + (64*64 - 1)) >> Log64 >> Log64);
			l3 = new UnsafeSpan<ulong>(allocator, (size + (64*64*64 - 1)) >> Log64 >> Log64 >> Log64);
			l1.FillZeros();
			l2.FillZeros();
			l3.FillZeros();
		}

		public bool IsCreated => Capacity > 0;

		public void Dispose () {
			l1.Free(allocator);
			l2.Free(allocator);
			l3.Free(allocator);
			this = default;
		}

		public int Capacity {
			get {
				return l1.Length << Log64;
			}
			set {
				if (value < Capacity) throw new System.ArgumentException("Shrinking the bitset is not supported");
				if (value == Capacity) return;
				var b = new HierarchicalBitset(value, allocator);

				// Copy the old data
				l1.CopyTo(b.l1);
				l2.CopyTo(b.l2);
				l3.CopyTo(b.l3);

				Dispose();
				this = b;
			}
		}

		/// <summary>Number of set bits in the bitset</summary>
		public int Count () {
			int count = 0;
			for (int i = 0; i < l1.Length; i++) {
				count += math.countbits(l1[i]);
			}
			return count;
		}

		/// <summary>True if the bitset is empty</summary>
		public bool IsEmpty {
			get {
				for (int i = 0; i < l3.Length; i++) {
					if (l3[i] != 0) return false;
				}
				return true;
			}
		}

		/// <summary>Clear all bits</summary>
		public void Clear () {
			// TODO: Optimize?
			l1.FillZeros();
			l2.FillZeros();
			l3.FillZeros();
		}

		public void GetIndices (NativeList<int> result) {
			var buffer = new NativeArray<int>(256, Allocator.Temp);
			var iter = GetIterator(buffer.AsUnsafeSpan());
			while (iter.MoveNext()) {
				var span = iter.Current;
				for (int i = 0; i < span.Length; i++) {
					result.Add(span[i]);
				}
			}
		}


		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		static bool SetAtomic (ref UnsafeSpan<ulong> span, int index) {
			var cellIndex = index >> Log64;
			var currentValue = span[cellIndex];
			// Note: 1 << index will only use the lower 6 bits of index
			if ((currentValue & (1UL << index)) != 0) {
				// Bit already set
				return true;
			}

			// TODO: Use Interlocked.Or in newer .net versions
			while (true) {
				var actualValue = (ulong)System.Threading.Interlocked.CompareExchange(ref UnsafeUtility.As<ulong, long>(ref span[cellIndex]), (long)(currentValue | (1UL << index)), (long)currentValue);
				if (actualValue != currentValue) currentValue = actualValue;
				else break;
			}
			return false;
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		static bool ResetAtomic (ref UnsafeSpan<ulong> span, int index) {
			var cellIndex = index >> Log64;
			var currentValue = span[cellIndex];
			// Note: 1 << index will only use the lower 6 bits of index
			if ((currentValue & (1UL << index)) == 0) {
				// Bit already cleared
				return true;
			}

			// TODO: Use Interlocked.Or in newer .net versions
			while (true) {
				var actualValue = (ulong)System.Threading.Interlocked.CompareExchange(ref UnsafeUtility.As<ulong, long>(ref span[cellIndex]), (long)(currentValue & ~(1UL << index)), (long)currentValue);
				if (actualValue != currentValue) currentValue = actualValue;
				else break;
			}
			return false;
		}

		/// <summary>Get the value of a bit</summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public bool Get (int index) {
			// Note: 1 << index will only use the lower 6 bits of index
			return (l1[index >> Log64] & (1UL << index)) != 0;
		}

		/// <summary>Set a given bit to 1</summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void Set (int index) {
			if (SetAtomic(ref l1, index)) return;
			SetAtomic(ref l2, index >> Log64);
			SetAtomic(ref l3, index >> (2*Log64));
		}

		/// <summary>Set a given bit to 0</summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void Reset (int index) {
			if (ResetAtomic(ref l1, index)) return;
			if (l1[index >> Log64] == 0) ResetAtomic(ref l2, index >> Log64);
			if (l2[index >> (2*Log64)] == 0) ResetAtomic(ref l3, index >> (2*Log64));
		}

		/// <summary>Get an iterator over all set bits.</summary>
		/// <param name="scratchBuffer">A buffer to use for temporary storage. A slice of this buffer will be returned on each iteration, filled with the indices of the set bits.</param>
		public Iterator GetIterator (UnsafeSpan<int> scratchBuffer) {
			return new Iterator(this, scratchBuffer);
		}

		[BurstCompile]
		public struct Iterator : IEnumerator<UnsafeSpan<int> >, IEnumerable<UnsafeSpan<int> > {
			HierarchicalBitset bitSet;
			UnsafeSpan<int> result;
			int resultCount;
			int l3index;
			int l3bitIndex;
			int l2bitIndex;

			public UnsafeSpan<int> Current => result.Slice(0, resultCount);

			object IEnumerator.Current => throw new System.NotImplementedException();

			public void Reset() => throw new System.NotImplementedException();

			public void Dispose () {}

			public IEnumerator<UnsafeSpan<int> > GetEnumerator() => this;

			IEnumerator IEnumerable.GetEnumerator() => throw new System.NotImplementedException();

			static int l2index(int l3index, int l3bitIndex) => (l3index << Log64) + l3bitIndex;
			static int l1index(int l2index, int l2bitIndex) => (l2index << Log64) + l2bitIndex;

			public Iterator (HierarchicalBitset bitSet, UnsafeSpan<int> result) {
				this.bitSet = bitSet;
				this.result = result;
				resultCount = 0;
				l3index = 0;
				l3bitIndex = 0;
				l2bitIndex = 0;
				if (result.Length < 128) {
					// Minimum is actually 64, but that can be very inefficient
					throw new System.ArgumentException("Result array must be at least 128 elements long");
				}
			}

			public bool MoveNext () {
				return MoveNextBurst(ref this);
			}

			[BurstCompile]
			public static bool MoveNextBurst (ref Iterator iter) {
				return iter.MoveNextInternal();
			}

			// Inline
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			bool MoveNextInternal () {
				// Store various data in local variables to avoid writing them to memory every time they are updated
				uint resultCount = 0;
				int l3index = this.l3index;
				int l3bitIndex = this.l3bitIndex;
				int l2bitIndex = this.l2bitIndex;
				Assert.IsTrue(l2bitIndex < 64 && l3bitIndex < 64);

				for (; l3index < bitSet.l3.length; l3index++) {
					// Get the L3 cell, and mask out all bits we have already visited
					var l3cell = bitSet.l3[l3index] & (~0UL << l3bitIndex);
					if (l3cell == 0) continue;

					while (l3cell != 0) {
						// Find the next set bit in the L3 cell
						l3bitIndex = math.tzcnt(l3cell);

						// Nest check for level 2
						int l2index = Iterator.l2index(l3index, l3bitIndex);
						// The l2 cell is guaranteed to be non-zero, even after masking out the bits we have already visited
						var l2cell = bitSet.l2[l2index] & (~0UL << l2bitIndex);
						Assert.AreNotEqual(0, l2cell);

						while (l2cell != 0) {
							l2bitIndex = math.tzcnt(l2cell);
							// Stop the loop if we have almost filled the result array
							// Each L1 cell may contain up to 64 set bits
							if (resultCount + 64 > result.Length) {
								this.resultCount = (int)resultCount;
								this.l3index = l3index;
								this.l3bitIndex = l3bitIndex;
								this.l2bitIndex = l2bitIndex;
								return true;
							}

							int l1index = Iterator.l1index(l2index, l2bitIndex);
							var l1cell = bitSet.l1[l1index];
							int l1indexStart = l1index << Log64;
							Assert.AreNotEqual(0, l1cell);

							while (l1cell != 0) {
								var l1bitIndex = math.tzcnt(l1cell);
								l1cell &= l1cell - 1UL; // clear lowest bit
								int index = l1indexStart + l1bitIndex;
								Unity.Burst.CompilerServices.Hint.Assume(resultCount < (uint)result.Length);
								result[resultCount++] = index;
							}

							l2cell &= l2cell - 1UL;
						}

						// Skip a bit at the L3 level
						l3cell &= l3cell - 1UL; // clear lowest bit
						// Enter new L2 level
						l2bitIndex = 0;
					}

					l2bitIndex = 0;
					l3bitIndex = 0;
				}

				this.resultCount = (int)resultCount;
				this.l3index = l3index;
				this.l3bitIndex = l3bitIndex;
				this.l2bitIndex = l2bitIndex;
				return resultCount > 0;
			}
		}
	}
}
