#define PREALLOCATE_NODES
using System.Collections.Generic;
using Pathfinding.Serialization;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding {
	/// <summary>Node used for the GridGraph</summary>
	public class GridNode : GridNodeBase {
		public GridNode() {
		}
		public GridNode (AstarPath astar) {
			astar.InitializeNode(this);
		}

#if !ASTAR_NO_GRID_GRAPH
		private static GridGraph[] _gridGraphs = new GridGraph[0];
		public static GridGraph GetGridGraph (uint graphIndex) { return _gridGraphs[(int)graphIndex]; }

		public static void SetGridGraph (int graphIndex, GridGraph graph) {
			if (_gridGraphs.Length <= graphIndex) {
				var gg = new GridGraph[graphIndex+1];
				for (int i = 0; i < _gridGraphs.Length; i++) gg[i] = _gridGraphs[i];
				_gridGraphs = gg;
			}

			_gridGraphs[graphIndex] = graph;
		}

		public static void ClearGridGraph (int graphIndex, GridGraph graph) {
			if (graphIndex < _gridGraphs.Length && _gridGraphs[graphIndex] == graph) {
				_gridGraphs[graphIndex] = null;
			}
		}

		/// <summary>Internal use only</summary>
		internal ushort InternalGridFlags {
			get { return gridFlags; }
			set { gridFlags = value; }
		}

		const int GridFlagsConnectionOffset = 0;
		const int GridFlagsConnectionBit0 = 1 << GridFlagsConnectionOffset;
		const int GridFlagsConnectionMask = 0xFF << GridFlagsConnectionOffset;
		const int GridFlagsAxisAlignedConnectionMask = 0xF << GridFlagsConnectionOffset;

		const int GridFlagsEdgeNodeOffset = 10;
		const int GridFlagsEdgeNodeMask = 1 << GridFlagsEdgeNodeOffset;

		public override bool HasConnectionsToAllEightNeighbours {
			get {
				return (InternalGridFlags & GridFlagsConnectionMask) == GridFlagsConnectionMask;
			}
		}

		public override bool HasConnectionsToAllAxisAlignedNeighbours {
			get {
				return (InternalGridFlags & GridFlagsAxisAlignedConnectionMask) == GridFlagsAxisAlignedConnectionMask;
			}
		}

		/// <summary>
		/// True if the node has a connection in the specified direction.
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
		/// See: <see cref="SetConnectionInternal"/>
		/// See: <see cref="GridGraph.neighbourXOffsets"/>
		/// See: <see cref="GridGraph.neighbourZOffsets"/>
		/// See: <see cref="GridGraph.neighbourOffsets"/>
		/// See: <see cref="GridGraph.GetNeighbourDirections"/>
		/// </summary>
		public override bool HasConnectionInDirection (int dir) {
			return (gridFlags >> dir & GridFlagsConnectionBit0) != 0;
		}

		/// <summary>
		/// Enables or disables a connection in a specified direction on the graph.
		///
		/// Note: This only changes the connection from this node to the other node. You may also want to call the same method on the other node with the opposite direction.
		///
		/// See: <see cref="HasConnectionInDirection"/>
		/// See: <see cref="OppositeConnectionDirection"/>
		/// </summary>
		public void SetConnection (int dir, bool value) {
			SetConnectionInternal(dir, value);
			var grid = GetGridGraph(GraphIndex);
			grid.nodeDataRef.connections[NodeInGridIndex] = (ulong)GetAllConnectionInternal();
		}

		/// <summary>
		/// Enables or disables a connection in a specified direction on the graph.
		/// See: <see cref="HasConnectionInDirection"/>
		///
		/// Warning: Using this method can make the graph data inconsistent. It's recommended to use other ways to update the graph, instead, for example <see cref="SetConnection"/>.
		/// </summary>
		public void SetConnectionInternal (int dir, bool value) {
			// Set bit number #dir to 1 or 0 depending on #value
			unchecked { gridFlags = (ushort)(gridFlags & ~((ushort)1 << GridFlagsConnectionOffset << dir) | (value ? (ushort)1 : (ushort)0) << GridFlagsConnectionOffset << dir); }
			AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
		}

		/// <summary>
		/// Sets the state of all grid connections.
		///
		/// See: SetConnectionInternal
		///
		/// Warning: Using this method can make the graph data inconsistent. It's recommended to use other ways to update the graph, instead.
		/// </summary>
		/// <param name="connections">a bitmask of the connections (bit 0 is the first connection, bit 1 the second connection, etc.).</param>
		public void SetAllConnectionInternal (int connections) {
			unchecked { gridFlags = (ushort)((gridFlags & ~GridFlagsConnectionMask) | (connections << GridFlagsConnectionOffset)); }
			AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
		}

		/// <summary>Bitpacked int containing all 8 grid connections</summary>
		public int GetAllConnectionInternal () {
			return (gridFlags & GridFlagsConnectionMask) >> GridFlagsConnectionOffset;
		}

		public override bool HasAnyGridConnections => GetAllConnectionInternal() != 0;

		/// <summary>
		/// Disables all grid connections from this node.
		/// Note: Other nodes might still be able to get to this node.
		/// Therefore it is recommended to also disable the relevant connections on adjacent nodes.
		///
		/// Warning: Using this method can make the graph data inconsistent. It's recommended to use other ways to update the graph, instead.
		/// </summary>
		public override void ResetConnectionsInternal () {
			unchecked {
				gridFlags = (ushort)(gridFlags & ~GridFlagsConnectionMask);
			}
			AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
		}

		/// <summary>
		/// Work in progress for a feature that required info about which nodes were at the border of the graph.
		/// Note: This property is not functional at the moment.
		/// </summary>
		public bool EdgeNode {
			get {
				return (gridFlags & GridFlagsEdgeNodeMask) != 0;
			}
			set {
				unchecked { gridFlags = (ushort)(gridFlags & ~GridFlagsEdgeNodeMask | (value ? GridFlagsEdgeNodeMask : 0)); }
			}
		}

		public override GridNodeBase GetNeighbourAlongDirection (int direction) {
			if (HasConnectionInDirection(direction)) {
				GridGraph gg = GetGridGraph(GraphIndex);
				return gg.nodes[NodeInGridIndex+gg.neighbourOffsets[direction]];
			}
			return null;
		}

		public override void ClearConnections (bool alsoReverse) {
			if (alsoReverse) {
				// Note: This assumes that all connections are bidirectional
				// which should hold for all grid graphs unless some custom code has been added
				for (int i = 0; i < 8; i++) {
					var other = GetNeighbourAlongDirection(i) as GridNode;
					if (other != null) {
						// Remove reverse connection. See doc for GridGraph.neighbourOffsets to see which indices are used for what.
						other.SetConnectionInternal(OppositeConnectionDirection(i), false);
					}
				}
			}

			ResetConnectionsInternal();

#if !ASTAR_GRID_NO_CUSTOM_CONNECTIONS
			base.ClearConnections(alsoReverse);
#endif
		}

		public override void GetConnections<T>(GetConnectionsWithData<T> action, ref T data, int connectionFilter) {
			if ((connectionFilter & (Connection.IncomingConnection | Connection.OutgoingConnection)) == 0) return;

			GridGraph gg = GetGridGraph(GraphIndex);

			var neighbourOffsets = gg.neighbourOffsets;
			var nodes = gg.nodes;

			for (int i = 0; i < 8; i++) {
				if ((gridFlags >> i & GridFlagsConnectionBit0) != 0) {
					var other = nodes[NodeInGridIndex + neighbourOffsets[i]];
					if (other != null) action(other, ref data);
				}
			}

#if !ASTAR_GRID_NO_CUSTOM_CONNECTIONS
			base.GetConnections(action, ref data, connectionFilter);
#endif
		}

		public override bool GetPortal (GraphNode other, out Vector3 left, out Vector3 right) {
			if (other.GraphIndex != GraphIndex) {
				left = right = Vector3.zero;
				return false;
			}

			GridGraph gg = GetGridGraph(GraphIndex);
			var cellOffset = (other as GridNodeBase).CoordinatesInGrid - CoordinatesInGrid;
			var dir = OffsetToConnectionDirection(cellOffset.x, cellOffset.y);
			if (dir == -1 || !HasConnectionInDirection(dir)) {
				left = right = Vector3.zero;
				return false;
			}

			UnityEngine.Assertions.Assert.AreEqual(other, gg.nodes[NodeInGridIndex + gg.neighbourOffsets[dir]]);

			if (dir < 4) {
				Vector3 middle = ((Vector3)(position + other.position))*0.5f;
				var cross = gg.transform.TransformVector(new Vector3(cellOffset.y, 0, -cellOffset.x)*0.5f);
				left = middle - cross;
				right = middle + cross;
			} else {
				bool rClear = false;
				bool lClear = false;
				if (HasConnectionInDirection(dir-4)) {
					var n2 = gg.nodes[NodeInGridIndex + gg.neighbourOffsets[dir-4]];
					if (n2.Walkable && n2.HasConnectionInDirection((dir-4+1)%4)) {
						rClear = true;
					}
				}

				if (HasConnectionInDirection((dir-4+1)%4)) {
					var n2 = gg.nodes[NodeInGridIndex + gg.neighbourOffsets[(dir-4+1)%4]];
					if (n2.Walkable && n2.HasConnectionInDirection(dir-4)) {
						lClear = true;
					}
				}

				Vector3 middle = ((Vector3)(position + other.position))*0.5f;
				var cross = gg.transform.TransformVector(new Vector3(cellOffset.y, 0, -cellOffset.x));
				left = middle - (lClear ? cross : Vector3.zero);
				right = middle + (rClear ? cross : Vector3.zero);
			}
			return true;
		}

		/// <summary>
		/// Filters diagonal connections based on the non-diagonal ones to prevent corner cutting and similar things.
		///
		/// This involves some complicated bitshifting to calculate which diagonal connections
		/// should be active based on the non-diagonal ones.
		/// For example a path should not be able to pass from A to B if the \<see cref="s"/> represent nodes
		/// that we cannot traverse.
		///
		/// <code>
		///    # B
		///    A #
		/// </code>
		///
		/// Additionally if corner cutting is disabled we will also prevent a connection from A to B in this case:
		///
		/// <code>
		///      B
		///    A #
		/// </code>
		///
		/// If neighbours = 4 then only the 4 axis aligned connections will be enabled.
		///
		/// If neighbours = 6 then only the connections which are valid for hexagonal graphs will be enabled.
		/// </summary>
		public static int FilterDiagonalConnections (int conns, NumNeighbours neighbours, bool cutCorners) {
			switch (neighbours) {
			case NumNeighbours.Four:
				// The first 4 bits are the axis aligned connections
				return conns & 0xF;
			// Default case exists only to make the compiler happy, it is never intended to be used.
			default:
			case NumNeighbours.Eight:
				if (cutCorners) {
					int axisConns = conns & 0xF;
					// If at least one axis aligned connection
					// is adjacent to this diagonal, then we can add a connection.
					// Bitshifting is a lot faster than calling node.HasConnectionInDirection.
					// We need to check if connection i and i+1 are enabled
					// but i+1 may overflow 4 and in that case need to be wrapped around
					// (so 3+1 = 4 goes to 0). We do that by checking both connection i+1
					// and i+1-4 at the same time. Either i+1 or i+1-4 will be in the range
					// from 0 to 4 (exclusive)
					int diagConns = (axisConns | (axisConns >> 1 | axisConns << 3)) << 4;

					// Filter out diagonal connections that are invalid
					// This will also filter out some junk bits which may be set to true above bit 8
					diagConns &= conns;
					return axisConns | diagConns;
				} else {
					int axisConns = conns & 0xF;
					// If exactly 2 axis aligned connections are adjacent to a diagonal connection
					// then the diagonal connection is ok to use.
					int diagConns = (axisConns & (axisConns >> 1 | axisConns << 3)) << 4;

					// Filter out diagonal connections that are invalid
					// This will also filter out some junk bits which may be set above bit 8
					diagConns &= conns;
					return axisConns | diagConns;
				}
			case NumNeighbours.Six:
				// Hexagon layout
				return conns & GridGraph.HexagonConnectionMask;
			}
		}

		public override void Open (Path path, uint pathNodeIndex, uint gScore) {
			GridGraph gg = GetGridGraph(GraphIndex);

			int[] neighbourOffsets = gg.neighbourOffsets;
			uint[] neighbourCosts = gg.neighbourCosts;
			var nodes = gg.nodes;
			var index = NodeInGridIndex;

			// Bitmask of the 8 connections out of this node
			// Each bit represents one connection.
			var conns = gridFlags & GridFlagsConnectionMask;

			// Loop over all connections, first the 4 axis aligned ones and then the 4 diagonal ones.
			for (int dir = 0; dir < 8; dir++) {
				// dir=4 is the first diagonal connection.
				// At this point we know exactly which orthogonal (not diagonal) connections are actually traversable.
				// So we do some filtering to determine which diagonals should be traversable.
				//
				// We do this dynamically because each path may use different tags or different
				// ITraversalProviders that affect the result.
				//
				// When the grid graph is scanned this exact method is also run to pre-filter connections
				// based on their walkability values.
				// Doing pre-filtering is good because it allows users to use `HasConnectionInDirection`
				// and it will return accurate values even for diagonals (even though it will of course not
				// take into account any additional constraints such as tags or ITraversalProviders).
				if (dir == 4 && (path.traversalProvider == null || path.traversalProvider.filterDiagonalGridConnections)) {
					conns = FilterDiagonalConnections(conns, gg.neighbours, gg.cutCorners);
				}

				// Check if we have a connection in this direction
				if (((conns >> dir) & GridFlagsConnectionBit0) != 0) {
					var other = nodes[index + neighbourOffsets[dir]];
					if (path.CanTraverse(this, other)) {
						path.OpenCandidateConnection(pathNodeIndex, other.NodeIndex, gScore, neighbourCosts[dir], 0, other.position);
					} else {
						// Mark that connection as not valid
						conns &= ~(GridFlagsConnectionBit0 << dir);
					}
				}
			}

			base.Open(path, pathNodeIndex, gScore);
		}

		public override void SerializeNode (GraphSerializationContext ctx) {
			base.SerializeNode(ctx);
			ctx.SerializeInt3(position);
			ctx.writer.Write(gridFlags);
		}

		public override void DeserializeNode (GraphSerializationContext ctx) {
			base.DeserializeNode(ctx);
			position = ctx.DeserializeInt3();
			gridFlags = ctx.reader.ReadUInt16();
		}

		public override void AddPartialConnection (GraphNode node, uint cost, bool isOutgoing, bool isIncoming) {
			// In case the node was already added as an internal grid connection,
			// we need to remove that connection before we insert it as a custom connection.
			// Using a custom connection is necessary because it has a custom cost.
			if (node is GridNode gn && gn.GraphIndex == GraphIndex) {
				RemoveGridConnection(gn);
			}
			base.AddPartialConnection(node, cost, isOutgoing, isIncoming);
		}

		public override void RemovePartialConnection (GraphNode node) {
			base.RemovePartialConnection(node);
			// If the node is a grid node on the same graph, it might be added as an internal connection and not a custom one.
			if (node is GridNode gn && gn.GraphIndex == GraphIndex) {
				RemoveGridConnection(gn);
			}
		}

		/// <summary>
		/// Removes a connection from the internal grid connections.
		/// See: SetConnectionInternal
		/// </summary>
		protected void RemoveGridConnection (GridNode node) {
			var nodeIndex = NodeInGridIndex;
			var gg = GetGridGraph(GraphIndex);

			for (int i = 0; i < 8; i++) {
				if (nodeIndex + gg.neighbourOffsets[i] == node.NodeInGridIndex && GetNeighbourAlongDirection(i) == node) {
					SetConnectionInternal(i, false);
					break;
				}
			}
		}
#else
		public override void AddPartialConnection (GraphNode node, uint cost, bool isOutgoing, bool isIncoming) {
			throw new System.NotImplementedException();
		}

		public override void ClearConnections (bool alsoReverse) {
			throw new System.NotImplementedException();
		}

		public override void GetConnections (GraphNodeDelegate del) {
			throw new System.NotImplementedException();
		}

		public override void Open (Path path, PathNode pathNode, PathHandler handler) {
			throw new System.NotImplementedException();
		}

		public override void AddPartialConnection (GraphNode node) {
			throw new System.NotImplementedException();
		}
#endif
	}
}
