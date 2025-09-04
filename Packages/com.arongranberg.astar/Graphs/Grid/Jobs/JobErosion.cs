using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Pathfinding.Jobs;
using UnityEngine.Assertions;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Calculates erosion.
	/// Note that to ensure that connections are completely up to date after updating a node you
	/// have to calculate the connections for both the changed node and its neighbours.
	///
	/// In a layered grid graph, this will recalculate the connections for all nodes
	/// in the (x,z) cell (it may have multiple layers of nodes).
	///
	/// See: CalculateConnections(GridNodeBase)
	/// </summary>
	[BurstCompile]
	public struct JobErosion<AdjacencyMapper> : IJob where AdjacencyMapper : GridAdjacencyMapper, new() {
		public IntBounds bounds;
		public IntBounds writeMask;
		public NumNeighbours neighbours;
		public int erosion;
		public bool erosionUsesTags;
		public int erosionStartTag;

		[ReadOnly]
		public NativeArray<ulong> nodeConnections;

		[ReadOnly]
		public NativeArray<bool> nodeWalkable;

		[WriteOnly]
		public NativeArray<bool> outNodeWalkable;

		public NativeArray<int> nodeTags;
		public int erosionTagsPrecedenceMask;

		// Note: the first 3 connections are to nodes with a higher x or z coordinate
		// The last 3 connections are to nodes with a lower x or z coordinate
		// This is required for the grassfire transform to work properly
		// This is a permutation of GridGraph.hexagonNeighbourIndices
		static readonly int[] hexagonNeighbourIndices = { 1, 2, 5, 0, 3, 7 };

		public void Execute () {
			var slice = new Slice3D(bounds, bounds);
			var size = slice.slice.size;
			slice.AssertMatchesOuter(nodeConnections);
			slice.AssertMatchesOuter(nodeWalkable);
			slice.AssertMatchesOuter(outNodeWalkable);
			slice.AssertMatchesOuter(nodeTags);
			Assert.IsTrue(bounds.Contains(writeMask));

			var(outerStrideX, outerStrideY, outerStrideZ) = slice.outerStrides;
			var(innerStrideX, innerStrideY, innerStrideZ) = slice.innerStrides;
			NativeArray<int> neighbourOffsets = new NativeArray<int>(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < 8; i++) neighbourOffsets[i] = GridGraph.neighbourZOffsets[i] * innerStrideZ + GridGraph.neighbourXOffsets[i] * innerStrideX;

			var erosionDistances = new NativeArray<int>(slice.length, Allocator.Temp, NativeArrayOptions.ClearMemory);
			var adjacencyMapper = new AdjacencyMapper();
			var layers = adjacencyMapper.LayerCount(slice.slice);
			var outerOffset = slice.outerStartIndex;
			if (neighbours == NumNeighbours.Six) {
				// Use the grassfire transform: https://en.wikipedia.org/wiki/Grassfire_transform extended to hexagonal graphs
				for (int z = 1; z < size.z - 1; z++) {
					for (int x = 1; x < size.x - 1; x++) {
						for (int y = 0; y < layers; y++) {
							// Note: This is significantly faster than using csum, because burst can optimize it better
							int outerIndex = z * outerStrideZ + x * outerStrideX + y * outerStrideY + outerOffset;
							var innerIndexXZ = z * innerStrideZ + x * innerStrideX;
							int innerIndex = innerIndexXZ + y * innerStrideY;
							int v = int.MaxValue;
							for (int i = 3; i < 6; i++) {
								int connection = hexagonNeighbourIndices[i];
								if (!adjacencyMapper.HasConnection(outerIndex, connection, nodeConnections)) v = -1;
								else v = math.min(v, erosionDistances[adjacencyMapper.GetNeighbourIndex(innerIndexXZ, innerIndex, connection, nodeConnections, neighbourOffsets, innerStrideY)]);
							}

							erosionDistances[innerIndex] = v + 1;
						}
					}
				}

				for (int z = size.z - 2; z > 0; z--) {
					for (int x = size.x - 2; x > 0; x--) {
						for (int y = 0; y < layers; y++) {
							int outerIndex = z * outerStrideZ + x * outerStrideX + y * outerStrideY + outerOffset;
							var innerIndexXZ = z * innerStrideZ + x * innerStrideX;
							int innerIndex = innerIndexXZ + y * innerStrideY;
							int v = int.MaxValue;
							for (int i = 3; i < 6; i++) {
								int connection = hexagonNeighbourIndices[i];
								if (!adjacencyMapper.HasConnection(outerIndex, connection, nodeConnections)) v = -1;
								else v = math.min(v, erosionDistances[adjacencyMapper.GetNeighbourIndex(innerIndexXZ, innerIndex, connection, nodeConnections, neighbourOffsets, innerStrideY)]);
							}

							erosionDistances[innerIndex] = math.min(erosionDistances[innerIndex], v + 1);
						}
					}
				}
			} else {
				/* Index offset to get neighbour nodes. Added to a node's index to get a neighbour node index.
				 *
				 * \code
				 *         Z
				 *         |
				 *         |
				 *
				 *      6  2  5
				 *       \ | /
				 * --  3 - X - 1  ----- X
				 *       / | \
				 *      7  0  4
				 *
				 *         |
				 *         |
				 * \endcode
				 */
				const int DirectionDown = 0;
				const int DirectionRight = 1;
				const int DirectionUp = 2;
				const int DirectionLeft = 3;

				// Use the grassfire transform: https://en.wikipedia.org/wiki/Grassfire_transform
				for (int z = 1; z < size.z - 1; z++) {
					for (int x = 1; x < size.x - 1; x++) {
						for (int y = 0; y < layers; y++) {
							int outerIndex = z * outerStrideZ + x * outerStrideX + y * outerStrideY + outerOffset;
							var innerIndexXZ = z * innerStrideZ + x * innerStrideX;
							int innerIndex = innerIndexXZ + y * innerStrideY;
							var v1 = -1;
							if (adjacencyMapper.HasConnection(outerIndex, DirectionDown, nodeConnections)) v1 = erosionDistances[adjacencyMapper.GetNeighbourIndex(innerIndexXZ, innerIndex, DirectionDown, nodeConnections, neighbourOffsets, innerStrideY)];
							var v2 = -1;
							if (adjacencyMapper.HasConnection(outerIndex, DirectionLeft, nodeConnections)) v2 = erosionDistances[adjacencyMapper.GetNeighbourIndex(innerIndexXZ, innerIndex, DirectionLeft, nodeConnections, neighbourOffsets, innerStrideY)];

							erosionDistances[innerIndex] = math.min(v1, v2) + 1;
						}
					}
				}

				for (int z = size.z - 2; z > 0; z--) {
					for (int x = size.x - 2; x > 0; x--) {
						for (int y = 0; y < layers; y++) {
							int outerIndex = z * outerStrideZ + x * outerStrideX + y * outerStrideY + outerOffset;
							var innerIndexXZ = z * innerStrideZ + x * innerStrideX;
							int innerIndex = innerIndexXZ + y * innerStrideY;
							var v1 = -1;
							if (adjacencyMapper.HasConnection(outerIndex, DirectionUp, nodeConnections)) v1 = erosionDistances[adjacencyMapper.GetNeighbourIndex(innerIndexXZ, innerIndex, DirectionUp, nodeConnections, neighbourOffsets, innerStrideY)];
							var v2 = -1;
							if (adjacencyMapper.HasConnection(outerIndex, DirectionRight, nodeConnections)) v2 = erosionDistances[adjacencyMapper.GetNeighbourIndex(innerIndexXZ, innerIndex, DirectionRight, nodeConnections, neighbourOffsets, innerStrideY)];

							erosionDistances[innerIndex] = math.min(erosionDistances[outerIndex], math.min(v1, v2) + 1);
						}
					}
				}
			}

			var relativeWriteMask = writeMask.Offset(-bounds.min);

			// Erosion tags are allowed to overwrite the ones the user specifies, as well as the ones that are already reserved for erosion.
			for (int i = erosionStartTag; i < erosionStartTag + erosion; i++) erosionTagsPrecedenceMask |= 1 << i;

			for (int y = relativeWriteMask.min.y; y < relativeWriteMask.max.y; y++) {
				for (int z = relativeWriteMask.min.z; z < relativeWriteMask.max.z; z++) {
					for (int x = relativeWriteMask.min.x; x < relativeWriteMask.max.x; x++) {
						int outerIndex = x * outerStrideX + y * outerStrideY + z * outerStrideZ + outerOffset;
						int innerIndex = x * innerStrideX + y * innerStrideY + z * innerStrideZ;
						if (erosionUsesTags) {
							var prevTag = nodeTags[outerIndex];
							outNodeWalkable[outerIndex] = nodeWalkable[outerIndex];

							if (erosionDistances[innerIndex] < erosion) {
								if (((erosionTagsPrecedenceMask >> prevTag) & 0x1) != 0) {
									nodeTags[outerIndex] = nodeWalkable[outerIndex] ? math.min(GraphNode.MaxTagIndex, erosionDistances[innerIndex] + erosionStartTag) : 0;
								}
							} else if (prevTag >= erosionStartTag && prevTag < erosionStartTag + erosion) {
								// If the node already had a tag that was reserved for erosion, but it shouldn't have that tag, then we remove it.
								nodeTags[outerIndex] = 0;
							}
						} else {
							outNodeWalkable[outerIndex] = nodeWalkable[outerIndex] & (erosionDistances[innerIndex] >= erosion);
						}
					}
				}
			}
		}
	}
}
