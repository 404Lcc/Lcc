using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding {
	using UnityEngine.Profiling;
	using Pathfinding.Util;
	using Pathfinding.Serialization;
	using Unity.Collections;
	using Unity.Jobs;
	using Pathfinding.Graphs.Navmesh.Jobs;
	using Pathfinding.Graphs.Navmesh;
	using Unity.Mathematics;

	/// <summary>
	/// Generates graphs based on navmeshes.
	/// [Open online documentation to see images]
	///
	/// Navmeshes are meshes in which each triangle defines a walkable area.
	/// These are great because the AI can get so much more information on how it can walk.
	/// Polygons instead of points mean that the <see cref="FunnelModifier"/> can produce really nice looking paths, and the graphs are also really fast to search
	/// and have a low memory footprint because fewer nodes are usually needed to describe the same area compared to grid graphs.
	///
	/// The navmesh graph requires that you create a navmesh manually. The package also has support for generating navmeshes automatically using the <see cref="RecastGraph"/>.
	///
	/// For a tutorial on how to configure a navmesh graph, take a look at getstarted2 (view in online documentation for working links).
	///
	/// [Open online documentation to see images]
	///
	/// \section navmeshgraph-inspector Inspector
	/// [Open online documentation to see images]
	///
	/// \inspectorField{Source Mesh, sourceMesh}
	/// \inspectorField{Offset, offset}
	/// \inspectorField{Rotation, rotation}
	/// \inspectorField{Scale, scale}
	/// \inspectorField{Recalculate Normals, recalculateNormals}
	/// \inspectorField{Affected By Navmesh Cuts, enableNavmeshCutting}
	/// \inspectorField{Agent Radius, navmeshCuttingCharacterRadius}
	/// \inspectorField{Initial Penalty, initialPenalty}
	///
	/// See: <see cref="RecastGraph"/>
	/// </summary>
	[JsonOptIn]
	[Pathfinding.Util.Preserve]
	public class NavMeshGraph : NavmeshBase, IUpdatableGraph {
		/// <summary>Mesh to construct navmesh from</summary>
		[JsonMember]
		public Mesh sourceMesh;

		/// <summary>Offset in world space</summary>
		[JsonMember]
		public Vector3 offset;

		/// <summary>Rotation in degrees</summary>
		[JsonMember]
		public Vector3 rotation;

		/// <summary>Scale of the graph</summary>
		[JsonMember]
		public float scale = 1;

		/// <summary>
		/// Determines how normals are calculated.
		/// Disable for spherical graphs or other complicated surfaces that allow the agents to e.g walk on walls or ceilings.
		///
		/// By default the normals of the mesh will be flipped so that they point as much as possible in the upwards direction.
		/// The normals are important when connecting adjacent nodes. Two adjacent nodes will only be connected if they are oriented the same way.
		/// This is particularly important if you have a navmesh on the walls or even on the ceiling of a room. Or if you are trying to make a spherical navmesh.
		/// If you do one of those things then you should set disable this setting and make sure the normals in your source mesh are properly set.
		///
		/// If you for example take a look at the image below. In the upper case then the nodes on the bottom half of the
		/// mesh haven't been connected with the nodes on the upper half because the normals on the lower half will have been
		/// modified to point inwards (as that is the direction that makes them face upwards the most) while the normals on
		/// the upper half point outwards. This causes the nodes to not connect properly along the seam. When this option
		/// is set to false instead the nodes are connected properly as in the original mesh all normals point outwards.
		/// [Open online documentation to see images]
		///
		/// The default value of this field is true to reduce the risk for errors in the common case. If a mesh is supplied that
		/// has all normals pointing downwards and this option is false, then some methods like <see cref="PointOnNavmesh"/> will not work correctly
		/// as they assume that the normals point upwards. For a more complicated surface like a spherical graph those methods make no sense anyway
		/// as there is no clear definition of what it means to be "inside" a triangle when there is no clear up direction.
		/// </summary>
		[JsonMember]
		public bool recalculateNormals = true;

		/// <summary>
		/// Cached bounding box minimum of <see cref="sourceMesh"/>.
		/// This is important when the graph has been saved to a file and is later loaded again, but the original mesh does not exist anymore (or has been moved).
		/// In that case we still need to be able to find the bounding box since the <see cref="CalculateTransform"/> method uses it.
		/// </summary>
		[JsonMember]
		Vector3 cachedSourceMeshBoundsMin;

		/// <summary>
		/// Radius to use when expanding navmesh cuts.
		///
		/// See: <see cref="NavmeshCut.radiusExpansionMode"/>
		/// </summary>
		[JsonMember]
		public float navmeshCuttingCharacterRadius = 0.5f;

		public override float NavmeshCuttingCharacterRadius => navmeshCuttingCharacterRadius;

		public override bool RecalculateNormals => recalculateNormals;

		public override float TileWorldSizeX => forcedBoundsSize.x;

		public override float TileWorldSizeZ => forcedBoundsSize.z;

		// Tiles are not supported, so this is irrelevant
		public override float MaxTileConnectionEdgeDistance => 0f;

		/// <summary>
		/// True if the point is inside the bounding box of this graph.
		///
		/// Warning: If your input mesh is entirely flat, the bounding box will also end up entirely flat (with a height of zero), this will make this function return false for almost all points, unless they are at exactly the right y-coordinate.
		///
		/// Note: For an unscanned graph, this will always return false.
		/// </summary>
		public override bool IsInsideBounds (Vector3 point) {
			if (this.tiles == null || this.tiles.Length == 0 || sourceMesh == null) return false;

			var local = transform.InverseTransform(point);
			var size = sourceMesh.bounds.size*scale;

			// Allow a small margin
			const float EPS = 0.0001f;

			return local.x >= -EPS && local.y >= -EPS && local.z >= -EPS && local.x <= size.x + EPS && local.y <= size.y + EPS && local.z <= size.z + EPS;
		}

		/// <summary>
		/// World bounding box for the graph.
		///
		/// This always contains the whole graph.
		///
		/// Note: Since this is an axis-aligned bounding box, it may not be particularly tight if the graph is significantly rotated.
		///
		/// If no mesh has been assigned, this will return a zero sized bounding box at the origin.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public override Bounds bounds {
			get {
				if (sourceMesh == null) return default;
				var m = (float4x4)CalculateTransform().matrix;
				var b = new ToWorldMatrix(new float3x3(m.c0.xyz, m.c1.xyz, m.c2.xyz)).ToWorld(new Bounds(Vector3.zero, sourceMesh.bounds.size * scale));
				return b;
			}
		}

		public override GraphTransform CalculateTransform () {
			return new GraphTransform(Matrix4x4.TRS(offset, Quaternion.Euler(rotation), Vector3.one) * Matrix4x4.TRS(sourceMesh != null ? sourceMesh.bounds.min * scale : cachedSourceMeshBoundsMin * scale, Quaternion.identity, Vector3.one));
		}

		class NavMeshGraphUpdatePromise : IGraphUpdatePromise {
			public NavMeshGraph graph;
			public List<GraphUpdateObject> graphUpdates;

			public void Apply (IGraphUpdateContext ctx) {
				for (int i = 0; i < graphUpdates.Count; i++) {
					var graphUpdate = graphUpdates[i];
					UpdateArea(graphUpdate, graph);
					// TODO: Not strictly accurate, since the update may affect node that have a surface that extends
					// outside of the bounds.
					ctx.DirtyBounds(graphUpdate.bounds);
				}
			}
		}

		IGraphUpdatePromise IUpdatableGraph.ScheduleGraphUpdates (List<GraphUpdateObject> graphUpdates) => new NavMeshGraphUpdatePromise { graph = this, graphUpdates = graphUpdates };

		public static void UpdateArea (GraphUpdateObject o, INavmeshHolder graph) {
			Bounds bounds = graph.transform.InverseTransform(o.bounds);

			// Bounding rectangle with integer coordinates
			var irect = new IntRect(
				Mathf.FloorToInt(bounds.min.x*Int3.Precision),
				Mathf.FloorToInt(bounds.min.z*Int3.Precision),
				Mathf.CeilToInt(bounds.max.x*Int3.Precision),
				Mathf.CeilToInt(bounds.max.z*Int3.Precision)
				);

			// Corners of the bounding rectangle
			var a = new Int3(irect.xmin, 0, irect.ymin);
			var b = new Int3(irect.xmin, 0, irect.ymax);
			var c = new Int3(irect.xmax, 0, irect.ymin);
			var d = new Int3(irect.xmax, 0, irect.ymax);

			var ymin = ((Int3)bounds.min).y;
			var ymax = ((Int3)bounds.max).y;

			// Loop through all nodes and check if they intersect the bounding box
			graph.GetNodes(_node => {
				var node = _node as TriangleMeshNode;

				bool inside = false;

				int allLeft = 0;
				int allRight = 0;
				int allTop = 0;
				int allBottom = 0;

				// Check bounding box rect in XZ plane
				for (int v = 0; v < 3; v++) {
					Int3 p = node.GetVertexInGraphSpace(v);

					if (irect.Contains(p.x, p.z)) {
						inside = true;
						break;
					}

					if (p.x < irect.xmin) allLeft++;
					if (p.x > irect.xmax) allRight++;
					if (p.z < irect.ymin) allTop++;
					if (p.z > irect.ymax) allBottom++;
				}

				if (!inside && (allLeft == 3 || allRight == 3 || allTop == 3 || allBottom == 3)) {
					return;
				}

				// Check if the polygon edges intersect the bounding rect
				for (int v = 0; v < 3; v++) {
					int v2 = v > 1 ? 0 : v+1;

					Int3 vert1 = node.GetVertexInGraphSpace(v);
					Int3 vert2 = node.GetVertexInGraphSpace(v2);

					if (VectorMath.SegmentsIntersectXZ(a, b, vert1, vert2)) { inside = true; break; }
					if (VectorMath.SegmentsIntersectXZ(a, c, vert1, vert2)) { inside = true; break; }
					if (VectorMath.SegmentsIntersectXZ(c, d, vert1, vert2)) { inside = true; break; }
					if (VectorMath.SegmentsIntersectXZ(d, b, vert1, vert2)) { inside = true; break; }
				}

				// Check if the node contains any corner of the bounding rect
				if (inside || node.ContainsPointInGraphSpace(a) || node.ContainsPointInGraphSpace(b) || node.ContainsPointInGraphSpace(c) || node.ContainsPointInGraphSpace(d)) {
					inside = true;
				}

				if (!inside) {
					return;
				}

				int allAbove = 0;
				int allBelow = 0;

				// Check y coordinate
				for (int v = 0; v < 3; v++) {
					Int3 p = node.GetVertexInGraphSpace(v);
					if (p.y < ymin) allBelow++;
					if (p.y > ymax) allAbove++;
				}

				// Polygon is either completely above the bounding box or completely below it
				if (allBelow == 3 || allAbove == 3) return;

				// Triangle is inside the bounding box!
				// Update it!
				o.WillUpdateNode(node);
				o.Apply(node);
			});
		}

		class NavMeshGraphScanPromise : IGraphUpdatePromise {
			public NavMeshGraph graph;
			bool emptyGraph;
			GraphTransform transform;
			NavmeshTile[] tiles;
			Vector3 forcedBoundsSize;
			IntRect tileRect;
			NavmeshUpdates.NavmeshUpdateSettings cutSettings;

			public IEnumerator<JobHandle> Prepare () {
				var sourceMesh = graph.sourceMesh;
				graph.cachedSourceMeshBoundsMin = sourceMesh != null ? sourceMesh.bounds.min : Vector3.zero;
				transform = graph.CalculateTransform();

				if (sourceMesh == null) {
					emptyGraph = true;
					yield break;
				}

				if (!sourceMesh.isReadable) {
					Debug.LogError("The source mesh " + sourceMesh.name + " is not readable. Enable Read/Write in the mesh's import settings.", sourceMesh);
					emptyGraph = true;
					yield break;
				}

				Profiler.BeginSample("GetMeshData");
				var meshDatas = Mesh.AcquireReadOnlyMeshData(sourceMesh);
				MeshUtility.GetMeshData(meshDatas, 0, out var vertices, out var indices);
				meshDatas.Dispose();
				Profiler.EndSample();

				// Convert the vertices to graph space
				// so that the minimum of the bounding box of the mesh is at the origin
				// (the vertices will later be transformed to world space)
				var scale = graph.scale;
				var meshToGraphSpace = Matrix4x4.TRS(-sourceMesh.bounds.min * scale, Quaternion.identity, Vector3.one * scale);

				var promise = JobBuildTileMeshFromVertices.Schedule(vertices, indices, meshToGraphSpace, graph.RecalculateNormals);
				forcedBoundsSize = sourceMesh.bounds.size * scale;
				tileRect = new IntRect(0, 0, 0, 0);
				tiles = new NavmeshTile[tileRect.Area];
				var tilesGCHandle = System.Runtime.InteropServices.GCHandle.Alloc(tiles);
				var tileLayout = new TileLayout(new Bounds(transform.Transform(forcedBoundsSize*0.5f), forcedBoundsSize), Quaternion.Euler(graph.rotation), 0.001f, 0, false);
				cutSettings = new NavmeshUpdates.NavmeshUpdateSettings(graph, tileLayout);

				var cutPromise = RecastBuilder.CutTiles(graph, cutSettings.clipperLookup, tileLayout).Schedule(promise);
				var tileNodeConnections = new NativeArray<JobCalculateTriangleConnections.TileNodeConnectionsUnsafe>(tiles.Length, Allocator.Persistent);
				var postCutInput = cutPromise.GetValue();
				var preCutInput = promise.GetValue();

				NativeArray<TileMesh.TileMeshUnsafe> finalTileMeshes;
				if (postCutInput.tileMeshes.tileMeshes.IsCreated) {
					UnityEngine.Assertions.Assert.AreEqual(postCutInput.tileMeshes.tileMeshes.Length, tileRect.Area);
					finalTileMeshes = postCutInput.tileMeshes.tileMeshes;
				} else {
					finalTileMeshes = preCutInput.tileMeshes.tileMeshes;
				}

				var calculateConnectionsJob = new JobCalculateTriangleConnections {
					tileMeshes = finalTileMeshes,
					nodeConnections = tileNodeConnections,
				}.Schedule(cutPromise.handle);

				var createTilesJob = new JobCreateTiles {
					// If any cutting is done, we need to save the pre-cut data to be able to re-cut tiles later
					preCutTileMeshes = postCutInput.tileMeshes.tileMeshes.IsCreated ? preCutInput.tileMeshes.tileMeshes : default,
					tileMeshes = finalTileMeshes,
					tiles = tilesGCHandle,
					tileRect = tileRect,
					graphTileCount = new Vector2Int(tileRect.Width, tileRect.Height),
					graphIndex = graph.graphIndex,
					initialPenalty = graph.initialPenalty,
					recalculateNormals = graph.recalculateNormals,
					graphToWorldSpace = transform.matrix,
					tileWorldSize = tileLayout.TileWorldSize,
				}.Schedule(cutPromise.handle);
				var applyConnectionsJob = new JobWriteNodeConnections {
					tiles = tilesGCHandle,
					nodeConnections = tileNodeConnections,
				}.Schedule(JobHandle.CombineDependencies(createTilesJob, calculateConnectionsJob));

				yield return applyConnectionsJob;

				// This has already been used in the createTilesJob
				promise.Complete().Dispose();
				cutPromise.Complete().Dispose();
				tileNodeConnections.Dispose();

				vertices.Dispose();
				indices.Dispose();

				tilesGCHandle.Free();
			}

			public void Apply (IGraphUpdateContext ctx) {
				if (emptyGraph) {
					graph.forcedBoundsSize = Vector3.zero;
					graph.transform = transform;
					graph.tileZCount = graph.tileXCount = 1;
					TriangleMeshNode.SetNavmeshHolder(AstarPath.active.data.GetGraphIndex(graph), graph);
					graph.FillWithEmptyTiles();
					graph.navmeshUpdateData.Dispose();
					return;
				}

				// Destroy all previous nodes (if any)
				graph.DestroyAllNodes();

				// Initialize all nodes that were created in the jobs
				for (int j = 0; j < tiles.Length; j++) AstarPath.active.InitializeNodes(tiles[j].nodes);

				// Assign all data as one atomic operation (from the viewpoint of the main thread)
				graph.forcedBoundsSize = forcedBoundsSize;
				graph.transform = transform;
				graph.tileXCount = tileRect.Width;
				graph.tileZCount = tileRect.Height;
				graph.tiles = tiles;
				TriangleMeshNode.SetNavmeshHolder(graph.active.data.GetGraphIndex(graph), graph);
				cutSettings.AttachToGraph();

				if (graph.OnRecalculatedTiles != null) graph.OnRecalculatedTiles(tiles.Clone() as NavmeshTile[]);
			}
		}

		protected override IGraphUpdatePromise ScanInternal (bool async) => new NavMeshGraphScanPromise { graph = this };

		protected override void PostDeserialization (GraphSerializationContext ctx) {
			if (ctx.meta.version < AstarSerializer.V4_3_74) {
				this.navmeshCuttingCharacterRadius = 0;
			}
			base.PostDeserialization(ctx);
		}
	}
}
