using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Pathfinding.Jobs;
using UnityEngine.Assertions;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Writes node data from unmanaged arrays into managed <see cref="GridNodeBase"/> objects.
	///
	/// This is done after burst jobs have been working on graph data, as they cannot access the managed objects directly.
	///
	/// Earlier, data will have been either calculated from scratch, or read from the managed objects using the <see cref="JobReadNodeData"/> job.
	/// </summary>
	public struct JobWriteNodeData : IJobParallelForBatched {
		public System.Runtime.InteropServices.GCHandle nodesHandle;
		public uint graphIndex;

		/// <summary>(width, depth) of the array that the <see cref="nodesHandle"/> refers to</summary>
		public int3 nodeArrayBounds;
		public IntBounds dataBounds;
		public IntBounds writeMask;

		[ReadOnly]
		public NativeArray<Vector3> nodePositions;

		[ReadOnly]
		public NativeArray<uint> nodePenalties;

		[ReadOnly]
		public NativeArray<int> nodeTags;

		[ReadOnly]
		public NativeArray<ulong> nodeConnections;

		[ReadOnly]
		public NativeArray<bool> nodeWalkableWithErosion;

		[ReadOnly]
		public NativeArray<bool> nodeWalkable;

		public bool allowBoundsChecks => false;

		public void Execute (int startIndex, int count) {
			// This is a managed type, we need to trick Unity to allow this inside of a job
			var nodes = (GridNodeBase[])nodesHandle.Target;

			var relativeMask = writeMask.Offset(-dataBounds.min);

			// Determinstically convert the indices to rows. It is much easier to process a number of whole rows.
			var writeSize = writeMask.size;
			var zstart = startIndex / (writeSize.x*writeSize.y);
			var zend = (startIndex+count) / (writeSize.x*writeSize.y);

			Assert.IsTrue(zstart >= 0 && zstart <= writeSize.z);
			Assert.IsTrue(zend >= 0 && zend <= writeSize.z);
			relativeMask.min.z = writeMask.min.z + zstart - dataBounds.min.z;
			relativeMask.max.z = writeMask.min.z + zend - dataBounds.min.z;

			var dataSize = dataBounds.size;
			for (int y = relativeMask.min.y; y < relativeMask.max.y; y++) {
				for (int z = relativeMask.min.z; z < relativeMask.max.z; z++) {
					var rowOffset1 = (y*dataSize.z + z)*dataSize.x;
					var rowOffset2 = (z + dataBounds.min.z)*nodeArrayBounds.x + dataBounds.min.x;
					var rowOffset3 = (y + dataBounds.min.y)*nodeArrayBounds.z*nodeArrayBounds.x + rowOffset2;
					for (int x = relativeMask.min.x; x < relativeMask.max.x; x++) {
						int dataIndex = rowOffset1 + x;
						int nodeIndex = rowOffset3 + x;
						var node = nodes[nodeIndex];
						if (node != null) {
							node.GraphIndex = graphIndex;
							node.NodeInGridIndex = rowOffset2 + x;
							// TODO: Use UnsafeSpan
							node.position = (Int3)nodePositions[dataIndex];
							node.Penalty = nodePenalties[dataIndex];
							node.Tag = (uint)nodeTags[dataIndex];
							if (node is GridNode gridNode) {
								gridNode.SetAllConnectionInternal((int)nodeConnections[dataIndex]);
							} else if (node is LevelGridNode levelGridNode) {
								levelGridNode.LayerCoordinateInGrid = y + dataBounds.min.y;
								levelGridNode.SetAllConnectionInternal(nodeConnections[dataIndex]);
							}
							node.Walkable = nodeWalkableWithErosion[dataIndex];
							node.WalkableErosion = nodeWalkable[dataIndex];
						}
					}
				}
			}
		}
	}
}
