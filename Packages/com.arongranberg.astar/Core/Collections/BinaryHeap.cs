// #define VALIDATE_BINARY_HEAP
#pragma warning disable 162
#pragma warning disable 429
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Unity.Burst.CompilerServices;

namespace Pathfinding {
	using Pathfinding.Collections;

	/// <summary>
	/// Binary heap implementation.
	/// Binary heaps are really fast for ordering nodes in a way that
	/// makes it possible to get the node with the lowest F score.
	/// Also known as a priority queue.
	///
	/// This has actually been rewritten as a 4-ary heap
	/// for performance, but it's the same principle.
	///
	/// See: http://en.wikipedia.org/wiki/Binary_heap
	/// See: https://en.wikipedia.org/wiki/D-ary_heap
	/// </summary>
	[BurstCompile]
	public struct BinaryHeap {
		/// <summary>Internal backing array for the heap</summary>
		private UnsafeSpan<HeapNode> heap;

		/// <summary>Number of items in the tree</summary>
		public int numberOfItems;
		uint insertionOrder;

		/// <summary>Ties between elements that have the same F score can be broken by the H score or by insertion order</summary>
		public TieBreaking tieBreaking;

		public enum TieBreaking : byte {
			HScore,
			InsertionOrder,
		}

		/// <summary>The tree will grow by at least this factor every time it is expanded</summary>
		public const float GrowthFactor = 2;

		/// <summary>
		/// Number of children of each node in the tree.
		/// Different values have been tested and 4 has been empirically found to perform the best.
		/// See: https://en.wikipedia.org/wiki/D-ary_heap
		/// </summary>
		const int D = 4;

		public const ushort NotInHeap = 0xFFFF;

		/// <summary>True if the heap does not contain any elements</summary>
		public bool isEmpty => numberOfItems <= 0;

		/// <summary>Item in the heap</summary>
		private struct HeapNode {
			public uint pathNodeIndex;
			/// <summary>Bitpacked F and G scores</summary>
			public ulong sortKey;

			public HeapNode (uint pathNodeIndex, uint tieBreaker, uint f) {
				this.pathNodeIndex = pathNodeIndex;
				this.sortKey = ((ulong)f << 32) | (ulong)tieBreaker;
			}

			public uint F {
				get => (uint)(sortKey >> 32);
				set => sortKey = (sortKey & 0xFFFFFFFFUL) | ((ulong)value << 32);
			}

			public uint TieBreaker {
				get => (uint)sortKey;
				set => sortKey = (sortKey & 0xFFFFFFFF00000000UL) | (ulong)value;
			}
		}

		/// <summary>
		/// Rounds up v so that it has remainder 1 when divided by D.
		/// I.e it is of the form n*D + 1 where n is any non-negative integer.
		/// </summary>
		static int RoundUpToNextMultipleMod1 (int v) {
			// I have a feeling there is a nicer way to do this
			return v + (4 - ((v-1) % D)) % D;
		}

		/// <summary>Create a new heap with the specified initial capacity</summary>
		public BinaryHeap (int capacity) {
			// Make sure the size has remainder 1 when divided by D
			// This allows us to always guarantee that indices used in the Remove method
			// will never throw out of bounds exceptions
			capacity = RoundUpToNextMultipleMod1(capacity);

			heap = new UnsafeSpan<HeapNode>(Unity.Collections.Allocator.Persistent, capacity);
			numberOfItems = 0;
			insertionOrder = 0;
			tieBreaking = TieBreaking.HScore;
		}

		public void Dispose () {
			unsafe {
				AllocatorManager.Free<HeapNode>(Allocator.Persistent, heap.ptr, heap.Length);
			}
		}

		/// <summary>Removes all elements from the heap</summary>
		public void Clear (UnsafeSpan<PathNode> pathNodes) {
			// Clear all heap indices, and references to the heap nodes
			for (int i = 0; i < numberOfItems; i++) {
				pathNodes[heap[i].pathNodeIndex].heapIndex = NotInHeap;
			}

			numberOfItems = 0;
			insertionOrder = 0;
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public uint GetPathNodeIndex(int heapIndex) => heap[heapIndex].pathNodeIndex;

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public uint GetH(int heapIndex) => tieBreaking == TieBreaking.HScore ? heap[heapIndex].TieBreaker : 0;

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public uint GetF(int heapIndex) => heap[heapIndex].F;

		/// <summary>
		/// Replaces the H score of a node in the heap.
		///
		/// Warning: Assumes that ties are broken by H scores.
		/// </summary>
		public void SetH (int heapIndex, uint h) {
			UnityEngine.Assertions.Assert.IsTrue(tieBreaking == TieBreaking.HScore);
			ref var node = ref heap[heapIndex];
			var g = node.F - node.TieBreaker;
			node.TieBreaker = h;
			node.F = g + h;
		}

		/// <summary>Expands to a larger backing array when the current one is too small</summary>
		static void Expand (ref UnsafeSpan<HeapNode> heap) {
			// 65533 == 1 mod 4 and slightly smaller than 1<<16 = 65536
			int newSize = math.max(heap.Length+4, math.min(65533, (int)math.round(heap.Length*GrowthFactor)));

			// Make sure the size has remainder 1 when divided by D
			// This allows us to always guarantee that indices used in the Remove method
			// will never throw out of bounds exceptions
			newSize = RoundUpToNextMultipleMod1(newSize);

			// Check if the heap is really large
			// Also note that heaps larger than this are not supported
			// since PathNode.heapIndex is a ushort and can only store
			// values up to 65535 (NotInHeap = 65535 is reserved however)
			if (newSize > (1<<16) - 2) {
				throw new System.Exception("Binary Heap Size really large (>65534). A heap size this large is probably the cause of pathfinding running in an infinite loop. ");
			}

			var newHeap = new UnsafeSpan<HeapNode>(Unity.Collections.Allocator.Persistent, newSize);
			newHeap.CopyFrom(heap);
			unsafe {
				AllocatorManager.Free<HeapNode>(Allocator.Persistent, heap.ptr, heap.Length);
			}
#if ASTARDEBUG
			UnityEngine.Debug.Log("Resizing binary heap to "+newSize);
#endif
			heap = newHeap;
		}

		/// <summary>Adds a node to the heap</summary>
		public void Add (UnsafeSpan<PathNode> nodes, uint pathNodeIndex, uint g, uint h) {
			Add(ref this, ref nodes, pathNodeIndex, g, h, insertionOrder++, tieBreaking);
		}

		[BurstCompile]
		static void Add (ref BinaryHeap binaryHeap, ref UnsafeSpan<PathNode> nodes, uint pathNodeIndex, uint g, uint h, uint insertionOrder, TieBreaking tieBreaking) {
			ref var numberOfItems = ref binaryHeap.numberOfItems;
			ref var heap = ref binaryHeap.heap;
			var f = g + h;

			// We need to shift the insertion order by 2 bits, since there are some bit-stealing shenanigans going on in the Remove method
			var tieBreaker = tieBreaking == TieBreaking.HScore ? h : (insertionOrder << 2);

			// Check if node is already in the heap
			ref var node = ref nodes[pathNodeIndex];
			if (node.heapIndex != NotInHeap) {
				var activeNode = new HeapNode(pathNodeIndex, tieBreaker, f);
				UnityEngine.Assertions.Assert.AreEqual(activeNode.pathNodeIndex, pathNodeIndex);
				DecreaseKey(heap, nodes, activeNode, node.heapIndex);
				Validate(ref nodes, ref binaryHeap);
			} else {
				if (numberOfItems == heap.Length) {
					Expand(ref heap);
				}
				Validate(ref nodes, ref binaryHeap);

				DecreaseKey(heap, nodes, new HeapNode(pathNodeIndex, tieBreaker, f), (ushort)numberOfItems);
				numberOfItems++;
				Validate(ref nodes, ref binaryHeap);
			}
		}

		static void DecreaseKey (UnsafeSpan<HeapNode> heap, UnsafeSpan<PathNode> nodes, HeapNode node, ushort index) {
			// This is where 'obj' is in the binary heap logically speaking
			// (for performance reasons we don't actually store it there until
			// we know the final index, that's just a waste of CPU cycles)
			uint bubbleIndex = index;

			while (bubbleIndex != 0) {
				// Parent node of the bubble node
				uint parentIndex = (bubbleIndex-1) / D;

				Hint.Assume(parentIndex < heap.length);
				Hint.Assume(bubbleIndex < heap.length);
				if (node.sortKey < heap[parentIndex].sortKey) {
					// Swap the bubble node and parent node
					// (we don't really need to store the bubble node until we know the final index though
					// so we do that after the loop instead)
					heap[bubbleIndex] = heap[parentIndex];
					nodes[heap[bubbleIndex].pathNodeIndex].heapIndex = (ushort)bubbleIndex;
					bubbleIndex = parentIndex;
				} else {
					break;
				}
			}

			Hint.Assume(bubbleIndex < heap.length);
			heap[bubbleIndex] = node;
			nodes[node.pathNodeIndex].heapIndex = (ushort)bubbleIndex;
		}

		/// <summary>
		/// Returns the node with the lowest F score from the heap.
		///
		/// Note: If <see cref="tieBreaking"/> is set not set to HScore, the returned h score will be 0.
		/// </summary>
		public uint Remove (UnsafeSpan<PathNode> nodes, out uint g, out uint h) {
			var index = Remove(ref nodes, ref this, out var removedTieBreaker, out var removedF);
			h = tieBreaking == TieBreaking.HScore ? removedTieBreaker : 0;
			g = removedF - h;
			return index;
		}

		[BurstCompile]
		static uint Remove (ref UnsafeSpan<PathNode> nodes, ref BinaryHeap binaryHeap, [NoAlias] out uint removedTieBreaker, [NoAlias] out uint removedF) {
			ref var numberOfItems = ref binaryHeap.numberOfItems;
			var heap = binaryHeap.heap;

			if (numberOfItems == 0) {
				throw new System.InvalidOperationException("Removing item from empty heap");
			}

			// This is the smallest item in the heap.
			// Mark it as removed from the heap.
			Hint.Assume(0UL < heap.length);
			uint returnIndex = heap[0].pathNodeIndex;
			nodes[returnIndex].heapIndex = NotInHeap;
			removedTieBreaker = heap[0].TieBreaker;
			removedF = heap[0].F;

			numberOfItems--;
			if (numberOfItems == 0) {
				return returnIndex;
			}

			// Last item in the heap array
			Hint.Assume((uint)numberOfItems < heap.length);
			var swapItem = heap[numberOfItems];
			uint swapIndex = 0;
			ulong comparisonKey = swapItem.sortKey & ~0x3UL;

			// Trickle upwards
			while (true) {
				var parent = swapIndex;
				uint pd = parent * D + 1;

				// If this holds, then the indices used
				// below are guaranteed to not throw an index out of bounds
				// exception since we choose the size of the array in that way
				if (pd < numberOfItems) {
					// Find the child node with the smallest F score, or if equal, the smallest G score.
					// The sorting key is the tuple (F,G).
					// However, to be able to easily get the smallest index, we steal the lowest 2 bits of G
					// and use it for the child index (0..3) instead.
					// This means that tie-breaking will not be perfect, but in all practical cases it will
					// yield exactly the same result since G scores typically differ by more than 4 anyway.
					Hint.Assume(pd+0 < heap.length);
					ulong l0 = (heap[pd+0].sortKey & ~0x3UL) | 0;
					Hint.Assume(pd+1 < heap.length);
					ulong l1 = (heap[pd+1].sortKey & ~0x3UL) | 1;
					Hint.Assume(pd+2 < heap.length);
					ulong l2 = (heap[pd+2].sortKey & ~0x3UL) | 2;
					Hint.Assume(pd+3 < heap.length);
					ulong l3 = (heap[pd+3].sortKey & ~0x3UL) | 3;

					ulong smallest = l0;
					// Not all children may exist, so we need to check that the index is valid
					// TODO: Could optimize by ensuring that the sort keys for invalid children are always set to ulong.MaxValue
					if (pd+1 < numberOfItems) smallest = math.min(smallest, l1);
					if (pd+2 < numberOfItems) smallest = math.min(smallest, l2);
					if (pd+3 < numberOfItems) smallest = math.min(smallest, l3);

					if ((smallest & ~0x3UL) < comparisonKey) {
						swapIndex = pd + (uint)(smallest & 0x3UL);

						// One of the parent's children are smaller or equal, swap them
						// (actually we are just pretenting we swapped them, we hold the swapItem
						// in local variable and only assign it once we know the final index)
						Hint.Assume(parent < heap.length);
						Hint.Assume(swapIndex < heap.length);
						heap[parent] = heap[swapIndex];
						Hint.Assume(swapIndex < heap.length);
						nodes[heap[swapIndex].pathNodeIndex].heapIndex = (ushort)parent;
					} else {
						break;
					}
				} else {
					break;
				}
			}

			// Assign element to the final position
			Hint.Assume(swapIndex < heap.length);
			heap[swapIndex] = swapItem;
			nodes[swapItem.pathNodeIndex].heapIndex = (ushort)swapIndex;

			// For debugging
			Validate(ref nodes, ref binaryHeap);

			return returnIndex;
		}

		[System.Diagnostics.Conditional("VALIDATE_BINARY_HEAP")]
		static void Validate (ref UnsafeSpan<PathNode> nodes, ref BinaryHeap binaryHeap) {
			for (int i = 1; i < binaryHeap.numberOfItems; i++) {
				int parentIndex = (i-1)/D;
				if (binaryHeap.heap[parentIndex].F > binaryHeap.heap[i].F) {
					throw new System.Exception("Invalid state at " + i + ":" +  parentIndex + " ( " + binaryHeap.heap[parentIndex].F + " > " + binaryHeap.heap[i].F + " ) ");
				}

				if (binaryHeap.heap[parentIndex].sortKey > binaryHeap.heap[i].sortKey) {
					throw new System.Exception("Invalid state at " + i + ":" +  parentIndex + " ( " + binaryHeap.heap[parentIndex].F + " > " + binaryHeap.heap[i].F + " ) ");
				}

				if (nodes[binaryHeap.heap[i].pathNodeIndex].heapIndex != i) {
					throw new System.Exception("Invalid heap index");
				}
			}
		}

		/// <summary>
		/// Rebuilds the heap by trickeling down all items.
		/// Usually called after the hTarget on a path has been changed
		/// </summary>
		public void Rebuild (UnsafeSpan<PathNode> nodes) {
#if ASTARDEBUG
			int changes = 0;
#endif

			for (int i = 2; i < numberOfItems; i++) {
				int bubbleIndex = i;
				var node = heap[i];
				uint nodeF = node.F;
				while (bubbleIndex != 1) {
					int parentIndex = bubbleIndex / D;

					if (nodeF < heap[parentIndex].F) {
						heap[bubbleIndex] = heap[parentIndex];
						nodes[heap[bubbleIndex].pathNodeIndex].heapIndex = (ushort)bubbleIndex;

						heap[parentIndex] = node;
						nodes[heap[parentIndex].pathNodeIndex].heapIndex = (ushort)parentIndex;

						bubbleIndex = parentIndex;
#if ASTARDEBUG
						changes++;
#endif
					} else {
						break;
					}
				}
			}

#if ASTARDEBUG
			UnityEngine.Debug.Log("+++ Rebuilt Heap - "+changes+" changes +++");
#endif
		}
	}
}
