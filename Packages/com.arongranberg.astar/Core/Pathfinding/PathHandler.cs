using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Pathfinding {
	using Pathfinding.Collections;
	using Unity.Profiling;

	/// <summary>
	/// NNConstraint which also takes an <see cref="ITraversalProvider"/> into account.
	///
	/// Paths will automatically use this if an ITraversalProvider is set on the path.
	/// </summary>
	public class NNConstraintWithTraversalProvider : NNConstraint {
		public ITraversalProvider traversalProvider;
		public NNConstraint baseConstraint;
		public Path path;

		public void Reset () {
			traversalProvider = null;
			baseConstraint = null;
			path = null;
		}

		public bool isSet => traversalProvider != null;

		public void Set (Path path, NNConstraint constraint, ITraversalProvider traversalProvider) {
			this.path = path;
			this.traversalProvider = traversalProvider;
			// Note: We need to pass most requests to the base constraint, because it may be a subclass of NNConstraint and have additional logic
			baseConstraint = constraint;
			// We also need to copy some fields to this instance, because
			// some fields are used directly. Primarily distanceMetric and constrainDistance,
			// but we copy all of them for good measure.
			this.graphMask = constraint.graphMask;
			this.constrainArea = constraint.constrainArea;
			this.area = constraint.area;
			this.distanceMetric = constraint.distanceMetric;
			this.constrainWalkability = constraint.constrainWalkability;
			this.walkable = constraint.walkable;
			this.constrainTags = constraint.constrainTags;
			this.tags = constraint.tags;
			this.constrainDistance = constraint.constrainDistance;
		}

		public override bool SuitableGraph (int graphIndex, NavGraph graph) {
			return baseConstraint.SuitableGraph(graphIndex, graph);
		}

		public override bool Suitable (GraphNode node) {
			return baseConstraint.Suitable(node) && traversalProvider.CanTraverse(path, node);
		}
	}

	/// <summary>
	/// Stores temporary node data for a single pathfinding request.
	/// Every node has one PathNode per thread used.
	/// It stores e.g G score, H score and other temporary variables needed
	/// for path calculation, but which are not part of the graph structure.
	///
	/// See: Pathfinding.PathHandler
	/// See: https://en.wikipedia.org/wiki/A*_search_algorithm
	/// </summary>
	public struct PathNode {
		/// <summary>The path request (in this thread, if multithreading is used) which last used this node</summary>
		public ushort pathID;

		/// <summary>
		/// Index of the node in the binary heap.
		/// The open list in the A* algorithm is backed by a binary heap.
		/// To support fast 'decrease key' operations, the index of the node
		/// is saved here.
		/// </summary>
		public ushort heapIndex;

		/// <summary>Bitpacked variable which stores several fields</summary>
		private uint flags;

		public static readonly PathNode Default = new PathNode { pathID = 0, heapIndex = BinaryHeap.NotInHeap, flags = 0 };

		/// <summary>Parent index uses the first 26 bits</summary>
		private const uint ParentIndexMask = (1U << 26) - 1U;

		private const int FractionAlongEdgeOffset = 26;
		private const uint FractionAlongEdgeMask = ((1U << 30) - 1U) & ~ParentIndexMask;
		public const int FractionAlongEdgeQuantization = 1 << (30 - 26);

		public static uint ReverseFractionAlongEdge(uint v) => (FractionAlongEdgeQuantization - 1) - v;

		public static uint QuantizeFractionAlongEdge (float v) {
			v *= FractionAlongEdgeQuantization - 1;
			v += 0.5f;
			return Unity.Mathematics.math.clamp((uint)v, 0, FractionAlongEdgeQuantization - 1);
		}

		public static float UnQuantizeFractionAlongEdge (uint v) {
			return (float)v * (1.0f / (FractionAlongEdgeQuantization - 1));
		}

		/// <summary>Flag 1 is at bit 30</summary>
		private const int Flag1Offset = 30;
		private const uint Flag1Mask = 1U << Flag1Offset;

		/// <summary>Flag 2 is at bit 31</summary>
		private const int Flag2Offset = 31;
		private const uint Flag2Mask = 1U << Flag2Offset;

		public uint fractionAlongEdge {
			get => (flags & FractionAlongEdgeMask) >> FractionAlongEdgeOffset;
			set => flags = (flags & ~FractionAlongEdgeMask) | ((value << FractionAlongEdgeOffset) & FractionAlongEdgeMask);
		}

		public uint parentIndex {
			get => flags & ParentIndexMask;
			set => flags = (flags & ~ParentIndexMask) | value;
		}

		/// <summary>
		/// Use as temporary flag during pathfinding.
		/// Path types can use this during pathfinding to mark
		/// nodes. When done, this flag should be reverted to its default state (false) to
		/// avoid messing up other pathfinding requests.
		/// </summary>
		public bool flag1 {
			get => (flags & Flag1Mask) != 0;
			set => flags = (flags & ~Flag1Mask) | (value ? Flag1Mask : 0U);
		}

		/// <summary>
		/// Use as temporary flag during pathfinding.
		/// Path types can use this during pathfinding to mark
		/// nodes. When done, this flag should be reverted to its default state (false) to
		/// avoid messing up other pathfinding requests.
		/// </summary>
		public bool flag2 {
			get => (flags & Flag2Mask) != 0;
			set => flags = (flags & ~Flag2Mask) | (value ? Flag2Mask : 0U);
		}
	}

	public enum TemporaryNodeType {
		Start,
		End,
		Ignore,
	}

	public struct TemporaryNode {
		public uint associatedNode;
		public Int3 position;
		public int targetIndex;
		public TemporaryNodeType type;
	}

	/// <summary>Handles thread specific path data.</summary>
	public class PathHandler {
		/// <summary>
		/// Current PathID.
		/// See: <see cref="PathID"/>
		/// </summary>
		private ushort pathID;

		public readonly int threadID;
		public readonly int totalThreadCount;
		public readonly NNConstraintWithTraversalProvider constraintWrapper = new NNConstraintWithTraversalProvider();
		internal readonly GlobalNodeStorage nodeStorage;
		public int numTemporaryNodes { [IgnoredByDeepProfiler] get; private set; }

		/// <summary>
		/// All path nodes with an index greater or equal to this are temporary nodes that only exist for the duration of a single path.
		///
		/// This is a copy of NodeStorage.nextNodeIndex. This is used to avoid having to access the NodeStorage while pathfinding as it's an extra indirection.
		/// </summary>
		public uint temporaryNodeStartIndex { [IgnoredByDeepProfiler] get; private set; }
		UnsafeSpan<TemporaryNode> temporaryNodes;

		/// <summary>
		/// Reference to the per-node data for this thread.
		///
		/// Note: Only guaranteed to point to a valid allocation while the path is being calculated.
		///
		/// Be careful when storing copies of this array, as it may be re-allocated by the AddTemporaryNode method.
		/// </summary>
		public UnsafeSpan<PathNode> pathNodes;
#if UNITY_EDITOR
		UnsafeSpan<GlobalNodeStorage.DebugPathNode> debugPathNodes;
#endif

		/// <summary>
		/// Binary heap to keep track of nodes on the "Open list".
		/// See: https://en.wikipedia.org/wiki/A*_search_algorithm
		/// </summary>
		public BinaryHeap heap = new BinaryHeap(128);

		/// <summary>ID for the path currently being calculated or last path that was calculated</summary>
		public ushort PathID { get { return pathID; } }

		/// <summary>
		/// StringBuilder that paths can use to build debug strings.
		/// Better for performance and memory usage to use a single StringBuilder instead of each path creating its own
		/// </summary>
		public readonly System.Text.StringBuilder DebugStringBuilder = new System.Text.StringBuilder();

		internal PathHandler (GlobalNodeStorage nodeStorage, int threadID, int totalThreadCount) {
			this.threadID = threadID;
			this.totalThreadCount = totalThreadCount;
			this.nodeStorage = nodeStorage;
			temporaryNodes = default;
		}

		public void InitializeForPath (Path p) {
			var lastPathId = pathID;
			pathID = p.pathID;
			numTemporaryNodes = 0;
			// Get the path nodes for this thread (may have been resized since last we calculated a path)
			pathNodes = nodeStorage.pathfindingThreadData[threadID].pathNodes;

			// The index at which temporary nodes start. The GlobalNodeStorage allocates some extra path nodes
			// for use as temporary nodes. This is usually a small number, as they are only needed for the start/end points,
			// and for off-mesh links (in some cases).
			// We could hypothetically start already at nodeStorage.nextNodeIndex, but this could lead to us
			// allocating an unnecessarily large array for the #temporaryNodes array, which would be a waste of memory.
			temporaryNodeStartIndex = nodeStorage.reservedPathNodeData;
			// The number of temporary nodes we are allowed to use during this path calculation.
			// Note: This value will never shrink between path calculations, because the GlobalNodeStorage will never
			// reduce the number of temporary nodes it reserves.
			var tempNodeCount = pathNodes.Length - (int)temporaryNodeStartIndex;
			if (tempNodeCount > temporaryNodes.Length) {
				temporaryNodes = temporaryNodes.Reallocate(Allocator.Persistent, tempNodeCount);
			}

#if UNITY_EDITOR
			var astar = AstarPath.active;
			var shouldLog = astar.showGraphs && (astar.debugMode == GraphDebugMode.F || astar.debugMode == GraphDebugMode.H || astar.debugMode == GraphDebugMode.G || astar.showSearchTree);
			debugPathNodes = shouldLog ? nodeStorage.pathfindingThreadData[threadID].debugPathNodes : default;
#endif

			// Path IDs have overflowed 65K, cleanup is needed to avoid bugs where we think
			// we have already visited a node when we haven't.
			// Since pathIDs are handed out sequentially, we can check if the new path id
			// is smaller than the last one.
			if (pathID < lastPathId) {
				ClearPathIDs();
			}
		}

		/// <summary>
		/// Returns the PathNode corresponding to the specified node.
		/// The PathNode is specific to this PathHandler since multiple PathHandlers
		/// are used at the same time if multithreading is enabled.
		/// </summary>
		public PathNode GetPathNode (GraphNode node, uint variant = 0) {
			return pathNodes[node.NodeIndex + variant];
		}

		public bool IsTemporaryNode(uint pathNodeIndex) => pathNodeIndex >= temporaryNodeStartIndex;

		/// <summary>
		/// Add a new temporary node for this path request.
		///
		/// Warning: This may invalidate all memory references to path nodes in this path.
		/// </summary>
		public uint AddTemporaryNode (TemporaryNode node) {
			if (numTemporaryNodes >= temporaryNodes.Length) {
				// Reallocate the node storage to fit more temporary nodes
				// This will invalidate all references to path nodes and temporary nodes,
				// so we must ensure that no `ref` variables life across this call.
				nodeStorage.GrowTemporaryNodeStorage(threadID);
				pathNodes = nodeStorage.pathfindingThreadData[threadID].pathNodes;
				temporaryNodes = temporaryNodes.Reallocate(Allocator.Persistent, pathNodes.Length - (int)temporaryNodeStartIndex);
			}

			var index = temporaryNodeStartIndex + (uint)numTemporaryNodes;
			temporaryNodes[numTemporaryNodes] = node;
			pathNodes[index] = PathNode.Default;
			numTemporaryNodes++;
			return index;
		}

		public GraphNode GetNode(uint nodeIndex) => nodeStorage.GetNode(nodeIndex);

		public ref TemporaryNode GetTemporaryNode (uint nodeIndex) {
			if (nodeIndex < temporaryNodeStartIndex || nodeIndex >= temporaryNodeStartIndex + numTemporaryNodes)
				throw new System.ArgumentOutOfRangeException();
			return ref temporaryNodes[(int)(nodeIndex - temporaryNodeStartIndex)];
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public void LogVisitedNode (uint pathNodeIndex, uint h, uint g) {
#if UNITY_EDITOR
			if (debugPathNodes.Length > 0 && !IsTemporaryNode(pathNodeIndex)) {
				var parent = pathNodes[pathNodeIndex].parentIndex;
				debugPathNodes[pathNodeIndex] = new GlobalNodeStorage.DebugPathNode {
					h = h,
					g = g,
					parentIndex = parent >= temporaryNodeStartIndex ? 0 : parent,
					pathID = pathID,
					fractionAlongEdge = (byte)pathNodes[pathNodeIndex].fractionAlongEdge,
				};
			}
#endif
		}

		/// <summary>
		/// Set all nodes' pathIDs to 0.
		/// See: Pathfinding.PathNode.pathID
		/// </summary>
		public void ClearPathIDs () {
			for (int i = 0; i < pathNodes.Length; i++) {
				pathNodes[i].pathID = 0;
			}
		}

		public void Dispose () {
			heap.Dispose();
			temporaryNodes.Free(Allocator.Persistent);
			pathNodes = default;
		}
	}
}
