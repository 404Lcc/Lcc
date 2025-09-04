using UnityEngine;
using Pathfinding.Serialization;

namespace Pathfinding {
	/// <summary>
	/// Node used for the PointGraph.
	/// This is just a simple point with a list of connections (and associated costs) to other nodes.
	/// It does not have any concept of a surface like many other node types.
	///
	/// See: PointGraph
	/// </summary>
	public class PointNode : GraphNode {
		/// <summary>
		/// All connections from this node.
		/// See: <see cref="Connect"/>
		/// See: <see cref="Disconnect"/>
		/// See: <see cref="GetConnections"/>
		///
		/// Note: If you modify this array or the contents of it you must call <see cref="SetConnectivityDirty"/>.
		///
		/// Note: If you modify this array or the contents of it you must call <see cref="PointGraph.RegisterConnectionLength"/> with the length of the new connections.
		///
		/// This may be null if the node has no connections to other nodes.
		/// </summary>
		public Connection[] connections;

		/// <summary>
		/// GameObject this node was created from (if any).
		/// Warning: When loading a graph from a saved file or from cache, this field will be null.
		///
		/// <code>
		/// var node = AstarPath.active.GetNearest(transform.position).node;
		/// var pointNode = node as PointNode;
		///
		/// if (pointNode != null) {
		///     Debug.Log("That node was created from the GameObject named " + pointNode.gameObject.name);
		/// } else {
		///     Debug.Log("That node is not a PointNode");
		/// }
		/// </code>
		/// </summary>
		public GameObject gameObject;

		[System.Obsolete("Set node.position instead")]
		public void SetPosition (Int3 value) {
			position = value;
		}

		public PointNode() { }
		public PointNode (AstarPath astar) {
			astar.InitializeNode(this);
		}

		/// <summary>
		/// Closest point on the surface of this node to the point p.
		///
		/// For a point node this is always the node's <see cref="position"/> sicne it has no surface.
		/// </summary>
		public override Vector3 ClosestPointOnNode (Vector3 p) {
			return (Vector3)this.position;
		}

		/// <summary>
		/// Checks if point is inside the node when seen from above.
		///
		/// Since point nodes have no surface area, this method always returns false.
		/// </summary>
		public override bool ContainsPoint (Vector3 point) {
			return false;
		}

		/// <summary>
		/// Checks if point is inside the node in graph space.
		///
		/// Since point nodes have no surface area, this method always returns false.
		/// </summary>
		public override bool ContainsPointInGraphSpace (Int3 point) {
			return false;
		}

		public override void GetConnections<T>(GetConnectionsWithData<T> action, ref T data, int connectionFilter) {
			if (connections == null) return;
			for (int i = 0; i < connections.Length; i++) if ((connections[i].shapeEdgeInfo & connectionFilter) != 0) action(connections[i].node, ref data);
		}

		public override void ClearConnections (bool alsoReverse) {
			if (alsoReverse && connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					connections[i].node.RemovePartialConnection(this);
				}
			}

			connections = null;
			AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
		}

		public override bool ContainsOutgoingConnection (GraphNode node) {
			if (connections == null) return false;
			for (int i = 0; i < connections.Length; i++) if (connections[i].node == node && connections[i].isOutgoing) return true;
			return false;
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

			// Make sure the graph knows that there exists a connection with this length
			if (this.Graph is PointGraph pg) pg.RegisterConnectionLength((node.position - position).sqrMagnitudeLong);
		}

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

		public override void Open (Path path, uint pathNodeIndex, uint gScore) {
			path.OpenCandidateConnectionsToEndNode(position, pathNodeIndex, pathNodeIndex, gScore);

			if (connections == null) return;

			for (int i = 0; i < connections.Length; i++) {
				GraphNode other = connections[i].node;

				if (connections[i].isOutgoing && path.CanTraverse(this, other)) {
					if (other is PointNode) {
						path.OpenCandidateConnection(pathNodeIndex, other.NodeIndex, gScore, connections[i].cost, 0, other.position);
					} else {
						// When connecting to a non-point node, use a special function to open the connection.
						// The typical case for this is that we are at the end of an off-mesh link and we are connecting to a navmesh node.
						// In that case, this node's position is in the interior of the navmesh node. We let the navmesh node decide how
						// that should be handled.
						other.OpenAtPoint(path, pathNodeIndex, position, gScore);
					}
				}
			}
		}

		public override void OpenAtPoint (Path path, uint pathNodeIndex, Int3 pos, uint gScore) {
			if (path.CanTraverse(this)) {
				// TODO: Ideally we should only allow connections to the temporary end node directly from the temporary start node
				// iff they lie on the same connection edge. Otherwise we need to pass through the center of this node.
				//
				//   N1---E----N2
				//   |   /
				//   | /
				//   S
				//   |
				//   N3
				//
				path.OpenCandidateConnectionsToEndNode(pos, pathNodeIndex, pathNodeIndex, gScore);

				var cost = (uint)(pos - this.position).costMagnitude;
				path.OpenCandidateConnection(pathNodeIndex, NodeIndex, gScore, cost, 0, position);
			}
		}

		public override int GetGizmoHashCode () {
			var hash = base.GetGizmoHashCode();

			if (connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					hash ^= 17 * connections[i].GetHashCode();
				}
			}
			return hash;
		}

		public override void SerializeNode (GraphSerializationContext ctx) {
			base.SerializeNode(ctx);
			ctx.SerializeInt3(position);
		}

		public override void DeserializeNode (GraphSerializationContext ctx) {
			base.DeserializeNode(ctx);
			position = ctx.DeserializeInt3();
		}

		public override void SerializeReferences (GraphSerializationContext ctx) {
			ctx.SerializeConnections(connections, true);
		}

		public override void DeserializeReferences (GraphSerializationContext ctx) {
			connections = ctx.DeserializeConnections(ctx.meta.version >= AstarSerializer.V4_3_85);
		}
	}
}
