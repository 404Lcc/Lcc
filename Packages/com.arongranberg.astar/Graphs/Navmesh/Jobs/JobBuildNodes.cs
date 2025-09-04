using Pathfinding.Jobs;
using Pathfinding.Sync;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>
	/// Builds nodes and tiles and prepares them for pathfinding.
	///
	/// Takes input from a <see cref="TileBuilder"/> job and outputs a <see cref="BuildNodeTilesOutput"/>.
	///
	/// This job takes the following steps:
	/// - Calculate connections between nodes inside each tile
	/// - Create node and tile objects
	/// - Connect adjacent tiles together
	/// </summary>
	public struct JobBuildNodes {
		uint graphIndex;
		public uint initialPenalty;
		public bool recalculateNormals;
		public float maxTileConnectionEdgeDistance;
		Matrix4x4 graphToWorldSpace;
		TileLayout tileLayout;

		public class BuildNodeTilesOutput : IProgress, System.IDisposable {
			public TileBuilder.TileBuilderOutput progressSource;
			public NavmeshTile[] tiles;

			public float Progress => progressSource.Progress;

			public void Dispose () {
			}
		}

		internal JobBuildNodes(RecastGraph graph, TileLayout tileLayout) {
			this.tileLayout = tileLayout;
			this.graphIndex = graph.graphIndex;
			this.initialPenalty = graph.initialPenalty;
			this.recalculateNormals = graph.RecalculateNormals;
			this.maxTileConnectionEdgeDistance = graph.MaxTileConnectionEdgeDistance;
			this.graphToWorldSpace = tileLayout.transform.matrix;
		}

		public Promise<BuildNodeTilesOutput> Schedule (DisposeArena arena, Promise<TileBuilder.TileBuilderOutput> preCutDependency, Promise<TileCutter.TileCutterOutput> postCutDependency) {
			var postCutInput = postCutDependency.GetValue();
			var preCutInput = preCutDependency.GetValue();
			var tileRect = preCutInput.tileMeshes.tileRect;

			NativeArray<TileMesh.TileMeshUnsafe> finalTileMeshes;
			if (postCutInput.tileMeshes.tileMeshes.IsCreated) {
				UnityEngine.Assertions.Assert.AreEqual(postCutInput.tileMeshes.tileMeshes.Length, tileRect.Area);
				finalTileMeshes = postCutInput.tileMeshes.tileMeshes;
			} else {
				finalTileMeshes = preCutInput.tileMeshes.tileMeshes;
			}

			UnityEngine.Assertions.Assert.AreEqual(preCutInput.tileMeshes.tileMeshes.Length, tileRect.Area);
			var tiles = new NavmeshTile[tileRect.Area];
			var tilesGCHandle = System.Runtime.InteropServices.GCHandle.Alloc(tiles);
			var nodeConnections = new NativeArray<JobCalculateTriangleConnections.TileNodeConnectionsUnsafe>(tileRect.Area, Allocator.Persistent);

			var calculateConnectionsJob = new JobCalculateTriangleConnections {
				tileMeshes = finalTileMeshes,
				nodeConnections = nodeConnections,
			}.Schedule(postCutDependency.handle);

			var tileWorldSize = new Vector2(tileLayout.TileWorldSizeX, tileLayout.TileWorldSizeZ);
			var createTilesJob = new JobCreateTiles {
				// If any cutting is done, we need to save the pre-cut data to be able to re-cut tiles later
				preCutTileMeshes = postCutInput.tileMeshes.tileMeshes.IsCreated ? preCutInput.tileMeshes.tileMeshes : default,
				tileMeshes = finalTileMeshes,
				tiles = tilesGCHandle,
				tileRect = tileRect,
				graphTileCount = tileLayout.tileCount,
				graphIndex = graphIndex,
				initialPenalty = initialPenalty,
				recalculateNormals = recalculateNormals,
				graphToWorldSpace = this.graphToWorldSpace,
				tileWorldSize = tileWorldSize,
			}.Schedule(postCutDependency.handle);

			var applyConnectionsJob = new JobWriteNodeConnections {
				nodeConnections = nodeConnections,
				tiles = tilesGCHandle,
			}.Schedule(JobHandle.CombineDependencies(calculateConnectionsJob, createTilesJob));

			Profiler.BeginSample("Scheduling ConnectTiles");
			var connectTilesDependency = JobConnectTiles.ScheduleBatch(tilesGCHandle, applyConnectionsJob, tileRect, tileWorldSize, maxTileConnectionEdgeDistance);
			Profiler.EndSample();

			arena.Add(tilesGCHandle);
			arena.Add(nodeConnections);

			return new Promise<BuildNodeTilesOutput>(connectTilesDependency, new BuildNodeTilesOutput {
				progressSource = preCutInput,
				tiles = tiles,
			});
		}
	}
}
