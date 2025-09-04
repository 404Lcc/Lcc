using Pathfinding.Graphs.Navmesh.Jobs;
using Pathfinding.Collections;

namespace Pathfinding.Graphs.Navmesh {
	/// <summary>Helper methods for scanning a recast graph</summary>
	public struct RecastBuilder {
		/// <summary>
		/// Builds meshes for the given tiles in a graph.
		/// Call Schedule on the returned object to actually start the job.
		///
		/// You may want to adjust the settings on the returned object before calling Schedule.
		///
		/// <code>
		/// // Scans the first 6x6 chunk of tiles of the recast graph (the IntRect uses inclusive coordinates)
		/// var graph = AstarPath.active.data.recastGraph;
		/// var buildSettings = RecastBuilder.BuildTileMeshes(graph, new TileLayout(graph), new IntRect(0, 0, 5, 5));
		/// var disposeArena = new Pathfinding.Jobs.DisposeArena();
		/// var promise = buildSettings.Schedule(disposeArena);
		///
		/// AstarPath.active.AddWorkItem(() => {
		///     // Block until the asynchronous job completes
		///     var result = promise.Complete();
		///     TileMeshes tiles = result.tileMeshes.ToManaged();
		///     // Take the scanned tiles and place them in the graph,
		///     // but not at their original location, but 2 tiles away, rotated 90 degrees.
		///     tiles.tileRect = tiles.tileRect.Offset(new Vector2Int(2, 0));
		///     tiles.Rotate(1);
		///     graph.ReplaceTiles(tiles);
		///
		///     // Dispose unmanaged data
		///     disposeArena.DisposeAll();
		///     result.Dispose();
		/// });
		/// </code>
		/// </summary>
		public static TileBuilder BuildTileMeshes (RecastGraph graph, TileLayout tileLayout, IntRect tileRect) {
			return new TileBuilder(graph, tileLayout, tileRect);
		}

		/// <summary>
		/// Builds nodes given some tile meshes.
		/// Call Schedule on the returned object to actually start the job.
		///
		/// See: <see cref="BuildTileMeshes"/>
		/// </summary>
		public static JobBuildNodes BuildNodeTiles (RecastGraph graph, TileLayout tileLayout) {
			return new JobBuildNodes(graph, tileLayout);
		}

		public static TileCutter CutTiles (NavmeshBase graph, GridLookup<NavmeshClipper> cuts, TileLayout tileLayout) {
			return new TileCutter(graph, cuts, tileLayout);
		}
	}
}
