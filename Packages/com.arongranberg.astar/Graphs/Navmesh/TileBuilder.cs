using System.Collections.Generic;
using Pathfinding.Graphs.Navmesh.Jobs;
using Pathfinding.Jobs;
using Pathfinding.Pooling;
using Pathfinding.Sync;
using Pathfinding.Graphs.Navmesh.Voxelization.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Assertions;

namespace Pathfinding.Graphs.Navmesh {
	/// <summary>
	/// Settings for building tile meshes in a recast graph.
	///
	/// See: <see cref="RecastGraph"/> for more documentation on the individual fields.
	/// See: <see cref="RecastBuilder"/>
	/// </summary>
	public struct TileBuilder {
		public float walkableClimb;
		public RecastGraph.CollectionSettings collectionSettings;
		public RecastGraph.RelevantGraphSurfaceMode relevantGraphSurfaceMode;
		public RecastGraph.DimensionMode dimensionMode;
		public RecastGraph.BackgroundTraversability backgroundTraversability;

		// TODO: Don't store in struct
		public int tileBorderSizeInVoxels;
		public float walkableHeight;
		public float maxSlope;
		// TODO: Specify in world units
		public int characterRadiusInVoxels;
		public int minRegionSize;
		public float maxEdgeLength;
		public float contourMaxError;
		public TileLayout tileLayout;
		public IntRect tileRect;
		public List<RecastGraph.PerLayerModification> perLayerModifications;

		public class TileBuilderOutput : IProgress, System.IDisposable {
			public NativeReference<int> currentTileCounter;
			public TileMeshesUnsafe tileMeshes;
#if UNITY_EDITOR
			public List<(UnityEngine.Object, Mesh)> meshesUnreadableAtRuntime;
#endif

			public float Progress {
				get {
					var tileCount = tileMeshes.tileRect.Area;
					var currentTile = Mathf.Min(tileCount, currentTileCounter.Value);
					return tileCount > 0 ? currentTile / (float)tileCount : 0; // "Scanning tiles: " + currentTile + " of " + (tileCount) + " tiles...");
				}
			}

			public void Dispose () {
				tileMeshes.Dispose(Allocator.Persistent);
				if (currentTileCounter.IsCreated) currentTileCounter.Dispose();
#if UNITY_EDITOR
				if (meshesUnreadableAtRuntime != null) ListPool<(UnityEngine.Object, Mesh)>.Release(ref meshesUnreadableAtRuntime);
#endif
			}
		}

		public TileBuilder (RecastGraph graph, TileLayout tileLayout, IntRect tileRect) {
			this.tileLayout = tileLayout;
			this.tileRect = tileRect;
			// A walkableClimb higher than walkableHeight can cause issues when generating the navmesh since then it can in some cases
			// Both be valid for a character to walk under an obstacle and climb up on top of it (and that cannot be handled with navmesh without links)
			// The editor scripts also enforce this, but we enforce it here too just to be sure
			this.walkableClimb = Mathf.Min(graph.walkableClimb, graph.walkableHeight);
			this.collectionSettings = graph.collectionSettings;
			this.dimensionMode = graph.dimensionMode;
			this.backgroundTraversability = graph.backgroundTraversability;
			this.tileBorderSizeInVoxels = graph.TileBorderSizeInVoxels;
			this.walkableHeight = graph.walkableHeight;
			this.maxSlope = graph.maxSlope;
			this.characterRadiusInVoxels = graph.CharacterRadiusInVoxels;
			this.minRegionSize = Mathf.RoundToInt(graph.minRegionSize);
			this.maxEdgeLength = graph.maxEdgeLength;
			this.contourMaxError = graph.contourMaxError;
			this.relevantGraphSurfaceMode = graph.relevantGraphSurfaceMode;
			this.perLayerModifications = graph.perLayerModifications;

			if (collectionSettings.physicsScene == null) collectionSettings.physicsScene = graph.active.gameObject.scene.GetPhysicsScene();
			if (collectionSettings.physicsScene2D == null) collectionSettings.physicsScene2D = graph.active.gameObject.scene.GetPhysicsScene2D();
		}

		/// <summary>
		/// Number of extra voxels on each side of a tile to ensure accurate navmeshes near the tile border.
		/// The width of a tile is expanded by 2 times this value (1x to the left and 1x to the right)
		/// </summary>
		int TileBorderSizeInVoxels {
			get {
				return characterRadiusInVoxels + 3;
			}
		}

		float TileBorderSizeInWorldUnits {
			get {
				return TileBorderSizeInVoxels*tileLayout.cellSize;
			}
		}

		/// <summary>Get the world space bounds for all tiles, including an optional (graph space) padding around the tiles in the x and z axis</summary>
		public Bounds GetWorldSpaceBounds (float xzPadding = 0) {
			var graphSpaceBounds = tileLayout.GetTileBoundsInGraphSpace(tileRect.xmin, tileRect.ymin, tileRect.Width, tileRect.Height);
			graphSpaceBounds.Expand(new Vector3(2*xzPadding, 0, 2*xzPadding));
			return tileLayout.transform.Transform(graphSpaceBounds);
		}

		public RecastMeshGatherer.MeshCollection CollectMeshes (Bounds bounds) {
			Profiler.BeginSample("Find Meshes for rasterization");
			var mask = collectionSettings.layerMask;
			var tagMask = collectionSettings.tagMask;
			if (collectionSettings.collectionMode == RecastGraph.CollectionSettings.FilterMode.Layers) {
				tagMask = null;
			} else {
				mask = 0;
			}
			var meshGatherer = new RecastMeshGatherer(collectionSettings.physicsScene.Value, collectionSettings.physicsScene2D.Value, bounds, collectionSettings.terrainHeightmapDownsamplingFactor, mask, tagMask, perLayerModifications, tileLayout.cellSize / collectionSettings.colliderRasterizeDetail);

			if (collectionSettings.rasterizeMeshes && dimensionMode == RecastGraph.DimensionMode.Dimension3D) {
				Profiler.BeginSample("Find meshes");
				meshGatherer.CollectSceneMeshes();
				Profiler.EndSample();
			}

			Profiler.BeginSample("Find RecastNavmeshModifiers");
			meshGatherer.CollectRecastNavmeshModifiers();
			Profiler.EndSample();

			if (collectionSettings.rasterizeTerrain && dimensionMode == RecastGraph.DimensionMode.Dimension3D) {
				Profiler.BeginSample("Find terrains");
				// Split terrains up into meshes approximately the size of a single tile
				var desiredTerrainChunkSize = 0.51f * tileLayout.cellSize*(math.max(tileLayout.tileSizeInVoxels.x, tileLayout.tileSizeInVoxels.y) + 2*TileBorderSizeInVoxels);
				meshGatherer.CollectTerrainMeshes(collectionSettings.rasterizeTrees, desiredTerrainChunkSize);
				Profiler.EndSample();
			}

			if (collectionSettings.rasterizeColliders || dimensionMode == RecastGraph.DimensionMode.Dimension2D) {
				Profiler.BeginSample("Find colliders");
				if (dimensionMode == RecastGraph.DimensionMode.Dimension3D) {
					meshGatherer.CollectColliderMeshes();
				} else {
					meshGatherer.Collect2DColliderMeshes();
				}
				Profiler.EndSample();
			}

			if (collectionSettings.onCollectMeshes != null) {
				Profiler.BeginSample("Custom mesh collection");
				collectionSettings.onCollectMeshes(meshGatherer);
				Profiler.EndSample();
			}

			Profiler.BeginSample("Finalizing");
			var result = meshGatherer.Finalize();
			Profiler.EndSample();

			// Warn if no meshes were found, but only if the tile rect covers the whole graph.
			// If it's just a partial update, the user is probably not interested in this warning,
			// as it is completely normal that there are some empty tiles.
			if (tileRect == new IntRect(0, 0, tileLayout.tileCount.x - 1, tileLayout.tileCount.y - 1) && result.meshes.Length == 0) {
				Debug.LogWarning("No rasterizable objects were found contained in the layers specified by the 'mask' variables");
			}

			Profiler.EndSample();
			return result;
		}

		/// <summary>A mapping from tiles to the meshes that each tile touches</summary>
		public struct BucketMapping {
			/// <summary>All meshes that should be voxelized</summary>
			public NativeArray<RasterizationMesh> meshes;
			/// <summary>Indices into the <see cref="meshes"/> array</summary>
			public NativeArray<int> pointers;
			/// <summary>
			/// For each tile, the range of pointers in <see cref="pointers"/> that correspond to that tile.
			/// This is a cumulative sum of the number of pointers in each bucket.
			///
			/// Bucket i will contain pointers in the range [i > 0 ? bucketRanges[i-1] : 0, bucketRanges[i]).
			///
			/// The length is the same as the number of tiles.
			/// </summary>
			public NativeArray<int> bucketRanges;
		}

		/// <summary>Creates a list for every tile and adds every mesh that touches a tile to the corresponding list</summary>
		BucketMapping PutMeshesIntoTileBuckets (RecastMeshGatherer.MeshCollection meshCollection, IntRect tileBuckets) {
			var bucketCount = tileBuckets.Width*tileBuckets.Height;
			var buckets = new NativeList<int>[bucketCount];
			var borderExpansion = TileBorderSizeInWorldUnits;

			for (int i = 0; i < buckets.Length; i++) {
				buckets[i] = new NativeList<int>(Allocator.Persistent);
			}

			var offset = -tileBuckets.Min;
			var clamp = new IntRect(0, 0, tileBuckets.Width - 1, tileBuckets.Height - 1);
			var meshes = meshCollection.meshes;
			for (int i = 0; i < meshes.Length; i++) {
				var mesh = meshes[i];
				var bounds = mesh.bounds;
				var rect = tileLayout.GetTouchingTiles(bounds, borderExpansion);
				rect = IntRect.Intersection(rect.Offset(offset), clamp);
				for (int z = rect.ymin; z <= rect.ymax; z++) {
					for (int x = rect.xmin; x <= rect.xmax; x++) {
						buckets[x + z*tileBuckets.Width].Add(i);
					}
				}
			}

			// Concat buckets
			int allPointersCount = 0;
			for (int i = 0; i < buckets.Length; i++) allPointersCount += buckets[i].Length;
			var allPointers = new NativeArray<int>(allPointersCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			var bucketRanges = new NativeArray<int>(bucketCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			allPointersCount = 0;
			for (int i = 0; i < buckets.Length; i++) {
				// If we have an empty bucket at the end of the array then allPointersCount might be equal to allPointers.Length which would cause an assert to trigger.
				// So for empty buckets don't call the copy method
				if (buckets[i].Length > 0) {
					NativeArray<int>.Copy(buckets[i].AsArray(), 0, allPointers, allPointersCount, buckets[i].Length);
				}
				allPointersCount += buckets[i].Length;
				bucketRanges[i] = allPointersCount;
				buckets[i].Dispose();
			}

			return new BucketMapping {
					   meshes = meshCollection.meshes,
					   pointers = allPointers,
					   bucketRanges = bucketRanges,
			};
		}

		public Promise<TileBuilderOutput> Schedule (DisposeArena arena) {
			var tileCount = tileRect.Area;
			Assert.IsTrue(tileCount > 0);

			var tileRectWidth = tileRect.Width;
			var tileRectDepth = tileRect.Height;

			// Find all meshes that could affect the graph
			var worldBounds = GetWorldSpaceBounds(TileBorderSizeInWorldUnits);
			if (dimensionMode == RecastGraph.DimensionMode.Dimension2D) {
				// In 2D mode, the bounding box of the graph only bounds it in the X and Y dimensions
				worldBounds.extents = new Vector3(worldBounds.extents.x, worldBounds.extents.y, float.PositiveInfinity);
			}
			var meshes = CollectMeshes(worldBounds);

			Profiler.BeginSample("PutMeshesIntoTileBuckets");
			var buckets = PutMeshesIntoTileBuckets(meshes, tileRect);
			Profiler.EndSample();

			Profiler.BeginSample("Allocating tiles");
			var tileMeshes = new NativeArray<TileMesh.TileMeshUnsafe>(tileCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);

			int width = tileLayout.tileSizeInVoxels.x + tileBorderSizeInVoxels*2;
			int depth = tileLayout.tileSizeInVoxels.y + tileBorderSizeInVoxels*2;
			var cellHeight = tileLayout.CellHeight;
			// TODO: Move inside BuildTileMeshBurst
			var voxelWalkableHeight = (uint)(walkableHeight/cellHeight);
			var voxelWalkableClimb = Mathf.RoundToInt(walkableClimb/cellHeight);

			var tileGraphSpaceBounds = new NativeArray<Bounds>(tileCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

			for (int z = 0; z < tileRectDepth; z++) {
				for (int x = 0; x < tileRectWidth; x++) {
					int tileIndex = x + z*tileRectWidth;
					var tileBounds = tileLayout.GetTileBoundsInGraphSpace(tileRect.xmin + x, tileRect.ymin + z);
					// Expand borderSize voxels on each side
					tileBounds.Expand(new Vector3(1, 0, 1)*TileBorderSizeInWorldUnits*2);
					tileGraphSpaceBounds[tileIndex] = tileBounds;
				}
			}

			Profiler.EndSample();
			Profiler.BeginSample("Scheduling jobs");

			var builders = new TileBuilderBurst[Mathf.Max(1, Mathf.Min(tileCount, Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount + 1))];
			var currentTileCounter = new NativeReference<int>(0, Allocator.Persistent);
			JobHandle dependencies = default;

			var relevantGraphSurfaces = new NativeList<JobBuildRegions.RelevantGraphSurfaceInfo>(Allocator.Persistent);
			var c = RelevantGraphSurface.Root;
			while (c != null) {
				relevantGraphSurfaces.Add(new JobBuildRegions.RelevantGraphSurfaceInfo {
					position = c.transform.position,
					range = c.maxRange,
				});
				c = c.Next;
			}


			// Having a few long running jobs is bad because Unity cannot inject more high priority jobs
			// in between tile calculations. So we run each builder a number of times.
			// Each step will just calculate one tile.
			int tilesPerJob = Mathf.CeilToInt(Mathf.Sqrt(tileCount));
			// Number of tiles calculated if every builder runs once
			int tilesPerStep = tilesPerJob * builders.Length;
			// Round up to make sure we run the jobs enough times
			// We multiply by 2 to run a bit more jobs than strictly necessary.
			// This is to ensure that if one builder just gets a bunch of long running jobs
			// then the other builders can steal some work from it.
			int jobSteps = 2 * (tileCount + tilesPerStep - 1) / tilesPerStep;
			var jobTemplate = new JobBuildTileMeshFromVoxels {
				tileBuilder = builders[0],
				inputMeshes = buckets,
				tileGraphSpaceBounds = tileGraphSpaceBounds,
				voxelWalkableClimb = voxelWalkableClimb,
				voxelWalkableHeight = voxelWalkableHeight,
				voxelToTileSpace = Matrix4x4.Scale(new Vector3(tileLayout.cellSize, cellHeight, tileLayout.cellSize)) * Matrix4x4.Translate(-new Vector3(1, 0, 1)*TileBorderSizeInVoxels),
				cellSize = tileLayout.cellSize,
				cellHeight = cellHeight,
				maxSlope = Mathf.Max(maxSlope, 0.0001f), // Ensure maxSlope is not 0, as then horizontal surfaces can sometimes get excluded due to floating point errors
				dimensionMode = dimensionMode,
				backgroundTraversability = backgroundTraversability,
				graphToWorldSpace = tileLayout.transform.matrix,
				// Crop all tiles to ensure they are inside the graph bounds (even if the tiles did not line up perfectly with the bounding box).
				// Add the character radius, since it will be eroded away anyway, but subtract 1 voxel to ensure the nodes are strictly inside the bounding box
				graphSpaceLimits = new Vector2(tileLayout.graphSpaceSize.x + (characterRadiusInVoxels-1)*tileLayout.cellSize, tileLayout.graphSpaceSize.z + (characterRadiusInVoxels-1)*tileLayout.cellSize),
				characterRadiusInVoxels = characterRadiusInVoxels,
				tileBorderSizeInVoxels = tileBorderSizeInVoxels,
				minRegionSize = minRegionSize,
				maxEdgeLength = maxEdgeLength,
				contourMaxError = contourMaxError,
				maxTiles = tilesPerJob,
				relevantGraphSurfaces = relevantGraphSurfaces.AsArray(),
				relevantGraphSurfaceMode = this.relevantGraphSurfaceMode,
			};
			jobTemplate.SetOutputMeshes(tileMeshes);
			jobTemplate.SetCounter(currentTileCounter);
			int maximumVoxelYCoord = (int)(tileLayout.graphSpaceSize.y / cellHeight);
			for (int i = 0; i < builders.Length; i++) {
				jobTemplate.tileBuilder = builders[i] = new TileBuilderBurst(width, depth, (int)voxelWalkableHeight, maximumVoxelYCoord);
				var dep = new JobHandle();
				for (int j = 0; j < jobSteps; j++) {
					dep = jobTemplate.Schedule(dep);
				}
				dependencies = JobHandle.CombineDependencies(dependencies, dep);
			}
			JobHandle.ScheduleBatchedJobs();

			Profiler.EndSample();

			arena.Add(tileGraphSpaceBounds);
			arena.Add(relevantGraphSurfaces);
			arena.Add(buckets.bucketRanges);
			arena.Add(buckets.pointers);
			// Note: buckets.meshes references data in #meshes, so we don't have to dispose it separately
			arena.Add(meshes);

			// Dispose the mesh data after all jobs are completed.
			// Note that the jobs use pointers to this data which are not tracked by the safety system.
			for (int i = 0; i < builders.Length; i++) arena.Add(builders[i]);

			return new Promise<TileBuilderOutput>(dependencies, new TileBuilderOutput {
				tileMeshes = new TileMeshesUnsafe(tileMeshes, tileRect, new Vector2(tileLayout.TileWorldSizeX, tileLayout.TileWorldSizeZ)),
				currentTileCounter = currentTileCounter,
#if UNITY_EDITOR
				meshesUnreadableAtRuntime = meshes.meshesUnreadableAtRuntime,
#endif
			});
		}
	}
}
