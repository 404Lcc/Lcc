using Pathfinding.Collections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Allocates and deallocates nodes in a grid graph.
	///
	/// This will inspect every cell in the dataBounds and allocate or deallocate the node depending on if that slot should have a node or not according to the nodeNormals array (pure zeroes means no node).
	///
	/// This is only used for incremental updates of grid graphs.
	/// The initial layer of the grid graph (which is always filled with nodes) is allocated in the <see cref="GridGraph.AllocateNodesJob"/> method.
	/// </summary>
	public struct JobAllocateNodes : IJob {
		public AstarPath active;
		[ReadOnly]
		public NativeArray<float4> nodeNormals;
		public IntBounds dataBounds;
		public int3 nodeArrayBounds;
		public GridNodeBase[] nodes;
		public System.Func<GridNodeBase> newGridNodeDelegate;

		public void Execute () {
			var size = dataBounds.size;

			// Start at y=1 because all nodes at y=0 are guaranteed to already be allocated (they are always allocated in a layered grid graph).
			var nodeNormalsSpan = nodeNormals.AsUnsafeReadOnlySpan();
			for (int y = 1; y < size.y; y++) {
				for (int z = 0; z < size.z; z++) {
					var rowOffset = ((y + dataBounds.min.y) * nodeArrayBounds.z + (z + dataBounds.min.z)) * nodeArrayBounds.x + dataBounds.min.x;
					for (int x = 0; x < size.x; x++) {
						var nodeIndex = rowOffset + x;
						var shouldHaveNode = math.any(nodeNormalsSpan[nodeIndex]);
						var node = nodes[nodeIndex];
						var hasNode = node != null;
						if (shouldHaveNode != hasNode) {
							if (shouldHaveNode) {
								node = nodes[nodeIndex] = newGridNodeDelegate();
								active.InitializeNode(node);
							} else {
								// Clear custom connections first and clear connections from other nodes to this one
								node.ClearCustomConnections(true);
								// Clear grid connections without clearing the connections from other nodes to this one (a bit slow)
								// Since this is inside a graph update we guarantee that the grid connections will be correct at the end
								// of the update anyway
								node.ResetConnectionsInternal();
								node.Destroy();
								nodes[nodeIndex] = null;
							}
						}
					}
				}
			}
		}
	}
}
