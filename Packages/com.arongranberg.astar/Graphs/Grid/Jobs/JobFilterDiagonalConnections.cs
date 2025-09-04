using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Pathfinding.Jobs;
using Pathfinding.Util;
using Pathfinding.Collections;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Filters out diagonal connections that are not allowed in layered grid graphs.
	///
	/// This is a IJobParallelForBatched job which is parallelelized over the z coordinate of the <see cref="slice"/>.
	///
	/// The <see cref="JobCalculateGridConnections"/> job will run first, and calculate the connections for all nodes.
	/// However, for layered grid graphs, the connections for diagonal nodes may be incorrect, and this
	/// post-processing pass is needed to validate the diagonal connections.
	/// </summary>
	[BurstCompile]
	public struct JobFilterDiagonalConnections : IJobParallelForBatched {
		public Slice3D slice;
		public NumNeighbours neighbours;
		public bool cutCorners;

		/// <summary>All bitpacked node connections</summary>
		public UnsafeSpan<ulong> nodeConnections;

		public bool allowBoundsChecks => false;

		public void Execute (int start, int count) {
			slice.AssertMatchesOuter(nodeConnections);

			// For single layer graphs this will have already been done in the JobCalculateGridConnections job
			// but for layered grid graphs we need to handle things differently because the data layout is different

			int3 size = slice.outerSize;
			NativeArray<int> neighbourOffsets = new NativeArray<int>(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

			for (int i = 0; i < 8; i++) neighbourOffsets[i] = GridGraph.neighbourZOffsets[i] * size.x + GridGraph.neighbourXOffsets[i];

			ulong hexagonConnectionMask = 0;
			for (int i = 0; i < GridGraph.hexagonNeighbourIndices.Length; i++) hexagonConnectionMask |= (ulong)LevelGridNode.ConnectionMask << (LevelGridNode.ConnectionStride*GridGraph.hexagonNeighbourIndices[i]);

			int adjacencyThreshold = cutCorners ? 1 : 2;
			int layerStride = size.x * size.z;
			start += slice.slice.min.z;
			for (int y = slice.slice.min.y; y < slice.slice.max.y; y++) {
				// The loop is parallelized over z coordinates
				for (int z = start; z < start + count; z++) {
					for (int x = slice.slice.min.x; x < slice.slice.max.x; x++) {
						int nodeIndexXZ = z * size.x + x;
						int nodeIndex = nodeIndexXZ + y * layerStride;

						switch (neighbours) {
						case NumNeighbours.Four:
							// Mask out all the diagonal connections
							nodeConnections[nodeIndex] = nodeConnections[nodeIndex] | LevelGridNode.DiagonalConnectionsMask;
							break;
						case NumNeighbours.Eight:
							var conns = nodeConnections[nodeIndex];

							// Skip node if no connections are enabled already
							if (conns == LevelGridNode.AllConnectionsMask) continue;

							// When cutCorners is enabled then the diagonal connection is allowed
							// if at least one axis aligned connection is adjacent to this diagonal.
							// Otherwise both axis aligned connections must be present.
							//
							//   X ----- axis2
							//   | \
							//   |   \
							//   |     \
							// axis1   diagonal
							//
							//         Z
							//         |
							//         |
							//
							//      6  2  5
							//       \ | /
							// --  3 - X - 1  ----- X
							//       / | \
							//      7  0  4
							//
							//         |
							//         |
							//
							for (int dir = 0; dir < 4; dir++) {
								int adjacent = 0;
								var axis1 = (conns >> dir*LevelGridNode.ConnectionStride) & LevelGridNode.ConnectionMask;
								var axis2 = (conns >> ((dir+1) % 4)*LevelGridNode.ConnectionStride) & LevelGridNode.ConnectionMask;
								var diagonal = (conns >> (dir+4)*LevelGridNode.ConnectionStride) & LevelGridNode.ConnectionMask;

								// Check if the diagonal connection is present at all.
								// The JobCalculateGridConnections calculated this.
								if (diagonal == LevelGridNode.NoConnection) continue;

								if (axis1 != LevelGridNode.NoConnection) {
									// We also check that the neighbour node is also connected to the diagonal node
									var neighbourDir = (dir + 1) % 4;
									var neighbourIndex = nodeIndexXZ + neighbourOffsets[dir] + (int)axis1 * layerStride;
									if (((nodeConnections[neighbourIndex] >> neighbourDir*LevelGridNode.ConnectionStride) & LevelGridNode.ConnectionMask) == diagonal) {
										adjacent++;
									}
								}
								if (axis2 != LevelGridNode.NoConnection) {
									var neighbourDir = dir;
									var neighbourIndex = nodeIndexXZ + neighbourOffsets[(dir+1)%4] + (int)axis2 * layerStride;
									if (((nodeConnections[neighbourIndex] >> neighbourDir*LevelGridNode.ConnectionStride) & LevelGridNode.ConnectionMask) == diagonal) {
										adjacent++;
									}
								}

								if (adjacent < adjacencyThreshold) conns |= (ulong)LevelGridNode.NoConnection << (dir + 4)*LevelGridNode.ConnectionStride;
							}
							nodeConnections[nodeIndex] = conns;
							break;
						case NumNeighbours.Six:
							// Hexagon layout
							// Note that for layered nodes NoConnection is all bits set (see LevelGridNode.NoConnection)
							// So in contrast to the non-layered grid graph we do a bitwise OR here
							nodeConnections[nodeIndex] = (nodeConnections[nodeIndex] | ~hexagonConnectionMask) & LevelGridNode.AllConnectionsMask;
							break;
						}
					}
				}
			}
		}
	}
}
