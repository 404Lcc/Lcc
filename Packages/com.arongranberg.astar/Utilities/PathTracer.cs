using System.Collections.Generic;
using UnityEngine;
using Pathfinding.Util;
using UnityEngine.Assertions;
using Unity.Mathematics;
using Pathfinding.Drawing;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.Profiling;
using Unity.Profiling;
using System.Runtime.CompilerServices;
using Unity.Jobs.LowLevel.Unsafe;
using Pathfinding.Collections;
using Pathfinding.Pooling;

namespace Pathfinding {
	/// <summary>
	/// Helper for following a path.
	///
	/// This struct keeps track of the path from the agent's current position to the end of the path.
	/// Whenever the agent moves you should call <see cref="UpdateStart"/> to update the path. This will automatically
	/// update the path if the agent has moved to the next node, or repair the path if the agent has been pushed
	/// away into a node which wasn't even on the original path.
	/// If the destination changes you should call <see cref="UpdateEnd"/> to update the path. This also repairs the path
	/// and it allows you to instantly get a valid path to the new destination, unless the destination has
	/// changed so much that the repair is insufficient. In that case you will have to wait for the next
	/// path recalculation. Check <see cref="isStale"/> to see if the PathTracer recommends that the path be recalculated.
	///
	/// After repairing the path, it will be valid, but it will not necessarily be the shortest possible path.
	/// Therefore it is still recommended that you recalculate the path every now and then.
	///
	/// The PathTracer stores the current path as a series of nodes. When the direction to move in is requested (by calling <see cref="GetNextCorners)"/>,
	/// a path will be found which passes through all those nodes, using the funnel algorithm to simplify the path.
	/// In some cases the path will contain inner vertices which make the path longer than it needs to be. Those will be
	/// iteratively removed until the path is as short as possible. For performance only a limited number of iterations are performed per frame,
	/// but this is usually fast enough that the simplification appears to be instant.
	///
	/// Warning: This struct allocates unmanaged memory. You must call <see cref="Dispose"/> when you are done with it, to avoid memory leaks.
	///
	/// Note: This is a struct, not a class. This means that if you need to pass it around, or return it from a property, you must use the ref keyword, as otherwise C# will just make a copy.
	///
	/// <code>
	/// using Pathfinding;
	/// using Pathfinding.Drawing;
	/// using Pathfinding.Util;
	/// using Unity.Collections;
	/// using Unity.Mathematics;
	/// using UnityEngine;
	///
	/// /** Demonstrates how to use a PathTracer.
	///  *
	///  * The script will calculate a path to a point a few meters ahead of it, and then use the PathTracer to show the next 10 corners of the path in the scene view.
	///  * If you move the object around in the scene view, you'll see the path update in real time.
	///  */
	/// public class PathTracerTest : MonoBehaviour {
	///     PathTracer tracer;
	///
	///     /** Create a new movement plane that indicates that the agent moves in the XZ plane.
	///      * This is the default for 3D games.
	///      */
	///     NativeMovementPlane movementPlane => new NativeMovementPlane(Quaternion.identity);
	///     ABPath lastCalculatedPath;
	///     public PathRequestSettings pathRequestSettings = PathRequestSettings.Default;
	///
	///     void OnEnable () {
	///         tracer = new PathTracer(Allocator.Persistent);
	///     }
	///
	///     void OnDisable () {
	///         // Release all unmanaged memory from the path tracer, to avoid memory leaks
	///         tracer.Dispose();
	///     }
	///
	///     void Start () {
	///         // Schedule a path calculation to a point ahead of this object
	///         var path = ABPath.Construct(transform.position, transform.position + transform.forward*10, (p) => {
	///             // This callback will be called when the path has been calculated
	///             var path = p as ABPath;
	///
	///             if (path.error) {
	///                 // The path could not be calculated
	///                 Debug.LogError("Could not calculate path");
	///                 return;
	///             }
	///
	///             // Split the path into normal sequences of nodes, and off-mesh links
	///             var parts = Funnel.SplitIntoParts(path);
	///
	///             // Assign the path to the PathTracer
	///             tracer.SetPath(parts, path.path, path.originalStartPoint, path.originalEndPoint, movementPlane, pathRequestSettings, path);
	///             lastCalculatedPath = path;
	///         });
	///         path.UseSettings(pathRequestSettings);
	///         AstarPath.StartPath(path);
	///     }
	///
	///     void Update () {
	///         if (lastCalculatedPath == null || !tracer.isCreated) return;
	///
	///         // Repair the path to start from the transform's position
	///         // If you move the transform around in the scene view, you'll see the path update in real time
	///         tracer.UpdateStart(transform.position, PathTracer.RepairQuality.High, movementPlane, lastCalculatedPath.traversalProvider, lastCalculatedPath);
	///
	///         // Get up to the next 10 corners of the path
	///         var buffer = new NativeList<float3>(Allocator.Temp);
	///         NativeArray<int> scratchArray = default;
	///         tracer.GetNextCorners(buffer, 10, ref scratchArray, Allocator.Temp, lastCalculatedPath.traversalProvider, lastCalculatedPath);
	///
	///         // Draw the next 10 corners of the path in the scene view
	///         using (Draw.WithLineWidth(2)) {
	///             Draw.Polyline(buffer.AsArray(), Color.red);
	///         }
	///
	///         // Release all temporary unmanaged memory
	///         buffer.Dispose();
	///         scratchArray.Dispose();
	///     }
	/// }
	/// </code>
	/// </summary>
	[BurstCompile]
	public struct PathTracer {
		Funnel.PathPart[] parts;

		/// <summary>All nodes in the path</summary>
		CircularBuffer<GraphNode> nodes;

		/// <summary>
		/// Hashes of some important data for each node, to determine if the node has been invalidated in some way.
		///
		/// For e.g. the grid graph, this is done using the node's index in the grid. This ensures that the node is counted as invalid
		/// if the node is for example moved to the other side of the graph using the <see cref="ProceduralGraphMover"/>.
		///
		/// For all nodes, this includes if info about if the node has been destroyed, and if it is walkable.
		///
		/// This will always have the same length as the <see cref="nodes"/> array, and the absolute indices in this array will correspond to the absolute indices in the <see cref="nodes"/> array.
		/// </summary>
		CircularBuffer<int> nodeHashes;

		/// <summary>
		/// Indicates if portals are definitely not inner corners, or if they may be.
		/// For each portal, if bit 0 is set then the left side of the portal is definitely not an inner corner.
		/// If bit 1 is set that means the same thing but for the right side of the portal.
		///
		/// Should always have the same length as the portals in <see cref="funnelState"/>.
		/// </summary>
		CircularBuffer<byte> portalIsNotInnerCorner;

		Funnel.FunnelState funnelState;
		Vector3 unclampedEndPoint;
		Vector3 unclampedStartPoint;
		GraphNode startNodeInternal;

		NNConstraint nnConstraint;

		int firstPartIndex;
		bool startIsUpToDate;
		bool endIsUpToDate;

		/// <summary>
		/// If true, the first part contains destroyed nodes.
		/// This can happen if the graph is updated and some nodes are destroyed.
		///
		/// If this is true, the path is considered stale and should be recalculated.
		///
		/// The opposite is not necessarily true. If this is false, the path may still be stale.
		///
		/// See: <see cref="isStale"/>
		/// </summary>
		bool firstPartContainsDestroyedNodes;

		/// <summary>
		/// The type of graph that the current path part is on.
		///
		/// This is either a grid-like graph, or a navmesh-like graph.
		/// </summary>
		public PartGraphType partGraphType;

		/// <summary>Type of graph that the current path part is on</summary>
		public enum PartGraphType : byte {
			/// <summary>
			/// A navmesh-like graph.
			///
			/// Typically either a <see cref="NavMeshGraph"/> or a <see cref="RecastGraph"/>
			/// </summary>
			Navmesh,
			/// <summary>
			/// A grid-like graph.
			///
			/// Typically either a <see cref="GridGraph"/> or a <see cref="LayerGridGraph"/>
			/// </summary>
			Grid,
			OffMeshLink,
		}

		/// <summary>Incremented whenever the path is changed</summary>
		public ushort version { [IgnoredByDeepProfiler] get; [IgnoredByDeepProfiler] private set; }

		/// <summary>True until <see cref="Dispose"/> is called</summary>
		public readonly bool isCreated => funnelState.unwrappedPortals.IsCreated;

		/// <summary>
		/// Current start node of the path.
		/// Since the path is updated every time the agent moves, this will be the node which the agent is inside.
		///
		/// In case the path has become invalid, this will be set to the closest node to the agent, or if no such node could be found, it will be set to null.
		///
		/// Note: Not necessarily up to date unless <see cref="UpdateStart"/> has been called first.
		/// </summary>
		public GraphNode startNode {
			[IgnoredByDeepProfiler]
			readonly get => startNodeInternal != null && !startNodeInternal.Destroyed ? startNodeInternal : null;
			[IgnoredByDeepProfiler]
			private set => startNodeInternal = value;
		}

		/// <summary>
		/// True if the path is stale and should be recalculated as quickly as possible.
		/// This is true if the path has become invalid (e.g. due to a graph update), or if the destination has changed so much that we don't have a path to the destination at all.
		///
		/// For performance reasons, the agent tries to avoid checking if nodes have been destroyed unless it needs to access them to calculate its movement.
		/// Therefore, if a path is invalidated further ahead, the agent may not realize this until it has moved close enough.
		/// </summary>
		public readonly bool isStale {
			[IgnoredByDeepProfiler]
			get {
				return !endIsUpToDate || !startIsUpToDate || firstPartContainsDestroyedNodes;
			}
		}

		/// <summary>
		/// Number of parts in the path.
		/// A part is either a sequence of adjacent nodes, or an off-mesh link.
		/// </summary>
		public readonly int partCount => parts != null ? parts.Length - firstPartIndex : 0;

		/// <summary>True if there is a path to follow</summary>
		public readonly bool hasPath => partCount > 0;

		/// <summary>Start point of the path</summary>
		public readonly Vector3 startPoint => this.parts[this.firstPartIndex].startPoint;

		/// <summary>
		/// End point of the path.
		///
		/// This is not necessarily the same as the destination, as this point may be clamped to the graph.
		/// </summary>
		public readonly Vector3 endPoint => this.parts[this.parts.Length - 1].endPoint;

		/// <summary>
		/// End point of the current path part.
		///
		/// If the path has multiple parts, this is typically the start of an off-mesh link.
		/// If the path has only one part, this is the same as <see cref="endPoint"/>.
		/// </summary>
		public readonly Vector3 endPointOfFirstPart => this.parts[this.firstPartIndex].endPoint;

		/// <summary>
		/// The minimum number of corners to request from GetNextCornerIndices to ensure the path can be simplified well.
		///
		/// The path simplification algorithm requires at least 2 corners on navmesh graphs, but 3 corners on grid graphs.
		/// </summary>
		public int desiredCornersForGoodSimplification => partGraphType == PartGraphType.Grid ? 3 : 2;

		/// <summary>
		/// True if the next part in the path exists, and is a valid link.
		/// This is true if the path has at least 2 parts and the second part is an off-mesh link.
		///
		/// If any nodes in the second part have been destroyed, this will return false.
		/// </summary>
		public readonly bool isNextPartValidLink => partCount > 1 && GetPartType(1) == Funnel.PartType.OffMeshLink && !PartContainsDestroyedNodes(1);

		/// <summary>Create a new empty path tracer</summary>
		public PathTracer(Allocator allocator) {
			funnelState = new Funnel.FunnelState(16, allocator);
			parts = null;
			nodes = new CircularBuffer<GraphNode>(16);
			portalIsNotInnerCorner = new CircularBuffer<byte>(16);
			nodeHashes = new CircularBuffer<int>(16);
			unclampedEndPoint = unclampedStartPoint = Vector3.zero;
			firstPartIndex = 0;
			startIsUpToDate = false;
			endIsUpToDate = false;
			firstPartContainsDestroyedNodes = false;
			startNodeInternal = null;
			version = 1;
			nnConstraint = NNConstraint.Walkable;
			partGraphType = PartGraphType.Navmesh;
			Clear();
		}

		/// <summary>Disposes of all unmanaged memory allocated by this path tracer and resets all properties</summary>
		public void Dispose () {
			Clear();
			funnelState.Dispose();
		}

		public enum RepairQuality {
			Low,
			High
		}

		/// <summary>
		/// Update the start point of the path, clamping it to the graph, and repairing the path if necessary.
		///
		/// This may cause <see cref="isStale"/> to become true, if the path could not be repaired successfully.
		///
		/// Returns: The new start point, which has been clamped to the graph.
		///
		/// See: <see cref="UpdateEnd"/>
		/// </summary>
		public Vector3 UpdateStart (Vector3 position, RepairQuality quality, NativeMovementPlane movementPlane, ITraversalProvider traversalProvider, Path path) {
			Repair(position, true, quality, movementPlane, traversalProvider, path);
			return parts[firstPartIndex].startPoint;
		}

		/// <summary>
		/// Update the end point of the path, clamping it to the graph, and repairing the path if necessary.
		///
		/// This may cause <see cref="isStale"/> to become true, if the path could not be repaired successfully.
		///
		/// Returns: The new end point, which has been clamped to the graph.
		///
		/// See: <see cref="UpdateEnd"/>
		/// </summary>
		public Vector3 UpdateEnd (Vector3 position, RepairQuality quality, NativeMovementPlane movementPlane, ITraversalProvider traversalProvider, Path path) {
			Repair(position, false, quality, movementPlane, traversalProvider, path);
			return parts[parts.Length-1].endPoint;
		}

		void AppendNode (bool toStart, GraphNode node) {
			var partIndex = toStart ? firstPartIndex : parts.Length-1;
			ref var part = ref parts[partIndex];
			var prevNode = part.endIndex >= part.startIndex ? nodes.GetBoundaryValue(toStart) : null;

			// We can ignore appending duplicate nodes
			if (node == prevNode) return;
			if (node == null) throw new System.ArgumentNullException();

			if (part.endIndex >= part.startIndex + 1 && nodes.GetAbsolute(toStart ? part.startIndex + 1 : part.endIndex - 1) == node) {
				// Moving from A -> B -> A can be collapsed to just A. Pop B from the current path.
				if (toStart) part.startIndex++;
				else part.endIndex--;
				nodes.Pop(toStart);
				nodeHashes.Pop(toStart);
				if (partIndex == this.firstPartIndex && funnelState.leftFunnel.Length > 0) {
					funnelState.Pop(toStart);
					portalIsNotInnerCorner.Pop(toStart);
				}
				return;
			}

			if (partIndex == this.firstPartIndex) {
				if (prevNode != null) {
					Vector3 tmpLeft;
					Vector3 tmpRight;
					if (toStart) {
						if (!node.GetPortal(prevNode, out tmpLeft, out tmpRight)) {
							throw new System.NotImplementedException();
						}
					} else {
						if (!prevNode.GetPortal(node, out tmpLeft, out tmpRight)) {
							throw new System.NotImplementedException();
						}
					}
					funnelState.Push(toStart, tmpLeft, tmpRight);
					portalIsNotInnerCorner.Push(toStart, 0);
				}
			}
			nodes.Push(toStart, node);
			nodeHashes.Push(toStart, HashNode(node));
			if (toStart) {
				part.startIndex--;
			} else {
				part.endIndex++;
			}

			Assert.IsTrue(part.endIndex >= part.startIndex);
		}

		/// <summary>Appends the given nodes to the start or to the end of the path, one by one</summary>
		void AppendPath (bool toStart, CircularBuffer<GraphNode> path) {
			if (path.Length == 0) return;

			CheckInvariants();

			while (path.Length > 0) AppendNode(toStart, path.PopStart());
			if (toStart) {
				startNode = nodes.First;

				// Check a few nodes ahead to see if any have been destroyed.
				// We must do this after we have repaired the path, because repairing the path
				// may remove nodes from the path (e.g. it will take the sequence A -> B -> C + [repair C -> B] and turn it into A -> B).
				// Our invariants say that if firstPartContainsDestroyedNodes is true then the first part must contain destroyed nodes.
				var lastDestroyCheckIndex = Mathf.Min(parts[firstPartIndex].startIndex + NODES_TO_CHECK_FOR_DESTRUCTION, parts[firstPartIndex].endIndex);
				bool foundDestroyedNodes = false;
				for (int i = parts[firstPartIndex].startIndex; i <= lastDestroyCheckIndex; i++) {
					foundDestroyedNodes |= !ValidInPath(i);
				}
				firstPartContainsDestroyedNodes = foundDestroyedNodes;
			}

			CheckInvariants();
		}

		/// <summary>
		/// Checks that invariants are satisfied.
		/// This is only called in the editor for performance reasons.
		///
		/// - <see cref="firstPartIndex"/> must be in bounds of <see cref="parts"/>.
		/// - The first part must contain at least 1 node (unless there are no parts in the path at all).
		/// - The number of nodes in the first part must be equal to the number of portals in the funnel state + 1.
		/// - The number of portals in the funnel state must equal <see cref="portalIsNotInnerCorner.Length"/>.
		/// - The last node of the last part must end at the end of the path.
		/// - The first node of the first part must start at the start of the path.
		/// - <see cref="firstPartContainsDestroyedNodes"/> implies that there must be at least one destroyed node in the first part (this is an implication, not an equivalence).
		/// - If the first node is not destroyed, then <see cref="startNode"/> must be the first node in the first part.
		/// </summary>
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		void CheckInvariants () {
			// Invariant checking is stripped out of the final package
		}

		/// <summary>
		/// Removes nodes [startIndex, startIndex+toRemove) and then inserts the given nodes at startIndex.
		///
		/// Returns true if the splicing succeeded.
		/// Returns false if splicing failed because it would have to access destroyed nodes.
		/// In that case the path is left unmodified.
		/// </summary>
		/// <param name="startIndex">Absolute index of the first node to remove</param>
		/// <param name="toRemove">Number of nodes to remove</param>
		/// <param name="toInsert">Nodes to insert at startIndex. The nodes must not be destroyed. Passing null is equivalent to an empty list.</param>
		bool SplicePath (int startIndex, int toRemove, List<GraphNode> toInsert) {
			ref var part = ref parts[firstPartIndex];
			if (startIndex < part.startIndex || startIndex + toRemove - 1 > part.endIndex) throw new System.ArgumentException("This method can only handle splicing the first part of the path");

			// Ignore replacing nodes with the same nodes.
			// This is reasonably common when the path is repaired, especially for grid graphs.
			// This is mostly a performance optimization,
			// however it is required for correctness in some cases.
			// In particular, assume a grid graph path A B C D exists, but D is destroyed. Then if we'd try to
			// splice A B C into A E C, we would try to recalculate the portal between C and D if we did
			// not run this optimization.
			// This is a relatively common case when doing simplifications on grid graphs.
			if (toInsert != null) {
				int prefix = 0;
				int suffix = 0;
				while (prefix < toInsert.Count && prefix < toRemove && toInsert[prefix] == nodes.GetAbsolute(startIndex + prefix)) prefix++;

				// Check if we are replacing a sequence of nodes with the same nodes
				if (prefix == toInsert.Count && prefix == toRemove) return true;

				while (suffix < toInsert.Count - prefix && suffix < toRemove - prefix && toInsert[toInsert.Count - suffix - 1] == nodes.GetAbsolute(startIndex + toRemove - suffix - 1)) suffix++;

				toInsert.RemoveRange(toInsert.Count - suffix, suffix);
				toInsert.RemoveRange(0, prefix);
				startIndex += prefix;
				toRemove -= prefix + suffix;
				Assert.IsTrue(toInsert.Count >= 0);
				Assert.IsTrue(toRemove >= 0);
			}

			CheckInvariants();
#if UNITY_EDITOR
			if (toInsert != null) {
				for (int i = 0; i < toInsert.Count; i++) {
					if (!Valid(toInsert[i])) throw new System.ArgumentException("Cannot insert destroyed or unwalkable nodes");
				}
			}
#endif
			var numToInsert = toInsert != null ? toInsert.Count : 0;

			// We need to access the nodes next to the range we are inserting.
			// If those nodes are not valid, then we cannot continue
			if (startIndex - 1 >= part.startIndex && !ValidInPath(startIndex - 1)) return false;
			if (startIndex + toRemove <= part.endIndex && !ValidInPath(startIndex + toRemove)) return false;

			nodes.SpliceAbsolute(startIndex, toRemove, toInsert);
			nodeHashes.SpliceUninitializedAbsolute(startIndex, toRemove, numToInsert);
			if (toInsert != null) {
				for (int i = 0; i < toInsert.Count; i++) nodeHashes.SetAbsolute(startIndex + i, HashNode(toInsert[i]));
			}
			var nodesInserted = numToInsert - toRemove;

			var affectedPortalsStart1 = math.max(startIndex - 1, part.startIndex);
			var affectedPortalsEnd1 = math.min(startIndex + toRemove, part.endIndex);
			var portalsToRemove = affectedPortalsEnd1 - affectedPortalsStart1;

			part.endIndex += nodesInserted;
			for (int j = firstPartIndex + 1; j < parts.Length; j++) {
				parts[j].startIndex += nodesInserted;
				parts[j].endIndex += nodesInserted;
			}

			var tmpLeft = ListPool<float3>.Claim();
			var tmpRight = ListPool<float3>.Claim();
			var endIndex = startIndex + numToInsert - 1;

			// If we have a path A B C D E
			// and we splice it like: A B X Y Z E
			// then all portals between B|X and Z|E need to be recalculated
			var affectedPortalsStart2 = math.max(startIndex - 1, part.startIndex);
			var affectedPortalsEnd2 = math.min(endIndex + 1, part.endIndex);
			CalculateFunnelPortals(affectedPortalsStart2, affectedPortalsEnd2, tmpLeft, tmpRight);

			funnelState.Splice(affectedPortalsStart2 - part.startIndex, portalsToRemove, tmpLeft, tmpRight);
			portalIsNotInnerCorner.SpliceUninitialized(affectedPortalsStart2 - part.startIndex, portalsToRemove, tmpLeft.Count);
			for (int i = 0; i < tmpLeft.Count; i++) portalIsNotInnerCorner[affectedPortalsStart2 - part.startIndex + i] = 0;

			ListPool<float3>.Release(ref tmpLeft);
			ListPool<float3>.Release(ref tmpRight);
			CheckInvariants();
			return true;
		}

		static bool ContainsPoint (GraphNode node, Vector3 point, NativeMovementPlane plane) {
			if (node is TriangleMeshNode tnode) return tnode.ContainsPoint(point, plane);
			else return node.ContainsPoint(point);
		}

		/// <summary>
		/// Burstified function which checks if a point is inside a triangle-node and if so, projects that point onto the node's surface.
		/// Returns: If the point is inside the node.
		/// </summary>
		[BurstCompile]
		static bool ContainsAndProject (ref Int3 a, ref Int3 b, ref Int3 c, ref Vector3 p, float height, ref NativeMovementPlane movementPlane, out Vector3 projected) {
			var pa = (int3)a;
			var pb = (int3)b;
			var pc = (int3)c;
			var pp = (int3)(Int3)p;
			if (!Polygon.ContainsPoint(ref pa, ref pb, ref pc, ref pp, ref movementPlane)) {
				projected = Vector3.zero;
				return false;
			}

			var paf = (float3)(Vector3)a;
			var pbf = (float3)(Vector3)b;
			var pcf = (float3)(Vector3)c;
			var pf = (float3)p;
			var distThreshold = height * 0.5f;
			var projectedf = ProjectOnSurface(paf, pbf, pcf, pf, movementPlane.up);

			// If the agent is too far away from the surface,
			// fall back to a more thorough check
			if (math.lengthsq(projectedf - pf) > distThreshold*distThreshold) {
				projected = Vector3.zero;
				return false;
			}

			projected = (Vector3)projectedf;
			return true;
		}

		static float3 ProjectOnSurface (float3 a, float3 b, float3 c, float3 p, float3 up) {
			var nodeNormal = math.cross(c - a, b - a);
			var dot = math.dot(nodeNormal, up);
			if (math.abs(dot) > math.FLT_MIN_NORMAL) {
				var w = p - a;
				var fac = -math.dot(nodeNormal, w) / dot;
				return p + fac * up;
			} else {
				// Node plane is perpendicular to the movement plane
				return p;
			}
		}

		void Repair (Vector3 point, bool isStart, RepairQuality quality, NativeMovementPlane movementPlane, ITraversalProvider traversalProvider, Path path, bool allowCache = true) {
			int partIndex;
			GraphNode currentNode;
			bool samePoint;
			int currentIndexInPath;
			if (isStart) {
				partIndex = this.firstPartIndex;
				currentIndexInPath = parts[partIndex].startIndex;
				currentNode = nodes.GetAbsolute(currentIndexInPath);
				samePoint = unclampedStartPoint == point;
			} else {
				partIndex = this.parts.Length - 1;
				currentIndexInPath = parts[partIndex].endIndex;
				currentNode = nodes.GetAbsolute(currentIndexInPath);
				samePoint = unclampedEndPoint == point;
			}

			// Early return in case nothing has changed since last time
			// Note that the point may be outside the current node, in which case we should
			// keep the startIsUpToDate/endIsUpToDate flags as they are.
			// If the path is currently stale, it is possible that doing a full repair again
			// would find a better node (possibly even make it not stale anymore), but it is
			// common that this would just lead to wasted computations, so we don't.
			bool currentNodeValid = ValidInPath(currentIndexInPath);
			if (allowCache && samePoint && currentNodeValid) {
				return;
			}

			if (partGraphType == PartGraphType.OffMeshLink) throw new System.InvalidOperationException("Cannot repair path while on an off-mesh link");

			ref var part = ref parts[partIndex];

			if (!float.IsFinite(point.x)) {
				if (isStart) throw new System.ArgumentException("Position must be a finite vector");
				else {
					unclampedEndPoint = point;
					endIsUpToDate = false;
					// Remove all nodes except the first one from the path
					RemoveAllPartsExceptFirst();
					ref var firstPart = ref parts[firstPartIndex];
					if (firstPart.endIndex > firstPart.startIndex) SplicePath(firstPart.startIndex + 1, firstPart.endIndex - firstPart.startIndex, null);
					firstPart.endPoint = firstPart.startPoint;
					version++;

					CheckInvariants();
					return;
				}
			}

			const float height = 1.0f;
			if (currentNodeValid) {
				// Check if we are inside the same node as last frame. This is the common case.
				// We also ensure that we are not too far above the node as that might indicate
				// that we have been teleported to a point with the same XZ coordinates,
				// but for instance on a different floor of a building.
				// If so, we just project the position on the node's surface and return.
				bool insideCurrentNode = false;
				Vector3 newClampedPoint = Vector3.zero;
				if (currentNode is TriangleMeshNode triNode) {
					triNode.GetVertices(out var a, out var b, out var c);
					insideCurrentNode = ContainsAndProject(ref a, ref b, ref c, ref point, height, ref movementPlane, out newClampedPoint);
				} else if (currentNode is GridNode gnode) {
					// TODO: Can be optimized
					// TODO: Also check for height
					if (gnode.ContainsPoint(point)) {
						insideCurrentNode = true;
						newClampedPoint = gnode.ClosestPointOnNode(point);
					}
				}

				if (insideCurrentNode) {
					if (isStart) {
						part.startPoint = newClampedPoint;
						unclampedStartPoint = point;
						startIsUpToDate = true;
						// Setting startIsUpToDate may have made the path non-stale, and thus we may have to update the startNode
						// to preserve our invariants (see CheckInvariants).
						startNode = currentNode;
					} else {
						part.endPoint = newClampedPoint;
						unclampedEndPoint = point;
						endIsUpToDate = true;
					}
					version++;
					CheckInvariants();
					return;
				}
			}

			// Split out into a separate call because otherwise the lambda inside that function
			// will cause an allocation even if we make an early return before we even use it.
			RepairFull(point, isStart, quality, movementPlane, traversalProvider, path);
			version++;
			CheckInvariants();
		}

		private static readonly ProfilerMarker MarkerContains = new ProfilerMarker("ContainsNode");
		private static readonly ProfilerMarker MarkerClosest = new ProfilerMarker("ClosestPointOnNode");
		private static readonly ProfilerMarker MarkerGetNearest = new ProfilerMarker("GetNearest");
		const int NODES_TO_CHECK_FOR_DESTRUCTION = 5;

		/// <summary>
		/// Use a heuristic to determine when an agent has passed a portal and we need to pop it.
		///
		/// Assumes the start point/end point of the first part is point, and simplifies the funnel
		/// accordingly. It uses the cached portals to determine if the agent has passed a portal.
		/// This works even if nodes have been destroyed.
		///
		/// Note: Does not update the start/end point of the first part.
		/// </summary>
		void HeuristicallyPopPortals (bool isStartOfPart, Vector3 point) {
			ref var part = ref parts[firstPartIndex];
			if (isStartOfPart) {
				while (funnelState.IsReasonableToPopStart(point, part.endPoint)) {
					part.startIndex++;
					nodes.PopStart();
					nodeHashes.PopStart();
					funnelState.PopStart();
					portalIsNotInnerCorner.PopStart();
				}
				if (ValidInPath(nodes.AbsoluteStartIndex)) startNode = nodes.First;
			} else {
				var removed = 0;
				while (funnelState.IsReasonableToPopEnd(part.startPoint, point)) {
					part.endIndex--;
					removed++;
					funnelState.PopEnd();
					portalIsNotInnerCorner.PopEnd();
				}
				if (removed > 0) {
					nodes.SpliceAbsolute(part.endIndex + 1, removed, null);
					nodeHashes.SpliceAbsolute(part.endIndex + 1, removed, null);
					for (int i = firstPartIndex + 1; i < parts.Length; i++) {
						parts[i].startIndex -= removed;
						parts[i].endIndex -= removed;
					}
				}
			}

			// Check a few nodes ahead to see if any have been destroyed
			var lastDestroyCheckIndex = Mathf.Min(part.startIndex + NODES_TO_CHECK_FOR_DESTRUCTION, part.endIndex);
			bool foundDestroyedNodes = false;
			for (int i = part.startIndex; i <= lastDestroyCheckIndex; i++) {
				foundDestroyedNodes |= !ValidInPath(i);
			}

			// We may end up popping all destroyed nodes from the part.
			// In that case, we must set firstPartContainsDestroyedNodes to false
			// to satisfy our invariants.
			this.firstPartContainsDestroyedNodes = foundDestroyedNodes;
			CheckInvariants();
		}

		[System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
		void AssertValidInPath (int absoluteNodeIndex) {
			Assert.IsTrue(ValidInPath(absoluteNodeIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[IgnoredByDeepProfiler]
		readonly bool ValidInPath (int absoluteNodeIndex) {
			return HashNode(nodes.GetAbsolute(absoluteNodeIndex)) == nodeHashes.GetAbsolute(absoluteNodeIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[IgnoredByDeepProfiler]
		static bool Valid(GraphNode node) => !node.Destroyed && node.Walkable;

		/// <summary>
		/// Returns a hash with the most relevant information about a node.
		///
		/// See: <see cref="nodeHashes"/>
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static int HashNode (GraphNode node) {
			// Note: The node index will change if the node is destroyed
			int h = (int)node.NodeIndex;
			h ^= node.Walkable ? 100663319 : 0;
			if (node is GridNodeBase gnode) {
				// TODO: We should hash the position of the node instead.
				// The NodeInGridIndex will change for all nodes if a grid mover moves the graph,
				// but we only want to invalidate the nodes which actually changed.
				h ^= gnode.NodeInGridIndex * 25165843;
			}
			return h;
		}

		void RepairFull (Vector3 point, bool isStart, RepairQuality quality, NativeMovementPlane movementPlane, ITraversalProvider traversalProvider, Path path) {
			// TODO: Rewrite to use Int3 coordinates everywhere
			var maxNodesToSearch = quality == RepairQuality.High ? 16 : 9;
			var partIndex = isStart ? this.firstPartIndex : this.parts.Length - 1;
			ref var part = ref parts[partIndex];
			var currentIndexInPath = isStart ? part.startIndex : part.endIndex;

			// TODO
			// Restructure code so that we do the heuristic passed-portal check if the next portal is destroyed (current or next node is destroyed),
			// but we allow using the normal repair code if the current node exists, but the next node is destroyed.
			// We only search for the globally closest node if the current node is destroyed after the heuristic passed-portal check is done.
			// !!
			var nextPortalDestroyed = !ValidInPath(currentIndexInPath) || (part.endIndex != part.startIndex && !ValidInPath(isStart ? part.startIndex + 1 : part.endIndex - 1));

			if (nextPortalDestroyed && partIndex == firstPartIndex) {
				// If the current node or the next node is destroyed, we still need to keep a funnel that we can use for directions
				// while a new path is being calculated.
				// Even though the regular repair would work if the next node is destroyed, it would
				// not be able to simplify the path appropriately.
				// Take the following example:
				//
				// - The path is A -> B -> C (agent is at node A).
				// - Node B is destroyed and is replaced by a new node B' in roughly the same location
				// - The agent moves into node B'.
				// - If we used the normal repair code, the path would become B' -> A -> B -> C.
				//   This would cause the agent to turn around and go back to node A, and it would not look great.
				//   Instead, we heuristically pop the portal between A and B, and the path becomes B -> C.
				// - Now the current node is B which is destroyed, so we enter another IF-statement below.
				HeuristicallyPopPortals(isStart, point);
				currentIndexInPath = isStart ? part.startIndex : part.endIndex;
			}

			if (!ValidInPath(currentIndexInPath)) {
				// If the current node is destroyed, we must use a global GetNearest check to find the closest non-destroyed node.
				// We do this so that we can still clamp the agent to the navmesh even if the current node is destroyed.
				// We do not want to replace the path with just a single node, because that may cause odd movement for a few frames
				// while the path is being recalculated. So we only adjust the start point of th path, but keep following the
				// potentially stale path until the new path is ready.

				if (isStart) {
					firstPartContainsDestroyedNodes = true;
					unclampedStartPoint = point;
					startIsUpToDate = false;

					var nn = this.nnConstraint;
					nn.distanceMetric = DistanceMetric.ClosestAsSeenFromAboveSoft(movementPlane.ToWorld(float2.zero, 1));
					var globallyClosestNode = AstarPath.active != null? AstarPath.active.GetNearest(point, nn).node : null;

					// Filter using the traveral provider.
					// TODO: We need to change the APIs to allow the GetNearest method to use the traversal provider
					if (traversalProvider != null && globallyClosestNode != null && !traversalProvider.CanTraverse(path, globallyClosestNode)) globallyClosestNode = null;

					startNode = globallyClosestNode;
					if (startNode != null) {
						part.startPoint = startNode.ClosestPointOnNode(point);

						if (part.endIndex - part.startIndex < 10 && partCount <= 1) {
							// This is a short path, a local repair should probably be able to repair it.
							// The path will be repaired when the end of the path is being set (typically it is done right after setting the start point).

							// It is important to handle this case. Take the following example:
							// The agent is standing still, but a dynamic obstacle has just been created right on
							// top of the agent. If we continued following the stale path, it might be completely incorrect
							// and lead into the obstacle.
							// We do not do this for long paths, because the repair is unlikely to succeed, and we may end
							// up with a partial path that goes in the wrong direction. It is then better to just wait for
							// the path to be recalculated.

							var clampedStartPoint = part.startPoint;
							// Make the path into just a single node
							this.Clear();
							startNode = globallyClosestNode;
							this.partGraphType = PartGraphTypeFromNode(startNode);
							unclampedStartPoint = point;
							unclampedEndPoint = clampedStartPoint;
							this.nodes.PushEnd(globallyClosestNode);
							this.nodeHashes.PushEnd(HashNode(globallyClosestNode));
							this.parts = new Funnel.PathPart[1];
							this.parts[0] = new Funnel.PathPart {
								startIndex = nodes.AbsoluteStartIndex,
								endIndex = nodes.AbsoluteEndIndex,
								startPoint = clampedStartPoint,
								endPoint = clampedStartPoint,
							};
						}
					} else {
						part.startPoint = point;
					}
				} else {
					// We don't care as much about the end point of the path being clamped to the navmesh.
					// But we do mark the path as stale so that it will be recalculated.
					unclampedEndPoint = point;
					part.endPoint = point;
					endIsUpToDate = false;
				}
				CheckInvariants();
			} else {
				var repairPath = LocalSearch(nodes.GetAbsolute(currentIndexInPath), point, maxNodesToSearch, movementPlane, isStart, traversalProvider, path);

				{
					// When we repair the path we may have multiple different cases:
					//
					// 1. We find the actually globally closest node and can repair the current path to reach it. This is the common case, and also the easiest to handle.
					//
					// 2. We find the globally closest node, but the destination is not actually inside it (it might be slightly inside an obstacle).
					//    This is fine. We just repair to the closest point that we can reach.
					//
					// 3. We do not find a repair path to the globally closest node.
					//    In this case the globally closest node might be very far away, so the path is stale.
					//    Or, we cannot actually reach the globally closest node due to e.g. a tag which we cannot traverse.
					//    In the latter case, we don't want to mark the path as stale every single frame. So we use a heuristic.
					//
					//    Let us call the distance from a path's endpoint to the path's destination as its error.
					//    Typically this will be zero if the destination is on the navmesh, but may be positive if the destination is inside an obstacle or it cannot be reached.
					//    We say that the path is up to date if the repaired path's error is smaller or equal to the previous path's error (+ a small margin) and the previous end point was up to date.
					//    The same logic applies to the start point.

					var closestNode = repairPath.Last;
					Assert.IsTrue(Valid(closestNode));

					bool upToDate;
					Vector3 newClampedPoint;

					var nn = this.nnConstraint;
					nn.constrainArea = true;
					nn.area = (int)closestNode.Area;

					MarkerGetNearest.Begin();
					var globallyClosestNode = AstarPath.active.GetNearest(point, nn);
					nn.constrainArea = false;


					MarkerGetNearest.End();
					var oldClampedPoint = isStart ? part.startPoint : part.endPoint;
					// TODO: Check if distance to globally closest node is approximately the same instead, to avoid cases when two nodes are the same distance from the point
					if (globallyClosestNode.node == closestNode) {
						upToDate = true;
						newClampedPoint = globallyClosestNode.position;
					} else {
						var oldUnclampedPoint = isStart ? unclampedStartPoint : unclampedEndPoint;
						var oldError = (oldUnclampedPoint - oldClampedPoint).sqrMagnitude;
						var oldIsUpToDate = isStart ? startIsUpToDate : endIsUpToDate;
						MarkerClosest.Begin();
						newClampedPoint = closestNode.ClosestPointOnNode(point);
						MarkerClosest.End();
						var newError = (point - newClampedPoint).sqrMagnitude;

						// true => Ok. The node we have found is as good, or almost as good as the actually closest node
						// false => Bad. We didn't find the best node when repairing. We may need to recalculate our path.
						upToDate = oldIsUpToDate && newError <= oldError + 0.1f*0.1f;
					}

					// In case we did not find a good repair path, we don't update our path.
					// This is important especially in the following case:
					//
					//        wall
					//         v
					// - - ----|----------
					//      B  |    X
					// - - ----|----------
					//
					// If X is the agent, and we try to set its destination to B,
					// then we want it to wait until its path calculation is done and then
					// realize it has to move right to get around the wall and eventually reach B.
					// If we would update the path here, the agent would first move left, thinking that
					// this is the best it could repair the path, and then move right once its path
					// had been recalculated. This looks like a weird stutter and is not desirable.
					if (!upToDate && !isStart) {
						repairPath.Clear();
						newClampedPoint = oldClampedPoint;
					}

					AppendPath(isStart, repairPath);
					repairPath.Pool();

					if (isStart) {
						startNode = nodes.First;
						// upToDate &= !foundDestroyedNodes;
					}

					if (isStart) {
						const bool CLAMP = true;
						if (CLAMP) {
							unclampedStartPoint = point;
							part.startPoint = newClampedPoint;
							startIsUpToDate = true;
						} else {
							#pragma warning disable CS0162
							unclampedStartPoint = point;
							part.startPoint = newClampedPoint;
							startIsUpToDate = upToDate;
						}
					} else {
						unclampedEndPoint = point;
						part.endPoint = newClampedPoint;
						endIsUpToDate = upToDate;
					}
				}
			}
		}

		/// <summary>Calculates the squared distance from a point to the closest point on the node.</summary>
		/// <param name="node">The node to calculate the distance to</param>
		/// <param name="point">The point to calculate the distance from. For navmesh/recast/grid graphs this point should be in graph space.</param>
		/// <param name="projectionParams">Parameters for the projection, if the node is a triangle mesh node. The projection should be based on the node's graph.</param>
		static float SquaredDistanceToNode (GraphNode node, Vector3 point, ref BBTree.ProjectionParams projectionParams) {
			if (node is TriangleMeshNode tnode) {
				tnode.GetVerticesInGraphSpace(out var v1, out var v2, out var v3);
				Polygon.ClosestPointOnTriangleProjected(
					ref v1,
					ref v2,
					ref v3,
					ref projectionParams,
					ref Unity.Collections.LowLevel.Unsafe.UnsafeUtility.As<Vector3, float3>(ref point),
					out _,
					out var dist,
					out _
					);
				return dist;
			} else if (node is GridNodeBase gnode) {
				var c = gnode.CoordinatesInGrid;
				var xf = math.clamp(point.x, c.x, c.x + 1);
				var zf = math.clamp(point.z, c.y, c.y + 1);
				return math.lengthsq(new float3(xf, 0, zf) - (float3)point);
			} else {
				var closestOnNode = node.ClosestPointOnNode(point);
				return (point - closestOnNode).sqrMagnitude;
			}
		}

		struct QueueItem {
			public GraphNode node;
			public int parent;
			public float distance;
		}

		static bool QueueHasNode (QueueItem[] queue, int count, GraphNode node) {
			for (int i = 0; i < count; i++) if (queue[i].node == node) return true;
			return false;
		}

#if UNITY_2022_3_OR_NEWER
		static readonly QueueItem[][] TempQueues = new QueueItem[JobsUtility.ThreadIndexCount][];
		static readonly List<GraphNode>[] TempConnectionLists = new List<GraphNode>[JobsUtility.ThreadIndexCount];

		void GetTempQueue (out QueueItem[] queue, out List<GraphNode> connections) {
			var threadIndex = JobsUtility.ThreadIndex;
			queue = TempQueues[threadIndex];
			connections = TempConnectionLists[threadIndex];
			if (queue == null) {
				queue = TempQueues[threadIndex] = new QueueItem[16];
				connections = TempConnectionLists[threadIndex] = new List<GraphNode>();
			}
		}
#else
		void GetTempQueue (out QueueItem[] queue, out List<GraphNode> connections) {
			queue = new QueueItem[16];
			connections = new List<GraphNode>();
		}
#endif

		/// <summary>
		/// Searches from currentNode until it finds a node that contains the given point.
		///
		/// The return value is a list of nodes that start with currentNode and ends with the node that contains the given point, if it could be found.
		/// Otherwise, the return value will be an empty list.
		/// </summary>
		CircularBuffer<GraphNode> LocalSearch (GraphNode currentNode, Vector3 point, int maxNodesToSearch, NativeMovementPlane movementPlane, bool reverse, ITraversalProvider traversalProvider, Path path) {
			var nn = this.nnConstraint;
			nn.distanceMetric = DistanceMetric.ClosestAsSeenFromAboveSoft(movementPlane.up);

			// Grab a temporary queue and list to use for this thread
			// To improve performance, we don't want to allocate these every time this method is called,
			// and even the ArrayPool<T> and ListPool<T> classes add some overhead.
			GetTempQueue(out var queue, out var connections);
			Assert.IsTrue(queue.Length >= maxNodesToSearch);

			var queueHead = 0;
			var queueTail = 0;
			var graph = currentNode.Graph;
			BBTree.ProjectionParams projectionParams;
			Vector3 pointInGraphSpace;
			if (partGraphType == PartGraphType.Navmesh) {
				var navmeshGraph = graph as NavmeshBase;
				projectionParams = new BBTree.ProjectionParams(nn, navmeshGraph.transform);
				// Convert the point to graph space
				pointInGraphSpace = navmeshGraph.transform.InverseTransform(point);
			} else if (partGraphType == PartGraphType.Grid) {
				projectionParams = default;
				pointInGraphSpace = (graph as GridGraph).transform.InverseTransform(point);
			} else {
				projectionParams = default;
				pointInGraphSpace = point;
			}
			float bestDist = SquaredDistanceToNode(currentNode, pointInGraphSpace, ref projectionParams);
			queue[0] = new QueueItem { node = currentNode, parent = -1, distance = bestDist };
			queueTail++;
			int bestNode = 0;

			while (queueHead < queueTail) {
				var nodeQueueIndex = queueHead;
				var node = queue[nodeQueueIndex].node;
				queueHead++;
				Assert.IsTrue(Valid(node));
				MarkerContains.Begin();

				if (ContainsPoint(node, point, movementPlane)) {
					MarkerContains.End();
					bestDist = 0;
					bestNode = nodeQueueIndex;
					break;
				} else {
					MarkerContains.End();
					var dist = queue[nodeQueueIndex].distance;
					if (dist < bestDist) {
						bestDist = dist;
						bestNode = nodeQueueIndex;
					}

					// Check if the neighbour nodes are closer than the parent node.
					// Allow for a small margin to both avoid floating point errors and to allow
					// moving past very small local minima.
					var distanceThresholdSqr = dist*(1.05f*1.05f) + 0.05f;
					// TODO: For grid graphs we ideally want to get only the axis-aligned connections.
					// This is because otherwise we can use a diagonal connection that cannot be simplified to
					// two axis-aligned connections, which will make the RemoveGridPathDiagonals method fail.
					// This will only happen in very few games, but it is still a potential issue.
					node.GetConnections((GraphNode node, ref List<GraphNode> ls) => ls.Add(node), ref connections);

					for (int i = 0; i < connections.Count; i++) {
						var neighbour = connections[i];
						if (queueTail < maxNodesToSearch && neighbour.GraphIndex == node.GraphIndex && nn.Suitable(neighbour) && (traversalProvider == null || (reverse ? traversalProvider.CanTraverse(path, neighbour) && traversalProvider.CanTraverse(path, neighbour, node) : traversalProvider.CanTraverse(path, node, neighbour)))) {
							MarkerClosest.Begin();
							float distanceToNeighbourSqr = SquaredDistanceToNode(neighbour, pointInGraphSpace, ref projectionParams);
							MarkerClosest.End();
							if (distanceToNeighbourSqr < distanceThresholdSqr && !QueueHasNode(queue, queueTail, neighbour)) {
								queue[queueTail] = new QueueItem { node = neighbour, parent = nodeQueueIndex, distance = distanceToNeighbourSqr };
								queueTail++;
							}
						}
					}
					connections.Clear();
				}
			}

			// Trace the repair path back to connect to the current path
			var repairPath = new CircularBuffer<GraphNode>(8);
			while (bestNode != -1) {
				repairPath.PushStart(queue[bestNode].node);
				bestNode = queue[bestNode].parent;
			}

			// Clear node references, to not leave any references that prevent garbage collection
			connections.Clear();
			for (int i = 0; i < queueTail; i++) queue[i].node = null;

			if (partGraphType == PartGraphType.Grid) {
				// To make the path easier to handle, we replace all diagonals by two axis-aligned connections
				var hashes = new CircularBuffer<int>();
				RemoveGridPathDiagonals(null, 0, ref repairPath, ref hashes, nnConstraint, traversalProvider, path);
			}
			return repairPath;
		}

		/// <summary>Renders the funnel for debugging purposes.</summary>
		/// <param name="draw">The command builder to use for drawing.</param>
		/// <param name="movementPlane">The movement plane of the agent.</param>
		public void DrawFunnel (CommandBuilder draw, NativeMovementPlane movementPlane) {
			if (parts == null) return;
			var part = parts[firstPartIndex];
			funnelState.PushStart(part.startPoint, part.startPoint);
			funnelState.PushEnd(part.endPoint, part.endPoint);
			using (draw.WithLineWidth(2)) {
				draw.Polyline(funnelState.leftFunnel);
				draw.Polyline(funnelState.rightFunnel);
			}
			if (funnelState.unwrappedPortals.Length > 1) {
				using (draw.WithLineWidth(1)) {
					var up = movementPlane.up;
					var m = funnelState.UnwrappedPortalsToWorldMatrix(up);
					// Convert from 4x3 matrix to 4x4 matrix
					var m2 = new float4x4(m.c0, m.c1, new float4(0, 0, 1, 0), m.c2);

					using (draw.WithMatrix(m2)) {
						var prevLeft = funnelState.unwrappedPortals[0].xy;
						var prevRight = funnelState.unwrappedPortals[1].xy;
						for (int i = 0; i < funnelState.unwrappedPortals.Length; i++) {
							var left = funnelState.unwrappedPortals[i].xy;
							var right = funnelState.unwrappedPortals[i].zw;
							draw.xy.Line(left, right, Palette.Colorbrewer.Set1.Brown);
							draw.xy.Line(prevLeft, left, Palette.Colorbrewer.Set1.Brown);
							draw.xy.Line(prevRight, right, Palette.Colorbrewer.Set1.Brown);
							prevLeft = left;
							prevRight = right;
						}
					}
				}
			}

			using (draw.WithColor(new Color(0, 0, 0, 0.2f))) {
				for (int i = 0; i < funnelState.leftFunnel.Length - 1; i++) {
					draw.SolidTriangle(funnelState.leftFunnel[i], funnelState.rightFunnel[i], funnelState.rightFunnel[i+1]);
					draw.SolidTriangle(funnelState.leftFunnel[i], funnelState.leftFunnel[i+1], funnelState.rightFunnel[i+1]);
				}
			}

			using (draw.WithColor(new Color(0, 0, 1, 0.5f))) {
				for (int i = 0; i < funnelState.leftFunnel.Length; i++) {
					draw.Line(funnelState.leftFunnel[i], funnelState.rightFunnel[i]);
				}
			}

			funnelState.PopStart();
			funnelState.PopEnd();
		}

		static Int3 MaybeSetYZero (Int3 p, bool setYToZero) {
			if (setYToZero) p.y = 0;
			return p;
		}

		static bool IsInnerVertex (CircularBuffer<GraphNode> nodes, Funnel.PathPart part, int portalIndex, bool rightSide, List<GraphNode> alternativeNodes, NNConstraint nnConstraint, out int startIndex, out int endIndex, ITraversalProvider traversalProvider, Path path) {
			Assert.IsTrue(portalIndex >= part.startIndex && portalIndex < part.endIndex);
			var startNode = nodes.GetAbsolute(portalIndex);
			if (startNode is TriangleMeshNode) {
				return IsInnerVertexTriangleMesh(nodes, part, portalIndex, rightSide, alternativeNodes, nnConstraint, out startIndex, out endIndex, traversalProvider, path);
			} else if (startNode is GridNodeBase) {
				// Note: Code for this was removed right after #97f311079
				throw new System.InvalidOperationException("Grid nodes are not supported. Should have been handled by the SimplifyGridInnerVertex method");
			} else {
				startIndex = portalIndex;
				endIndex = portalIndex+1;
				return false;
			}
		}

		static bool IsInnerVertexTriangleMesh (CircularBuffer<GraphNode> nodes, Funnel.PathPart part, int portalIndex, bool rightSide, List<GraphNode> alternativeNodes, NNConstraint nnConstraint, out int startIndex, out int endIndex, ITraversalProvider traversalProvider, Path path) {
			startIndex = portalIndex;
			endIndex = portalIndex+1;

			var startNode = nodes.GetAbsolute(startIndex) as TriangleMeshNode;
			var endNode = nodes.GetAbsolute(endIndex) as TriangleMeshNode;

			if (startNode == null || endNode == null || !Valid(startNode) || !Valid(endNode)) return false;

			if (!startNode.GetPortalInGraphSpace(endNode, out Int3 left, out Int3 right, out _, out _)) return false;

			var graph = startNode.Graph as NavmeshBase;
			bool allowVertexYDifferences = graph.RecalculateNormals;

			// We ignore any difference in graph-space y coordinates.
			// This is because if two vertices are on different tiles, they may have slightly different y coordinates, even if they are semantically
			// the same vertex. In particular, this easily becomes the case when using a NavmeshPrefab component.
			var vertex = MaybeSetYZero(rightSide ? right : left, allowVertexYDifferences);

			// Find the first and last node which shares the same portal vertex.
			while (
				startIndex > part.startIndex &&
				nodes.GetAbsolute(startIndex-1) is TriangleMeshNode prevNode &&
				Valid(prevNode) &&
				prevNode.GetPortalInGraphSpace(startNode, out Int3 left2, out Int3 right2, out _, out _) &&
				MaybeSetYZero(rightSide ? right2 : left2, allowVertexYDifferences) == vertex
				) {
				startNode = prevNode;
				startIndex--;
			}
			while (
				endIndex < part.endIndex &&
				nodes.GetAbsolute(endIndex+1) is TriangleMeshNode nextNode &&
				Valid(nextNode) &&
				endNode.GetPortalInGraphSpace(nextNode, out Int3 left2, out Int3 right2, out _, out _) &&
				MaybeSetYZero(rightSide ? right2 : left2, allowVertexYDifferences) == vertex
				) {
				endNode = nextNode;
				endIndex++;

				// The path may go around the vertex in more than a full loop. That's very silly.
				// We break here to cut out the whole loop.
				// This has been observed in the "ITraversalProvider DirectionalTags" unit test.
				// In particular, if we start with the following situation:
				//
				// 4 nodes meet at a vertex. The nodes are named A, B, C, and D.
				// A path goes through A -> B -> C -> D.
				//
				//        │
				// ──D◄───┼───┐C
				//        │   ▲
				//        │   │
				// ───────┼───┼───
				//        │   │
				//    A───┼──►┘B
				//        │
				//
				// The agent moves from A to C over a single frame, the LocalSearch finds a repair path C -> D -> A.
				// This results in the path C -> D -> A -> B -> C -> D, which goes around the vertex more than a full loop.
				// We want to cut out the whole loop and simplify this path to C -> D.
				//
				//        │
				// ──D►───┼───┐C2
				//    ┌───┼─C1│
				//    │   │   │
				// ───┼───┼───┼──
				//    ▼   │   │
				//    A───┼─►─┘B
				//        │
				if (endNode == startNode) break;
			}

			var currentNode = startNode;
			int cnt = 0;
			alternativeNodes.Add(startNode);

			// The path may go around the vertex in a full loop. How silly! Let's fix that.
			if (startNode == endNode) return true;

			bool foundAlternativePath = false;
			while (!foundAlternativePath) {
				bool found = false;
				for (int j = 0; j < currentNode.connections.Length; j++) {
					if (currentNode.connections[j].node is TriangleMeshNode neighbour && (traversalProvider != null ? traversalProvider.CanTraverse(path, currentNode, neighbour) : nnConstraint.Suitable(neighbour)) && currentNode.connections[j].isOutgoing) {
						if (!currentNode.GetPortalInGraphSpace(neighbour, out var left2, out var right2, out _, out _)) continue;
						var candidateVertex = MaybeSetYZero(rightSide ? left2 : right2, allowVertexYDifferences);

						if (candidateVertex == vertex) {
							// Found a portal which shares the correct vertex
							// We try to follow it and see where we end up
							currentNode = neighbour;
							alternativeNodes.Add(currentNode);
							found = true;

							if (cnt++ > 100) throw new System.Exception("Caught in a potentially infinite loop. The navmesh probably contains degenerate geometry.");

							// Stop if we have found an alternative path around the vertex
							if (currentNode == endNode) foundAlternativePath = true;
							break;
						}
					}
				}
				if (!found) return false;
			}

			var costDerivate = 0;
			for (int i = 0; i < alternativeNodes.Count; i++) {
				costDerivate += traversalProvider != null ? (int)traversalProvider.GetTraversalCost(path, alternativeNodes[i]) : (int)DefaultITraversalProvider.GetTraversalCost(path, alternativeNodes[i]);
			}
			for (int i = startIndex; i < endIndex; i++) {
				costDerivate -= traversalProvider != null ? (int)traversalProvider.GetTraversalCost(path, nodes.GetAbsolute(i)) : (int)DefaultITraversalProvider.GetTraversalCost(path, nodes.GetAbsolute(i));
			}

			// If the alternative path is cheaper than the current path, we should use it.
			// Making the path shorter does not necessarily make it less costly if penalties are involved.
			// TODO: Should estimate the derivative properly when changing to use length-based penalties.
			return costDerivate <= 0;
		}

		bool FirstInnerVertex (NativeArray<int> indices, int numCorners, List<GraphNode> alternativePath, out int alternativeStartIndex, out int alternativeEndIndex, ITraversalProvider traversalProvider, Path path) {
			var part = parts[firstPartIndex];
			Assert.AreEqual(funnelState.leftFunnel.Length, portalIsNotInnerCorner.Length);

			for (int i = 0; i < numCorners; i++) {
				var idx = indices[i];
				var rightSide = (idx & Funnel.RightSideBit) != 0;
				var portalIndex = idx & Funnel.FunnelPortalIndexMask;
				Assert.IsTrue(portalIndex >= 0 && portalIndex < part.endIndex - part.startIndex);
				if ((portalIsNotInnerCorner[portalIndex] & (rightSide ? 0b01 : 0b10)) != 0) {
					continue;
				}

				alternativePath.Clear();
				if (IsInnerVertex(nodes, part, part.startIndex + portalIndex, rightSide, alternativePath, nnConstraint, out alternativeStartIndex, out alternativeEndIndex, traversalProvider, path)) {
					return true;
				} else {
					// Mark this corner as already tested
					portalIsNotInnerCorner[portalIndex] = (byte)(portalIsNotInnerCorner[portalIndex] | (rightSide ? 0b01 : 0b10));
				}
			}

			alternativeStartIndex = -1;
			alternativeEndIndex = -1;
			return false;
		}

		/// <summary>
		/// Estimates the remaining distance to the end of the current path part.
		///
		/// Note: This method may modify the internal PathTracer state, so it is not safe to call it from multiple threads at the same time.
		/// </summary>
		public float EstimateRemainingPath (int maxCorners, ref NativeMovementPlane movementPlane) {
			return EstimateRemainingPath(ref funnelState, ref parts[firstPartIndex], maxCorners, ref movementPlane);
		}

		[BurstCompile]
		static float EstimateRemainingPath (ref Funnel.FunnelState funnelState, ref Funnel.PathPart part, int maxCorners, ref NativeMovementPlane movementPlane) {
			var buffer = new NativeList<float3>(maxCorners, Allocator.Temp);
			var cornerIndices = new NativeArray<int>(maxCorners, Allocator.Temp);

			// Treat start point as a corner
			maxCorners -= 1;
			maxCorners = math.max(0, math.min(maxCorners, funnelState.leftFunnel.Length));
			var numCorners = funnelState.CalculateNextCornerIndices(maxCorners, cornerIndices, part.startPoint, part.endPoint, out bool lastCorner);
			funnelState.ConvertCornerIndicesToPath(cornerIndices, numCorners, false, part.startPoint, part.endPoint, lastCorner, buffer);

			var nativeBufferSpan = buffer.AsUnsafeSpan();
			var endOfPart = (float3)part.endPoint;
			return RemainingDistanceLowerBound(in nativeBufferSpan, in endOfPart, in movementPlane);
		}

		[System.ThreadStatic]
		private static List<GraphNode> scratchList;

		/// <summary>
		/// Calculate the next corners in the path.
		///
		/// This will also do additional simplifications to the path if possible. Inner corners will be removed.
		/// There is a limit to how many simplifications will be done per frame.
		///
		/// If the path contains destroyed nodes, then <see cref="isStale"/> will become true and a best-effort result will be returned.
		///
		/// Note: This method may modify the PathTracer state, so it is not safe to call it from multiple threads at the same time.
		/// </summary>
		/// <param name="buffer">The buffer to store the corners in. The first corner will be the start point.</param>
		/// <param name="maxCorners">The maximum number of corners to store in the buffer. At least 2 corners will always be stored.</param>
		/// <param name="scratchArray">A temporary array to use for calculations. This array will be resized if it is too small or uninitialized.</param>
		/// <param name="allocator">The allocator to use for the scratchArray, if it needs to be reallocated.</param>
		/// <param name="traversalProvider">The traversal provider to use for the path. Or null to use the default traversal provider.</param>
		/// <param name="path">The path to pass to the traversal provider. Or null.</param>
		public void GetNextCorners (NativeList<float3> buffer, int maxCorners, ref NativeArray<int> scratchArray, Allocator allocator, ITraversalProvider traversalProvider, Path path) {
			var numCorners = GetNextCornerIndices(ref scratchArray, maxCorners, allocator, out var lastCorner, traversalProvider, path);
			var part = parts[firstPartIndex];
			funnelState.ConvertCornerIndicesToPath(scratchArray, numCorners, false, part.startPoint, part.endPoint, lastCorner, buffer);
		}

		/// <summary>
		/// Calculate the indices of the next corners in the path.
		///
		/// This is like <see cref="GetNextCorners"/>, except that it returns indices referring to the internal <see cref="funnelState"/>.
		/// You can use <see cref="ConvertCornerIndicesToPathProjected"/> or <see cref="funnelState.ConvertCornerIndicesToPath"/> to convert the indices to world space positions.
		/// </summary>
		public int GetNextCornerIndices (ref NativeArray<int> buffer, int maxCorners, Allocator allocator, out bool lastCorner, ITraversalProvider traversalProvider, Path path) {
			const int MaxSimplifications = 3;
			int allowedSimplifications = MaxSimplifications;

			// Treat start point as a corner
			maxCorners -= 1;
			if (scratchList == null) scratchList = new List<GraphNode>(8);
			var alternativePath = scratchList;

			while (true) {
				// Limit max corners to the maximum possible number of corners given our current funnel.
				// We have to do this in every iteration of the loop because the simplification may cause the funnel to contain more nodes/corners even if it is shorter.
				var concreteMaxCorners = math.max(0, math.min(maxCorners, funnelState.leftFunnel.Length));
				if (!buffer.IsCreated || buffer.Length < concreteMaxCorners) {
					if (buffer.IsCreated) buffer.Dispose();
					buffer = new NativeArray<int>(math.ceilpow2(concreteMaxCorners), allocator, NativeArrayOptions.UninitializedMemory);
				}
				var cornerIndices = buffer;

				var part = parts[firstPartIndex];
				var numCorners = funnelState.CalculateNextCornerIndices(concreteMaxCorners, cornerIndices, part.startPoint, part.endPoint, out lastCorner);

				if (allowedSimplifications > 0) {
					if (partGraphType == PartGraphType.Grid) {
						MarkerSimplify.Begin();
						bool isInnerCorner = SimplifyGridInnerVertex(ref this.nodes, cornerIndices.AsUnsafeSpan().Slice(0, numCorners), part, ref portalIsNotInnerCorner, alternativePath, out int alternativeStartIndex, out int alternativeEndIndex, nnConstraint, traversalProvider, path, lastCorner);
						MarkerSimplify.End();
						if (isInnerCorner) {
							Profiler.BeginSample("Splice");
							if (SplicePath(alternativeStartIndex, alternativeEndIndex - alternativeStartIndex + 1, alternativePath)) {
								allowedSimplifications -= 1;
								version++;
								Profiler.EndSample();
								continue;
							} else {
								firstPartContainsDestroyedNodes = true;
								// Return best effort result
							}
							Profiler.EndSample();
						}
					} else {
						if (FirstInnerVertex(cornerIndices, numCorners, alternativePath, out int alternativeStartIndex, out int alternativeEndIndex, traversalProvider, path)) {
							if (SplicePath(alternativeStartIndex, alternativeEndIndex - alternativeStartIndex + 1, alternativePath)) {
								allowedSimplifications--;
								version++;
								continue;
							} else {
								firstPartContainsDestroyedNodes = true;
								// Return best effort result
							}
						}
					}
				}
				// We are either not allowed to simplify any more, or we couldn't find any simplification opportunities. Return the result.
				return numCorners;
			}
		}

		/// <summary>
		/// Converts corner indices to world space positions.
		///
		/// The corners will not necessarily be in the same world space position as the real corners. Instead the path will be unwrapped and flattened,
		/// and then transformed onto a plane that lines up with the first portal in the funnel. For most 2D and 3D worlds, this distinction is irrelevant,
		/// but for curved worlds (e.g. a spherical world) this can make a big difference. In particular, steering towards unwrapped corners
		/// is much more likely to work well than steering towards the real corners, as they can be e.g. on the other side of a round planet.
		/// </summary>
		/// <param name="cornerIndices">The corner indices to convert. You can get these from #GetNextCornerIndices.</param>
		/// <param name="numCorners">The number of indices in the cornerIndices array.</param>
		/// <param name="lastCorner">True if the last corner in the path has been reached.</param>
		/// <param name="buffer">The buffer to store the converted positions in.</param>
		/// <param name="up">The up axis of the agent's movement plane.</param>
		public void ConvertCornerIndicesToPathProjected (NativeArray<int> cornerIndices, int numCorners, bool lastCorner, NativeList<float3> buffer, float3 up) {
			var part = parts[firstPartIndex];
			funnelState.ConvertCornerIndicesToPathProjected(cornerIndices.AsUnsafeReadOnlySpan().Slice(0, numCorners), false, part.startPoint, part.endPoint, lastCorner, buffer, up);
		}

		/// <summary>
		/// Calculates a lower bound on the remaining distance to the end of the path part.
		///
		/// It assumes the agent will follow the path, and then move in a straight line to the end of the path part.
		/// </summary>
		[BurstCompile]
		public static float RemainingDistanceLowerBound (in UnsafeSpan<float3> nextCorners, in float3 endOfPart, in NativeMovementPlane movementPlane) {
			if (nextCorners.Length == 0) return 0;
			var prev = nextCorners[0];
			var remainingDistance = 0.0f;
			for (int i = 1; i < nextCorners.Length; i++) {
				var next = nextCorners[i];
				remainingDistance += math.length(movementPlane.ToPlane(next - prev));
				prev = next;
			}
			remainingDistance += math.length(movementPlane.ToPlane(prev - endOfPart));
			return remainingDistance;
		}

		/// <summary>
		/// Remove the first count parts of the path.
		///
		/// This is used when an agent has traversed an off-mesh-link, and we want to start following the path after the off-mesh-link.
		/// </summary>
		/// <param name="count">The number of parts to remove.</param>
		/// <param name="traversalProvider">The traversal provider to use for the path. Or null to use the default traversal provider.</param>
		/// <param name="path">The path to pass to the traversal provider. Or null.</param>
		public void PopParts (int count, ITraversalProvider traversalProvider, Path path) {
			if (firstPartIndex + count >= parts.Length) throw new System.InvalidOperationException("Cannot pop the last part of a path");
			firstPartIndex += count;
			version++;
			var part = parts[firstPartIndex];
			while (nodes.AbsoluteStartIndex < part.startIndex) {
				nodes.PopStart();
				nodeHashes.PopStart();
			}
			this.startNode = nodes.Length > 0 ? nodes.First : null;
			firstPartContainsDestroyedNodes = false;
			if (GetPartType() == Funnel.PartType.OffMeshLink) {
				this.partGraphType = PartGraphType.OffMeshLink;
				SetFunnelState(part);
				CheckInvariants();
				return;
			}
			this.partGraphType = PartGraphTypeFromNode(startNode);

			for (int i = part.startIndex; i <= part.endIndex; i++) {
				if (!ValidInPath(i)) {
					// The part contains invalid nodes.
					// However, we didn't get a chance to calculate funnel portals for this part.
					// In order to preserve the invariant that we have funnel portals for the first part,
					// we must remove all nodes after and including this one.
					// We mark the path as stale in order to trigger a path recalculation as soon as possible.
					// If this is the first node in the part, we leave only this node, to ensure the path is not empty.
					RemoveAllPartsExceptFirst();
					while (nodes.AbsoluteEndIndex > i) {
						nodes.PopEnd();
						nodeHashes.PopEnd();
					}
					part.endIndex = i;
					parts[firstPartIndex] = part;

					if (i == part.startIndex) {
						firstPartContainsDestroyedNodes = true;
						// The part has no valid nodes.
						// Replace it with a dummy part
						Assert.IsTrue(nodes.Length == 1);
						Assert.AreEqual(part.startIndex, part.endIndex);
						// Invalid path, and we don't have any nodes to fall back on.
						// This path tracer is completely invalid
						funnelState.Clear();
						portalIsNotInnerCorner.Clear();
						startNode = null;
						CheckInvariants();

						// var nn = NNConstraint.Walkable;
						// nn.distanceMetric = DistanceMetric.ClosestAsSeenFromAboveSoft(movementPlane.ToWorld(float2.zero, 1));
						// var globallyClosestNode = AstarPath.active != null ? AstarPath.active.GetNearest(unclampedStartPoint, nn) : NNInfo.Empty;
						// if (globallyClosestNode.node != null) {
						// 	SetFromSingleNode(globallyClosestNode.node, globallyClosestNode.position, movementPlane.value);
						// } else {

						// }
						return;
					} else {
						endIsUpToDate = false;
						// It's not the first node, we can remove this node too
						nodes.PopEnd();
						nodeHashes.PopEnd();
						part.endIndex = i - 1;
						parts[firstPartIndex] = part;
						break;
					}
				}
			}

			if (partGraphType == PartGraphType.Grid) {
				RemoveGridPathDiagonals(this.parts, firstPartIndex, ref this.nodes, ref this.nodeHashes, nnConstraint, traversalProvider, path);
				part = parts[firstPartIndex];
			}

			SetFunnelState(part);
			CheckInvariants();
		}

		public void RemoveAllButFirstNode (NativeMovementPlane movementPlane, ITraversalProvider traversalProvider) {
			var pathfindingSettings = new PathRequestSettings {
				graphMask = nnConstraint.graphMask,
				traversableTags = nnConstraint.tags,
				tagPenalties = null,
				traversalProvider = traversalProvider,
			};
			SetFromSingleNode(startNode, startPoint, movementPlane, pathfindingSettings);
		}

		void RemoveAllPartsExceptFirst () {
			if (partCount <= 1) return;
			var newParts = new Funnel.PathPart[1];
			newParts[0] = parts[firstPartIndex];
			this.parts = newParts;
			firstPartIndex = 0;
			// Remove all nodes in subsequent parts
			while (nodes.AbsoluteEndIndex > parts[0].endIndex) {
				nodes.PopEnd();
				nodeHashes.PopEnd();
			}
			version++;
		}

		/// <summary>Indicates if the given path part is a regular path part or an off-mesh link.</summary>
		/// <param name="partIndex">The index of the path part. Zero is the always the current path part.</param>
		public readonly Funnel.PartType GetPartType (int partIndex = 0) {
			return parts[this.firstPartIndex + partIndex].type;
		}

		public readonly bool PartContainsDestroyedNodes (int partIndex = 0) {
			if (partIndex < 0 || partIndex >= partCount) throw new System.ArgumentOutOfRangeException(nameof(partIndex));

			var part = parts[firstPartIndex + partIndex];
			for (int i = part.startIndex; i <= part.endIndex; i++) {
				if (!ValidInPath(i)) return true;
			}
			return false;
		}

		public OffMeshLinks.OffMeshLinkTracer GetLinkInfo (int partIndex = 0) {
			if (partIndex < 0 || partIndex >= partCount) throw new System.ArgumentOutOfRangeException(nameof(partIndex));
			if (GetPartType(partIndex) != Funnel.PartType.OffMeshLink) throw new System.ArgumentException("Part is not an off-mesh link");
			var part = parts[firstPartIndex + partIndex];
			var startNode = nodes.GetAbsolute(part.startIndex) as LinkNode;
			var endNode = nodes.GetAbsolute(part.endIndex) as LinkNode;
			if (startNode == null) throw new System.Exception("Expected a link node");
			if (endNode == null) throw new System.Exception("Expected a link node");
			if (startNode.Destroyed) throw new System.Exception("Start node is destroyed");
			if (endNode.Destroyed) throw new System.Exception("End node is destroyed");

			bool isReverse;
			if (startNode.linkConcrete.startLinkNode == startNode) {
				isReverse = false;
			} else if (startNode.linkConcrete.startLinkNode == endNode) {
				isReverse = true;
			} else {
				throw new System.Exception("Link node is not part of the link");
			}
			return new OffMeshLinks.OffMeshLinkTracer(startNode.linkConcrete, isReverse);
		}

		void SetFunnelState (Funnel.PathPart part) {
			Profiler.BeginSample("SetFunnelState");
			this.funnelState.Clear();
			this.portalIsNotInnerCorner.Clear();

			if (part.type == Funnel.PartType.NodeSequence) {
				var startNode = nodes.GetAbsolute(part.startIndex);
				if (startNode.Graph is GridGraph gridGraph) {
					funnelState.projectionAxis = gridGraph.transform.WorldUpAtGraphPosition(Vector3.zero);
				}

				var tmpLeft = ListPool<float3>.Claim(part.endIndex - part.startIndex);
				var tmpRight = ListPool<float3>.Claim(part.endIndex - part.startIndex);
				CalculateFunnelPortals(part.startIndex, part.endIndex, tmpLeft, tmpRight);
				this.funnelState.Splice(0, 0, tmpLeft, tmpRight);
				for (int i = 0; i < tmpLeft.Count; i++) {
					this.portalIsNotInnerCorner.PushEnd(0);
				}
				ListPool<float3>.Release(ref tmpLeft);
				ListPool<float3>.Release(ref tmpRight);
			}
			Profiler.EndSample();
			version++;
		}

		void CalculateFunnelPortals (int startNodeIndex, int endNodeIndex, List<float3> outLeftPortals, List<float3> outRightPortals) {
			Profiler.BeginSample("CalculatePortals");
			var prevNode = this.nodes.GetAbsolute(startNodeIndex);
			AssertValidInPath(startNodeIndex);

			for (int i = startNodeIndex + 1; i <= endNodeIndex; i++) {
				var node = this.nodes.GetAbsolute(i);
				AssertValidInPath(i);
				if (prevNode.GetPortal(node, out var left, out var right)) {
					outLeftPortals.Add(left);
					outRightPortals.Add(right);
				} else {
					throw new System.InvalidOperationException("Couldn't find a portal from " + prevNode + " " + node + " " + prevNode.ContainsOutgoingConnection(node));
				}
				prevNode = node;
			}
			Profiler.EndSample();
		}

		/// <summary>Replaces the current path with a single node</summary>
		public void SetFromSingleNode (GraphNode node, Vector3 position, NativeMovementPlane movementPlane, PathRequestSettings pathfindingSettings) {
			SetPath(
				new List<Funnel.PathPart> {
				new Funnel.PathPart { startIndex = 0, endIndex = 0, startPoint = position, endPoint = position }
			},
				new List<GraphNode> { node },
				position,
				position,
				movementPlane,
				pathfindingSettings,
				null
				);
		}

		/// <summary>Clears the current path</summary>
		public void Clear () {
			funnelState.Clear();
			parts = null;
			nodes.Clear();
			nodeHashes.Clear();
			portalIsNotInnerCorner.Clear();
			unclampedEndPoint = unclampedStartPoint = Vector3.zero;
			firstPartIndex = 0;
			startIsUpToDate = false;
			endIsUpToDate = false;
			firstPartContainsDestroyedNodes = false;
			startNodeInternal = null;
			partGraphType = PartGraphType.Navmesh;
			CheckInvariants();
		}

		static int2 ResolveNormalizedGridPoint (GridGraph grid, ref CircularBuffer<GraphNode> nodes, UnsafeSpan<int> cornerIndices, Funnel.PathPart part, int index, out int nodeIndex) {
			if (index < 0 || index >= cornerIndices.Length) {
				var p = index < 0 ? part.startPoint : part.endPoint;
				nodeIndex = index < 0 ? part.startIndex : part.endIndex;
				var pointInGraphSpace = grid.transform.InverseTransform(p);
				var node = nodes.GetAbsolute(nodeIndex) as GridNodeBase;
				var nodeCoords = node.CoordinatesInGrid;
				var normalizedPoint = new int2(
					math.clamp((int)(GridGraph.FixedPrecisionScale * (pointInGraphSpace.x - nodeCoords.x)), 0, GridGraph.FixedPrecisionScale),
					math.clamp((int)(GridGraph.FixedPrecisionScale * (pointInGraphSpace.z - nodeCoords.y)), 0, GridGraph.FixedPrecisionScale)
					);
				return normalizedPoint;
			} else {
				var rightSide = (cornerIndices[index] & Funnel.RightSideBit) != 0;
				nodeIndex = part.startIndex + (cornerIndices[index] & Funnel.FunnelPortalIndexMask);
				Assert.IsTrue(nodeIndex >= part.startIndex && nodeIndex < part.endIndex);

				var node = nodes.GetAbsolute(nodeIndex) as GridNodeBase;
				var node2 = nodes.GetAbsolute(nodeIndex + 1) as GridNodeBase;
				var node1Coords = node.CoordinatesInGrid;
				var node2Coords = node2.CoordinatesInGrid;
				var dx = node2Coords.x - node1Coords.x;
				var dz = node2Coords.y - node1Coords.y;
				var dir = GridNodeBase.OffsetToConnectionDirection(dx, dz);
				if (dir > 4) {
					throw new System.Exception("Diagonal connections are not supported");
				}

				// Both of these values will be either -1 or +1.
				var vertexDx = GridGraph.neighbourXOffsets[dir] + GridGraph.neighbourXOffsets[(dir + (rightSide ? -1 : 1) + 4) % 4];
				var vertexDz = GridGraph.neighbourZOffsets[dir] + GridGraph.neighbourZOffsets[(dir + (rightSide ? -1 : 1) + 4) % 4];
				return new int2(512 + 512*vertexDx, 512 + 512*vertexDz);
			}
		}

		static int[] SplittingCoefficients = new int[] {
			0, 1,
			1, 2,
			1, 4,
			3, 4,
			1, 8,
			3, 8,
			5, 8,
			7, 8,
		};

		private static readonly ProfilerMarker MarkerSimplify = new ProfilerMarker("Simplify");

		static bool SimplifyGridInnerVertex (ref CircularBuffer<GraphNode> nodes, UnsafeSpan<int> cornerIndices, Funnel.PathPart part, ref CircularBuffer<byte> portalIsNotInnerCorner, List<GraphNode> alternativePath, out int alternativeStartIndex, out int alternativeEndIndex, NNConstraint nnConstraint, ITraversalProvider traversalProvider, Path path, bool lastCorner) {
			var corners = lastCorner ? cornerIndices.Length : cornerIndices.Length - 1;
			alternativeStartIndex = -1;
			alternativeEndIndex = -1;
			if (corners == 0) {
				return false;
			}

			// Only try to simplify every 2^value frame, unless the path changes
			const int EveryNthLog2 = 2;
			int i = 0;
			var idx = cornerIndices[i];
			var rightSide = (idx & Funnel.RightSideBit) != 0;
			var portalIndex = idx & Funnel.FunnelPortalIndexMask;
			var splitting = portalIsNotInnerCorner[portalIndex] % (8 * (1 << EveryNthLog2));
			portalIsNotInnerCorner[portalIndex] = (byte)(splitting + 1);

			// Only try to simplify every 2^n'th frame
			if ((splitting & ((1 << EveryNthLog2) - 1)) != 0) {
				return false;
			}
			splitting /= 1 << EveryNthLog2;

			Assert.IsTrue(portalIndex >= 0 && portalIndex < part.endIndex - part.startIndex);

			// The ResolveNormalizedGridPoint method will access up to the cornerIndices[1] node and the one that follows it in the path
			var lastRelevantNodeIndex = cornerIndices.length < 2 ? part.endIndex : math.min(part.endIndex, part.startIndex + (cornerIndices[1] & Funnel.FunnelPortalIndexMask) + 1);
			for (int j = part.startIndex; j < lastRelevantNodeIndex; j++) {
				var a = nodes.GetAbsolute(j);
				var b = nodes.GetAbsolute(j + 1);
				if (!Valid(b) || !a.ContainsOutgoingConnection(b)) {
					// The path is no longer valid
					return false;
				}
			}

			var grid = GridNode.GetGridGraph(nodes.GetAbsolute(part.startIndex).GraphIndex);

			var normalizedPointStart = ResolveNormalizedGridPoint(grid, ref nodes, cornerIndices, part, i - 1, out var startNodeIndex);
			var normalizedPointEnd = ResolveNormalizedGridPoint(grid, ref nodes, cornerIndices, part, i + 1, out var endNodeIndex);
			var normalizedPointMid = ResolveNormalizedGridPoint(grid, ref nodes, cornerIndices, part, i, out var midNodeIndex);

			var startNode = nodes.GetAbsolute(startNodeIndex) as GridNodeBase;
			var midNode = nodes.GetAbsolute(midNodeIndex) as GridNodeBase;
			var endNode = nodes.GetAbsolute(endNodeIndex) as GridNodeBase;

			// Try to simplify using different linecast target points every frame.
			// Assume we have the path
			//
			// A ----- B
			//           \
			//             \
			//              C
			//
			// Where A is the start point, B is a corner of the funnel, and C is either the next corner or the end point of the path.
			// Then we can pick any point on the line between B and C as the linecast target point. The origin will always be A.
			if (splitting > 0) {
				var a = SplittingCoefficients[splitting*2];
				var b = SplittingCoefficients[splitting*2+1];
				endNodeIndex = endNodeIndex + (midNodeIndex - endNodeIndex)*a/b;
				if (endNodeIndex == midNodeIndex) return false;

				var endCoords = endNode.CoordinatesInGrid;
				var midCoords = midNode.CoordinatesInGrid;
				var mp = new int2(midCoords.x * 1024, midCoords.y * 1024) + normalizedPointMid;
				var ep = new int2(endCoords.x * 1024, endCoords.y * 1024) + normalizedPointEnd;
				endNode = nodes.GetAbsolute(endNodeIndex) as GridNodeBase;
				endCoords = endNode.CoordinatesInGrid;
				var f = VectorMath.ClosestPointOnLineFactor(new Vector2Int(mp.x, mp.y), new Vector2Int(ep.x, ep.y), new Vector2Int(endCoords.x * 1024 + 512, endCoords.y * 1024 + 512));
				var p = new int2((int)math.lerp(mp.x, ep.x, f), (int)math.lerp(mp.y, ep.y, f)) - new int2(endCoords.x * 1024, endCoords.y * 1024);
				normalizedPointEnd = new int2(
					math.clamp(p.x, 0, GridGraph.FixedPrecisionScale),
					math.clamp(p.y, 0, GridGraph.FixedPrecisionScale)
					);
			}

			alternativePath.Clear();
			var obstructed = grid.Linecast(startNode, normalizedPointStart, endNode, normalizedPointEnd, out var _, alternativePath, null, false);
			if (!obstructed) {
				Assert.AreEqual(startNode, alternativePath[0]);
				Assert.AreEqual(endNode, alternativePath[alternativePath.Count-1]);

				// The linecast was unobstructed. But the new path may still be more costly than the old path, if any penalties are used.
				// The traversal provider or the NNConstraint may also disallow the new path.
				for (int j = 1; j < alternativePath.Count; j++) {
					if (traversalProvider != null ? !traversalProvider.CanTraverse(path, alternativePath[j-1], alternativePath[j]) : !nnConstraint.Suitable(alternativePath[j])) {
						return false;
					}
				}
				uint alternativeCost = 0;
				for (int j = 0; j < alternativePath.Count; j++) {
					alternativeCost += traversalProvider != null? traversalProvider.GetTraversalCost(path, alternativePath[j]) : DefaultITraversalProvider.GetTraversalCost(path, alternativePath[j]);
				}

				if (alternativeCost > 0) {
					// The new path *may* be more costly than the old one.
					// We have to do a thorough check to see if the new path is better.

					// Calculate the cost of the old path.
					uint oldCost = 0;
					for (int j = startNodeIndex; j <= endNodeIndex; j++) {
						oldCost += traversalProvider != null? traversalProvider.GetTraversalCost(path, nodes.GetAbsolute(j)) : DefaultITraversalProvider.GetTraversalCost(path, nodes.GetAbsolute(j));
					}

					if (alternativeCost > oldCost) {
						return false;
					}
				}
				alternativeStartIndex = startNodeIndex;
				alternativeEndIndex = endNodeIndex;
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Removes diagonal connections in a grid path and replaces them with two axis-aligned connections.
		///
		/// This is done to make the funnel algorithm work better on grid graphs.
		/// </summary>
		static void RemoveGridPathDiagonals (Funnel.PathPart[] parts, int partIndex, ref CircularBuffer<GraphNode> path, ref CircularBuffer<int> pathNodeHashes, NNConstraint nnConstraint, ITraversalProvider traversalProvider, Path pathObject) {
			int inserted = 0;
			var part = parts != null ? parts[partIndex] : new Funnel.PathPart { startIndex = path.AbsoluteStartIndex, endIndex = path.AbsoluteEndIndex };
			for (int i = part.endIndex - 1; i >= part.startIndex; i--) {
				var node = path.GetAbsolute(i) as GridNodeBase;
				var node2 = path.GetAbsolute(i + 1) as GridNodeBase;
				var dx = node2.XCoordinateInGrid - node.XCoordinateInGrid;
				var dz = node2.ZCoordinateInGrid - node.ZCoordinateInGrid;
				var dir = GridNodeBase.OffsetToConnectionDirection(dx, dz);
				if (dir >= 4) {
					var d1 = dir - 4;
					var d2 = (dir - 4 + 1) % 4;
					var n1 = node.GetNeighbourAlongDirection(d1);
					if (n1 != null && (traversalProvider != null ? !traversalProvider.CanTraverse(pathObject, node, n1) : !nnConstraint.Suitable(n1))) n1 = null;

					if (n1 != null && n1.GetNeighbourAlongDirection(d2) == node2 && (traversalProvider == null || traversalProvider.CanTraverse(pathObject, n1, node2))) {
						path.InsertAbsolute(i+1, n1);
						if (pathNodeHashes.Length > 0) pathNodeHashes.InsertAbsolute(i+1, HashNode(n1));
						inserted++;
					} else {
						var n2 = node.GetNeighbourAlongDirection(d2);
						if (n2 != null && (traversalProvider != null ? !traversalProvider.CanTraverse(pathObject, node, n2) : !nnConstraint.Suitable(n2))) n2 = null;

						if (n2 != null && n2.GetNeighbourAlongDirection(d1) == node2 && (traversalProvider == null || traversalProvider.CanTraverse(pathObject, n2, node2))) {
							path.InsertAbsolute(i+1, n2);
							if (pathNodeHashes.Length > 0) pathNodeHashes.InsertAbsolute(i+1, HashNode(n2));
							inserted++;
						} else {
							throw new System.Exception("Axis-aligned connection not found");
						}
					}
				}
			}

			if (parts != null) {
				parts[partIndex].endIndex += inserted;
				for (int i = partIndex + 1; i < parts.Length; i++) {
					parts[i].startIndex += inserted;
					parts[i].endIndex += inserted;
				}
			}
		}

		static PartGraphType PartGraphTypeFromNode (GraphNode node) {
			if (node == null) {
				return PartGraphType.Navmesh;
			} else if (node is GridNodeBase) {
				return PartGraphType.Grid;
			} else if (node is TriangleMeshNode) {
				return PartGraphType.Navmesh;
			} else {
				throw new System.Exception("The PathTracer (and by extension FollowerEntity component) cannot be used on graphs of type " + node.Graph.GetType().Name);
			}
		}

		/// <summary>Replaces the current path with the given path.</summary>
		/// <param name="path">The path to follow.</param>
		/// <param name="movementPlane">The movement plane of the agent.</param>
		public void SetPath (ABPath path, NativeMovementPlane movementPlane) {
			var parts = Funnel.SplitIntoParts(path);
			var pathfindingSettings = new PathRequestSettings {
				graphMask = path.nnConstraint.graphMask,
				traversableTags = path.nnConstraint.tags,
				tagPenalties = null,
				traversalProvider = path.traversalProvider,
			};

			SetPath(parts, path.path, path.originalStartPoint, path.originalEndPoint, movementPlane, pathfindingSettings, path);
			Pathfinding.Pooling.ListPool<Funnel.PathPart>.Release(ref parts);
		}

		/// <summary>Replaces the current path with the given path.</summary>
		/// <param name="parts">The individual parts of the path. See \reflink{Funnel.SplitIntoParts}.</param>
		/// <param name="nodes">All nodes in the path. The path parts refer to slices of this array.</param>
		/// <param name="unclampedStartPoint">The start point of the path. This is typically the start point that was passed to the path request, or the agent's current position.</param>
		/// <param name="unclampedEndPoint">The end point of the path. This is typically the destination point that was passed to the path request.</param>
		/// <param name="movementPlane">The movement plane of the agent.</param>
		/// <param name="pathfindingSettings">Pathfinding settings that the path was calculated with. You may pass PathRequestSettings.Default if you don't use tags, traversal providers, or multiple graphs.</param>
		/// <param name="path">The path to pass to the traversal provider. Or null.</param>
		public void SetPath (List<Funnel.PathPart> parts, List<GraphNode> nodes, Vector3 unclampedStartPoint, Vector3 unclampedEndPoint, NativeMovementPlane movementPlane, PathRequestSettings pathfindingSettings, Path path) {
			this.nnConstraint.UseSettings(pathfindingSettings);
			this.startNode = nodes.Count > 0 ? nodes[0] : null;
			partGraphType = PartGraphTypeFromNode(this.startNode);
			this.unclampedEndPoint = unclampedEndPoint;
			this.unclampedStartPoint = unclampedStartPoint;
			firstPartContainsDestroyedNodes = false;
			startIsUpToDate = true;
			endIsUpToDate = true;
			this.parts = parts.ToArray();
			this.nodes.Clear();
			this.nodes.AddRange(nodes);
			this.nodeHashes.Clear();
			for (int i = 0; i < nodes.Count; i++) {
				this.nodeHashes.PushEnd(HashNode(nodes[i]));
			}
			this.firstPartIndex = 0;

			if (partGraphType == PartGraphType.Grid) {
				// SimplifyGridPath(this.parts, 0, ref this.nodes, int.MaxValue);
				RemoveGridPathDiagonals(this.parts, 0, ref this.nodes, ref this.nodeHashes, nnConstraint, pathfindingSettings.traversalProvider, path);
			}

			SetFunnelState(this.parts[firstPartIndex]);
			version++;
			CheckInvariants();
			// This is necessary because the path may not have used the same distance metric that the path tracer uses.
			// And if we don't do this, then the start/end points of the funnel may be slightly incorrect.
			Repair(unclampedStartPoint, true, RepairQuality.Low, movementPlane, pathfindingSettings.traversalProvider, path, false);
			Repair(unclampedEndPoint, false, RepairQuality.Low, movementPlane, pathfindingSettings.traversalProvider, path, false);
		}

		/// <summary>Returns a deep clone of this object</summary>
		public PathTracer Clone () {
			return new PathTracer {
					   parts = parts != null? parts.Clone() as Funnel.PathPart[] : null,
					   nodes = nodes.Clone(),
					   nodeHashes = nodeHashes.Clone(),
					   portalIsNotInnerCorner = portalIsNotInnerCorner.Clone(),
					   funnelState = funnelState.Clone(),
					   unclampedEndPoint = unclampedEndPoint,
					   unclampedStartPoint = unclampedStartPoint,
					   startNodeInternal = startNodeInternal,

					   firstPartIndex = firstPartIndex,
					   startIsUpToDate = startIsUpToDate,
					   endIsUpToDate = endIsUpToDate,
					   firstPartContainsDestroyedNodes = firstPartContainsDestroyedNodes,
					   version = version,
					   nnConstraint = NNConstraint.Walkable,
					   partGraphType = partGraphType,
			};
		}
	}
}
