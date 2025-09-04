using UnityEngine;
using System.Collections;

namespace Pathfinding {
	using Pathfinding.Util;
	using Unity.Mathematics;
	using UnityEngine.Profiling;
	using Pathfinding.Graphs.Navmesh;
	using Pathfinding.Jobs;
	using Pathfinding.Drawing;
	using System.Collections.Generic;
	using Unity.Jobs;

	/// <summary>
	/// Moves a grid or recast graph to follow a target.
	///
	/// This is useful if you have a very large, or even infinite, world, but pathfinding is only necessary in a small region around an object (for example the player).
	/// This component will move a graph around so that its center stays close to the <see cref="target"/> object.
	///
	/// Note: This component can only be used with grid graphs, layered grid graphs and (tiled) recast graphs.
	///
	/// <b>Usage</b>
	/// Take a look at the example scene called "Procedural" for an example of how to use this script
	///
	/// Attach this to some object in the scene and assign the target to e.g the player.
	/// Then the graph will follow that object around as it moves.
	///
	/// [Open online documentation to see videos]
	///
	/// [Open online documentation to see videos]
	///
	/// <b>Performance</b>
	/// When the graph is moved you may notice an fps drop.
	/// If this grows too large you can try a few things:
	///
	/// General advice:
	/// - Turn on multithreading (A* Inspector -> Settings)
	/// - Make sure you have 'Show Graphs' disabled in the A* inspector, since gizmos in the scene view can take some
	///   time to update when the graph moves, and thus make it seem like this script is slower than it actually is.
	///
	/// For grid graphs:
	/// - Avoid using any erosion in the grid graph settings. This is relatively slow. Each erosion iteration requires expanding the region that is updated by 1 node.
	/// - Reduce the grid size or resolution.
	/// - Reduce the <see cref="updateDistance"/>. This will make the updates smaller but more frequent.
	///   This only works to some degree however since an update has an inherent overhead.
	/// - Disable Height Testing or Collision Testing in the grid graph if you can. This can give a performance boost
	///   since fewer calls to the physics engine need to be done.
	///
	/// For recast graphs:
	/// - Rasterize colliders instead of meshes. This is typically faster.
	/// - Use a reasonable tile size. Very small tiles can cause more overhead, and too large tiles might mean that you are updating too much in one go.
	///    Typical values are around 64 to 256 voxels.
	/// - Use a larger cell size. A lower cell size will give better quality graphs, but it will also be slower to scan.
	///
	/// The graph updates will be offloaded to worker threads as much as possible.
	///
	/// See: large-worlds (view in online documentation for working links)
	/// </summary>
	[AddComponentMenu("Pathfinding/Procedural Graph Mover")]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/proceduralgraphmover.html")]
	public class ProceduralGraphMover : VersionedMonoBehaviour {
		/// <summary>
		/// Grid graphs will be updated if the target is more than this number of nodes from the graph center.
		/// Note that this is in nodes, not world units.
		///
		/// Note: For recast graphs, this setting has no effect.
		/// </summary>
		public float updateDistance = 10;

		/// <summary>Graph will be moved to follow this target</summary>
		public Transform target;

		/// <summary>True while the graph is being updated by this script</summary>
		public bool updatingGraph { get; private set; }

		/// <summary>
		/// Graph to update.
		/// This will be set at Start based on <see cref="graphIndex"/>.
		/// During runtime you may set this to any graph or to null to disable updates.
		/// </summary>
		public NavGraph graph;

		/// <summary>
		/// Index for the graph to update.
		/// This will be used at Start to set <see cref="graph"/>.
		///
		/// This is an index into the AstarPath.active.data.graphs array.
		/// </summary>
		[HideInInspector]
		public int graphIndex;

		void Start () {
			if (AstarPath.active == null) throw new System.Exception("There is no AstarPath object in the scene");

			// If one creates this component via a script then they may have already set the graph field.
			// In that case don't replace it.
			if (graph == null) {
				if (graphIndex < 0) throw new System.Exception("Graph index should not be negative");
				if (graphIndex >= AstarPath.active.data.graphs.Length) throw new System.Exception("The ProceduralGraphMover was configured to use graph index " + graphIndex + ", but only " + AstarPath.active.data.graphs.Length + " graphs exist");

				graph = AstarPath.active.data.graphs[graphIndex];
				if (!(graph is GridGraph || graph is RecastGraph)) throw new System.Exception("The ProceduralGraphMover was configured to use graph index " + graphIndex + " but that graph either does not exist or is not a GridGraph, LayerGridGraph or RecastGraph");

				if (graph is RecastGraph rg && !rg.useTiles) Debug.LogWarning("The ProceduralGraphMover component only works with tiled recast graphs. Enable tiling in the recast graph inspector.", this);
			}

			UpdateGraph();
		}

		void OnDisable () {
			// Just in case this script is performing an update while being disabled
			if (AstarPath.active != null) AstarPath.active.FlushWorkItems();

			updatingGraph = false;
		}

		/// <summary>Update is called once per frame</summary>
		void Update () {
			if (AstarPath.active == null || graph == null || !graph.isScanned) return;

			if (graph is GridGraph gg) {
				// Calculate where the graph center and the target position is in graph space
				// In graph space, (0,0,0) is bottom left corner of the graph
				// For grid graphs, one unit along the X and Z axes in graph space equals the distance between two nodes.
				// The Y axis still uses world units
				var graphCenterInGraphSpace = gg.transform.InverseTransform(gg.center);
				var targetPositionInGraphSpace = gg.transform.InverseTransform(target.position);

				// Check the distance in graph space
				// We only care about the X and Z axes since the Y axis is the "height" coordinate of the nodes (in graph space)
				// We only care about the plane that the nodes are placed in
				if (VectorMath.SqrDistanceXZ(graphCenterInGraphSpace, targetPositionInGraphSpace) > updateDistance*updateDistance) {
					UpdateGraph();
				}
			} else if (graph is RecastGraph rg) {
				UpdateGraph();
			} else {
				throw new System.Exception("ProceduralGraphMover cannot be used with graphs of type " + graph.GetType().Name);
			}
		}

		/// <summary>
		/// Updates the graph asynchronously.
		/// This will move the graph so that the target's position is (roughly) the center of the graph.
		/// If the graph is already being updated, the call will be ignored.
		///
		/// The image below shows which nodes will be updated when the graph moves.
		/// The whole graph is not recalculated each time it is moved, but only those
		/// nodes that have to be updated, the rest will keep their old values.
		/// The image is a bit simplified but it shows the main idea.
		/// [Open online documentation to see images]
		///
		/// If you want to move the graph synchronously then pass false to the async parameter.
		/// </summary>
		public void UpdateGraph (bool async = true) {
			if (!enabled) throw new System.InvalidOperationException("This component has been disabled");

			if (updatingGraph) {
				// We are already updating the graph
				// so ignore this call
				return;
			}

			if (graph is GridGraph gg) {
				UpdateGridGraph(gg, async);
			} else if (graph is RecastGraph rg) {
				var delta = RecastGraphTileShift(rg, target.position);
				if (delta.x != 0 || delta.y != 0) {
					updatingGraph = true;
					UpdateRecastGraph(rg, delta, async);
				}
			}
		}

		void UpdateGridGraph (GridGraph graph, bool async) {
			// Start a work item for updating the graph
			// This will pause the pathfinding threads
			// so that it is safe to update the graph
			// and then do it over several frames
			// to avoid too large FPS drops

			updatingGraph = true;
			List<(IGraphUpdatePromise, IEnumerator<JobHandle>)> promises = new List<(IGraphUpdatePromise, IEnumerator<JobHandle>)>();
			AstarPath.active.AddWorkItem(new AstarWorkItem(
				ctx => {
				// Find the direction that we want to move the graph in.
				// Calculate this in graph space (where a distance of one is the size of one node)
				Vector3 dir = graph.transform.InverseTransformVector(target.position - graph.center);

				// Snap to a whole number of nodes to offset in each direction
				int dx = Mathf.RoundToInt(dir.x);
				int dz = Mathf.RoundToInt(dir.z);

				if (dx != 0 || dz != 0) {
					var promise = graph.TranslateInDirection(dx, dz);
					promises.Add((promise, promise.Prepare()));
				}
			},
				(ctx, force) => {
				if (GraphUpdateProcessor.ProcessGraphUpdatePromises(promises, ctx, force ? TimeSlice.Infinite : TimeSlice.MillisFromNow(2)) == -1) {
					updatingGraph = false;
					return true;
				}
				return false;
			}
				));
			if (!async) AstarPath.active.FlushWorkItems();
		}

		static Vector2Int RecastGraphTileShift (RecastGraph graph, Vector3 targetCenter) {
			// Find the direction that we want to move the graph in.
			// Calcuculate this in graph space, to take the graph rotation into account
			Vector3 dir = graph.transform.InverseTransform(targetCenter) - graph.transform.InverseTransform(graph.forcedBoundsCenter);

			// Only move in one direction at a time for simplicity
			if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z)) dir.z = 0;
			else dir.x = 0;

			// Calculate how many whole tiles to move.
			// Avoid moving unless we want to move at least 0.5+#Hysteresis full tiles
			// Hysteresis must be at least 0.
			const float Hysteresis = 0.2f;
			return new Vector2Int(
				(int)(Mathf.Max(0, Mathf.Abs(dir.x) / graph.TileWorldSizeX + 0.5f - Hysteresis) * Mathf.Sign(dir.x)),
				(int)(Mathf.Max(0, Mathf.Abs(dir.z) / graph.TileWorldSizeZ + 0.5f - Hysteresis) * Mathf.Sign(dir.z))
				);
		}

		void UpdateRecastGraph (RecastGraph graph, Vector2Int delta, bool async) {
			updatingGraph = true;
			List<(IGraphUpdatePromise, IEnumerator<JobHandle>)> promises = new List<(IGraphUpdatePromise, IEnumerator<JobHandle>)>();
			AstarPath.active.AddWorkItem(new AstarWorkItem(
				ctx => {
				var promise = graph.TranslateInDirection(delta.x, delta.y);
				promises.Add((promise, promise.Prepare()));
			},
				(ctx, force) => {
				if (GraphUpdateProcessor.ProcessGraphUpdatePromises(promises, ctx, force ? TimeSlice.Infinite : TimeSlice.MillisFromNow(2)) == -1) {
					updatingGraph = false;
					return true;
				}
				return false;
			}
				));
			if (!async) AstarPath.active.FlushWorkItems();
		}
	}

	/// <summary>
	/// This class has been renamed to <see cref="ProceduralGraphMover"/>.
	///
	/// Deprecated: Use <see cref="ProceduralGraphMover"/> instead
	/// </summary>
	[System.Obsolete("This class has been renamed to ProceduralGraphMover", true)]
	public class ProceduralGridMover {
	}
}
