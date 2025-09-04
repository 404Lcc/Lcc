#pragma warning disable 0162
using UnityEngine;
using Pathfinding.Serialization;
using Pathfinding.Collections;
using UnityEngine.Assertions;
using Unity.Mathematics;
using Pathfinding.Util;
using Unity.Burst;
using Unity.Profiling;

namespace Pathfinding {
	/// <summary>Interface for something that holds a triangle based navmesh</summary>
	public interface INavmeshHolder : ITransformedGraph {
		void GetNodes(System.Action<GraphNode> del);

		/// <summary>Position of vertex number i in the world</summary>
		Int3 GetVertex(int i);

		/// <summary>
		/// Position of vertex number i in coordinates local to the graph.
		/// The up direction is always the +Y axis for these coordinates.
		/// </summary>
		Int3 GetVertexInGraphSpace(int i);

		int GetVertexArrayIndex(int index);

		/// <summary>Transforms coordinates from graph space to world space</summary>
		void GetTileCoordinates(int tileIndex, out int x, out int z);
	}

	/// <summary>Node represented by a triangle</summary>
	[Unity.Burst.BurstCompile]
	// Sealing the class provides a nice performance boost (~5-10%) during pathfinding, because the JIT can inline more things and use non-virtual calls.
	public sealed class TriangleMeshNode : MeshNode {
		public TriangleMeshNode () {
			HierarchicalNodeIndex = 0;
			NodeIndex = DestroyedNodeIndex;
		}

		public TriangleMeshNode (AstarPath astar) {
			astar.InitializeNode(this);
		}

		/// <summary>
		/// Legacy compatibility.
		/// Enabling this will make pathfinding use node centers, which leads to less accurate paths (but it's faster).
		/// </summary>
		public const bool InaccuratePathSearch = false;
		internal override int PathNodeVariants => InaccuratePathSearch ? 1 : 3;

		/// <summary>Internal vertex index for the first vertex</summary>
		public int v0;

		/// <summary>Internal vertex index for the second vertex</summary>
		public int v1;

		/// <summary>Internal vertex index for the third vertex</summary>
		public int v2;

		/// <summary>Holds INavmeshHolder references for all graph indices to be able to access them in a performant manner</summary>
		static INavmeshHolder[] _navmeshHolders = new INavmeshHolder[0];

		/// <summary>Used for synchronised access to the <see cref="_navmeshHolders"/> array</summary>
		static readonly System.Object lockObject = new System.Object();

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static INavmeshHolder GetNavmeshHolder (uint graphIndex) {
			return _navmeshHolders[(int)graphIndex];
		}

		/// <summary>
		/// Tile index in the recast or navmesh graph that this node is part of.
		/// See: <see cref="NavmeshBase.GetTiles"/>
		/// </summary>
		public int TileIndex => (v0 >> NavmeshBase.TileIndexOffset) & NavmeshBase.TileIndexMask;

		/// <summary>
		/// Sets the internal navmesh holder for a given graph index.
		/// Warning: Internal method
		/// </summary>
		public static void SetNavmeshHolder (int graphIndex, INavmeshHolder graph) {
			// We need to lock to make sure that
			// the resize operation is thread safe
			lock (lockObject) {
				if (graphIndex >= _navmeshHolders.Length) {
					var gg = new INavmeshHolder[graphIndex+1];
					_navmeshHolders.CopyTo(gg, 0);
					_navmeshHolders = gg;
				}
				_navmeshHolders[graphIndex] = graph;
			}
		}

		public static void ClearNavmeshHolder (int graphIndex, INavmeshHolder graph) {
			lock (lockObject) {
				if (graphIndex < _navmeshHolders.Length && _navmeshHolders[graphIndex] == graph) {
					_navmeshHolders[graphIndex] = null;
				}
			}
		}

		/// <summary>Set the position of this node to the average of its 3 vertices</summary>
		public void UpdatePositionFromVertices () {
			Int3 a, b, c;

			GetVertices(out a, out b, out c);
			position = (a + b + c) * 0.333333f;
		}

		/// <summary>
		/// Return a number identifying a vertex.
		/// This number does not necessarily need to be a index in an array but two different vertices (in the same graph) should
		/// not have the same vertex numbers.
		/// </summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		[IgnoredByDeepProfiler]
		public int GetVertexIndex (int i) {
			return i == 0 ? v0 : (i == 1 ? v1 : v2);
		}

		/// <summary>
		/// Return a number specifying an index in the source vertex array.
		/// The vertex array can for example be contained in a recast tile, or be a navmesh graph, that is graph dependant.
		/// This is slower than GetVertexIndex, if you only need to compare vertices, use GetVertexIndex.
		/// </summary>
		public int GetVertexArrayIndex (int i) {
			return GetNavmeshHolder(GraphIndex).GetVertexArrayIndex(i == 0 ? v0 : (i == 1 ? v1 : v2));
		}

		/// <summary>Returns all 3 vertices of this node in world space</summary>
		public void GetVertices (out Int3 v0, out Int3 v1, out Int3 v2) {
			// Get the object holding the vertex data for this node
			// This is usually a graph or a recast graph tile
			var holder = GetNavmeshHolder(GraphIndex);

			v0 = holder.GetVertex(this.v0);
			v1 = holder.GetVertex(this.v1);
			v2 = holder.GetVertex(this.v2);
		}

		/// <summary>Returns all 3 vertices of this node in graph space</summary>
		public void GetVerticesInGraphSpace (out Int3 v0, out Int3 v1, out Int3 v2) {
			// Get the object holding the vertex data for this node
			// This is usually a graph or a recast graph tile
			var holder = GetNavmeshHolder(GraphIndex);

			v0 = holder.GetVertexInGraphSpace(this.v0);
			v1 = holder.GetVertexInGraphSpace(this.v1);
			v2 = holder.GetVertexInGraphSpace(this.v2);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		[IgnoredByDeepProfiler]
		public override Int3 GetVertex (int i) {
			return GetNavmeshHolder(GraphIndex).GetVertex(GetVertexIndex(i));
		}

		public Int3 GetVertexInGraphSpace (int i) {
			return GetNavmeshHolder(GraphIndex).GetVertexInGraphSpace(GetVertexIndex(i));
		}

		public override int GetVertexCount () {
			// A triangle has 3 vertices
			return 3;
		}

		/// <summary>
		/// Projects the given point onto the plane of this node's surface.
		///
		/// The point will be projected down to a plane that contains the surface of the node.
		/// If the point is not contained inside the node, it is projected down onto this plane anyway.
		/// </summary>
		public Vector3 ProjectOnSurface (Vector3 point) {
			Int3 a, b, c;

			GetVertices(out a, out b, out c);
			var pa = (Vector3)a;
			var pb = (Vector3)b;
			var pc = (Vector3)c;
			var up = Vector3.Cross(pb-pa, pc-pa).normalized;
			return point - up * Vector3.Dot(up, point-pa);
		}

		public override Vector3 ClosestPointOnNode (Vector3 p) {
			Int3 a, b, c;

			GetVertices(out a, out b, out c);
			return Pathfinding.Polygon.ClosestPointOnTriangle((float3)(Vector3)a, (float3)(Vector3)b, (float3)(Vector3)c, (float3)p);
		}

		/// <summary>
		/// Closest point on the node when seen from above.
		/// This method is mostly for internal use as the <see cref="Pathfinding.NavmeshBase.Linecast"/> methods use it.
		///
		/// - The returned point is the closest one on the node to p when seen from above (relative to the graph).
		///   This is important mostly for sloped surfaces.
		/// - The returned point is an Int3 point in graph space.
		/// - It is guaranteed to be inside the node, so if you call <see cref="ContainsPointInGraphSpace"/> with the return value from this method the result is guaranteed to be true.
		///
		/// This method is slower than e.g <see cref="ClosestPointOnNode"/> or <see cref="ClosestPointOnNodeXZ"/>.
		/// However they do not have the same guarantees as this method has.
		/// </summary>
		internal Int3 ClosestPointOnNodeXZInGraphSpace (Vector3 p) {
			// Get the vertices that make up the triangle
			Int3 a, b, c;

			GetVerticesInGraphSpace(out a, out b, out c);

			// Convert p to graph space
			p = GetNavmeshHolder(GraphIndex).transform.InverseTransform(p);

			// Find the closest point on the triangle to p when looking at the triangle from above (relative to the graph)
			var closest = Pathfinding.Polygon.ClosestPointOnTriangleXZ((Vector3)a, (Vector3)b, (Vector3)c, p);

			// Make sure the point is actually inside the node
			var i3closest = (Int3)closest;
			if (ContainsPointInGraphSpace(i3closest)) {
				// Common case
				return i3closest;
			} else {
				// Annoying...
				// The closest point when converted from floating point coordinates to integer coordinates
				// is not actually inside the node. It needs to be inside the node for some methods
				// (like for example Linecast) to work properly.

				// Try the 8 integer coordinates around the closest point
				// and check if any one of them are completely inside the node.
				// This will most likely succeed as it should be very close.
				for (int dx = -1; dx <= 1; dx++) {
					for (int dz = -1; dz <= 1; dz++) {
						if ((dx != 0 || dz != 0)) {
							var candidate = new Int3(i3closest.x + dx, i3closest.y, i3closest.z + dz);
							if (ContainsPointInGraphSpace(candidate)) return candidate;
						}
					}
				}

				// Happens veery rarely.
				// Pick the closest vertex of the triangle.
				// The vertex is guaranteed to be inside the triangle.
				var da = (a - i3closest).sqrMagnitudeLong;
				var db = (b - i3closest).sqrMagnitudeLong;
				var dc = (c - i3closest).sqrMagnitudeLong;
				return da < db ? (da < dc ? a : c) : (db < dc ? b : c);
			}
		}

		public override Vector3 ClosestPointOnNodeXZ (Vector3 p) {
			// Get all 3 vertices for this node
			GetVertices(out Int3 tp1, out Int3 tp2, out Int3 tp3);
			return Polygon.ClosestPointOnTriangleXZ((Vector3)tp1, (Vector3)tp2, (Vector3)tp3, p);
		}

		/// <summary>
		/// Checks if point is inside the node when seen from above.
		///
		/// Note that <see cref="ContainsPointInGraphSpace"/> is faster than this method as it avoids
		/// some coordinate transformations. If you are repeatedly calling this method
		/// on many different nodes but with the same point then you should consider
		/// transforming the point first and then calling ContainsPointInGraphSpace.
		///
		/// <code>
		/// Int3 p = (Int3)graph.transform.InverseTransform(point);
		///
		/// node.ContainsPointInGraphSpace(p);
		/// </code>
		/// </summary>
		public override bool ContainsPoint (Vector3 p) {
			return ContainsPointInGraphSpace((Int3)GetNavmeshHolder(GraphIndex).transform.InverseTransform(p));
		}

		/// <summary>Checks if point is inside the node when seen from above, as defined by the movement plane</summary>
		public bool ContainsPoint (Vector3 p, NativeMovementPlane movementPlane) {
			// Get all 3 vertices for this node
			GetVertices(out var a, out var b, out var c);
			var pa = (int3)a;
			var pb = (int3)b;
			var pc = (int3)c;
			var pp = (int3)(Int3)p;
			return Polygon.ContainsPoint(ref pa, ref pb, ref pc, ref pp, ref movementPlane);
		}

		/// <summary>
		/// Checks if point is inside the node in graph space.
		///
		/// In graph space the up direction is always the Y axis so in principle
		/// we project the triangle down on the XZ plane and check if the point is inside the 2D triangle there.
		/// </summary>
		public override bool ContainsPointInGraphSpace (Int3 p) {
			// Get all 3 vertices for this node
			GetVerticesInGraphSpace(out var a, out var b, out var c);

			if ((long)(b.x - a.x) * (long)(p.z - a.z) - (long)(p.x - a.x) * (long)(b.z - a.z) > 0) return false;

			if ((long)(c.x - b.x) * (long)(p.z - b.z) - (long)(p.x - b.x) * (long)(c.z - b.z) > 0) return false;

			if ((long)(a.x - c.x) * (long)(p.z - c.z) - (long)(p.x - c.x) * (long)(a.z - c.z) > 0) return false;

			return true;
			// Equivalent code, but the above code is faster
			//return Polygon.IsClockwiseMargin (a,b, p) && Polygon.IsClockwiseMargin (b,c, p) && Polygon.IsClockwiseMargin (c,a, p);

			//return Polygon.ContainsPoint(g.GetVertex(v0),g.GetVertex(v1),g.GetVertex(v2),p);
		}

		public static readonly Unity.Profiling.ProfilerMarker MarkerDecode = new Unity.Profiling.ProfilerMarker("Decode");
		public static readonly Unity.Profiling.ProfilerMarker MarkerGetVertices = new Unity.Profiling.ProfilerMarker("GetVertex");
		public static readonly Unity.Profiling.ProfilerMarker MarkerClosest = new Unity.Profiling.ProfilerMarker("MarkerClosest");

		public override Int3 DecodeVariantPosition (uint pathNodeIndex, uint fractionAlongEdge) {
			var edge = (int)(pathNodeIndex - NodeIndex);
			var p1 = GetVertex(edge);
			var p2 = GetVertex((edge + 1) % 3);
			InterpolateEdge(ref p1, ref p2, fractionAlongEdge, out var pos);
			return pos;
		}

		[BurstCompile(FloatMode = FloatMode.Fast)]
		static void InterpolateEdge (ref Int3 p1, ref Int3 p2, uint fractionAlongEdge, out Int3 pos) {
			var p = (int3)math.lerp((float3)(int3)p1, (float3)(int3)p2, PathNode.UnQuantizeFractionAlongEdge(fractionAlongEdge));
			pos = new Int3(p.x, p.y, p.z);
		}

		public override void OpenAtPoint (Path path, uint pathNodeIndex, Int3 point, uint gScore) {
			if (InaccuratePathSearch) {
				Open(path, pathNodeIndex, gScore);
			} else {
				OpenAtPoint(path, pathNodeIndex, point, -1, gScore);
			}
		}

		public override void Open (Path path, uint pathNodeIndex, uint gScore) {
			var pathHandler = (path as IPathInternals).PathHandler;
			if (InaccuratePathSearch) {
				var pn = pathHandler.pathNodes[pathNodeIndex];
				if (pn.flag1) path.OpenCandidateConnectionsToEndNode(position, pathNodeIndex, NodeIndex, gScore);

				if (connections != null) {
					// Iterate over all adjacent nodes
					for (int i = connections.Length-1; i >= 0; i--) {
						var conn = connections[i];
						var other = conn.node;
						if (conn.isOutgoing && other.NodeIndex != pn.parentIndex) {
							path.OpenCandidateConnection(pathNodeIndex, other.NodeIndex, gScore, conn.cost + path.GetTraversalCost(other), 0, other.position);
						}
					}
				}
				return;
			}
			// One path node variant is created for each side of the triangle
			// This particular path node represents just one of the sides of the triangle.
			var edge = (int)(pathNodeIndex - NodeIndex);
			OpenAtPoint(path, pathNodeIndex, DecodeVariantPosition(pathNodeIndex, pathHandler.pathNodes[pathNodeIndex].fractionAlongEdge), edge, gScore);
		}

		void OpenAtPoint (Path path, uint pathNodeIndex, Int3 pos, int edge, uint gScore) {
			var pathHandler = (path as IPathInternals).PathHandler;
			var pn = pathHandler.pathNodes[pathNodeIndex];
			if (pn.flag1) path.OpenCandidateConnectionsToEndNode(pos, pathNodeIndex, NodeIndex, gScore);
			int visitedEdges = 0;
			bool cameFromOtherEdgeInThisTriangle = pn.parentIndex >= NodeIndex && pn.parentIndex < NodeIndex + 3;

			if (connections != null) {
				// Iterate over all adjacent nodes
				for (int i = connections.Length-1; i >= 0; i--) {
					var conn = connections[i];
					if (!conn.isOutgoing) continue;
					var other = conn.node;

					// Check if we are moving from a side of this triangle, to the corresponding side on an adjacent triangle.
					if (conn.isEdgeShared) {
						var sharedEdgeOnOtherNode = conn.adjacentShapeEdge;
						var adjacentPathNodeIndex = other.NodeIndex + (uint)sharedEdgeOnOtherNode;

						// Skip checking our parent node. This is purely a performance optimization.
						if (adjacentPathNodeIndex == pn.parentIndex) continue;

						if (conn.shapeEdge == edge) {
							// Make sure we can traverse the neighbour
							if (path.CanTraverse(this, other)) {
								var tOther = other as TriangleMeshNode;

								// Fast path out if we know we have already searched this node and we cannot improve it
								if (!path.ShouldConsiderPathNode(adjacentPathNodeIndex)) {
									continue;
								}

								if (conn.edgesAreIdentical) {
									// The edge on the other node is identical to this edge (but reversed).
									// This means that no other node can reach the other node through that edge.
									// This is great, because we can then skip adding that node to the heap just
									// to immediatelly pop it again. This is a performance optimization.

									var otherGScore = gScore + path.GetTraversalCost(other);
									path.SkipOverNode(adjacentPathNodeIndex, pathNodeIndex, PathNode.ReverseFractionAlongEdge(pn.fractionAlongEdge), uint.MaxValue, otherGScore);
									tOther.OpenAtPoint(path, adjacentPathNodeIndex, pos, sharedEdgeOnOtherNode, otherGScore);
								} else {
									OpenSingleEdge(path, pathNodeIndex, tOther, sharedEdgeOnOtherNode, pos, gScore);
								}
							}
						} else {
							// The other node is a node which shares a different edge with this node.
							// We will consider this connection at another time.

							// However, we will consider the move to another side of this triangle,
							// namely to the side that *is* shared with the other node.
							// If a side of this triangle doesn't share an edge with any connection, we will
							// not bother searching it (we will not reach this part of the code), because
							// we know its a dead end.

							// If we came from another side of this triangle, it is completely redundant to try to move back to
							// another edge in this triangle, because we could always have reached it faster from the parent.
							// We also make sure we don't attempt to move to the same edge twice, as that's just a waste of time.
							if (!cameFromOtherEdgeInThisTriangle && (visitedEdges & (1 << conn.shapeEdge)) == 0) {
								visitedEdges |= 1 << conn.shapeEdge;
								OpenSingleEdge(path, pathNodeIndex, this, conn.shapeEdge, pos, gScore);
							}
						}
					} else if (!cameFromOtherEdgeInThisTriangle) {
						// This is a connection to some other node type, most likely. For example an off-mesh link.
						if (path.CanTraverse(this, other) && path.ShouldConsiderPathNode(other.NodeIndex)) {
							var cost = (uint)(other.position - pos).costMagnitude;

							if (edge != -1) {
								// We are moving from an edge of this triangle
								path.OpenCandidateConnection(pathNodeIndex, other.NodeIndex, gScore, cost, 0, other.position);
							} else {
								// In some situations we may be moving directly from one off-mesh link to another one without
								// passing through any concrete nodes in between. In this case we need to create a temporary node
								// to allow the correct path to be reconstructed later. The only important part of the temporary
								// node is that we save this node as the associated node.
								// This is somewhat ugly, and it limits the number of times we can encounter this case during
								// a single search (there's a limit to the number of temporary nodes we can have at the same time).
								// Fortunately, this case only happens if there is more than 1 off-mesh link connected to a single
								// node, which is quite rare in most games.
								// In this case, pathNodeIndex will be another node's index, not a path node belonging to this node.
								var viaNode = pathHandler.AddTemporaryNode(new TemporaryNode {
									associatedNode = NodeIndex,
									position = pos,
									targetIndex = 0,
									type = TemporaryNodeType.Ignore,
								});
								ref var viaPathNode = ref pathHandler.pathNodes[viaNode];
								viaPathNode.pathID = path.pathID;
								viaPathNode.parentIndex = pathNodeIndex;
								path.OpenCandidateConnection(viaNode, other.NodeIndex, gScore, cost, 0, other.position);
							}
						}
					}
				}
			}

			if (edge == -1) {
				// If we have entered this node via an off-mesh link, or if this was the first node in the path,
				// then we must consider moving directly from #pos to the end point of the path.
				// Otherwise we would just consider paths that first to the side of the triangle and then back to the end point of the path.
				// Note: flag1 checks if this node is connected to the end node of the path.
				if (pathHandler.pathNodes[NodeIndex].flag1) {
					// Note: If we entered this node via an off-mesh link, then #pathNodeIndex may not belong to this node,
					// but instead to the off-mesh link. This is fine. The path can still be reconstructed correctly later.
					path.OpenCandidateConnectionsToEndNode(pos, pathNodeIndex, NodeIndex, gScore);
				}

				// Sometimes we may enter a node via e.g. an off-mesh link that doesn't have any other adjacent triangles.
				// In this case we still want to visit at least one side of the triangle, to ensure that paths like the
				// ConstantPath notice that the node has been visited.
				// The code above would otherwise skip this node completely.
				if (visitedEdges == 0) {
					OpenSingleEdge(path, pathNodeIndex, this, 0, pos, gScore);
				}
			}
		}

		void OpenSingleEdge (Path path, uint pathNodeIndex, TriangleMeshNode other, int sharedEdgeOnOtherNode, Int3 pos, uint gScore) {
			var adjacentPathNodeIndex = other.NodeIndex + (uint)sharedEdgeOnOtherNode;

			// Fast path out if we know we have already searched this node and we cannot improve it
			if (!path.ShouldConsiderPathNode(adjacentPathNodeIndex)) {
				return;
			}

			var s1 = other.GetVertex(sharedEdgeOnOtherNode);
			var s2 = other.GetVertex((sharedEdgeOnOtherNode + 1) % 3);

			var pathHandler = (path as IPathInternals).PathHandler;
			// TODO: Incorrect, counts nodes multiple times
			var otherEnteringCost = path.GetTraversalCost(other);

			var candidateG = gScore + otherEnteringCost;

			OpenSingleEdgeBurst(
				ref s1,
				ref s2,
				ref pos,
				path.pathID,
				pathNodeIndex,
				adjacentPathNodeIndex,
				other.NodeIndex,
				candidateG,
				ref pathHandler.pathNodes,
				ref pathHandler.heap,
				ref path.heuristicObjectiveInternal
				);
		}

		[Unity.Burst.BurstCompile]
		static void OpenSingleEdgeBurst (ref Int3 s1, ref Int3 s2, ref Int3 pos, ushort pathID, uint pathNodeIndex, uint candidatePathNodeIndex, uint candidateNodeIndex, uint candidateG, ref UnsafeSpan<PathNode> pathNodes, ref BinaryHeap heap, ref HeuristicObjective heuristicObjective) {
			CalculateBestEdgePosition(ref s1, ref s2, ref pos, out var closestPointAlongEdge, out var quantizedFractionAlongEdge, out var cost);
			candidateG += cost;

			var pars = new Path.OpenCandidateParams {
				pathID = pathID,
				parentPathNode = pathNodeIndex,
				targetPathNode = candidatePathNodeIndex,
				targetNodeIndex = candidateNodeIndex,
				candidateG = candidateG,
				fractionAlongEdge = quantizedFractionAlongEdge,
				targetNodePosition = closestPointAlongEdge,
				pathNodes = pathNodes,
			};
			Path.OpenCandidateConnectionBurst(ref pars, ref heap, ref heuristicObjective);
		}

		[Unity.Burst.BurstCompile]
		static void CalculateBestEdgePosition (ref Int3 s1, ref Int3 s2, ref Int3 pos, out int3 closestPointAlongEdge, out uint quantizedFractionAlongEdge, out uint cost) {
			// Find the closest point on the other edge. From here on, we will let the position of that path node be this closest point.
			// This is much better than using the edge midpoint, and also better than any interpolation between closestFractionAlongEdge
			// and the midpoint (0.5).
			// In my tests, using the edge midpoint leads to path costs that are rougly 1.3-1.6 times greater than the real distance,
			// but using the closest point leads to path costs that are only 1.1-1.2 times greater than the real distance.
			// Using triangle centers is the worst option, it leads to path costs that are roughly 1.6-2.0 times greater than the real distance.
			// Triangle centers were always used before version 4.3.67.
			var v1 = (float3)(int3)s1;
			var v2 = (float3)(int3)s2;
			var posi = (int3)pos;
			var closestFractionAlongEdge = math.clamp(VectorMath.ClosestPointOnLineFactor(v1, v2, (float3)posi), 0, 1);
			quantizedFractionAlongEdge = PathNode.QuantizeFractionAlongEdge(closestFractionAlongEdge);
			closestFractionAlongEdge = PathNode.UnQuantizeFractionAlongEdge(quantizedFractionAlongEdge);
			var closestPointAlongEdgeV = math.lerp(v1, v2, closestFractionAlongEdge);
			closestPointAlongEdge = (int3)closestPointAlongEdgeV;

			var diff = posi - closestPointAlongEdge;
			cost = (uint)new Int3(diff.x, diff.y, diff.z).costMagnitude;
		}

		/// <summary>
		/// Returns the edge which is shared with other.
		///
		/// If there is no shared edge between the two nodes, then -1 is returned.
		///
		/// The vertices in the edge can be retrieved using
		/// <code>
		/// var edge = node.SharedEdge(other);
		/// var a = node.GetVertex(edge);
		/// var b = node.GetVertex((edge+1) % node.GetVertexCount());
		/// </code>
		///
		/// See: <see cref="GetPortal"/> which also handles edges that are shared over tile borders and some types of node links
		/// </summary>
		public int SharedEdge (GraphNode other) {
			var edge = -1;

			if (connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					if (connections[i].node == other && connections[i].isEdgeShared) edge = connections[i].shapeEdge;
				}
			}
			return edge;
		}

		public override bool GetPortal (GraphNode toNode, out Vector3 left, out Vector3 right) {
			return GetPortal(toNode, out left, out right, out _, out _);
		}

		public bool GetPortalInGraphSpace (TriangleMeshNode toNode, out Int3 a, out Int3 b, out int aIndex, out int bIndex) {
			aIndex = -1;
			bIndex = -1;
			a = Int3.zero;
			b = Int3.zero;

			// If the nodes are in different graphs, this function has no idea on how to find a shared edge.
			if (toNode.GraphIndex != GraphIndex) return false;

			int edge = -1;
			int otherEdge = -1;
			if (connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					if (connections[i].node == toNode && connections[i].isEdgeShared) {
						edge = connections[i].shapeEdge;
						otherEdge = connections[i].adjacentShapeEdge;
					}
				}
			}

			// -1: No connection was found between the nodes
			if (edge == -1) return false;

			aIndex = edge;
			bIndex = (edge + 1) % 3;

			// Get the vertices of the shared edge for the first node
			var graph = GetNavmeshHolder(GraphIndex);
			a = graph.GetVertexInGraphSpace(GetVertexIndex(aIndex));
			b = graph.GetVertexInGraphSpace(GetVertexIndex(bIndex));

			// Get tiles the nodes are contained in
			int tileIndex1 = TileIndex;
			int tileIndex2 = toNode.TileIndex;

			if (tileIndex1 != tileIndex2) {
				// When the nodes are in different tiles, the edges may not be completely identical
				// so another technique is needed.

				// When the nodes are in different tiles, they might not share exactly the same edge
				// so we clamp the portal to the segment of the edges which they both have..

				// Get the vertices of the shared edge for the second node
				Int3 v2a = toNode.GetVertexInGraphSpace(otherEdge);
				Int3 v2b = toNode.GetVertexInGraphSpace((otherEdge+1) % 3);
				graph.GetTileCoordinates(tileIndex1, out var tileX1, out var tileZ1);
				graph.GetTileCoordinates(tileIndex2, out var tileX2, out var tileZ2);
				var axis = tileX1 == tileX2 ? 0 : 2;
				Assert.IsTrue(axis == 0 ? tileX1 == tileX2 : tileZ1 == tileZ2);
				// This tile-edge aligned coordinate of the vertices should ideally be identical.
				// But somewhere in the pipeline some errors may crop up, and thus they may be off by one.
				// TODO: Fix this.
				Assert.IsTrue(Mathf.Abs(a[2 - axis] - b[2 - axis]) <= 1);
				var mn = Mathf.Min(v2a[axis], v2b[axis]);
				var mx = Mathf.Max(v2a[axis], v2b[axis]);

				a[axis] = Mathf.Clamp(a[axis], mn, mx);
				b[axis] = Mathf.Clamp(b[axis], mn, mx);
			}

			return true;
		}

		public bool GetPortal (GraphNode toNode, out Vector3 left, out Vector3 right, out int aIndex, out int bIndex) {
			if (toNode is TriangleMeshNode toTriNode && GetPortalInGraphSpace(toTriNode, out var a, out var b, out aIndex, out bIndex)) {
				var graph = GetNavmeshHolder(GraphIndex);
				// All triangles should be laid out in clockwise order so b is the rightmost vertex (seen from this node)
				left = graph.transform.Transform((Vector3)a);
				right = graph.transform.Transform((Vector3)b);
				return true;
			} else {
				aIndex = -1;
				bIndex = -1;
				left = Vector3.zero;
				right = Vector3.zero;
				return false;
			}
		}

		/// <summary>TODO: This is the area in XZ space, use full 3D space for higher correctness maybe?</summary>
		public override float SurfaceArea () {
			var holder = GetNavmeshHolder(GraphIndex);

			return System.Math.Abs(VectorMath.SignedTriangleAreaTimes2XZ(holder.GetVertex(v0), holder.GetVertex(v1), holder.GetVertex(v2))) * 0.5f;
		}

		public override Vector3 RandomPointOnSurface () {
			// Find a random point inside the triangle
			// This generates uniformly distributed trilinear coordinates
			// See http://mathworld.wolfram.com/TrianglePointPicking.html
			float2 r;

			do {
				r = AstarMath.ThreadSafeRandomFloat2();
			} while (r.x+r.y > 1);

			// Pick the point corresponding to the trilinear coordinate
			GetVertices(out var v0, out var v1, out var v2);
			return ((Vector3)(v1-v0))*r.x + ((Vector3)(v2-v0))*r.y + (Vector3)v0;
		}

		public override void SerializeNode (GraphSerializationContext ctx) {
			base.SerializeNode(ctx);
			ctx.writer.Write(v0);
			ctx.writer.Write(v1);
			ctx.writer.Write(v2);
		}

		public override void DeserializeNode (GraphSerializationContext ctx) {
			base.DeserializeNode(ctx);
			v0 = ctx.reader.ReadInt32();
			v1 = ctx.reader.ReadInt32();
			v2 = ctx.reader.ReadInt32();
		}
	}
}
