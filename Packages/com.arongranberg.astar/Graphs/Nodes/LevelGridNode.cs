#if !ASTAR_NO_GRID_GRAPH
#if !ASTAR_LEVELGRIDNODE_MORE_LAYERS
#define ASTAR_LEVELGRIDNODE_FEW_LAYERS
#endif
using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Serialization;

namespace Pathfinding {
	/// <summary>
	/// Describes a single node for the LayerGridGraph.
	/// Works almost the same as a grid node, except that it also stores to which layer the connections go to
	/// </summary>
	public class LevelGridNode : GridNodeBase {
		public LevelGridNode() {
		}

		public LevelGridNode (AstarPath astar) {
			astar.InitializeNode(this);
		}

		private static LayerGridGraph[] _gridGraphs = new LayerGridGraph[0];
		public static LayerGridGraph GetGridGraph (uint graphIndex) { return _gridGraphs[(int)graphIndex]; }

		public static void SetGridGraph (int graphIndex, LayerGridGraph graph) {
			// LayeredGridGraphs also show up in the grid graph list
			// This is required by e.g the XCoordinateInGrid properties
			GridNode.SetGridGraph(graphIndex, graph);
			if (_gridGraphs.Length <= graphIndex) {
				var newGraphs = new LayerGridGraph[graphIndex+1];
				for (int i = 0; i < _gridGraphs.Length; i++) newGraphs[i] = _gridGraphs[i];
				_gridGraphs = newGraphs;
			}

			_gridGraphs[graphIndex] = graph;
		}

		public static void ClearGridGraph (int graphIndex, LayerGridGraph graph) {
			if (graphIndex < _gridGraphs.Length && _gridGraphs[graphIndex] == graph) {
				_gridGraphs[graphIndex] = null;
			}
		}

#if ASTAR_LEVELGRIDNODE_FEW_LAYERS
		public uint gridConnections;
#else
		public ulong gridConnections;
#endif

		protected static LayerGridGraph[] gridGraphs;

		const int MaxNeighbours = 8;
#if ASTAR_LEVELGRIDNODE_FEW_LAYERS
		public const int ConnectionMask = 0xF;
		public const int ConnectionStride = 4;
		public const int AxisAlignedConnectionsMask = 0xFFFF;
		public const uint AllConnectionsMask = 0xFFFFFFFF;
#else
		public const int ConnectionMask = 0xFF;
		public const int ConnectionStride = 8;
		public const ulong AxisAlignedConnectionsMask = 0xFFFFFFFF;
		public const ulong AllConnectionsMask = 0xFFFFFFFFFFFFFFFF;
#endif
		public const int NoConnection = ConnectionMask;

		internal const ulong DiagonalConnectionsMask = ((ulong)NoConnection << 4*ConnectionStride) | ((ulong)NoConnection << 5*ConnectionStride) | ((ulong)NoConnection << 6*ConnectionStride) | ((ulong)NoConnection << 7*ConnectionStride);

		/// <summary>
		/// Maximum number of layers the layered grid graph supports.
		///
		/// This can be changed in the A* Inspector -> Optimizations tab by enabling or disabling the ASTAR_LEVELGRIDNODE_MORE_LAYERS option.
		/// </summary>
		public const int MaxLayerCount = ConnectionMask;

		/// <summary>
		/// Removes all grid connections from this node.
		///
		/// Warning: Using this method can make the graph data inconsistent. It's recommended to use other ways to update the graph, instead.
		/// </summary>
		public override void ResetConnectionsInternal () {
#if ASTAR_LEVELGRIDNODE_FEW_LAYERS
			gridConnections = unchecked ((uint)-1);
#else
			gridConnections = unchecked ((ulong)-1);
#endif
			AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
		}

#if ASTAR_LEVELGRIDNODE_FEW_LAYERS
		public override bool HasAnyGridConnections => gridConnections != unchecked ((uint)-1);
#else
		public override bool HasAnyGridConnections => gridConnections != unchecked ((ulong)-1);
#endif

		public override bool HasConnectionsToAllEightNeighbours {
			get {
				for (int i = 0; i < 8; i++) {
					if (!HasConnectionInDirection(i)) return false;
				}
				return true;
			}
		}

		public override bool HasConnectionsToAllAxisAlignedNeighbours {
			get {
				return (gridConnections & AxisAlignedConnectionsMask) == AxisAlignedConnectionsMask;
			}
		}

		/// <summary>
		/// Layer coordinate of the node in the grid.
		/// If there are multiple nodes in the same (x,z) cell, then they will be stored in different layers.
		/// Together with NodeInGridIndex, you can look up the node in the nodes array
		/// <code>
		/// int index = node.NodeInGridIndex + node.LayerCoordinateInGrid * graph.width * graph.depth;
		/// Assert(node == graph.nodes[index]);
		/// </code>
		///
		/// See: XCoordInGrid
		/// See: ZCoordInGrid
		/// See: NodeInGridIndex
		/// </summary>
		public int LayerCoordinateInGrid { get { return nodeInGridIndex >> NodeInGridIndexLayerOffset; } set { nodeInGridIndex = (nodeInGridIndex & NodeInGridIndexMask) | (value << NodeInGridIndexLayerOffset); } }

		public void SetPosition (Int3 position) {
			this.position = position;
		}

		public override int GetGizmoHashCode () {
			return base.GetGizmoHashCode() ^ (int)((805306457UL * gridConnections) ^ (402653189UL * (gridConnections >> 32)));
		}

		public override GridNodeBase GetNeighbourAlongDirection (int direction) {
			int conn = GetConnectionValue(direction);

			if (conn != NoConnection) {
				LayerGridGraph graph = GetGridGraph(GraphIndex);
				return graph.nodes[NodeInGridIndex+graph.neighbourOffsets[direction] + graph.lastScannedWidth*graph.lastScannedDepth*conn];
			}
			return null;
		}

		public override void ClearConnections (bool alsoReverse) {
			if (alsoReverse) {
				LayerGridGraph graph = GetGridGraph(GraphIndex);
				int[] neighbourOffsets = graph.neighbourOffsets;
				var nodes = graph.nodes;

				for (int i = 0; i < MaxNeighbours; i++) {
					int conn = GetConnectionValue(i);
					if (conn != LevelGridNode.NoConnection) {
						var other = nodes[NodeInGridIndex+neighbourOffsets[i] + graph.lastScannedWidth*graph.lastScannedDepth*conn] as LevelGridNode;
						if (other != null) {
							// Remove reverse connection
							other.SetConnectionValue((i + 2) % 4, NoConnection);
						}
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

			LayerGridGraph graph = GetGridGraph(GraphIndex);

			int[] neighbourOffsets = graph.neighbourOffsets;
			var nodes = graph.nodes;
			int index = NodeInGridIndex;

			for (int i = 0; i < MaxNeighbours; i++) {
				int conn = GetConnectionValue(i);
				if (conn != LevelGridNode.NoConnection) {
					var other = nodes[index+neighbourOffsets[i] + graph.lastScannedWidth*graph.lastScannedDepth*conn];
					if (other != null) action(other, ref data);
				}
			}

#if !ASTAR_GRID_NO_CUSTOM_CONNECTIONS
			base.GetConnections(action, ref data, connectionFilter);
#endif
		}

		public override bool HasConnectionInDirection (int direction) {
			return ((gridConnections >> direction*ConnectionStride) & ConnectionMask) != NoConnection;
		}

		/// <summary>
		/// Set which layer a grid connection goes to.
		///
		/// Warning: Using this method can make the graph data inconsistent. It's recommended to use other ways to update the graph, instead.
		/// </summary>
		/// <param name="dir">Direction for the connection.</param>
		/// <param name="value">The layer of the connected node or #NoConnection if there should be no connection in that direction.</param>
		public void SetConnectionValue (int dir, int value) {
#if ASTAR_LEVELGRIDNODE_FEW_LAYERS
			gridConnections = gridConnections & ~(((uint)NoConnection << dir*ConnectionStride)) | ((uint)value << dir*ConnectionStride);
#else
			gridConnections = gridConnections & ~(((ulong)NoConnection << dir*ConnectionStride)) | ((ulong)value << dir*ConnectionStride);
#endif
			AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
		}

#if ASTAR_LEVELGRIDNODE_FEW_LAYERS
		public void SetAllConnectionInternal (ulong value) {
			gridConnections = (uint)value;
		}
#else
		public void SetAllConnectionInternal (ulong value) {
			gridConnections = value;
		}
#endif


		/// <summary>
		/// Which layer a grid connection goes to.
		/// Returns: The layer of the connected node or <see cref="NoConnection"/> if there is no connection in that direction.
		/// </summary>
		/// <param name="dir">Direction for the connection.</param>
		public int GetConnectionValue (int dir) {
			return (int)((gridConnections >> dir*ConnectionStride) & ConnectionMask);
		}

		public override void AddPartialConnection (GraphNode node, uint cost, bool isOutgoing, bool isIncoming) {
			// In case the node was already added as an internal grid connection,
			// we need to remove that connection before we insert it as a custom connection.
			// Using a custom connection is necessary because it has a custom cost.
			if (node is LevelGridNode gn && gn.GraphIndex == GraphIndex) {
				RemoveGridConnection(gn);
			}
			base.AddPartialConnection(node, cost, isOutgoing, isIncoming);
		}

		public override void RemovePartialConnection (GraphNode node) {
			base.RemovePartialConnection(node);
			// If the node is a grid node on the same graph, it might be added as an internal connection and not a custom one.
			if (node is LevelGridNode gn && gn.GraphIndex == GraphIndex) {
				RemoveGridConnection(gn);
			}
		}

		/// <summary>
		/// Removes a connection from the internal grid connections, not the list of custom connections.
		/// See: SetConnectionValue
		/// </summary>
		protected void RemoveGridConnection (LevelGridNode node) {
			var nodeIndex = NodeInGridIndex;
			var gg = GetGridGraph(GraphIndex);

			for (int i = 0; i < 8; i++) {
				if (nodeIndex + gg.neighbourOffsets[i] == node.NodeInGridIndex && GetNeighbourAlongDirection(i) == node) {
					SetConnectionValue(i, NoConnection);
					break;
				}
			}
		}

		public override bool GetPortal (GraphNode other, out Vector3 left, out Vector3 right) {
			if (other.GraphIndex != GraphIndex) {
				left = right = Vector3.zero;
				return false;
			}

			LayerGridGraph gg = GetGridGraph(GraphIndex);
			var cellOffset = (other as GridNodeBase).CoordinatesInGrid - CoordinatesInGrid;
			var dir = OffsetToConnectionDirection(cellOffset.x, cellOffset.y);
			if (dir == -1 || GetNeighbourAlongDirection(dir) != other) {
				left = right = Vector3.zero;
				return false;
			}

			if (dir < 4) {
				Vector3 middle = ((Vector3)(position + other.position))*0.5f;
				var cross = gg.transform.TransformVector(new Vector3(cellOffset.y, 0, -cellOffset.x)*0.5f);
				left = middle - cross;
				right = middle + cross;
			} else {
				bool rClear = false;
				bool lClear = false;
				var n2 = GetNeighbourAlongDirection(dir-4);
				if (n2 != null && n2.Walkable && n2.GetNeighbourAlongDirection((dir-4+1)%4) == other) {
					rClear = true;
				}

				n2 = GetNeighbourAlongDirection((dir-4+1)%4);
				if (n2 != null && n2.Walkable && n2.GetNeighbourAlongDirection(dir-4) == other) {
					lClear = true;
				}

				Vector3 middle = ((Vector3)(position + other.position))*0.5f;
				var cross = gg.transform.TransformVector(new Vector3(cellOffset.y, 0, -cellOffset.x));
				left = middle - (lClear ? cross : Vector3.zero);
				right = middle + (rClear ? cross : Vector3.zero);
			}
			return true;
		}

		public override void Open (Path path, uint pathNodeIndex, uint gScore) {
			LayerGridGraph graph = GetGridGraph(GraphIndex);

			int[] neighbourOffsets = graph.neighbourOffsets;
			uint[] neighbourCosts = graph.neighbourCosts;
			var nodes = graph.nodes;
			int index = NodeInGridIndex;

			// Bitmask of the 8 connections out of this node.
			// Each bit represents one connection.
			// We only use this to be able to dynamically handle
			// things like cutCorners and other diagonal connection filtering
			// based on things like the tags or ITraversalProvider set for just this path.
			// It starts off with all connections enabled but then in the following loop
			// we will remove connections which are not traversable.
			// When we get to the first diagonal connection we run a pass to
			// filter out any diagonal connections which shouldn't be enabled.
			// See the documentation for FilterDiagonalConnections for more info.
			// The regular grid graph does a similar thing.
			var conns = 0xFF;

			for (int dir = 0; dir < MaxNeighbours; dir++) {
				if (dir == 4 && (path.traversalProvider == null || path.traversalProvider.filterDiagonalGridConnections)) {
					conns = GridNode.FilterDiagonalConnections(conns, graph.neighbours, graph.cutCorners);
				}

				int conn = GetConnectionValue(dir);
				if (conn != LevelGridNode.NoConnection && ((conns >> dir) & 0x1) != 0) {
					GraphNode other = nodes[index+neighbourOffsets[dir] + graph.lastScannedWidth*graph.lastScannedDepth*conn];

					if (!path.CanTraverse(this, other)) {
						conns &= ~(1 << dir);
						continue;
					}

					path.OpenCandidateConnection(pathNodeIndex, other.NodeIndex, gScore, neighbourCosts[dir], 0, other.position);
				} else {
					conns &= ~(1 << dir);
				}
			}

			base.Open(path, pathNodeIndex, gScore);
		}

		public override void SerializeNode (GraphSerializationContext ctx) {
			base.SerializeNode(ctx);
			ctx.SerializeInt3(position);
			ctx.writer.Write(gridFlags);
			// gridConnections are now always serialized as 64 bits for easier compatibility handling
#if ASTAR_LEVELGRIDNODE_FEW_LAYERS
			// Convert from 32 bits to 64-bits
			ulong connectionsLong = 0;
			for (int i = 0; i < 8; i++) connectionsLong |= (ulong)GetConnectionValue(i) << (i*8);
#else
			ulong connectionsLong = gridConnections;
#endif
			ctx.writer.Write(connectionsLong);
		}

		public override void DeserializeNode (GraphSerializationContext ctx) {
			base.DeserializeNode(ctx);
			position = ctx.DeserializeInt3();
			gridFlags = ctx.reader.ReadUInt16();
			if (ctx.meta.version < AstarSerializer.V4_3_12) {
				// Note: assumes ASTAR_LEVELGRIDNODE_FEW_LAYERS was false when saving, which was the default
				// This info not saved with the graph unfortunately and in 4.3.12 the default changed.
				ulong conns;
				if (ctx.meta.version < AstarSerializer.V3_9_0) {
					// Set the upper 32 bits for compatibility
					conns = ctx.reader.ReadUInt32() | (((ulong)NoConnection << 56) | ((ulong)NoConnection << 48) | ((ulong)NoConnection << 40) | ((ulong)NoConnection << 32));
				} else {
					conns = ctx.reader.ReadUInt64();
				}
				const int stride = 8;
				const int mask = (1 << stride) - 1;
				gridConnections = 0;
				for (int i = 0; i < 8; i++) {
					var y = (conns >> (i*stride)) & mask;
					// 4.3.12 by default only supports 15 layers. So we may have to disable some connections when loading from earlier versions.
					if ((y & ConnectionMask) != y) y = NoConnection;
					SetConnectionValue(i, (int)y);
				}
			} else {
				var gridConnectionsLong = ctx.reader.ReadUInt64();
#if ASTAR_LEVELGRIDNODE_FEW_LAYERS
				uint c = 0;
				if (ctx.meta.version < AstarSerializer.V4_3_83) {
					// The default during 4.3.12..4.3.83 was that ASTAR_LEVELGRIDNODE_FEW_LAYERS was enabled, but it was serialized just as 32-bits zero-extended to 64 bits
					c = (uint)gridConnectionsLong;
				} else {
					// Convert from 64 bits to 32-bits
					for (int i = 0; i < 8; i++) {
						c |= ((uint)(gridConnectionsLong >> (i*8)) & LevelGridNode.ConnectionMask) << (LevelGridNode.ConnectionStride*i);
					}
				}
				gridConnections = c;
#else
				gridConnections = gridConnectionsLong;
#endif
			}
		}
	}
}
#endif
