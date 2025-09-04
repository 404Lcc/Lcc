using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine.Profiling;
using Pathfinding.Collections;

namespace Pathfinding {
	using Pathfinding.Util;
	using Pathfinding.Jobs;
	using UnityEngine.Assertions;

	internal class GlobalNodeStorage {
		readonly AstarPath astar;
		Unity.Jobs.JobHandle lastAllocationJob;

		/// <summary>
		/// Holds the next node index which has not been used by any previous node.
		/// See: <see cref="nodeIndexPools"/>
		/// </summary>
		public uint nextNodeIndex = 1;

		/// <summary>
		/// The number of nodes for which path node data has been reserved.
		/// Will be at least as high as <see cref="nextNodeIndex"/>
		/// </summary>
		public uint reservedPathNodeData = 0;

		/// <summary>Number of nodes that have been destroyed in total</summary>
		public uint destroyedNodesVersion { get; private set; }

		const int InitialTemporaryNodes = 256;

		int temporaryNodeCount = InitialTemporaryNodes;

		/// <summary>
		/// Holds indices for nodes that have been destroyed.
		/// To avoid trashing a lot of memory structures when nodes are
		/// frequently deleted and created, node indices are reused.
		///
		/// There's one pool for each possible number of node variants (1, 2 and 3).
		/// </summary>
		readonly IndexedStack<uint>[] nodeIndexPools = new [] {
			new IndexedStack<uint>(),
			new IndexedStack<uint>(),
			new IndexedStack<uint>(),
		};

		public PathfindingThreadData[] pathfindingThreadData = new PathfindingThreadData[0];

		/// <summary>Maps from NodeIndex to node</summary>
		GraphNode[] nodes = new GraphNode[0];

		public GlobalNodeStorage (AstarPath astar) {
			this.astar = astar;
		}

		public GraphNode GetNode(uint nodeIndex) => nodes[nodeIndex];

#if UNITY_EDITOR
		public struct DebugPathNode {
			public uint g;
			public uint h;
			public uint parentIndex;
			public ushort pathID;
			public byte fractionAlongEdge;
		}
#endif

		public struct PathfindingThreadData {
			public UnsafeSpan<PathNode> pathNodes;
#if UNITY_EDITOR
			public UnsafeSpan<DebugPathNode> debugPathNodes;
#endif
		}

		class IndexedStack<T> {
			T[] buffer = new T[4];

			public int Count { get; private set; }

			public void Push (T v) {
				if (Count == buffer.Length) {
					Util.Memory.Realloc(ref buffer, buffer.Length * 2);
				}

				buffer[Count] = v;
				Count++;
			}

			public void Clear () {
				Count = 0;
			}

			public T Pop () {
				Count--;
				return buffer[Count];
			}

			/// <summary>Pop the last N elements and store them in the buffer. The items will be in insertion order.</summary>
			public void PopMany (T[] resultBuffer, int popCount) {
				if (popCount > Count) throw new System.IndexOutOfRangeException();
				System.Array.Copy(buffer, Count - popCount, resultBuffer, 0, popCount);
				Count -= popCount;
			}
		}

		void DisposeThreadData () {
			if (pathfindingThreadData.Length > 0) {
				for (int i = 0; i < pathfindingThreadData.Length; i++) {
					unsafe {
						pathfindingThreadData[i].pathNodes.Free(Allocator.Persistent);
#if UNITY_EDITOR
						pathfindingThreadData[i].debugPathNodes.Free(Allocator.Persistent);
#endif
					}
				}
				pathfindingThreadData = new PathfindingThreadData[0];
			}
		}

		public void SetThreadCount (int threadCount) {
			if (pathfindingThreadData.Length != threadCount) {
				DisposeThreadData();
				pathfindingThreadData = new PathfindingThreadData[threadCount];

				for (int i = 0; i < pathfindingThreadData.Length; i++) {
					// Allocate per-thread data.
					// We allocate using UnsafeSpans because this code may run inside jobs, and Unity does not allow us to allocate NativeArray memory
					// using the persistent allocator inside jobs.
					pathfindingThreadData[i].pathNodes = new UnsafeSpan<PathNode>(Allocator.Persistent, (int)reservedPathNodeData + temporaryNodeCount);
#if UNITY_EDITOR
					pathfindingThreadData[i].debugPathNodes = new UnsafeSpan<DebugPathNode>(Allocator.Persistent, (int)reservedPathNodeData);
					pathfindingThreadData[i].debugPathNodes.FillZeros();
#endif
					var pnodes = pathfindingThreadData[i].pathNodes;
					pnodes.Fill(PathNode.Default);
				}
			}
		}

		/// <summary>
		/// Grows temporary node storage for the given thread.
		///
		/// This can happen if a path traverses a lot of off-mesh links, or if it is a multi-target path with a lot of targets.
		///
		/// If enough nodes are created that we have a to grow the regular node storage, then the number of temporary nodes will grow to the same value on all threads.
		/// </summary>
		public void GrowTemporaryNodeStorage (int threadID) {
			var threadData = pathfindingThreadData[threadID];
			int currentTempNodeCount = threadData.pathNodes.Length - (int)reservedPathNodeData;
			Assert.IsTrue(currentTempNodeCount >= 0 && currentTempNodeCount <= temporaryNodeCount);
			// We don't want to grow this often, since we will have to re-allocate the storage for *ALL* nodes,
			// not just the temporary nodes. So we use a high growth factor.
			temporaryNodeCount = System.Math.Max(temporaryNodeCount, currentTempNodeCount * 8);
			var prevLength = threadData.pathNodes.Length;
			threadData.pathNodes = threadData.pathNodes.Reallocate(Allocator.Persistent, (int)reservedPathNodeData + temporaryNodeCount);
			// Fill the new nodes with default values
			threadData.pathNodes.Slice(prevLength).Fill(PathNode.Default);
			pathfindingThreadData[threadID] = threadData;
		}

		/// <summary>
		/// Initializes temporary path data for a node.
		/// Warning: This method should not be called directly.
		///
		/// See: <see cref="AstarPath.InitializeNode"/>
		/// </summary>
		public void InitializeNode (GraphNode node) {
			var variants = node.PathNodeVariants;

			// Graphs may initialize nodes from different threads,
			// so we need to lock.
			// Luckily, uncontested locks are really really cheap in C#
			lock (this) {
				if (nodeIndexPools[variants-1].Count > 0) {
					node.NodeIndex = nodeIndexPools[variants-1].Pop();
				} else {
					// Highest node index in the new list of nodes
					node.NodeIndex = nextNodeIndex;
					nextNodeIndex += (uint)variants;
					ReserveNodeIndices(nextNodeIndex);
				}

				for (int i = 0; i < variants; i++) {
					nodes[node.NodeIndex + i] = node;
				}

				astar.hierarchicalGraph.OnCreatedNode(node);
			}
		}

		/// <summary>
		/// Reserves space for global node data.
		///
		/// Warning: Must be called only when a lock is held on this object.
		/// </summary>
		void ReserveNodeIndices (uint nextNodeIndex) {
			if (nextNodeIndex <= reservedPathNodeData) return;

			reservedPathNodeData = math.ceilpow2(nextNodeIndex);

			// Allocate more internal pathfinding data for the new nodes
			astar.hierarchicalGraph.ReserveNodeIndices(reservedPathNodeData);
			var threadCount = pathfindingThreadData.Length;
			DisposeThreadData();
			SetThreadCount(threadCount);
			Memory.Realloc(ref nodes, (int)reservedPathNodeData);
		}

		/// <summary>
		/// Destroyes the given node.
		/// This is to be called after the node has been disconnected from the graph so that it cannot be reached from any other nodes.
		/// It should only be called during graph updates, that is when the pathfinding threads are either not running or paused.
		///
		/// Warning: This method should not be called by user code. It is used internally by the system.
		/// </summary>
		public void DestroyNode (GraphNode node) {
			var nodeIndex = node.NodeIndex;
			if (nodeIndex == GraphNode.DestroyedNodeIndex) return;

			destroyedNodesVersion++;
			int variants = node.PathNodeVariants;
			nodeIndexPools[variants - 1].Push(nodeIndex);
			for (int i = 0; i < variants; i++) {
				nodes[nodeIndex + i] = null;
			}

			for (int t = 0; t < pathfindingThreadData.Length; t++) {
				var threadData = pathfindingThreadData[t];
				for (uint i = 0; i < variants; i++) {
					// This is not required for pathfinding, but not clearing it may confuse gizmo drawing for a fraction of a second.
					// Especially when 'Show Search Tree' is enabled
					threadData.pathNodes[nodeIndex + i].pathID = 0;
#if UNITY_EDITOR
					threadData.debugPathNodes[nodeIndex + i].pathID = 0;
#endif
				}
			}

			astar.hierarchicalGraph.OnDestroyedNode(node);
		}

		public void OnDisable () {
			lastAllocationJob.Complete();
			nextNodeIndex = 1;
			reservedPathNodeData = 0;
			for (int i = 0; i < nodeIndexPools.Length; i++) nodeIndexPools[i].Clear();
			nodes = new GraphNode[0];
			DisposeThreadData();
		}

		struct JobAllocateNodes<T> : IJob where T : GraphNode {
			public T[] result;
			public int count;
			public GlobalNodeStorage nodeStorage;
			public uint variantsPerNode;
			public System.Func<T> createNode;

			public bool allowBoundsChecks => false;

			public void Execute () {
				Profiler.BeginSample("Allocating nodes");
				var hierarchicalGraph = nodeStorage.astar.hierarchicalGraph;

				lock (nodeStorage) {
					var pool = nodeStorage.nodeIndexPools[variantsPerNode-1];
					uint nextNodeIndex = nodeStorage.nextNodeIndex;

					// Allocate the actual nodes
					for (uint i = 0; i < count; i++) {
						var node = result[i] = createNode();

						// Get a new node index. Re-use one from a previously destroyed node if possible
						if (pool.Count > 0) {
							node.NodeIndex = pool.Pop();
						} else {
							node.NodeIndex = nextNodeIndex;
							nextNodeIndex += variantsPerNode;
						}
					}

					// Allocate more internal pathfinding data for the new nodes
					nodeStorage.ReserveNodeIndices(nextNodeIndex);

					// Mark the node indices as used
					nodeStorage.nextNodeIndex = nextNodeIndex;

					for (int i = 0; i < count; i++) {
						var node = result[i];
						hierarchicalGraph.AddDirtyNode(node);
						nodeStorage.nodes[node.NodeIndex] = node;
					}
				}
				Profiler.EndSample();
			}
		}

		public Unity.Jobs.JobHandle  AllocateNodesJob<T>(T[] result, int count, System.Func<T> createNode, uint variantsPerNode) where T : GraphNode {
			// Get all node indices that we are going to recycle and store them in a new buffer.
			// It's best to store them in a new buffer to avoid multithreading issues.
			UnityEngine.Assertions.Assert.IsTrue(variantsPerNode > 0 && variantsPerNode <= 3);

			// It may be tempting to use a parallel job for this
			// but it seems like allocation (new) in C# uses some kind of locking.
			// Therefore it is not faster (it may even be slower) to try to allocate the nodes in multiple threads in parallel.
			// The job will use locking internally for safety, but it's still nice to set appropriate dependencies, to avoid lots of worker threads
			// just stalling because they are waiting for a lock, in case this method is called multiple times in parallel.
			lastAllocationJob = new JobAllocateNodes<T> {
				result = result,
				count = count,
				nodeStorage = this,
				variantsPerNode = variantsPerNode,
				createNode = createNode,
			}.ScheduleManaged(lastAllocationJob);

			return lastAllocationJob;
		}
	}
}
