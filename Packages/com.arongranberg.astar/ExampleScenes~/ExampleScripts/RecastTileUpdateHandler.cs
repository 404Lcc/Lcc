using UnityEngine;

namespace Pathfinding {
	/// <summary>
	/// Helper for easier fast updates to recast graphs.
	///
	/// When updating recast graphs, you might just have plonked down a few
	/// GraphUpdateScene objects or issued a few GraphUpdateObjects to the
	/// system. This works fine if you are only issuing a few but the problem
	/// is that they don't have any coordination in between themselves. So if
	/// you have 10 GraphUpdateScene objects in one tile, they will all update
	/// that tile (10 times in total) instead of just updating it once which
	/// is all that is required (meaning it will be 10 times slower than just
	/// updating one tile). This script exists to help with updating only the
	/// tiles that need updating and only updating them once instead of
	/// multiple times.
	///
	/// It is coupled with the RecastTileUpdate component, which works a bit
	/// like the GraphUpdateScene component, just with fewer options. You can
	/// attach the RecastTileUpdate to any GameObject to have it schedule an
	/// update for the tile(s) that contain the GameObject. E.g if you are
	/// creating a new building somewhere, you can attach the RecastTileUpdate
	/// component to it to make it update the graph when it is instantiated.
	///
	/// If a single tile contains multiple RecastTileUpdate components and
	/// many try to update the graph at the same time, only one tile update
	/// will be done, which greatly improves performance.
	///
	/// If you have objects that are instantiated at roughly the same time
	/// but not exactly the same frame, you can use the maxThrottlingDelay
	/// field. It will delay updates up to that number of seconds to allow
	/// more updates to be batched together.
	///
	/// Note: You should only have one instance of this script in the scene
	/// if you only have a single recast graph. If you have more than one
	/// graph you can have more than one instance of this script but you need
	/// to manually call the SetGraph method to configure it with the correct
	/// graph.
	///
	/// Note: This does not use navmesh cutting. If you only ever add
	/// obstacles, but never add any new walkable surfaces then you might
	/// want to use navmesh cutting instead. See navmeshcutting (view in online documentation for working links).
	///
	/// Deprecated: Since version 5.0, this component is not necessary, since normal graph updates on recast graphs are now batched together if they update the same tiles.
	/// Use e.g. the <see cref="DynamicObstacle"/> component instead.
	/// </summary>
	[System.Obsolete("This component is no longer necessary. Normal graph updates on recast graphs are now batched together if they update the same tiles. Use the DynamicObstacle component instead")]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/recasttileupdatehandler.html")]
	public class RecastTileUpdateHandler : MonoBehaviour {
		/// <summary>Graph that handles the updates</summary>
		RecastGraph graph;

		/// <summary>True for a tile if it needs updating</summary>
		bool[] dirtyTiles;

		/// <summary>True if any elements in dirtyTiles are true</summary>
		bool anyDirtyTiles = false;

		/// <summary>Earliest update request we are handling right now</summary>
		float earliestDirty = float.NegativeInfinity;

		/// <summary>All tile updates will be performed within (roughly) this number of seconds</summary>
		public float maxThrottlingDelay = 0.5f;

		public void SetGraph (RecastGraph graph) {
			this.graph = graph;

			if (graph == null)
				return;

			dirtyTiles = new bool[graph.tileXCount*graph.tileZCount];
			anyDirtyTiles = false;
		}

		/// <summary>Requests an update to all tiles which touch the specified bounds</summary>
		public void ScheduleUpdate (Bounds bounds) {
			if (graph == null) {
				// If no graph has been set, use the first graph available
				if (AstarPath.active != null) {
					SetGraph(AstarPath.active.data.recastGraph);
				}

				if (graph == null) {
					Debug.LogError("Received tile update request (from RecastTileUpdate), but no RecastGraph could be found to handle it");
					return;
				}
			}

			// Make sure that tiles which do not strictly
			// contain this bounds object but which still
			// might need to be updated are actually updated
			int voxelCharacterRadius = Mathf.CeilToInt(graph.characterRadius/graph.cellSize);
			int borderSize = voxelCharacterRadius + 3;

			// Expand borderSize voxels on each side
			bounds.Expand(new Vector3(borderSize, 0, borderSize)*graph.cellSize*2);

			var touching = graph.GetTouchingTiles(bounds);

			if (touching.Width * touching.Height > 0) {
				if (!anyDirtyTiles) {
					earliestDirty = Time.time;
					anyDirtyTiles = true;
				}

				for (int z = touching.ymin; z <= touching.ymax; z++) {
					for (int x = touching.xmin; x <= touching.xmax; x++) {
						dirtyTiles[z*graph.tileXCount + x] = true;
					}
				}
			}
		}

		void OnEnable () {
			RecastTileUpdate.OnNeedUpdates += ScheduleUpdate;
		}

		void OnDisable () {
			RecastTileUpdate.OnNeedUpdates -= ScheduleUpdate;
		}

		void Update () {
			if (anyDirtyTiles && Time.time - earliestDirty >= maxThrottlingDelay && graph != null) {
				UpdateDirtyTiles();
			}
		}

		/// <summary>Update all dirty tiles now</summary>
		public void UpdateDirtyTiles () {
			if (graph == null) {
				new System.InvalidOperationException("No graph is set on this object");
			}

			if (graph.tileXCount * graph.tileZCount != dirtyTiles.Length) {
				Debug.LogError("Graph has changed dimensions. Clearing queued graph updates and resetting.");
				SetGraph(graph);
				return;
			}

			for (int z = 0; z < graph.tileZCount; z++) {
				for (int x = 0; x < graph.tileXCount; x++) {
					if (dirtyTiles[z*graph.tileXCount + x]) {
						dirtyTiles[z*graph.tileXCount + x] = false;

						var bounds = graph.GetTileBounds(x, z);

						// Shrink it a bit to make sure other tiles
						// are not included because of rounding errors
						bounds.extents *= 0.5f;

						var guo = new GraphUpdateObject(bounds);
						guo.nnConstraint.graphMask = 1 << (int)graph.graphIndex;

						AstarPath.active.UpdateGraphs(guo);
					}
				}
			}

			anyDirtyTiles = false;
		}
	}
}
