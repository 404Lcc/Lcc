using Pathfinding.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>
	/// Builds tiles optimized for pathfinding, from a list of <see cref="TileMesh.TileMeshUnsafe"/>.
	///
	/// This job takes the following steps:
	/// - Transform all vertices using the <see cref="graphToWorldSpace"/> matrix.
	/// - Remove duplicate vertices
	/// - If <see cref="recalculateNormals"/> is enabled: ensure all triangles are laid out in the clockwise direction.
	/// </summary>
	public struct JobCreateTiles : IJob {
		/// <summary>An array of <see cref="TileMesh.TileMeshUnsafe"/> of length tileRect.Width*tileRect.Height, or an uninitialized array</summary>
		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<TileMesh.TileMeshUnsafe> preCutTileMeshes;

		/// <summary>An array of <see cref="TileMesh.TileMeshUnsafe"/> of length tileRect.Width*tileRect.Height</summary>
		[ReadOnly]
		public NativeArray<TileMesh.TileMeshUnsafe> tileMeshes;

		/// <summary>
		/// An array of <see cref="NavmeshTile"/> of length tileRect.Width*tileRect.Height.
		/// This array will be filled with the created tiles.
		/// </summary>
		public System.Runtime.InteropServices.GCHandle tiles;

		/// <summary>Graph index of the graph that these nodes will be added to</summary>
		public uint graphIndex;

		/// <summary>
		/// Number of tiles in the graph.
		///
		/// This may be much bigger than the <see cref="tileRect"/> that we are actually processing.
		/// For example if a graph update is performed, the <see cref="tileRect"/> will just cover the tiles that are recalculated,
		/// while <see cref="graphTileCount"/> will contain all tiles in the graph.
		/// </summary>
		public Vector2Int graphTileCount;

		/// <summary>
		/// Rectangle of tiles that we are processing.
		///
		/// (xmax, ymax) must be smaller than graphTileCount.
		/// If for examples <see cref="graphTileCount"/> is (10, 10) and <see cref="tileRect"/> is {2, 3, 5, 6} then we are processing tiles (2, 3) to (5, 6) inclusive.
		/// </summary>
		public IntRect tileRect;

		/// <summary>Initial penalty for all nodes in the tile</summary>
		public uint initialPenalty;

		/// <summary>
		/// If true, all triangles will be guaranteed to be laid out in clockwise order.
		/// If false, their original order will be preserved.
		/// </summary>
		public bool recalculateNormals;

		/// <summary>Size of a tile in world units along the graph's X and Z axes</summary>
		public Vector2 tileWorldSize;

		/// <summary>Matrix to convert from graph space to world space</summary>
		public Matrix4x4 graphToWorldSpace;

		public void Execute () {
			var tiles = (NavmeshTile[])this.tiles.Target;
			Assert.AreEqual(tileMeshes.Length, tiles.Length);
			Assert.AreEqual(tileMeshes.Length, tileRect.Area);
			Assert.IsTrue(tileRect.xmax < graphTileCount.x);
			Assert.IsTrue(tileRect.ymax < graphTileCount.y);

			var tileRectWidth = tileRect.Width;
			var tileRectDepth = tileRect.Height;

			bool isUsingCuts = preCutTileMeshes.IsCreated;
			if (isUsingCuts) {
				Assert.AreEqual(preCutTileMeshes.Length, tiles.Length);
				Assert.AreEqual(preCutTileMeshes.Length, tileRect.Area);
			}

			for (int z = 0; z < tileRectDepth; z++) {
				for (int x = 0; x < tileRectWidth; x++) {
					var tileIndex = z*tileRectWidth + x;
					// If we are just updating a part of the graph we still want to assign the nodes the proper global tile index
					var graphTileIndex = (z + tileRect.ymin)*graphTileCount.x + (x + tileRect.xmin);
					var mesh = tileMeshes[tileIndex];

					// Convert tile space to graph space and world space
					var verticesInGraphSpace = mesh.verticesInTileSpace.Clone(Allocator.Persistent);
					var verticesInWorldSpace = verticesInGraphSpace.Clone(Allocator.Persistent);
					var tileSpaceToGraphSpaceOffset = (Int3) new Vector3(tileWorldSize.x * (x + tileRect.xmin), 0, tileWorldSize.y * (z + tileRect.ymin));
					for (int i = 0; i < verticesInGraphSpace.Length; i++) {
						var v = verticesInGraphSpace[i] + tileSpaceToGraphSpaceOffset;
						verticesInGraphSpace[i] = v;
						verticesInWorldSpace[i] = (Int3)graphToWorldSpace.MultiplyPoint3x4((Vector3)v);
					}

					// Create a new navmesh tile and assign its settings
					var triangles = mesh.triangles.Clone(Allocator.Persistent);
					var tile = new NavmeshTile {
						x = x + tileRect.xmin,
						z = z + tileRect.ymin,
						w = 1,
						d = 1,
						tris = triangles,
						vertsInGraphSpace = verticesInGraphSpace,
						verts = verticesInWorldSpace,
						bbTree = new BBTree(triangles, verticesInGraphSpace),
						nodes = new TriangleMeshNode[triangles.Length/3],
						// Leave empty for now, it will be filled in later
						graph = null,
						isCut = false,
					};

					if (isUsingCuts) {
						// Copy pre-cut data to be able to reference it when re-cutting the tile.
						// If no cuts are used, we don't save the pre-cut data, to reduce memory usage,
						// as it is identical to the post-cut data.
						// These arrays can be re-created from the other tile data, if needed.
						var preCutMesh = preCutTileMeshes[tileIndex];
						tile.preCutVertsInTileSpace = preCutMesh.verticesInTileSpace.Clone(Allocator.Persistent);
						tile.preCutTris = preCutMesh.triangles.Clone(Allocator.Persistent);
						tile.preCutTags = preCutMesh.tags.Clone(Allocator.Persistent);
						tile.isCut = true;
					}

					Profiler.BeginSample("CreateNodes");
					NavmeshBase.CreateNodes(tile, tile.tris, graphTileIndex, graphIndex, mesh.tags, false, null, initialPenalty, false);
					Profiler.EndSample();

					tiles[tileIndex] = tile;
				}
			}
		}
	}
}
