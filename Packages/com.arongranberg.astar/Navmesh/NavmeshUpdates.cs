using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using Pathfinding.Collections;

namespace Pathfinding.Graphs.Navmesh {
	/// <summary>
	/// Helper for navmesh cut objects.
	/// Responsible for keeping track of which navmesh cuts have moved and coordinating graph updates to account for those changes.
	///
	/// See: navmeshcutting (view in online documentation for working links)
	/// See: <see cref="AstarPath.navmeshUpdates"/>
	/// See: <see cref="NavmeshBase.enableNavmeshCutting"/>
	/// </summary>
	[System.Serializable]
	public class NavmeshUpdates {
		/// <summary>
		/// How often to check if an update needs to be done (real seconds between checks).
		/// For worlds with a very large number of NavmeshCut objects, it might be bad for performance to do this check every frame.
		/// If you think this is a performance penalty, increase this number to check less often.
		///
		/// For almost all games, this can be kept at 0.
		///
		/// If negative, no updates will be done. They must be manually triggered using <see cref="ForceUpdate"/>.
		///
		/// <code>
		/// // Check every frame (the default)
		/// AstarPath.active.navmeshUpdates.updateInterval = 0;
		///
		/// // Check every 0.1 seconds
		/// AstarPath.active.navmeshUpdates.updateInterval = 0.1f;
		///
		/// // Never check for changes
		/// AstarPath.active.navmeshUpdates.updateInterval = -1;
		/// // You will have to schedule updates manually using
		/// AstarPath.active.navmeshUpdates.ForceUpdate();
		/// </code>
		///
		/// You can also find this in the AstarPath inspector under Settings.
		/// [Open online documentation to see images]
		/// </summary>
		public float updateInterval;
		internal AstarPath astar;
		List<NavmeshUpdateSettings> listeners = new List<NavmeshUpdateSettings>();

		/// <summary>Last time navmesh cuts were applied</summary>
		float lastUpdateTime = float.NegativeInfinity;

		/// <summary>Stores navmesh cutting related data for a single graph</summary>
		// When enabled the following invariant holds:
		// - This class should be listening for updates to the NavmeshCut.allEnabled list
		// - The clipperLookup should be non-null
		// - The tileLayout should be valid
		// - The dirtyTiles array should be valid
		//
		// When disabled the following invariant holds:
		// - This class is not listening for updates to the NavmeshCut.allEnabled list
		// - The clipperLookup should be null
		// - The dirtyTiles array should be disposed
		// - dirtyTileCoordinates should be empty
		//
		public class NavmeshUpdateSettings : System.IDisposable {
			internal readonly NavmeshBase graph;
			public GridLookup<NavmeshClipper> clipperLookup;
			public TileLayout tileLayout;
			UnsafeBitArray dirtyTiles;
			List<Vector2Int> dirtyTileCoordinates = new List<Vector2Int>();

			public bool attachedToGraph { get; private set; }
			public bool enabled => clipperLookup != null;
			public bool anyTilesDirty => dirtyTileCoordinates.Count > 0;

			void AssertEnabled () {
				if (!enabled) throw new System.InvalidOperationException($"This method cannot be called when the {nameof(NavmeshUpdateSettings)} is disabled");
			}

			public NavmeshUpdateSettings(NavmeshBase graph) {
				this.graph = graph;
				// Note: This must not be initialized here. This is because this class may be created when the AstarPath component is disabled, or if it's a prefab.
				// It's very hard to properly handled disposing unmanaged memory in those cases (unity doesn't send all useful lifetime events).
				// So we ensure that we don't initialize unmanaged memory until Enable is called.
				dirtyTiles = default;
			}

			public NavmeshUpdateSettings(NavmeshBase graph, TileLayout tileLayout) {
				this.graph = graph;
				if (graph.enableNavmeshCutting) SetLayout(tileLayout);
			}

			public void UpdateLayoutFromGraph () {
				if (enabled) ForceUpdateLayoutFromGraph();
			}

			void ForceUpdateLayoutFromGraph () {
				Assert.IsNotNull(graph.GetTiles());
				if (graph is NavMeshGraph navmeshGraph) {
					SetLayout(new TileLayout(navmeshGraph));
				} else if (graph is RecastGraph recastGraph) {
					SetLayout(new TileLayout(recastGraph));
				}
			}

			void SetLayout (TileLayout tileLayout) {
				Dispose();
				this.tileLayout = tileLayout;
				clipperLookup = new GridLookup<NavmeshClipper>(tileLayout.tileCount);
				dirtyTiles = new UnsafeBitArray(tileLayout.tileCount.x*tileLayout.tileCount.y, Allocator.Persistent);
				graph.active.navmeshUpdates.AddListener(this);
			}

			internal void MarkTilesDirty (IntRect rect) {
				if (!enabled) return;

				rect = IntRect.Intersection(rect, new IntRect(0, 0, tileLayout.tileCount.x-1, tileLayout.tileCount.y-1));
				for (int z = rect.ymin; z <= rect.ymax; z++) {
					for (int x = rect.xmin; x <= rect.xmax; x++) {
						var index = x + z * tileLayout.tileCount.x;
						if (!dirtyTiles.IsSet(index)) {
							dirtyTiles.Set(index, true);
							dirtyTileCoordinates.Add(new Vector2Int(x, z));
						}
					}
				}
			}

			public void ReloadAllTiles () {
				if (!enabled) return;

				MarkTilesDirty(new IntRect(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue));
				ScheduleDirtyTilesReload();
			}

			public void AttachToGraph () {
				Assert.AreNotEqual(graph.navmeshUpdateData, this);
				if (graph.navmeshUpdateData != null) {
					graph.navmeshUpdateData.Dispose();
					graph.navmeshUpdateData.attachedToGraph = false;
				}
				graph.navmeshUpdateData = this;
				attachedToGraph = true;
			}

			public void Enable () {
				if (enabled) throw new System.InvalidOperationException("Already enabled");

				ForceUpdateLayoutFromGraph();
				ReloadAllTiles();
			}

			public void Disable () {
				if (!enabled) return;

				clipperLookup.Clear();
				ReloadAllTiles();

				// Reload all tiles immediately.
				// Disabling navmesh cutting is typically only done in the editor, so performance is not as critical.
				graph.active.FlushWorkItems();

				Dispose();
			}

			public void Dispose () {
				clipperLookup = null;
				if (dirtyTiles.IsCreated) dirtyTiles.Dispose();
				dirtyTiles = default;
				if (graph.active != null) graph.active.navmeshUpdates.RemoveListener(this);
			}

			public void DiscardPending () {
				if (!enabled) return;

				for (int j = 0; j < NavmeshClipper.allEnabled.Count; j++) {
					var cut = NavmeshClipper.allEnabled[j];
					var root = clipperLookup.GetRoot(cut);
					if (root != null) cut.NotifyUpdated(root);
				}

				dirtyTileCoordinates.Clear();
				dirtyTiles.Clear();
			}

			/// <summary>Called when the graph has been resized to a different tile count</summary>
			public void OnResized (IntRect newTileBounds, TileLayout tileLayout) {
				if (!enabled) return;

				clipperLookup.Resize(newTileBounds);
				this.tileLayout = tileLayout;

				var characterRadius = graph.NavmeshCuttingCharacterRadius;

				// New tiles may have been created when resizing. If a cut was on the edge of the graph bounds,
				// it may intersect with the new tiles and we will need to recalculate them in that case.
				var allCuts = clipperLookup.AllItems;
				for (var cut = allCuts; cut != null; cut = cut.next) {
					var newGraphSpaceBounds = ExpandedBounds(cut.obj.GetBounds(tileLayout.transform, characterRadius));
					var newTouchingTiles = tileLayout.GetTouchingTilesInGraphSpace(newGraphSpaceBounds);
					if (cut.previousBounds != newTouchingTiles) {
						clipperLookup.Dirty(cut.obj);
						clipperLookup.Move(cut.obj, newTouchingTiles);
					}
				}

				// Transform dirty tile coordinates to be relative to the new tile bounds
				for (int i = 0; i < dirtyTileCoordinates.Count; i++) {
					var p = dirtyTileCoordinates[i];
					if (newTileBounds.Contains(p.x, p.y)) {
						// Still dirty, but translate it to the new tile coordinates
						dirtyTileCoordinates[i] = new Vector2Int(p.x - newTileBounds.xmin, p.y - newTileBounds.ymin);
					} else {
						// Not in the new bounds, remove it
						dirtyTileCoordinates.RemoveAtSwapBack(i);
						i--;
					}
				}

#if MODULE_COLLECTIONS_2_1_0_OR_NEWER
				this.dirtyTiles.Resize(newTileBounds.Width * newTileBounds.Height);
				this.dirtyTiles.Clear();
#else
				this.dirtyTiles.Dispose();
				this.dirtyTiles = new UnsafeBitArray(newTileBounds.Width * newTileBounds.Height, Allocator.Persistent);
#endif
				for (int i = 0; i < dirtyTileCoordinates.Count; i++) {
					this.dirtyTiles.Set(dirtyTileCoordinates[i].x + dirtyTileCoordinates[i].y * newTileBounds.Width, true);
				}
			}

			public void Dirty (NavmeshClipper obj) {
				// If we have no clipperLookup then we can ignore this. If we would later create a clipperLookup the object would be automatically dirtied anyway.
				if (enabled) clipperLookup.Dirty(obj);
			}

			/// <summary>Called when a NavmeshCut or NavmeshAdd is enabled</summary>
			public void AddClipper (NavmeshClipper obj) {
				AssertEnabled();
				if (!obj.graphMask.Contains((int)graph.graphIndex)) return;

				var characterRadius = graph.NavmeshCuttingCharacterRadius;
				var graphSpaceBounds = ExpandedBounds(obj.GetBounds(tileLayout.transform, characterRadius));
				var touchingTiles = tileLayout.GetTouchingTilesInGraphSpace(graphSpaceBounds);
				clipperLookup.Add(obj, touchingTiles);
			}

			/// <summary>Called when a NavmeshCut or NavmeshAdd is disabled</summary>
			public void RemoveClipper (NavmeshClipper obj) {
				AssertEnabled();
				var root = clipperLookup.GetRoot(obj);

				if (root != null) {
					MarkTilesDirty(root.previousBounds);
					clipperLookup.Remove(obj);
				}
			}

			public void ScheduleDirtyTilesReload () {
				AssertEnabled();
				if (dirtyTileCoordinates.Count == 0) return;

				var size = this.tileLayout.tileCount;
				graph.active.AddWorkItem(ctx => {
					ctx.PreUpdate();
					ReloadDirtyTilesImmediately();
				});
			}

			public void ReloadDirtyTilesImmediately () {
				if (!enabled || dirtyTileCoordinates.Count == 0) return;

				var data = RecastBuilder.CutTiles(graph, clipperLookup, tileLayout).Schedule(dirtyTileCoordinates);
				data.Complete();
				var result = data.GetValue();
				graph.StartBatchTileUpdate();

				if (!result.tileMeshes.tileMeshes.IsCreated) {
					// The cut job output nothing, indicating that no cuts are affecting the tiles.
					// We can just replace the tiles with the non-cut tiles.
					for (int i = 0; i < dirtyTileCoordinates.Count; i++) {
						var tile = graph.GetTile(dirtyTileCoordinates[i].x, dirtyTileCoordinates[i].y);
						if (tile.isCut) {
							graph.ReplaceTilePostCut(tile.x, tile.z, tile.preCutVertsInTileSpace, tile.preCutTris, tile.preCutTags, true, true);
						} else {
							// Tile is not cut, and no new cuts are affecting it. Skip it.
						}
					}
				} else {
					for (int i = 0; i < result.tileMeshes.tileMeshes.Length; i++) {
						var tileMesh = result.tileMeshes.tileMeshes[i];
						graph.ReplaceTilePostCut(dirtyTileCoordinates[i].x, dirtyTileCoordinates[i].y, tileMesh.verticesInTileSpace, tileMesh.triangles, tileMesh.tags, true, true);
					}
				}
				result.Dispose();
				graph.EndBatchTileUpdate();
				dirtyTileCoordinates.Clear();
				dirtyTiles.Clear();
			}
		}

		static Rect ExpandedBounds (Rect rect) {
			rect.xMin -= TileHandler.TileSnappingMaxDistance * Int3.PrecisionFactor;
			rect.yMin -= TileHandler.TileSnappingMaxDistance * Int3.PrecisionFactor;
			rect.xMax += TileHandler.TileSnappingMaxDistance * Int3.PrecisionFactor;
			rect.yMax += TileHandler.TileSnappingMaxDistance * Int3.PrecisionFactor;
			return rect;
		}

		internal void OnEnable () {
			// Needs to reset the time if we are using Play Mode Edit Options that do not reset the scene or reload the domain when entering play mode
			lastUpdateTime = float.NegativeInfinity;
			Profiler.BeginSample("Refresh navmesh cut enabled list");
			NavmeshClipper.RefreshEnabledList();
			Profiler.EndSample();
			NavmeshClipper.AddEnableCallback(HandleOnEnableCallback, HandleOnDisableCallback);
		}

		internal void OnDisable () {
			NavmeshClipper.RemoveEnableCallback(HandleOnEnableCallback, HandleOnDisableCallback);
		}

		public void ForceUpdateAround (NavmeshClipper clipper) {
			for (int i = 0; i < listeners.Count; i++) {
				listeners[i].Dirty(clipper);
			}
		}

		/// <summary>Discards all pending updates caused by moved or modified navmesh cuts</summary>
		public void DiscardPending () {
			for (int i = 0; i < listeners.Count; i++) {
				listeners[i].DiscardPending();
			}
		}

		/// <summary>Called when a NavmeshCut or NavmeshAdd is enabled</summary>
		void HandleOnEnableCallback (NavmeshClipper obj) {
			for (int i = 0; i < listeners.Count; i++) {
				// Add the clipper to the individual graphs. Note that this automatically marks the clipper as dirty for that particular graph.
				listeners[i].AddClipper(obj);
			}
		}

		/// <summary>Called when a NavmeshCut or NavmeshAdd is disabled</summary>
		void HandleOnDisableCallback (NavmeshClipper obj) {
			for (int i = 0; i < listeners.Count; i++) {
				listeners[i].RemoveClipper(obj);
			}
			lastUpdateTime = float.NegativeInfinity;
		}

		void AddListener (NavmeshUpdateSettings listener) {
#if UNITY_EDITOR
			if (listeners.Contains(listener)) throw new System.ArgumentException("Trying to register a listener multiple times.");
#endif
			listeners.Add(listener);
			for (int i = 0; i < NavmeshClipper.allEnabled.Count; i++) listener.AddClipper(NavmeshClipper.allEnabled[i]);
		}

		void RemoveListener (NavmeshUpdateSettings listener) {
			listeners.Remove(listener);
		}

		/// <summary>Update is called once per frame</summary>
		internal void Update () {
			if (astar.isScanning) return;
			Profiler.BeginSample("Navmesh cutting");
			bool anyTilesDirty = false;
			RefreshEnabledState();

			for (int i = 0; i < listeners.Count; i++) {
				// Tiles can have already been dirtied by, for example, navmesh cuts being disabled
				anyTilesDirty |= listeners[i].anyTilesDirty;
			}

			if ((updateInterval >= 0 && Time.realtimeSinceStartup - lastUpdateTime > updateInterval) || anyTilesDirty) {
				ScheduleTileUpdates();
			}
			Profiler.EndSample();
		}

		/// <summary>
		/// Checks all NavmeshCut instances and updates graphs if needed.
		/// Note: This schedules updates for all necessary tiles to happen as soon as possible.
		/// The pathfinding threads will continue to calculate the paths that they were calculating when this function
		/// was called and then they will be paused and the graph updates will be carried out (this may be several frames into the
		/// future and the graph updates themselves may take several frames to complete).
		/// If you want to force all navmesh cutting to be completed in a single frame call this method
		/// and immediately after call AstarPath.FlushWorkItems.
		///
		/// <code>
		/// // Schedule pending updates to be done as soon as the pathfinding threads
		/// // are done with what they are currently doing.
		/// AstarPath.active.navmeshUpdates.ForceUpdate();
		/// // Block until the updates have finished
		/// AstarPath.active.FlushGraphUpdates();
		/// </code>
		/// </summary>
		public void ForceUpdate () {
			RefreshEnabledState();
			ScheduleTileUpdates();
		}

		void RefreshEnabledState () {
			var graphs = astar.graphs;
			for (int i = 0; i < graphs.Length; i++) {
				var graph = graphs[i];
				if (graph is NavmeshBase navmesh) {
					var shouldBeEnabled = navmesh.enableNavmeshCutting && navmesh.isScanned;
					if (navmesh.navmeshUpdateData.enabled != shouldBeEnabled) {
						if (shouldBeEnabled) {
							navmesh.navmeshUpdateData.Enable();
						} else {
							navmesh.navmeshUpdateData.Disable();
						}
					}
				}
			}
		}

		void ScheduleTileUpdates () {
			lastUpdateTime = Time.realtimeSinceStartup;

			foreach (var handler in listeners) {
				Assert.IsTrue(handler.enabled);
				if (!handler.attachedToGraph) continue;

				// Get all navmesh cuts in the scene
				var allCuts = handler.clipperLookup.AllItems;

				if (!handler.anyTilesDirty) {
					bool any = false;

					// Check if any navmesh cuts need updating
					for (var cut = allCuts; cut != null; cut = cut.next) {
						if (cut.obj.RequiresUpdate(cut)) {
							any = true;
							break;
						}
					}

					// Nothing needs to be done for now
					if (!any) continue;
				}

				var characterRadius = handler.graph.NavmeshCuttingCharacterRadius;
				// Reload all bounds touching the previous bounds and current bounds
				// of navmesh cuts that have moved or changed in some other way
				for (var cut = allCuts; cut != null; cut = cut.next) {
					if (cut.obj.RequiresUpdate(cut)) {
						// Make sure the tile where it was is updated
						handler.MarkTilesDirty(cut.previousBounds);

						var newGraphSpaceBounds = ExpandedBounds(cut.obj.GetBounds(handler.tileLayout.transform, characterRadius));
						var newTouchingTiles = handler.tileLayout.GetTouchingTilesInGraphSpace(newGraphSpaceBounds);
						handler.clipperLookup.Move(cut.obj, newTouchingTiles);
						handler.MarkTilesDirty(newTouchingTiles);

						// Notify the navmesh cut that it has been updated in this graph
						// This will cause RequiresUpdate to return false
						// until it is changed again.
						cut.obj.NotifyUpdated(cut);
					}
				}

				handler.ScheduleDirtyTilesReload();
			}
		}
	}
}
