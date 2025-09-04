using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Pathfinding.Jobs;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Reads node data from managed <see cref="GridNodeBase"/> objects into unmanaged arrays.
	///
	/// This is done so that burst jobs can later access this data directly.
	///
	/// Later, data will be written back to the managed objects using the <see cref="JobWriteNodeData"/> job.
	/// </summary>
	public struct JobReadNodeData : IJobParallelForBatched {
		public System.Runtime.InteropServices.GCHandle nodesHandle;
		public uint graphIndex;

		public Slice3D slice;

		[WriteOnly]
		public NativeArray<Vector3> nodePositions;

		[WriteOnly]
		public NativeArray<uint> nodePenalties;

		[WriteOnly]
		public NativeArray<int> nodeTags;

		[WriteOnly]
		public NativeArray<ulong> nodeConnections;

		[WriteOnly]
		public NativeArray<bool> nodeWalkableWithErosion;

		[WriteOnly]
		public NativeArray<bool> nodeWalkable;

		public bool allowBoundsChecks => false;

		struct Reader : GridIterationUtilities.ISliceAction {
			public GridNodeBase[] nodes;
			public NativeArray<Vector3> nodePositions;
			public NativeArray<uint> nodePenalties;
			public NativeArray<int> nodeTags;
			public NativeArray<ulong> nodeConnections;
			public NativeArray<bool> nodeWalkableWithErosion;
			public NativeArray<bool> nodeWalkable;

			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			public void Execute (uint outerIdx, uint innerIdx) {
				var dataIdx = (int)innerIdx;
				// The data bounds may have more layers than the existing nodes if a new layer is being added.
				// We can only copy from the nodes that exist.
				if (outerIdx < nodes.Length) {
					var node = nodes[outerIdx];
					if (node != null) {
						nodePositions[dataIdx] = (Vector3)node.position;
						nodePenalties[dataIdx] = node.Penalty;
						nodeTags[dataIdx] = (int)node.Tag;
						nodeConnections[dataIdx] = node is GridNode gn ? (ulong)gn.GetAllConnectionInternal() : (node as LevelGridNode).gridConnections;
						nodeWalkableWithErosion[dataIdx] = node.Walkable;
						nodeWalkable[dataIdx] = node.WalkableErosion;
						return;
					}
				}

				// Fallback in case the node was null (only happens for layered grid graphs),
				// or if we are adding more layers to the graph, in which case we are outside
				// the bounds of the nodes array.
				nodePositions[dataIdx] = Vector3.zero;
				nodePenalties[dataIdx] = 0;
				nodeTags[dataIdx] = 0;
				nodeConnections[dataIdx] = 0;
				nodeWalkableWithErosion[dataIdx] = false;
				nodeWalkable[dataIdx] = false;
			}
		}

		public void Execute (int startIndex, int count) {
			var reader = new Reader {
				// This is a managed type, we need to trick Unity to allow this inside of a job
				nodes = (GridNodeBase[])nodesHandle.Target,
				nodePositions = nodePositions,
				nodePenalties = nodePenalties,
				nodeTags = nodeTags,
				nodeConnections = nodeConnections,
				nodeWalkableWithErosion = nodeWalkableWithErosion,
				nodeWalkable = nodeWalkable
			};
			GridIterationUtilities.ForEachCellIn3DSlice(slice, ref reader);
		}
	}
}
