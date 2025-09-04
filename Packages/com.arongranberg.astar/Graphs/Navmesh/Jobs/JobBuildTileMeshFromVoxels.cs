using Pathfinding.Jobs;
using Pathfinding.Util;
using Pathfinding.Graphs.Navmesh.Voxelization.Burst;
using Pathfinding.Collections;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Unity.Profiling;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>
	/// Scratch space for building navmesh tiles using voxelization.
	///
	/// This uses quite a lot of memory, so it is used by a single worker thread for multiple tiles in order to minimize allocations.
	/// </summary>
	public struct TileBuilderBurst : IArenaDisposable {
		public LinkedVoxelField linkedVoxelField;
		public CompactVoxelField compactVoxelField;
		public NativeList<ushort> distanceField;
		public NativeQueue<Int3> tmpQueue1;
		public NativeQueue<Int3> tmpQueue2;
		public NativeList<VoxelContour> contours;
		public NativeList<int> contourVertices;
		public VoxelMesh voxelMesh;

		public TileBuilderBurst (int width, int depth, int voxelWalkableHeight, int maximumVoxelYCoord) {
			linkedVoxelField = new LinkedVoxelField(width, depth, maximumVoxelYCoord);
			compactVoxelField = new CompactVoxelField(width, depth, voxelWalkableHeight, Allocator.Persistent);
			tmpQueue1 = new NativeQueue<Int3>(Allocator.Persistent);
			tmpQueue2 = new NativeQueue<Int3>(Allocator.Persistent);
			distanceField = new NativeList<ushort>(0, Allocator.Persistent);
			contours = new NativeList<VoxelContour>(Allocator.Persistent);
			contourVertices = new NativeList<int>(Allocator.Persistent);
			voxelMesh = new VoxelMesh {
				verts = new NativeList<Int3>(Allocator.Persistent),
				tris = new NativeList<int>(Allocator.Persistent),
				areas = new NativeList<int>(Allocator.Persistent),
			};
		}

		void IArenaDisposable.DisposeWith (DisposeArena arena) {
			arena.Add(linkedVoxelField);
			arena.Add(compactVoxelField);
			arena.Add(distanceField);
			arena.Add(tmpQueue1);
			arena.Add(tmpQueue2);
			arena.Add(contours);
			arena.Add(contourVertices);
			arena.Add(voxelMesh);
		}
	}

	/// <summary>
	/// Builds tiles from a polygon soup using voxelization.
	///
	/// This job takes the following steps:
	/// - Voxelize the input meshes
	/// - Filter and process the resulting voxelization in various ways to remove unwanted artifacts and make it better suited for pathfinding.
	/// - Extract a walkable surface from the voxelization.
	/// - Triangulate this surface and create navmesh tiles from it.
	///
	/// This job uses work stealing to distribute the work between threads. The communication happens using a shared queue and the <see cref="currentTileCounter"/> atomic variable.
	/// </summary>
	[BurstCompile(CompileSynchronously = true)]
	// TODO: [BurstCompile(FloatMode = FloatMode.Fast)]
	public struct JobBuildTileMeshFromVoxels : IJob {
		public TileBuilderBurst tileBuilder;
		[ReadOnly]
		public TileBuilder.BucketMapping inputMeshes;
		[ReadOnly]
		public NativeArray<Bounds> tileGraphSpaceBounds;
		public Matrix4x4 voxelToTileSpace;

		/// <summary>
		/// Limits of the graph space bounds for the whole graph on the XZ plane.
		///
		/// Used to crop the border tiles to exactly the limits of the graph's bounding box.
		/// </summary>
		public Vector2 graphSpaceLimits;

		[NativeDisableUnsafePtrRestriction]
		public unsafe TileMesh.TileMeshUnsafe* outputMeshes;

		/// <summary>Max number of tiles to process in this job</summary>
		public int maxTiles;

		public int voxelWalkableClimb;
		public uint voxelWalkableHeight;
		public float cellSize;
		public float cellHeight;
		public float maxSlope;
		public RecastGraph.DimensionMode dimensionMode;
		public RecastGraph.BackgroundTraversability backgroundTraversability;
		public Matrix4x4 graphToWorldSpace;
		public int characterRadiusInVoxels;
		public int tileBorderSizeInVoxels;
		public int minRegionSize;
		public float maxEdgeLength;
		public float contourMaxError;
		[ReadOnly]
		public NativeArray<JobBuildRegions.RelevantGraphSurfaceInfo> relevantGraphSurfaces;
		public RecastGraph.RelevantGraphSurfaceMode relevantGraphSurfaceMode;

		[NativeDisableUnsafePtrRestriction]
		public unsafe int* currentTileCounter;

		public void SetOutputMeshes (NativeArray<TileMesh.TileMeshUnsafe> arr) {
			unsafe {
				outputMeshes = (TileMesh.TileMeshUnsafe*)arr.GetUnsafeReadOnlyPtr();
			}
		}

		public void SetCounter (NativeReference<int> counter) {
			unsafe {
				// Note: The pointer cast is only necessary when using early versions of the collections package.
				currentTileCounter = (int*)counter.GetUnsafePtr();
			}
		}

		private static readonly ProfilerMarker MarkerVoxelize = new ProfilerMarker("Voxelize");
		private static readonly ProfilerMarker MarkerFilterLedges = new ProfilerMarker("FilterLedges");
		private static readonly ProfilerMarker MarkerFilterLowHeightSpans = new ProfilerMarker("FilterLowHeightSpans");
		private static readonly ProfilerMarker MarkerBuildCompactField = new ProfilerMarker("BuildCompactField");
		private static readonly ProfilerMarker MarkerBuildConnections = new ProfilerMarker("BuildConnections");
		private static readonly ProfilerMarker MarkerErodeWalkableArea = new ProfilerMarker("ErodeWalkableArea");
		private static readonly ProfilerMarker MarkerBuildDistanceField = new ProfilerMarker("BuildDistanceField");
		private static readonly ProfilerMarker MarkerBuildRegions = new ProfilerMarker("BuildRegions");
		private static readonly ProfilerMarker MarkerBuildContours = new ProfilerMarker("BuildContours");
		private static readonly ProfilerMarker MarkerBuildMesh = new ProfilerMarker("BuildMesh");
		private static readonly ProfilerMarker MarkerConvertAreasToTags = new ProfilerMarker("ConvertAreasToTags");
		private static readonly ProfilerMarker MarkerRemoveDuplicateVertices = new ProfilerMarker("RemoveDuplicateVertices");
		private static readonly ProfilerMarker MarkerTransformTileCoordinates = new ProfilerMarker("TransformTileCoordinates");

		public void Execute () {
			for (int k = 0; k < maxTiles; k++) {
				// Grab the next tile index that we should calculate
				int i;
				unsafe {
					i = System.Threading.Interlocked.Increment(ref UnsafeUtility.AsRef<int>(currentTileCounter)) - 1;
				}
				if (i >= tileGraphSpaceBounds.Length) return;

				tileBuilder.linkedVoxelField.ResetLinkedVoxelSpans();
				if (dimensionMode == RecastGraph.DimensionMode.Dimension2D && backgroundTraversability == RecastGraph.BackgroundTraversability.Walkable) {
					tileBuilder.linkedVoxelField.SetWalkableBackground();
				}

				var bucketStart = i > 0 ? inputMeshes.bucketRanges[i-1] : 0;
				var bucketEnd = inputMeshes.bucketRanges[i];
				MarkerVoxelize.Begin();
				new JobVoxelize {
					inputMeshes = inputMeshes.meshes,
					bucket = inputMeshes.pointers.GetSubArray(bucketStart, bucketEnd - bucketStart),
					voxelWalkableClimb = voxelWalkableClimb,
					voxelWalkableHeight = voxelWalkableHeight,
					cellSize = cellSize,
					cellHeight = cellHeight,
					maxSlope = maxSlope,
					graphTransform = graphToWorldSpace,
					graphSpaceBounds = tileGraphSpaceBounds[i],
					graphSpaceLimits = graphSpaceLimits,
					voxelArea = tileBuilder.linkedVoxelField,
				}.Execute();
				MarkerVoxelize.End();



				MarkerFilterLedges.Begin();
				new JobFilterLedges {
					field = tileBuilder.linkedVoxelField,
					voxelWalkableClimb = voxelWalkableClimb,
					voxelWalkableHeight = voxelWalkableHeight,
					cellSize = cellSize,
					cellHeight = cellHeight,
				}.Execute();
				MarkerFilterLedges.End();

				MarkerFilterLowHeightSpans.Begin();
				new JobFilterLowHeightSpans {
					field = tileBuilder.linkedVoxelField,
					voxelWalkableHeight = voxelWalkableHeight,
				}.Execute();
				MarkerFilterLowHeightSpans.End();

				MarkerBuildCompactField.Begin();
				new JobBuildCompactField {
					input = tileBuilder.linkedVoxelField,
					output = tileBuilder.compactVoxelField,
				}.Execute();
				MarkerBuildCompactField.End();

				MarkerBuildConnections.Begin();
				new JobBuildConnections {
					field = tileBuilder.compactVoxelField,
					voxelWalkableHeight = (int)voxelWalkableHeight,
					voxelWalkableClimb = voxelWalkableClimb,
				}.Execute();
				MarkerBuildConnections.End();

				MarkerErodeWalkableArea.Begin();
				new JobErodeWalkableArea {
					field = tileBuilder.compactVoxelField,
					radius = characterRadiusInVoxels,
				}.Execute();
				MarkerErodeWalkableArea.End();

				MarkerBuildDistanceField.Begin();
				new JobBuildDistanceField {
					field = tileBuilder.compactVoxelField,
					output = tileBuilder.distanceField,
				}.Execute();
				MarkerBuildDistanceField.End();

				MarkerBuildRegions.Begin();
				new JobBuildRegions {
					field = tileBuilder.compactVoxelField,
					distanceField = tileBuilder.distanceField,
					borderSize = tileBorderSizeInVoxels,
					minRegionSize = Mathf.RoundToInt(minRegionSize),
					srcQue = tileBuilder.tmpQueue1,
					dstQue = tileBuilder.tmpQueue2,
					relevantGraphSurfaces = relevantGraphSurfaces,
					relevantGraphSurfaceMode = relevantGraphSurfaceMode,
					cellSize = cellSize,
					cellHeight = cellHeight,
					graphTransform = graphToWorldSpace,
					graphSpaceBounds = tileGraphSpaceBounds[i],
				}.Execute();
				MarkerBuildRegions.End();

				MarkerBuildContours.Begin();
				new JobBuildContours {
					field = tileBuilder.compactVoxelField,
					maxError = contourMaxError,
					maxEdgeLength = maxEdgeLength,
					buildFlags = VoxelUtilityBurst.RC_CONTOUR_TESS_WALL_EDGES | VoxelUtilityBurst.RC_CONTOUR_TESS_TILE_EDGES,
					cellSize = cellSize,
					outputContours = tileBuilder.contours,
					outputVerts = tileBuilder.contourVertices,
				}.Execute();
				MarkerBuildContours.End();

				MarkerBuildMesh.Begin();
				new JobBuildMesh {
					contours = tileBuilder.contours,
					contourVertices = tileBuilder.contourVertices,
					mesh = tileBuilder.voxelMesh,
					field = tileBuilder.compactVoxelField,
				}.Execute();
				MarkerBuildMesh.End();

				unsafe {
					TileMesh.TileMeshUnsafe* outputTileMesh = outputMeshes + i;

					MarkerConvertAreasToTags.Begin();
					new JobConvertAreasToTags {
						areas = tileBuilder.voxelMesh.areas,
					}.Execute();
					MarkerConvertAreasToTags.End();

					MarkerRemoveDuplicateVertices.Begin();
					new MeshUtility.JobMergeNearbyVertices {
						vertices = tileBuilder.voxelMesh.verts,
						triangles = tileBuilder.voxelMesh.tris,
						mergeRadiusSq = 0,
					}.Execute();
					new MeshUtility.JobRemoveDegenerateTriangles {
						vertices = tileBuilder.voxelMesh.verts,
						triangles = tileBuilder.voxelMesh.tris,
						tags = tileBuilder.voxelMesh.areas,
					}.Execute();
					MarkerRemoveDuplicateVertices.End();

					MarkerTransformTileCoordinates.Begin();
					new JobTransformTileCoordinates {
						vertices = tileBuilder.voxelMesh.verts.AsUnsafeSpan(),
						matrix = voxelToTileSpace,
					}.Execute();
					MarkerTransformTileCoordinates.End();

					*outputTileMesh = new TileMesh.TileMeshUnsafe {
						// Convert the buffers to spans that own their memory.
						verticesInTileSpace = tileBuilder.voxelMesh.verts.AsUnsafeSpan().Clone(Allocator.Persistent),
						triangles = tileBuilder.voxelMesh.tris.AsUnsafeSpan().Clone(Allocator.Persistent),
						tags = tileBuilder.voxelMesh.areas.AsUnsafeSpan().Reinterpret<uint>().Clone(Allocator.Persistent),
					};
				}
			}
		}
	}
}
