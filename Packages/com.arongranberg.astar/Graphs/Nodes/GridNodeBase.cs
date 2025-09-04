#define PREALLOCATE_NODES
using UnityEngine;
using Pathfinding.Serialization;

namespace Pathfinding {
	/// <summary>Base class for GridNode and LevelGridNode</summary>
	public abstract class GridNodeBase : GraphNode {
		const int GridFlagsWalkableErosionOffset = 8;
		const int GridFlagsWalkableErosionMask = 1 << GridFlagsWalkableErosionOffset;

		const int GridFlagsWalkableTmpOffset = 9;
		const int GridFlagsWalkableTmpMask = 1 << GridFlagsWalkableTmpOffset;

		public const int NodeInGridIndexLayerOffset = 24;
		protected const int NodeInGridIndexMask = 0xFFFFFF;

		/// <summary>
		/// Bitfield containing the x and z coordinates of the node as well as the layer (for layered grid graphs).
		/// See: NodeInGridIndex
		/// </summary>
		protected int nodeInGridIndex;
		protected ushort gridFlags;

#if !ASTAR_GRID_NO_CUSTOM_CONNECTIONS
		/// <summary>
		/// Custon non-grid connections from this node.
		/// See: <see cref="Connect"/>
		/// See: <see cref="Disconnect"/>
		///
		/// This field is removed if the ASTAR_GRID_NO_CUSTOM_CONNECTIONS compiler directive is used.
		/// Removing it can save a tiny bit of memory. You can enable the define in the Optimizations tab in the A* inspector.
		/// See: compiler-directives (view in online documentation for working links)
		///
		/// Note: If you modify this array or the contents of it you must call <see cref="SetConnectivityDirty"/>.
		/// </summary>
		public Connection[] connections;
#endif

		/// <summary>
		/// The index of the node in the grid.
		/// This is x + z*graph.width
		/// So you can get the X and Z indices using
		/// <code>
		/// int index = node.NodeInGridIndex;
		/// int x = index % graph.width;
		/// int z = index / graph.width;
		/// // where graph is GridNode.GetGridGraph (node.graphIndex), i.e the graph the nodes are contained in.
		/// </code>
		///
		/// See: <see cref="CoordinatesInGrid"/>
		/// </summary>
		public int NodeInGridIndex { get { return nodeInGridIndex & NodeInGridIndexMask; } set { nodeInGridIndex = (nodeInGridIndex & ~NodeInGridIndexMask) | value; } }

		/// <summary>
		/// X coordinate of the node in the grid.
		/// The node in the bottom left corner has (x,z) = (0,0) and the one in the opposite
		/// corner has (x,z) = (width-1, depth-1)
		///
		/// See: <see cref="ZCoordinateInGrid"/>
		/// See: <see cref="NodeInGridIndex"/>
		/// </summary>
		public int XCoordinateInGrid => NodeInGridIndex % GridNode.GetGridGraph(GraphIndex).width;

		/// <summary>
		/// Z coordinate of the node in the grid.
		/// The node in the bottom left corner has (x,z) = (0,0) and the one in the opposite
		/// corner has (x,z) = (width-1, depth-1)
		///
		/// See: <see cref="XCoordinateInGrid"/>
		/// See: <see cref="NodeInGridIndex"/>
		/// </summary>
		public int ZCoordinateInGrid => NodeInGridIndex / GridNode.GetGridGraph(GraphIndex).width;

		/// <summary>
		/// The X and Z coordinates of the node in the grid.
		///
		/// The node in the bottom left corner has (x,z) = (0,0) and the one in the opposite
		/// corner has (x,z) = (width-1, depth-1)
		///
		/// See: <see cref="XCoordinateInGrid"/>
		/// See: <see cref="ZCoordinateInGrid"/>
		/// See: <see cref="NodeInGridIndex"/>
		/// </summary>
		public Vector2Int CoordinatesInGrid {
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			get {
				var width = GridNode.GetGridGraph(GraphIndex).width;
				var index = NodeInGridIndex;
				var z = index / width;
				var x = index - z * width;
				return new Vector2Int(x, z);
			}
		}

		/// <summary>
		/// Stores walkability before erosion is applied.
		/// Used internally when updating the graph.
		/// </summary>
		public bool WalkableErosion {
			get {
				return (gridFlags & GridFlagsWalkableErosionMask) != 0;
			}
			set {
				unchecked { gridFlags = (ushort)(gridFlags & ~GridFlagsWalkableErosionMask | (value ? (ushort)GridFlagsWalkableErosionMask : (ushort)0)); }
			}
		}

		/// <summary>Temporary variable used internally when updating the graph.</summary>
		public bool TmpWalkable {
			get {
				return (gridFlags & GridFlagsWalkableTmpMask) != 0;
			}
			set {
				unchecked { gridFlags = (ushort)(gridFlags & ~GridFlagsWalkableTmpMask | (value ? (ushort)GridFlagsWalkableTmpMask : (ushort)0)); }
			}
		}

		/// <summary>
		/// True if the node has grid connections to all its 8 neighbours.
		/// Note: This will always return false if GridGraph.neighbours is set to anything other than Eight.
		/// See: GetNeighbourAlongDirection
		/// See: <see cref="HasConnectionsToAllAxisAlignedNeighbours"/>
		/// </summary>
		public abstract bool HasConnectionsToAllEightNeighbours { get; }

		/// <summary>
		/// True if the node has grid connections to all its 4 axis-aligned neighbours.
		/// See: GetNeighbourAlongDirection
		/// See: <see cref="HasConnectionsToAllEightNeighbours"/>
		/// </summary>
		public abstract bool HasConnectionsToAllAxisAlignedNeighbours { get; }

		/// <summary>
		/// The connection opposite the given one.
		///
		/// <code>
		///         Z
		///         |
		///         |
		///
		///      6  2  5
		///       \ | /
		/// --  3 - X - 1  ----- X
		///       / | \
		///      7  0  4
		///
		///         |
		///         |
		/// </code>
		///
		/// For example, dir=1 outputs 3, dir=6 outputs 4 and so on.
		///
		/// See: <see cref="HasConnectionInDirection"/>
		/// </summary>
		public static int OppositeConnectionDirection (int dir) {
			return dir < 4 ? ((dir + 2) % 4) : (((dir-2) % 4) + 4);
		}

		/// <summary>
		/// Converts from dx + 3*dz to a neighbour direction.
		///
		/// Used by <see cref="OffsetToConnectionDirection"/>.
		///
		/// Assumes that dx and dz are both in the range [0,2].
		/// See: <see cref="GridGraph.neighbourOffsets"/>
		/// </summary>
		internal static readonly int[] offsetToDirection = { 7, 0, 4, 3, -1, 1, 6, 2, 5 };

		/// <summary>
		/// Converts from a delta (dx, dz) to a neighbour direction.
		///
		/// For example, if dx=1 and dz=0, the return value will be 1, which is the direction to the right of a grid coordinate.
		///
		/// If dx=0 and dz=0, the return value will be -1.
		///
		/// See: <see cref="GridGraph.neighbourOffsets"/>
		///
		/// <code>
		///         Z
		///         |
		///         |
		///
		///      6  2  5
		///       \ | /
		/// --  3 - X - 1  ----- X
		///       / | \
		///      7  0  4
		///
		///         |
		///         |
		/// </code>
		///
		/// See: <see cref="HasConnectionInDirection"/>
		/// </summary>
		/// <param name="dx">X coordinate delta. Should be in the range [-1, 1]. Values outside this range will cause -1 to be returned.</param>
		/// <param name="dz">Z coordinate delta. Should be in the range [-1, 1]. Values outside this range will cause -1 to be returned.</param>
		public static int OffsetToConnectionDirection (int dx, int dz) {
			dx++; dz++;
			if ((uint)dx > 2 || (uint)dz > 2) return -1;
			return offsetToDirection[3*dz + dx];
		}

		/// <summary>
		/// Projects the given point onto the plane of this node's surface.
		///
		/// The point will be projected down to a plane that contains the surface of the node.
		/// If the point is not contained inside the node, it is projected down onto this plane anyway.
		/// </summary>
		public Vector3 ProjectOnSurface (Vector3 point) {
			var gg = GridNode.GetGridGraph(GraphIndex);
			var nodeCenter = (Vector3)position;
			var up = gg.transform.WorldUpAtGraphPosition(nodeCenter);
			return point - up * Vector3.Dot(up, point - nodeCenter);
		}

		public override Vector3 ClosestPointOnNode (Vector3 p) {
			var gg = GridNode.GetGridGraph(GraphIndex);

			// Convert to graph space
			var nodeCenter = (Vector3)position;
			// Calculate the offset from the node's center to the given point in graph space
			var offsetInGraphSpace = gg.transform.InverseTransformVector(p - nodeCenter);
			// Project onto the node's surface
			offsetInGraphSpace.y = 0;
			// Clamp to the node's extents
			offsetInGraphSpace.x = Mathf.Clamp(offsetInGraphSpace.x, -0.5f, 0.5f);
			offsetInGraphSpace.z = Mathf.Clamp(offsetInGraphSpace.z, -0.5f, 0.5f);
			// Convert back to world space
			return nodeCenter + gg.transform.TransformVector(offsetInGraphSpace);
		}

		/// <summary>
		/// Checks if point is inside the node when seen from above.
		///
		/// The borders of a node are considered to be inside the node.
		///
		/// Note that <see cref="ContainsPointInGraphSpace"/> is faster than this method as it avoids
		/// some coordinate transformations. If you are repeatedly calling this method
		/// on many different nodes but with the same point then you should consider
		/// transforming the point first and then calling ContainsPointInGraphSpace.
		/// <code>
		/// Int3 p = (Int3)graph.transform.InverseTransform(point);
		///
		/// node.ContainsPointInGraphSpace(p);
		/// </code>
		/// </summary>
		public override bool ContainsPoint (Vector3 point) {
			var gg = Graph as GridGraph;
			// Convert to graph space
			return ContainsPointInGraphSpace((Int3)gg.transform.InverseTransform(point));
		}

		/// <summary>
		/// Checks if point is inside the node in graph space.
		///
		/// The borders of a node are considered to be inside the node.
		///
		/// The y coordinate of the point is ignored.
		/// </summary>
		public override bool ContainsPointInGraphSpace (Int3 point) {
			// Calculate graph position of this node
			var x = XCoordinateInGrid*Int3.Precision;
			var z = ZCoordinateInGrid*Int3.Precision;

			return point.x >= x && point.x <= x + Int3.Precision && point.z >= z && point.z <= z + Int3.Precision;
		}

		public override float SurfaceArea () {
			GridGraph gg = GridNode.GetGridGraph(GraphIndex);

			return gg.nodeSize*gg.nodeSize;
		}

		public override Vector3 RandomPointOnSurface () {
			GridGraph gg = GridNode.GetGridGraph(GraphIndex);

			var graphSpacePosition = gg.transform.InverseTransform((Vector3)position);

			var r = AstarMath.ThreadSafeRandomFloat2();
			return gg.transform.Transform(graphSpacePosition + new Vector3(r.x - 0.5f, 0, r.y - 0.5f));
		}

		/// <summary>
		/// Transforms a world space point to a normalized point on this node's surface.
		/// (0.5,0.5) represents the node's center. (0,0), (1,0), (1,1) and (0,1) each represent the corners of the node.
		///
		/// See: <see cref="UnNormalizePoint"/>
		/// </summary>
		public Vector2 NormalizePoint (Vector3 worldPoint) {
			GridGraph gg = GridNode.GetGridGraph(GraphIndex);
			var graphSpacePosition = gg.transform.InverseTransform(worldPoint);

			return new Vector2(graphSpacePosition.x - this.XCoordinateInGrid, graphSpacePosition.z - this.ZCoordinateInGrid);
		}

		/// <summary>
		/// Transforms a normalized point on this node's surface to a world space point.
		/// (0.5,0.5) represents the node's center. (0,0), (1,0), (1,1) and (0,1) each represent the corners of the node.
		///
		/// See: <see cref="NormalizePoint"/>
		/// </summary>
		public Vector3 UnNormalizePoint (Vector2 normalizedPointOnSurface) {
			GridGraph gg = GridNode.GetGridGraph(GraphIndex);

			return (Vector3)this.position + gg.transform.TransformVector(new Vector3(normalizedPointOnSurface.x - 0.5f, 0, normalizedPointOnSurface.y - 0.5f));
		}

		public override int GetGizmoHashCode () {
			var hash = base.GetGizmoHashCode();

#if !ASTAR_GRID_NO_CUSTOM_CONNECTIONS
			if (connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					hash ^= 17 * connections[i].GetHashCode();
				}
			}
#endif
			hash ^= 109 * gridFlags;
			return hash;
		}

		/// <summary>
		/// Adjacent grid node in the specified direction.
		/// This will return null if the node does not have a connection to a node
		/// in that direction.
		///
		/// The dir parameter corresponds to directions in the grid as:
		/// <code>
		///         Z
		///         |
		///         |
		///
		///      6  2  5
		///       \ | /
		/// --  3 - X - 1  ----- X
		///       / | \
		///      7  0  4
		///
		///         |
		///         |
		/// </code>
		///
		/// See: <see cref="GetConnections"/>
		/// See: <see cref="GetNeighbourAlongDirection"/>
		///
		/// Note: This method only takes grid connections into account, not custom connections (i.e. those added using <see cref="Connect"/> or using node links).
		/// </summary>
		public abstract GridNodeBase GetNeighbourAlongDirection(int direction);

		/// <summary>
		/// True if the node has a connection to an adjecent node in the specified direction.
		///
		/// The dir parameter corresponds to directions in the grid as:
		/// <code>
		///         Z
		///         |
		///         |
		///
		///      6  2  5
		///       \ | /
		/// --  3 - X - 1  ----- X
		///       / | \
		///      7  0  4
		///
		///         |
		///         |
		/// </code>
		///
		/// See: <see cref="GetConnections"/>
		/// See: <see cref="GetNeighbourAlongDirection"/>
		/// See: <see cref="OffsetToConnectionDirection"/>
		///
		/// Note: This method only takes grid connections into account, not custom connections (i.e. those added using <see cref="Connect"/> or using node links).
		/// </summary>
		public virtual bool HasConnectionInDirection (int direction) {
			// TODO: Can be optimized if overriden in each subclass
			return GetNeighbourAlongDirection(direction) != null;
		}

		/// <summary>True if this node has any grid connections</summary>
		public abstract bool HasAnyGridConnections { get; }

		public override bool ContainsOutgoingConnection (GraphNode node) {
#if !ASTAR_GRID_NO_CUSTOM_CONNECTIONS
			if (connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					if (connections[i].node == node && connections[i].isOutgoing) {
						return true;
					}
				}
			}
#endif

			for (int i = 0; i < 8; i++) {
				if (node == GetNeighbourAlongDirection(i)) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Disables all grid connections from this node.
		/// Note: Other nodes might still be able to get to this node.
		/// Therefore it is recommended to also disable the relevant connections on adjacent nodes.
		/// </summary>
		public abstract void ResetConnectionsInternal();

		public override void OpenAtPoint (Path path, uint pathNodeIndex, Int3 pos, uint gScore) {
			path.OpenCandidateConnectionsToEndNode(pos, pathNodeIndex, pathNodeIndex, gScore);
			path.OpenCandidateConnection(pathNodeIndex, NodeIndex, gScore, 0, 0, position);
		}

		public override void Open (Path path, uint pathNodeIndex, uint gScore) {
			path.OpenCandidateConnectionsToEndNode(position, pathNodeIndex, pathNodeIndex, gScore);

#if !ASTAR_GRID_NO_CUSTOM_CONNECTIONS
			if (connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					GraphNode other = connections[i].node;
					if (!connections[i].isOutgoing || !path.CanTraverse(this, other)) continue;

					path.OpenCandidateConnection(pathNodeIndex, other.NodeIndex, gScore, connections[i].cost, 0, other.position);
				}
			}
#endif
		}

#if ASTAR_GRID_NO_CUSTOM_CONNECTIONS
		public override void AddPartialConnection (GraphNode node, uint cost, bool isOutgoing, bool isIncoming) {
			throw new System.NotImplementedException("GridNodes do not have support for adding manual connections with your current settings."+
				"\nPlease disable ASTAR_GRID_NO_CUSTOM_CONNECTIONS in the Optimizations tab in the A* Inspector");
		}

		public override void RemovePartialConnection (GraphNode node) {
			// Nothing to do because ASTAR_GRID_NO_CUSTOM_CONNECTIONS is enabled
		}

		public void ClearCustomConnections (bool alsoReverse) {
		}
#else
		/// <summary>Same as <see cref="ClearConnections"/>, but does not clear grid connections, only custom ones (e.g added by <see cref="AddConnection"/> or a NodeLink component)</summary>
		public void ClearCustomConnections (bool alsoReverse) {
			if (connections != null) {
				for (int i = 0; i < connections.Length; i++) connections[i].node.RemovePartialConnection(this);
				connections = null;
				AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
			}
		}

		public override void ClearConnections (bool alsoReverse) {
			ClearCustomConnections(alsoReverse);
		}

		public override void GetConnections<T>(GetConnectionsWithData<T> action, ref T data, int connectionFilter) {
			if (connections == null) return;
			for (int i = 0; i < connections.Length; i++) if ((connections[i].shapeEdgeInfo & connectionFilter) != 0) action(connections[i].node, ref data);
		}

		public override void AddPartialConnection (GraphNode node, uint cost, bool isOutgoing, bool isIncoming) {
			if (node == null) throw new System.ArgumentNullException();

			if (connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					if (connections[i].node == node) {
						connections[i].cost = cost;
						connections[i].shapeEdgeInfo = Connection.PackShapeEdgeInfo(isOutgoing, isIncoming);
						return;
					}
				}
			}

			int connLength = connections != null ? connections.Length : 0;

			var newconns = new Connection[connLength+1];
			for (int i = 0; i < connLength; i++) {
				newconns[i] = connections[i];
			}

			newconns[connLength] = new Connection(node, cost, isOutgoing, isIncoming);

			connections = newconns;
			AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
		}

		/// <summary>
		/// Removes any connection from this node to the specified node.
		/// If no such connection exists, nothing will be done.
		///
		/// Note: This only removes the connection from this node to the other node.
		/// You may want to call the same function on the other node to remove its eventual connection
		/// to this node.
		///
		/// Version: Before 4.3.48 This method only handled custom connections (those added using link components or the AddConnection method).
		/// Regular grid connections had to be added or removed using <see cref="Pathfinding.GridNode.SetConnectionInternal"/>. Starting with 4.3.48 this method
		/// can remove all types of connections.
		/// </summary>
		public override void RemovePartialConnection (GraphNode node) {
			if (connections == null) return;

			for (int i = 0; i < connections.Length; i++) {
				if (connections[i].node == node) {
					int connLength = connections.Length;

					var newconns = new Connection[connLength-1];
					for (int j = 0; j < i; j++) {
						newconns[j] = connections[j];
					}
					for (int j = i+1; j < connLength; j++) {
						newconns[j-1] = connections[j];
					}

					connections = newconns;
					AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
					return;
				}
			}
		}

		public override void SerializeReferences (GraphSerializationContext ctx) {
			ctx.SerializeConnections(connections, true);
		}

		public override void DeserializeReferences (GraphSerializationContext ctx) {
			// Grid nodes didn't serialize references before 3.8.3
			if (ctx.meta.version < AstarSerializer.V3_8_3)
				return;

			connections = ctx.DeserializeConnections(ctx.meta.version >= AstarSerializer.V4_3_85);
		}
#endif
	}
}
