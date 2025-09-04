using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

namespace Pathfinding.Graphs.Navmesh {
	using System;
	using Pathfinding;
	using Voxelization.Burst;
	using Pathfinding.Util;
	using Pathfinding.Jobs;
	using Pathfinding.Collections;
	using Pathfinding.Pooling;
	using UnityEngine.Profiling;
	using Unity.Collections.LowLevel.Unsafe;

	[BurstCompile]
	public class RecastMeshGatherer {
		readonly int terrainDownsamplingFactor;
		public readonly LayerMask mask;
		public readonly List<string> tagMask;
		readonly float maxColliderApproximationError;
		public readonly Bounds bounds;
		public readonly PhysicsScene physicsScene;
		public readonly PhysicsScene2D physicsScene2D;
		Dictionary<MeshCacheItem, int> cachedMeshes = new Dictionary<MeshCacheItem, int>();
		readonly Dictionary<GameObject, TreeInfo> cachedTreePrefabs = new Dictionary<GameObject, TreeInfo>();
		readonly List<NativeArray<Vector3> > vertexBuffers;
		readonly List<NativeArray<int> > triangleBuffers;
		readonly List<Mesh> meshData;
		readonly RecastGraph.PerLayerModification[] modificationsByLayer;
		readonly RecastGraph.PerLayerModification[] modificationsByLayer2D;
#if UNITY_EDITOR
		readonly List<(UnityEngine.Object, Mesh)> meshesUnreadableAtRuntime = new List<(UnityEngine.Object, Mesh)>();
#else
		bool anyNonReadableMesh = false;
#endif

		List<GatheredMesh> meshes;
		List<Material> dummyMaterials = new List<Material>();

		public RecastMeshGatherer (PhysicsScene physicsScene, PhysicsScene2D physicsScene2D, Bounds bounds, int terrainDownsamplingFactor, LayerMask mask, List<string> tagMask, List<RecastGraph.PerLayerModification> perLayerModifications, float maxColliderApproximationError) {
			// Clamp to at least 1 since that's the resolution of the heightmap
			terrainDownsamplingFactor = Math.Max(terrainDownsamplingFactor, 1);

			this.bounds = bounds;
			this.terrainDownsamplingFactor = terrainDownsamplingFactor;
			this.mask = mask;
			this.tagMask = tagMask ?? new List<string>();
			this.maxColliderApproximationError = maxColliderApproximationError;
			this.physicsScene = physicsScene;
			this.physicsScene2D = physicsScene2D;
			meshes = ListPool<GatheredMesh>.Claim();
			vertexBuffers = ListPool<NativeArray<Vector3> >.Claim();
			triangleBuffers = ListPool<NativeArray<int> >.Claim();
			cachedMeshes = ObjectPoolSimple<Dictionary<MeshCacheItem, int> >.Claim();
			meshData = ListPool<Mesh>.Claim();
			modificationsByLayer = RecastGraph.PerLayerModification.ToLayerLookup(perLayerModifications, RecastGraph.PerLayerModification.Default);
			// 2D colliders default to being unwalkable
			var default2D = RecastGraph.PerLayerModification.Default;
			default2D.mode = RecastNavmeshModifier.Mode.UnwalkableSurface;
			modificationsByLayer2D = RecastGraph.PerLayerModification.ToLayerLookup(perLayerModifications, default2D);
		}

		struct TreeInfo {
			public List<GatheredMesh> submeshes;
			public Vector3 localScale;
			public bool supportsRotation;
		}

		public struct MeshCollection : IArenaDisposable {
			List<NativeArray<Vector3> > vertexBuffers;
			List<NativeArray<int> > triangleBuffers;
			public NativeArray<RasterizationMesh> meshes;
#if UNITY_EDITOR
			public List<(UnityEngine.Object, Mesh)> meshesUnreadableAtRuntime;
#endif

			public MeshCollection (List<NativeArray<Vector3> > vertexBuffers, List<NativeArray<int> > triangleBuffers, NativeArray<RasterizationMesh> meshes
#if UNITY_EDITOR
								   , List<(UnityEngine.Object, Mesh)> meshesUnreadableAtRuntime
#endif
								   ) {
				this.vertexBuffers = vertexBuffers;
				this.triangleBuffers = triangleBuffers;
				this.meshes = meshes;
#if UNITY_EDITOR
				this.meshesUnreadableAtRuntime = meshesUnreadableAtRuntime;
#endif
			}

			void IArenaDisposable.DisposeWith (DisposeArena arena) {
				for (int i = 0; i < vertexBuffers.Count; i++) {
					arena.Add(vertexBuffers[i]);
					arena.Add(triangleBuffers[i]);
				}
				arena.Add(meshes);
			}
		}

		[BurstCompile]
		static void CalculateBounds (ref UnsafeSpan<float3> vertices, ref float4x4 localToWorldMatrix, out Bounds bounds) {
			if (vertices.Length == 0) {
				bounds = new Bounds();
			} else {
				float3 max = float.NegativeInfinity;
				float3 min = float.PositiveInfinity;
				for (uint i = 0; i < vertices.Length; i++) {
					var v = math.transform(localToWorldMatrix, vertices[i]);
					max = math.max(max, v);
					min = math.min(min, v);
				}
				bounds = new Bounds((min+max)*0.5f, max-min);
			}
		}

		public MeshCollection Finalize () {
#if UNITY_EDITOR
			// This skips the Mesh.isReadable check
			Mesh.MeshDataArray data = UnityEditor.MeshUtility.AcquireReadOnlyMeshData(meshData);
#else
			Mesh.MeshDataArray data = Mesh.AcquireReadOnlyMeshData(meshData);
#endif
			var meshes = new NativeArray<RasterizationMesh>(this.meshes.Count, Allocator.Persistent);
			int meshBufferOffset = vertexBuffers.Count;

			Profiler.BeginSample("Copying vertices");
			// TODO: We should be able to hold the `data` for the whole scan and not have to copy all vertices/triangles
			for (int i = 0; i < data.Length; i++) {
				MeshUtility.GetMeshData(data, i, out var verts, out var tris);
				vertexBuffers.Add(verts);
				triangleBuffers.Add(tris);
			}
			Profiler.EndSample();

			Profiler.BeginSample("Creating RasterizationMeshes");
			for (int i = 0; i < meshes.Length; i++) {
				var gatheredMesh = this.meshes[i];
				int bufferIndex;
				if (gatheredMesh.meshDataIndex >= 0) {
					bufferIndex = meshBufferOffset + gatheredMesh.meshDataIndex;
				} else {
					bufferIndex = -(gatheredMesh.meshDataIndex+1);
				}

				var bounds = gatheredMesh.bounds;
				var vertexSpan = vertexBuffers[bufferIndex].Reinterpret<float3>().AsUnsafeReadOnlySpan();
				if (bounds == new Bounds()) {
					// Recalculate bounding box
					float4x4 m = gatheredMesh.matrix;
					CalculateBounds(ref vertexSpan, ref m, out bounds);
				}

				var triangles = triangleBuffers[bufferIndex];
				meshes[i] = new RasterizationMesh {
					vertices = vertexSpan,
					triangles = triangles.AsUnsafeSpan().Slice(gatheredMesh.indexStart, (gatheredMesh.indexEnd != -1 ? gatheredMesh.indexEnd : triangles.Length) - gatheredMesh.indexStart),
					area = gatheredMesh.area,
					areaIsTag = gatheredMesh.areaIsTag,
					bounds = bounds,
					matrix = gatheredMesh.matrix,
					solid = gatheredMesh.solid,
					doubleSided = gatheredMesh.doubleSided,
					flatten = gatheredMesh.flatten,
				};
			}
			Profiler.EndSample();

			cachedMeshes.Clear();
			ObjectPoolSimple<Dictionary<MeshCacheItem, int> >.Release(ref cachedMeshes);
			ListPool<GatheredMesh>.Release(ref this.meshes);

			data.Dispose();

			return new MeshCollection(
				vertexBuffers,
				triangleBuffers,
				meshes
#if UNITY_EDITOR
				, this.meshesUnreadableAtRuntime
#endif
				);
		}

		/// <summary>
		/// Add vertex and triangle buffers that can later be used to create a <see cref="GatheredMesh"/>.
		///
		/// The returned index can be used in the <see cref="GatheredMesh.meshDataIndex"/> field of the <see cref="GatheredMesh"/> struct.
		///
		/// You can use the returned index multiple times with different matrices, to create instances of the same object in multiple locations.
		/// </summary>
		public int AddMeshBuffers (Vector3[] vertices, int[] triangles) {
			return AddMeshBuffers(new NativeArray<Vector3>(vertices, Allocator.Persistent), new NativeArray<int>(triangles, Allocator.Persistent));
		}

		/// <summary>
		/// Add vertex and triangle buffers that can later be used to create a <see cref="GatheredMesh"/>.
		///
		/// The returned index can be used in the <see cref="GatheredMesh.meshDataIndex"/> field of the <see cref="GatheredMesh"/> struct.
		///
		/// You can use the returned index multiple times with different matrices, to create instances of the same object in multiple locations.
		/// </summary>
		public int AddMeshBuffers (NativeArray<Vector3> vertices, NativeArray<int> triangles) {
			var meshDataIndex = -vertexBuffers.Count-1;

			vertexBuffers.Add(vertices);
			triangleBuffers.Add(triangles);
			return meshDataIndex;
		}

		/// <summary>Add a mesh to the list of meshes to rasterize</summary>
		public void AddMesh (Renderer renderer, Mesh gatheredMesh) {
			if (ConvertMeshToGatheredMesh(renderer, gatheredMesh, out var gm)) {
				meshes.Add(gm);
			}
		}

		/// <summary>Add a mesh to the list of meshes to rasterize</summary>
		public void AddMesh (GatheredMesh gatheredMesh) {
			meshes.Add(gatheredMesh);
		}

		/// <summary>Holds info about a mesh to be rasterized</summary>
		public struct GatheredMesh {
			/// <summary>
			/// Index in the meshData array.
			/// Can be retrieved from the <see cref="RecastMeshGatherer.AddMeshBuffers"/> method.
			/// </summary>
			public int meshDataIndex;
			/// <summary>
			/// Area ID of the mesh. 0 means walkable, and -1 indicates that the mesh should be treated as unwalkable.
			/// Other positive values indicate a custom area ID which will create a seam in the navmesh.
			/// </summary>
			public int area;
			/// <summary>Start index in the triangle array</summary>
			public int indexStart;
			/// <summary>End index in the triangle array. -1 indicates the end of the array.</summary>
			public int indexEnd;


			/// <summary>World bounds of the mesh. Assumed to already be multiplied with the <see cref="matrix"/>.</summary>
			public Bounds bounds;

			/// <summary>Matrix to transform the vertices by</summary>
			public Matrix4x4 matrix;

			/// <summary>
			/// If true then the mesh will be treated as solid and its interior will be unwalkable.
			/// The unwalkable region will be the minimum to maximum y coordinate in each cell.
			/// </summary>
			public bool solid;
			/// <summary>See <see cref="RasterizationMesh.doubleSided"/></summary>
			public bool doubleSided;
			/// <summary>See <see cref="RasterizationMesh.flatten"/></summary>
			public bool flatten;
			/// <summary>See <see cref="RasterizationMesh.areaIsTag"/></summary>
			public bool areaIsTag;

			/// <summary>
			/// Recalculate the <see cref="bounds"/> from the vertices.
			///
			/// The bounds will not be recalculated immediately.
			/// </summary>
			public void RecalculateBounds () {
				// This will cause the bounds to be recalculated later
				bounds = new Bounds();
			}

			public void ApplyNavmeshModifier (RecastNavmeshModifier navmeshModifier) {
				area = AreaFromSurfaceMode(navmeshModifier.mode, navmeshModifier.surfaceID);
				areaIsTag = navmeshModifier.mode == RecastNavmeshModifier.Mode.WalkableSurfaceWithTag;
				solid |= navmeshModifier.solid;
			}

			public void ApplyLayerModification (RecastGraph.PerLayerModification modification) {
				area = AreaFromSurfaceMode(modification.mode, modification.surfaceID);
				areaIsTag = modification.mode == RecastNavmeshModifier.Mode.WalkableSurfaceWithTag;
			}
		}

		enum MeshType {
			Mesh,
			Box,
			Capsule,
		}

		struct MeshCacheItem : IEquatable<MeshCacheItem> {
			public MeshType type;
			public Mesh mesh;
			public int rows;
			public int quantizedHeight;

			public MeshCacheItem (Mesh mesh) {
				type = MeshType.Mesh;
				this.mesh = mesh;
				rows = 0;
				quantizedHeight = 0;
			}

			public static readonly MeshCacheItem Box = new MeshCacheItem {
				type = MeshType.Box,
				mesh = null,
				rows = 0,
				quantizedHeight = 0,
			};

			public bool Equals (MeshCacheItem other) {
				return type == other.type && mesh == other.mesh && rows == other.rows && quantizedHeight == other.quantizedHeight;
			}

			public override int GetHashCode () {
				return (((int)type * 31 ^ (mesh != null ? mesh.GetHashCode() : -1)) * 31 ^ rows) * 31 ^ quantizedHeight;
			}
		}

		bool MeshFilterShouldBeIncluded (MeshFilter filter) {
			if (filter.TryGetComponent<Renderer>(out var rend)) {
				if (filter.sharedMesh != null && rend.enabled && (((1 << filter.gameObject.layer) & mask) != 0 || (tagMask.Count > 0 && tagMask.Contains(filter.tag)))) {
					if (!(filter.TryGetComponent<RecastNavmeshModifier>(out var rmo) && rmo.enabled)) {
						return true;
					}
				}
			}
			return false;
		}

		bool ConvertMeshToGatheredMesh (Renderer renderer, Mesh mesh, out GatheredMesh gatheredMesh) {
			// Ignore meshes that do not have a Position vertex attribute.
			// This can happen for meshes that are empty, i.e. have no vertices at all.
			if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Position)) {
				gatheredMesh = default;
				return false;
			}

#if !UNITY_EDITOR
			if (!mesh.isReadable) {
				// Cannot scan this
				if (!anyNonReadableMesh) {
					Debug.LogError("Some meshes could not be included when scanning the graph because they are marked as not readable. This includes the mesh '" + mesh.name + "'. You need to mark the mesh with read/write enabled in the mesh importer. Alternatively you can only rasterize colliders and not meshes. Mesh Collider meshes still need to be readable.", mesh);
				}
				anyNonReadableMesh = true;
				gatheredMesh = default;
				return false;
			}
#endif

			renderer.GetSharedMaterials(dummyMaterials);
			var submeshStart = renderer is MeshRenderer mrend ? mrend.subMeshStartIndex : 0;
			var submeshCount = dummyMaterials.Count;

			int indexStart = 0;
			int indexEnd = -1;
			if (submeshStart > 0 || submeshCount < mesh.subMeshCount) {
				var a = mesh.GetSubMesh(submeshStart);
				var b = mesh.GetSubMesh(submeshStart + submeshCount - 1);
				indexStart = a.indexStart;
				indexEnd = b.indexStart + b.indexCount;
			}

			// Check the cache to avoid allocating
			// a new array unless necessary
			if (!cachedMeshes.TryGetValue(new MeshCacheItem(mesh), out int meshBufferIndex)) {
#if UNITY_EDITOR
				if (!mesh.isReadable) meshesUnreadableAtRuntime.Add((renderer, mesh));
#endif
				meshBufferIndex = meshData.Count;
				meshData.Add(mesh);
				cachedMeshes[new MeshCacheItem(mesh)] = meshBufferIndex;
			}

			gatheredMesh = new GatheredMesh {
				meshDataIndex = meshBufferIndex,
				bounds = renderer.bounds,
				indexStart = indexStart,
				indexEnd = indexEnd,
				matrix = renderer.localToWorldMatrix,
				doubleSided = false,
				flatten = false,
			};
			return true;
		}

		GatheredMesh? GetColliderMesh (MeshCollider collider, Matrix4x4 localToWorldMatrix) {
			if (collider.sharedMesh != null) {
				Mesh mesh = collider.sharedMesh;

				// Ignore meshes that do not have a Position vertex attribute.
				// This can happen for meshes that are empty, i.e. have no vertices at all.
				if (!mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Position)) {
					return null;
				}

#if !UNITY_EDITOR
				if (!mesh.isReadable) {
					// Cannot scan this
					if (!anyNonReadableMesh) {
						Debug.LogError("Some mesh collider meshes could not be included when scanning the graph because they are marked as not readable. This includes the mesh '" + mesh.name + "'. You need to mark the mesh with read/write enabled in the mesh importer.", mesh);
					}
					anyNonReadableMesh = true;
					return null;
				}
#endif

				// Check the cache to avoid allocating
				// a new array unless necessary
				if (!cachedMeshes.TryGetValue(new MeshCacheItem(mesh), out int meshDataIndex)) {
#if UNITY_EDITOR
					if (!mesh.isReadable) meshesUnreadableAtRuntime.Add((collider, mesh));
#endif
					meshDataIndex = meshData.Count;
					meshData.Add(mesh);
					cachedMeshes[new MeshCacheItem(mesh)] = meshDataIndex;
				}

				return new GatheredMesh {
						   meshDataIndex = meshDataIndex,
						   bounds = collider.bounds,
						   areaIsTag = false,
						   area = 0,
						   indexStart = 0,
						   indexEnd = -1,
						   // Treat the collider as solid iff the collider is convex
						   solid = collider.convex,
						   matrix = localToWorldMatrix,
						   doubleSided = false,
						   flatten = false,
				};
			}

			return null;
		}

		public void CollectSceneMeshes () {
			if (tagMask.Count > 0 || mask != 0) {
				// This is unfortunately the fastest way to find all mesh filters.. and it is not particularly fast.
				// Note: We have to sort these because the recast graph is not completely deterministic in terms of ordering of meshes.
				// Different ordering can in rare cases lead to different spans being merged which can lead to different navmeshes.
				var meshFilters = UnityCompatibility.FindObjectsByTypeSorted<MeshFilter>();
				bool containedStatic = false;

				for (int i = 0; i < meshFilters.Length; i++) {
					MeshFilter filter = meshFilters[i];

					if (!MeshFilterShouldBeIncluded(filter)) continue;

					// Note, guaranteed to have a renderer as MeshFilterShouldBeIncluded checks for it.
					// but it can be either a MeshRenderer or a SkinnedMeshRenderer
					filter.TryGetComponent<Renderer>(out var rend);

					if (rend.isPartOfStaticBatch) {
						// Statically batched meshes cannot be used due to Unity limitations
						// log a warning about this
						containedStatic = true;
					} else {
						// Only include it if it intersects with the graph
						if (rend.bounds.Intersects(bounds)) {
							if (ConvertMeshToGatheredMesh(rend, filter.sharedMesh, out var gatheredMesh)) {
								gatheredMesh.ApplyLayerModification(modificationsByLayer[filter.gameObject.layer]);
								meshes.Add(gatheredMesh);
							}
						}
					}
				}

				if (containedStatic) {
					Debug.LogWarning("Some meshes were statically batched. These meshes can not be used for navmesh calculation" +
						" due to technical constraints.\nDuring runtime scripts cannot access the data of meshes which have been statically batched.\n" +
						"One way to solve this problem is to use cached startup (Save & Load tab in the inspector) to only calculate the graph when the game is not playing.");
				}
			}
		}

		static int AreaFromSurfaceMode (RecastNavmeshModifier.Mode mode, int surfaceID) {
			switch (mode) {
			default:
			case RecastNavmeshModifier.Mode.UnwalkableSurface:
				return -1;
			case RecastNavmeshModifier.Mode.WalkableSurface:
				return 0;
			case RecastNavmeshModifier.Mode.WalkableSurfaceWithSeam:
			case RecastNavmeshModifier.Mode.WalkableSurfaceWithTag:
				return surfaceID;
			}
		}

		/// <summary>Find all relevant RecastNavmeshModifier components and create ExtraMeshes for them</summary>
		public void CollectRecastNavmeshModifiers () {
			var buffer = ListPool<RecastNavmeshModifier>.Claim();

			// Get all recast navmesh modifiers inside the bounds
			RecastNavmeshModifier.GetAllInBounds(buffer, bounds);

			// Create an RasterizationMesh object
			// for each RecastNavmeshModifier
			for (int i = 0; i < buffer.Count; i++) {
				AddNavmeshModifier(buffer[i]);
			}

			ListPool<RecastNavmeshModifier>.Release(ref buffer);
		}

		void AddNavmeshModifier (RecastNavmeshModifier navmeshModifier) {
			if (navmeshModifier.includeInScan == RecastNavmeshModifier.ScanInclusion.AlwaysExclude) return;
			if (navmeshModifier.includeInScan == RecastNavmeshModifier.ScanInclusion.Auto && (((mask >> navmeshModifier.gameObject.layer) & 1) == 0 && !tagMask.Contains(navmeshModifier.tag))) return;

			navmeshModifier.ResolveMeshSource(out var filter, out var collider, out var collider2D);

			if (filter != null) {
				// Add based on mesh filter
				Mesh mesh = filter.sharedMesh;
				if (filter.TryGetComponent<MeshRenderer>(out var rend) && mesh != null) {
					if (ConvertMeshToGatheredMesh(rend, filter.sharedMesh, out var gatheredMesh)) {
						gatheredMesh.ApplyNavmeshModifier(navmeshModifier);
						meshes.Add(gatheredMesh);
					}
				}
			} else if (collider != null) {
				// Add based on collider

				if (ConvertColliderToGatheredMesh(collider) is GatheredMesh rmesh) {
					rmesh.ApplyNavmeshModifier(navmeshModifier);
					meshes.Add(rmesh);
				}
			} else if (collider2D != null) {
				// 2D colliders are handled separately
			} else {
				if (navmeshModifier.geometrySource == RecastNavmeshModifier.GeometrySource.Auto) {
					Debug.LogError("Couldn't get geometry source for RecastNavmeshModifier ("+navmeshModifier.gameObject.name +"). It didn't have a collider or MeshFilter+Renderer attached", navmeshModifier.gameObject);
				} else {
					Debug.LogError("Couldn't get geometry source for RecastNavmeshModifier ("+navmeshModifier.gameObject.name +"). It didn't have a " + navmeshModifier.geometrySource + " attached", navmeshModifier.gameObject);
				}
			}
		}

		public void CollectTerrainMeshes (bool rasterizeTrees, float desiredChunkSize) {
			// Find all terrains in the scene
			var terrains = Terrain.activeTerrains;

			if (terrains.Length > 0) {
				// Loop through all terrains in the scene
				for (int j = 0; j < terrains.Length; j++) {
					if (terrains[j].terrainData == null) continue;

					Profiler.BeginSample("Generate terrain chunks");
					bool anyTerrainChunks = GenerateTerrainChunks(terrains[j], bounds, desiredChunkSize);
					Profiler.EndSample();

					// Don't rasterize trees if the terrain did not intersect the graph bounds
					if (rasterizeTrees && anyTerrainChunks) {
						Profiler.BeginSample("Find tree meshes");
						// Rasterize all tree colliders on this terrain object
						CollectTreeMeshes(terrains[j]);
						Profiler.EndSample();
					}
				}
			}
		}

		static int NonNegativeModulus (int x, int m) {
			int r = x%m;
			return r < 0 ? r+m : r;
		}

		/// <summary>Returns ceil(lhs/rhs), i.e lhs/rhs rounded up</summary>
		static int CeilDivision (int lhs, int rhs) {
			return (lhs + rhs - 1)/rhs;
		}

		bool GenerateTerrainChunks (Terrain terrain, Bounds bounds, float desiredChunkSize) {
			var terrainData = terrain.terrainData;

			if (terrainData == null)
				throw new ArgumentException("Terrain contains no terrain data");

			Vector3 offset = terrain.GetPosition();
			var terrainSize = terrainData.size;
			Vector3 center = offset + terrainSize * 0.5F;

			// Figure out the bounds of the terrain in world space
			var terrainBounds = new Bounds(center, terrainSize);

			// Only include terrains which intersects the graph
			if (!terrainBounds.Intersects(bounds))
				return false;

			// Original heightmap size
			int heightmapWidth = terrainData.heightmapResolution;
			int heightmapDepth = terrainData.heightmapResolution;

			// Size of a single sample
			Vector3 sampleSize = terrainData.heightmapScale;
			sampleSize.y = terrainSize.y;

			// Make chunks at least 12 quads wide
			// since too small chunks just decreases performance due
			// to the overhead of checking for bounds and similar things
			const int MinChunkSize = 12;

			// Find the number of samples along each edge that corresponds to a world size of desiredChunkSize
			// Then round up to the nearest multiple of terrainSampleSize
			var chunkSizeAlongX = Mathf.CeilToInt(Mathf.Max(desiredChunkSize / (sampleSize.x * terrainDownsamplingFactor), MinChunkSize)) * terrainDownsamplingFactor;
			var chunkSizeAlongZ = Mathf.CeilToInt(Mathf.Max(desiredChunkSize / (sampleSize.z * terrainDownsamplingFactor), MinChunkSize)) * terrainDownsamplingFactor;
			chunkSizeAlongX = Mathf.Min(chunkSizeAlongX, heightmapWidth);
			chunkSizeAlongZ = Mathf.Min(chunkSizeAlongZ, heightmapDepth);

			Vector2Int startSample, chunks;
			if (float.IsFinite(bounds.size.x)) {
				startSample = new Vector2Int(
					Mathf.FloorToInt((bounds.min.x - offset.x) / sampleSize.x),
					Mathf.FloorToInt((bounds.min.z - offset.z) / sampleSize.z)
					);

				// Ensure we always start at a multiple of the terrainDownsamplingFactor.
				// Otherwise, the generated meshes may not look the same, depending on the original bounds.
				// Not rounding would not technically be wrong, but this makes it more predictable.
				startSample.x -= NonNegativeModulus(startSample.x, terrainDownsamplingFactor);
				startSample.y -= NonNegativeModulus(startSample.y, terrainDownsamplingFactor);

				// Figure out which chunks might intersect the bounding box
				var worldChunkSizeAlongX = chunkSizeAlongX * sampleSize.x;
				var worldChunkSizeAlongZ = chunkSizeAlongZ * sampleSize.z;
				chunks = new Vector2Int(
					Mathf.CeilToInt((bounds.max.x - offset.x - startSample.x * sampleSize.x) / worldChunkSizeAlongX),
					Mathf.CeilToInt((bounds.max.z - offset.z - startSample.y * sampleSize.z) / worldChunkSizeAlongZ)
					);
			} else {
				startSample = new Vector2Int(0, 0);
				chunks = new Vector2Int(CeilDivision(heightmapWidth, chunkSizeAlongX), CeilDivision(heightmapDepth, chunkSizeAlongZ));
			}

			// Figure out which samples we need from the terrain heightmap
			var sampleRect = new IntRect(0, 0, chunks.x * chunkSizeAlongX, chunks.y * chunkSizeAlongZ).Offset(startSample);
			var allSamples = new IntRect(0, 0, heightmapWidth - 1, heightmapDepth - 1);
			// Clamp the samples to the heightmap bounds
			sampleRect = IntRect.Intersection(sampleRect, allSamples);
			if (!sampleRect.IsValid()) return false;

			chunks = new Vector2Int(CeilDivision(sampleRect.Width, chunkSizeAlongX), CeilDivision(sampleRect.Height, chunkSizeAlongZ));

			Profiler.BeginSample("Get heightmap data");
			float[, ] heights = terrainData.GetHeights(
				sampleRect.xmin,
				sampleRect.ymin,
				sampleRect.Width,
				sampleRect.Height
				);
			bool[, ] holes = terrainData.GetHoles(
				sampleRect.xmin,
				sampleRect.ymin,
				sampleRect.Width - 1,
				sampleRect.Height - 1
				);
			Profiler.EndSample();

			unsafe {
				var heightsSpan = new UnsafeSpan<float>(heights, out var gcHandle1);
				var holesSpan = new UnsafeSpan<bool>(holes, out var gcHandle2);

				var chunksOffset = offset + new Vector3(sampleRect.xmin * sampleSize.x, 0, sampleRect.ymin * sampleSize.z);
				var chunksMatrix = Matrix4x4.TRS(chunksOffset, Quaternion.identity, sampleSize);
				for (int z = 0; z < chunks.y; z++) {
					for (int x = 0; x < chunks.x; x++) {
						Profiler.BeginSample("Generate chunk");
						GenerateHeightmapChunk(
							ref heightsSpan,
							ref holesSpan,
							sampleRect.Width,
							sampleRect.Height,
							x * chunkSizeAlongX,
							z * chunkSizeAlongZ,
							chunkSizeAlongX,
							chunkSizeAlongZ,
							terrainDownsamplingFactor,
							out var vertsSpan,
							out var trisSpan
							);
						Profiler.EndSample();

						var verts = vertsSpan.MoveToNativeArray(Allocator.Persistent);
						var tris = trisSpan.MoveToNativeArray(Allocator.Persistent);

						var meshDataIndex = AddMeshBuffers(verts, tris);

						var chunk = new GatheredMesh {
							meshDataIndex = meshDataIndex,
							// An empty bounding box indicates that it should be calculated from the vertices later.
							bounds = new Bounds(),
							indexStart = 0,
							indexEnd = -1,
							areaIsTag = false,
							area = 0,
							solid = false,
							matrix = chunksMatrix,
							doubleSided = false,
							flatten = false,
						};
						chunk.ApplyLayerModification(modificationsByLayer[terrain.gameObject.layer]);

						meshes.Add(chunk);
					}
				}

				UnsafeUtility.ReleaseGCObject(gcHandle1);
				UnsafeUtility.ReleaseGCObject(gcHandle2);
			}
			return true;
		}

		/// <summary>Generates a terrain chunk mesh</summary>
		[BurstCompile]
		public static void GenerateHeightmapChunk (ref UnsafeSpan<float> heights, ref UnsafeSpan<bool> holes, int heightmapWidth, int heightmapDepth, int x0, int z0, int width, int depth, int stride, out UnsafeSpan<Vector3> verts, out UnsafeSpan<int> tris) {
			// Downsample to a smaller mesh (full resolution will take a long time to rasterize)
			// Round up the width to the nearest multiple of terrainSampleSize and then add 1
			// (off by one because there are vertices at the edge of the mesh)
			int resultWidth = CeilDivision(Mathf.Min(width, heightmapWidth - x0), stride) + 1;
			int resultDepth = CeilDivision(Mathf.Min(depth, heightmapDepth - z0), stride) + 1;

			// Create a mesh from the heightmap
			var numVerts = resultWidth * resultDepth;
			var numTris = (resultWidth-1)*(resultDepth-1)*2*3;
			verts = new UnsafeSpan<Vector3>(Allocator.Persistent, numVerts);
			tris = new UnsafeSpan<int>(Allocator.Persistent, numTris);

			// Create lots of vertices
			for (int z = 0; z < resultDepth; z++) {
				int sampleZ = Math.Min(z0 + z*stride, heightmapDepth-1);
				for (int x = 0; x < resultWidth; x++) {
					int sampleX = Math.Min(x0 + x*stride, heightmapWidth-1);
					verts[z*resultWidth + x] = new Vector3(sampleX, heights[sampleZ*heightmapWidth + sampleX], sampleZ);
				}
			}

			// Create the mesh by creating triangles in a grid like pattern
			int triangleIndex = 0;
			for (int z = 0; z < resultDepth-1; z++) {
				for (int x = 0; x < resultWidth-1; x++) {
					// Try to check if the center of the cell is a hole or not.
					// Note that the holes array has a size which is 1 less than the heightmap size
					int sampleX = Math.Min(x0 + stride/2 + x*stride, heightmapWidth-2);
					int sampleZ = Math.Min(z0 + stride/2 + z*stride, heightmapDepth-2);

					if (holes[sampleZ*(heightmapWidth-1) + sampleX]) {
						// Not a hole, generate a mesh here
						tris[triangleIndex]   = z*resultWidth + x;
						tris[triangleIndex+1] = (z+1)*resultWidth + x+1;
						tris[triangleIndex+2] = z*resultWidth + x+1;
						triangleIndex += 3;
						tris[triangleIndex]   = z*resultWidth + x;
						tris[triangleIndex+1] = (z+1)*resultWidth + x;
						tris[triangleIndex+2] = (z+1)*resultWidth + x+1;
						triangleIndex += 3;
					}
				}
			}

			tris = tris.Slice(0, triangleIndex);
		}

		void CollectTreeMeshes (Terrain terrain) {
			Profiler.BeginSample("Get tree data from terrain");
			TerrainData data = terrain.terrainData;
			var treeInstances = data.treeInstances;
			var treePrototypes = data.treePrototypes;
			var terrainPos = terrain.transform.position;
			var terrainSize = data.size;
			Profiler.EndSample();

			Profiler.BeginSample("Process tree prototypes");
			var treeInfos = new TreeInfo[treePrototypes.Length];
			for (int i = 0; i < treePrototypes.Length; i++) {
				TreePrototype prot = treePrototypes[i];
				// Make sure that the tree prefab exists
				if (prot.prefab == null) {
					continue;
				}

				if (!cachedTreePrefabs.TryGetValue(prot.prefab, out TreeInfo treeInfo)) {
					treeInfo.submeshes = new List<GatheredMesh>();

					// The unity terrain system only supports rotation for trees with a LODGroup on the root object.
					// Unity still sets the instance.rotation field to values even they are not used, so we need to explicitly check for this.
					treeInfo.supportsRotation = prot.prefab.TryGetComponent<LODGroup>(out var dummy);
					treeInfo.localScale = prot.prefab.transform.localScale;

					var colliders = ListPool<Collider>.Claim();
					var rootMatrixInv = prot.prefab.transform.localToWorldMatrix.inverse;
					prot.prefab.GetComponentsInChildren(false, colliders);
					for (int j = 0; j < colliders.Count; j++) {
						// The prefab has a collider, use that instead
						var collider = colliders[j];

						// Generate a mesh from the collider
						if (ConvertColliderToGatheredMesh(collider, rootMatrixInv * collider.transform.localToWorldMatrix) is GatheredMesh mesh) {
							// For trees, we only suppport generating a mesh from a collider. So we ignore the navmeshModifier.geometrySource field.
							if (collider.gameObject.TryGetComponent<RecastNavmeshModifier>(out var navmeshModifier) && navmeshModifier.enabled) {
								if (navmeshModifier.includeInScan == RecastNavmeshModifier.ScanInclusion.AlwaysExclude) continue;

								mesh.ApplyNavmeshModifier(navmeshModifier);
							} else {
								mesh.ApplyLayerModification(modificationsByLayer[collider.gameObject.layer]);
							}

							// The bounds are incorrectly based on collider.bounds, and may not even be initialized by the physics system.
							// It is incorrect because the collider is on the prefab, not on the tree instance
							// so we need to recalculate the bounds based on the actual vertex positions
							mesh.RecalculateBounds();

							treeInfo.submeshes.Add(mesh);
						}
					}

					ListPool<Collider>.Release(ref colliders);
					cachedTreePrefabs[prot.prefab] = treeInfo;
				}
				treeInfos[i] = treeInfo;
			}
			Profiler.EndSample();

			Profiler.BeginSample("Convert trees to meshes");
			for (int i = 0; i < treeInstances.Length; i++) {
				TreeInstance instance = treeInstances[i];
				var treeInfo = treeInfos[instance.prototypeIndex];
				if (treeInfo.submeshes == null || treeInfo.submeshes.Count == 0) continue;

				var treePosition = terrainPos +  Vector3.Scale(instance.position, terrainSize);
				var instanceSize = new Vector3(instance.widthScale, instance.heightScale, instance.widthScale);
				var prefabScale = Vector3.Scale(instanceSize, treeInfo.localScale);
				var rotation = treeInfo.supportsRotation ? Quaternion.AngleAxis(instance.rotation * Mathf.Rad2Deg, Vector3.up) : Quaternion.identity;
				var matrix = Matrix4x4.TRS(treePosition, rotation, prefabScale);

				for (int j = 0; j < treeInfo.submeshes.Count; j++) {
					var item = treeInfo.submeshes[j];
					item.matrix = matrix * item.matrix;
					meshes.Add(item);
				}
			}
			Profiler.EndSample();
		}

		bool ShouldIncludeCollider (Collider collider) {
			if (!collider.enabled || collider.isTrigger || !collider.bounds.Intersects(bounds) || (collider.TryGetComponent<RecastNavmeshModifier>(out var rmo) && rmo.enabled)) return false;

			var go = collider.gameObject;
			if (((mask >> go.layer) & 1) != 0) return true;

			// Iterate over the tag mask and use CompareTag instead of tagMask.Includes(collider.tag), as this will not allocate.
			for (int i = 0; i < tagMask.Count; i++) {
				if (go.CompareTag(tagMask[i])) return true;
			}
			return false;
		}

		public void CollectColliderMeshes () {
			if (tagMask.Count == 0 && mask == 0) return;

			// Find all colliders that could possibly be inside the bounds
			// TODO: Benchmark?
			// Repeatedly do a OverlapBox check and make the buffer larger if it's too small.
			int numColliders = 256;
			Collider[] colliderBuffer = null;
			bool finiteBounds = math.all(math.isfinite(bounds.extents));
			if (!finiteBounds) {
				colliderBuffer = UnityCompatibility.FindObjectsByTypeSorted<Collider>();
				numColliders = colliderBuffer.Length;
			} else {
				do {
					if (colliderBuffer != null) ArrayPool<Collider>.Release(ref colliderBuffer);
					colliderBuffer = ArrayPool<Collider>.Claim(numColliders * 4);
					numColliders = physicsScene.OverlapBox(bounds.center, bounds.extents, colliderBuffer, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
				} while (numColliders == colliderBuffer.Length);
			}


			for (int i = 0; i < numColliders; i++) {
				Collider collider = colliderBuffer[i];

				if (ShouldIncludeCollider(collider)) {
					if (ConvertColliderToGatheredMesh(collider) is GatheredMesh mesh) {
						mesh.ApplyLayerModification(modificationsByLayer[collider.gameObject.layer]);
						meshes.Add(mesh);
					}
				}
			}

			if (finiteBounds) ArrayPool<Collider>.Release(ref colliderBuffer);
		}

		/// <summary>
		/// Box Collider triangle indices can be reused for multiple instances.
		/// Warning: This array should never be changed
		/// </summary>
		private readonly static int[] BoxColliderTris = {
			0, 1, 2,
			0, 2, 3,

			6, 5, 4,
			7, 6, 4,

			0, 5, 1,
			0, 4, 5,

			1, 6, 2,
			1, 5, 6,

			2, 7, 3,
			2, 6, 7,

			3, 4, 0,
			3, 7, 4
		};

		/// <summary>
		/// Box Collider vertices can be reused for multiple instances.
		/// Warning: This array should never be changed
		/// </summary>
		private readonly static Vector3[] BoxColliderVerts = {
			new Vector3(-1, -1, -1),
			new Vector3(1, -1, -1),
			new Vector3(1, -1, 1),
			new Vector3(-1, -1, 1),

			new Vector3(-1, 1, -1),
			new Vector3(1, 1, -1),
			new Vector3(1, 1, 1),
			new Vector3(-1, 1, 1),
		};

		/// <summary>
		/// Rasterizes a collider to a mesh.
		/// This will pass the col.transform.localToWorldMatrix to the other overload of this function.
		/// </summary>
		GatheredMesh? ConvertColliderToGatheredMesh (Collider col) {
			return ConvertColliderToGatheredMesh(col, col.transform.localToWorldMatrix);
		}

		/// <summary>
		/// Rasterizes a collider to a mesh assuming it's vertices should be multiplied with the matrix.
		/// Note that the bounds of the returned RasterizationMesh is based on collider.bounds. So you might want to
		/// call myExtraMesh.RecalculateBounds on the returned mesh to recalculate it if the collider.bounds would
		/// not give the correct value.
		/// </summary>
		public GatheredMesh? ConvertColliderToGatheredMesh (Collider col, Matrix4x4 localToWorldMatrix) {
			if (col is BoxCollider box) {
				return RasterizeBoxCollider(box, localToWorldMatrix);
			} else if (col is SphereCollider || col is CapsuleCollider) {
				var scollider = col as SphereCollider;
				var ccollider = col as CapsuleCollider;

				float radius = scollider != null ? scollider.radius : ccollider.radius;
				float height = scollider != null ? 0 : (ccollider.height*0.5f/radius) - 1;
				Quaternion rot = Quaternion.identity;
				// Capsule colliders can be aligned along the X, Y or Z axis
				if (ccollider != null) rot = Quaternion.Euler(ccollider.direction == 2 ? 90 : 0, 0, ccollider.direction == 0 ? 90 : 0);
				Matrix4x4 matrix = Matrix4x4.TRS(scollider != null ? scollider.center : ccollider.center, rot, Vector3.one*radius);

				matrix = localToWorldMatrix * matrix;

				return RasterizeCapsuleCollider(radius, height, col.bounds, matrix);
			} else if (col is MeshCollider collider) {
				return GetColliderMesh(collider, localToWorldMatrix);
			}

			return null;
		}

		GatheredMesh RasterizeBoxCollider (BoxCollider collider, Matrix4x4 localToWorldMatrix) {
			Matrix4x4 matrix = Matrix4x4.TRS(collider.center, Quaternion.identity, collider.size*0.5f);

			matrix = localToWorldMatrix * matrix;

			if (!cachedMeshes.TryGetValue(MeshCacheItem.Box, out int meshDataIndex)) {
				meshDataIndex = AddMeshBuffers(BoxColliderVerts, BoxColliderTris);
				cachedMeshes[MeshCacheItem.Box] = meshDataIndex;
			}

			return new GatheredMesh {
					   meshDataIndex = meshDataIndex,
					   bounds = collider.bounds,
					   indexStart = 0,
					   indexEnd = -1,
					   areaIsTag = false,
					   area = 0,
					   solid = true,
					   matrix = matrix,
					   doubleSided = false,
					   flatten = false,
			};
		}

		static int CircleSteps (Matrix4x4 matrix, float radius, float maxError) {
			// Take the maximum scale factor among the 3 axes.
			// If the current matrix has a uniform scale then they are all the same.
			var maxScaleFactor = math.sqrt(math.max(math.max(math.lengthsq((Vector3)matrix.GetColumn(0)), math.lengthsq((Vector3)matrix.GetColumn(1))), math.lengthsq((Vector3)matrix.GetColumn(2))));
			var realWorldRadius = radius * maxScaleFactor;

			var cosAngle = 1 - maxError / realWorldRadius;
			int steps = cosAngle < 0 ? 3 : (int)math.ceil(math.PI / math.acos(cosAngle));
			return steps;
		}

		/// <summary>
		/// If a circle is approximated by fewer segments, it will be slightly smaller than the original circle.
		/// This factor is used to adjust the radius of the circle so that the resulting circle will have roughly the same area as the original circle.
		/// </summary>
		static float CircleRadiusAdjustmentFactor (int steps) {
			return 0.5f * (1 - math.cos(2 * math.PI / steps));
		}

		GatheredMesh RasterizeCapsuleCollider (float radius, float height, Bounds bounds, Matrix4x4 localToWorldMatrix) {
			// Calculate the number of rows to use
			int rows = CircleSteps(localToWorldMatrix, radius, maxColliderApproximationError);

			int cols = rows;

			var cacheItem = new MeshCacheItem {
				type = MeshType.Capsule,
				mesh = null,
				rows = rows,
				// Capsules that differ by a very small amount in height will be rasterized in the same way
				quantizedHeight = Mathf.RoundToInt(height/maxColliderApproximationError),
			};

			if (!cachedMeshes.TryGetValue(cacheItem, out var meshDataIndex)) {
				// Generate a sphere/capsule mesh

				var verts = new NativeArray<Vector3>(rows*cols + 2, Allocator.Persistent);

				var tris = new NativeArray<int>(rows*cols*2*3, Allocator.Persistent);

				for (int r = 0; r < rows; r++) {
					for (int c = 0; c < cols; c++) {
						verts[c + r*cols] = new Vector3(Mathf.Cos(c*Mathf.PI*2/cols)*Mathf.Sin((r*Mathf.PI/(rows-1))), Mathf.Cos((r*Mathf.PI/(rows-1))) + (r < rows/2 ? height : -height), Mathf.Sin(c*Mathf.PI*2/cols)*Mathf.Sin((r*Mathf.PI/(rows-1))));
					}
				}

				verts[verts.Length-1] = Vector3.up;
				verts[verts.Length-2] = Vector3.down;

				int triIndex = 0;

				for (int i = 0, j = cols-1; i < cols; j = i++) {
					tris[triIndex + 0] = (verts.Length-1);
					tris[triIndex + 1] = (0*cols + j);
					tris[triIndex + 2] = (0*cols + i);
					triIndex += 3;
				}

				for (int r = 1; r < rows; r++) {
					for (int i = 0, j = cols-1; i < cols; j = i++) {
						tris[triIndex + 0] = (r*cols + i);
						tris[triIndex + 1] = (r*cols + j);
						tris[triIndex + 2] = ((r-1)*cols + i);
						triIndex += 3;

						tris[triIndex + 0] = ((r-1)*cols + j);
						tris[triIndex + 1] = ((r-1)*cols + i);
						tris[triIndex + 2] = (r*cols + j);
						triIndex += 3;
					}
				}

				for (int i = 0, j = cols-1; i < cols; j = i++) {
					tris[triIndex + 0] = (verts.Length-2);
					tris[triIndex + 1] = ((rows-1)*cols + j);
					tris[triIndex + 2] = ((rows-1)*cols + i);
					triIndex += 3;
				}

				UnityEngine.Assertions.Assert.AreEqual(triIndex, tris.Length);

				// TOOD: Avoid allocating original C# array
				// Store custom vertex buffers as negative indices
				meshDataIndex = AddMeshBuffers(verts, tris);
				cachedMeshes[cacheItem] = meshDataIndex;
			}

			return new GatheredMesh {
					   meshDataIndex = meshDataIndex,
					   bounds = bounds,
					   areaIsTag = false,
					   area = 0,
					   indexStart = 0,
					   indexEnd = -1,
					   solid = true,
					   matrix = localToWorldMatrix,
					   doubleSided = false,
					   flatten = false,
			};
		}

		bool ShouldIncludeCollider2D (Collider2D collider) {
			// Note: Some things are already checked, namely that:
			// - collider.enabled is true
			// - that the bounds intersect (at least approxmately)
			// - that the collider is not a trigger

			// This is not completely analogous to ShouldIncludeCollider, as this one will
			// always include the collider if it has an attached RecastNavmeshModifier, while
			// 3D colliders handle RecastNavmeshModifier components separately.
			if (((mask >> collider.gameObject.layer) & 1) != 0) return true;
			if ((collider.attachedRigidbody as Component ?? collider).TryGetComponent<RecastNavmeshModifier>(out var rmo) && rmo.enabled && rmo.includeInScan == RecastNavmeshModifier.ScanInclusion.AlwaysInclude) return true;

			for (int i = 0; i < tagMask.Count; i++) {
				if (collider.CompareTag(tagMask[i])) return true;
			}
			return false;
		}

		public void Collect2DColliderMeshes () {
			if (tagMask.Count == 0 && mask == 0) return;

			// Find all colliders that could possibly be inside the bounds
			// TODO: Benchmark?
			int numColliders = 256;
			Collider2D[] colliderBuffer = null;
			bool finiteBounds = math.isfinite(bounds.extents.x) && math.isfinite(bounds.extents.y);

			if (!finiteBounds) {
				colliderBuffer = UnityCompatibility.FindObjectsByTypeSorted<Collider2D>();
				numColliders = colliderBuffer.Length;
			} else {
				// Repeatedly do a OverlapArea check and make the buffer larger if it's too small.
				var min2D = (Vector2)bounds.min;
				var max2D = (Vector2)bounds.max;
				var filter = new ContactFilter2D().NoFilter();
				// It would be nice to add the layer mask filter here as well,
				// but we cannot since a collider may have a RecastNavmeshModifier component
				// attached, and in that case we want to include it even if it is on an excluded layer.
				// The user may also want to include objects based on tags.
				// But we can at least exclude all triggers.
				filter.useTriggers = false;

				do {
					if (colliderBuffer != null) ArrayPool<Collider2D>.Release(ref colliderBuffer);
					colliderBuffer = ArrayPool<Collider2D>.Claim(numColliders * 4);
					numColliders = physicsScene2D.OverlapArea(min2D, max2D, filter, colliderBuffer);
				} while (numColliders == colliderBuffer.Length);
			}

			// Filter out colliders that should not be included
			for (int i = 0; i < numColliders; i++) {
				if (!ShouldIncludeCollider2D(colliderBuffer[i])) colliderBuffer[i] = null;
			}

			int shapeMeshCount = ColliderMeshBuilder2D.GenerateMeshesFromColliders(colliderBuffer, numColliders, maxColliderApproximationError, out var vertices, out var indices, out var shapeMeshes);
			var bufferIndex = AddMeshBuffers(vertices.Reinterpret<Vector3>(), indices);

			for (int i = 0; i < shapeMeshCount; i++) {
				var shape = shapeMeshes[i];

				// Skip if the shape is not inside the bounds.
				// This is a more granular check than the one done by the OverlapArea call above,
				// since each collider may generate multiple shapes with different bounds.
				// This is particularly important for TilemapColliders which may generate a lot of shapes.
				if (!bounds.Intersects(shape.bounds)) continue;

				var coll = colliderBuffer[shape.tag];
				(coll.attachedRigidbody as Component ?? coll).TryGetComponent<RecastNavmeshModifier>(out var navmeshModifier);

				var rmesh = new GatheredMesh {
					meshDataIndex = bufferIndex,
					bounds = shape.bounds,
					indexStart = shape.startIndex,
					indexEnd = shape.endIndex,
					areaIsTag = false,
					// Colliders default to being unwalkable
					area = -1,
					solid = false,
					matrix = shape.matrix,
					doubleSided = true,
					flatten = true,
				};

				if (navmeshModifier != null) {
					if (navmeshModifier.includeInScan == RecastNavmeshModifier.ScanInclusion.AlwaysExclude) continue;
					rmesh.ApplyNavmeshModifier(navmeshModifier);
				} else {
					rmesh.ApplyLayerModification(modificationsByLayer2D[coll.gameObject.layer]);
				}

				// 2D colliders are never solid
				rmesh.solid = false;

				meshes.Add(rmesh);
			}

			if (finiteBounds) ArrayPool<Collider2D>.Release(ref colliderBuffer);
			shapeMeshes.Dispose();
		}
	}
}
