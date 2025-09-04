using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>
	/// Calculates node connections between triangles within each tile.
	/// Connections between tiles are handled at a later stage in <see cref="JobConnectTiles"/>.
	/// </summary>
	[BurstCompile]
	public struct JobCalculateTriangleConnections : IJob {
		[ReadOnly]
		public NativeArray<TileMesh.TileMeshUnsafe> tileMeshes;
		[WriteOnly]
		public NativeArray<TileNodeConnectionsUnsafe> nodeConnections;

		public struct TileNodeConnectionsUnsafe {
			/// <summary>Stream of packed connection edge infos (from <see cref="Connection.PackShapeEdgeInfo"/>)</summary>
			public Unity.Collections.LowLevel.Unsafe.UnsafeAppendBuffer neighbours;
			/// <summary>Number of neighbours for each triangle</summary>
			public Unity.Collections.LowLevel.Unsafe.UnsafeAppendBuffer neighbourCounts;
		}

		public void Execute () {
			Assert.AreEqual(tileMeshes.Length, nodeConnections.Length);

			var nodeRefs = new NativeParallelHashMap<int2, uint>(128, Allocator.Temp);
			bool duplicates = false;
			for (int ti = 0; ti < tileMeshes.Length; ti++) {
				nodeRefs.Clear();
				var tile = tileMeshes[ti];
				var numIndices = tile.triangles.Length;
				var neighbours = new Unity.Collections.LowLevel.Unsafe.UnsafeAppendBuffer(numIndices * 2 * 4, 4, Allocator.Persistent);
				var neighbourCounts = new Unity.Collections.LowLevel.Unsafe.UnsafeAppendBuffer(numIndices * 4, 4, Allocator.Persistent);
				const int TriangleIndexBits = 28;
				unsafe {
					Assert.IsTrue(numIndices % 3 == 0);
					// Access data via the raw pointer to avoid bounds checks
					var triangles = tile.triangles.ptr;
					for (int i = 0, j = 0; i < numIndices; i += 3, j++) {
						duplicates |= !nodeRefs.TryAdd(new int2(triangles[i+0], triangles[i+1]), (uint)j | (0 << TriangleIndexBits));
						duplicates |= !nodeRefs.TryAdd(new int2(triangles[i+1], triangles[i+2]), (uint)j | (1 << TriangleIndexBits));
						duplicates |= !nodeRefs.TryAdd(new int2(triangles[i+2], triangles[i+0]), (uint)j | (2 << TriangleIndexBits));
					}

					for (int i = 0; i < numIndices; i += 3) {
						var cnt = 0;
						for (int edge = 0; edge < 3; edge++) {
							if (nodeRefs.TryGetValue(new int2(triangles[i+((edge+1) % 3)], triangles[i+edge]), out var match)) {
								var other = match & ((1 << TriangleIndexBits) - 1);
								var otherEdge = (int)(match >> TriangleIndexBits);
								neighbours.Add(other);
								var edgeInfo = Connection.PackShapeEdgeInfo((byte)edge, (byte)otherEdge, true, true, true);
								neighbours.Add((int)edgeInfo);
								cnt += 1;
							}
						}
						neighbourCounts.Add(cnt);
					}
				}
				nodeConnections[ti] = new TileNodeConnectionsUnsafe {
					neighbours = neighbours,
					neighbourCounts = neighbourCounts,
				};
			}

			if (duplicates) {
				UnityEngine.Debug.LogWarning("Duplicate triangle edges were found in the input mesh. These have been removed. Are you sure your mesh is suitable for being used as a navmesh directly?\nThis could be caused by the mesh's normals not being consistent.");
			}
		}
	}
}
