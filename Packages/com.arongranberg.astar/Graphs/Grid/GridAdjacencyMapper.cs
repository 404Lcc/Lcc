using Unity.Collections;

namespace Pathfinding.Graphs.Grid {
	public interface GridAdjacencyMapper {
		int LayerCount(IntBounds bounds);
		int GetNeighbourIndex(int nodeIndexXZ, int nodeIndex, int direction, NativeArray<ulong> nodeConnections, NativeArray<int> neighbourOffsets, int layerStride);
		bool HasConnection(int nodeIndex, int direction, NativeArray<ulong> nodeConnections);
	}

	public struct FlatGridAdjacencyMapper : GridAdjacencyMapper {
		public int LayerCount (IntBounds bounds) {
			UnityEngine.Assertions.Assert.IsTrue(bounds.size.y == 1);
			return 1;
		}
		public int GetNeighbourIndex (int nodeIndexXZ, int nodeIndex, int direction, NativeArray<ulong> nodeConnections, NativeArray<int> neighbourOffsets, int layerStride) {
			return nodeIndex + neighbourOffsets[direction];
		}
		public bool HasConnection (int nodeIndex, int direction, NativeArray<ulong> nodeConnections) {
			return ((nodeConnections[nodeIndex] >> direction) & 0x1) != 0;
		}
	}

	public struct LayeredGridAdjacencyMapper : GridAdjacencyMapper {
		public int LayerCount(IntBounds bounds) => bounds.size.y;
		public int GetNeighbourIndex (int nodeIndexXZ, int nodeIndex, int direction, NativeArray<ulong> nodeConnections, NativeArray<int> neighbourOffsets, int layerStride) {
			return nodeIndexXZ + neighbourOffsets[direction] + (int)((nodeConnections[nodeIndex] >> LevelGridNode.ConnectionStride*direction) & LevelGridNode.ConnectionMask) * layerStride;
		}
		public bool HasConnection (int nodeIndex, int direction, NativeArray<ulong> nodeConnections) {
			return ((nodeConnections[nodeIndex] >> LevelGridNode.ConnectionStride*direction) & LevelGridNode.ConnectionMask) != LevelGridNode.NoConnection;
		}
	}
}
