using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Serialization;
using Unity.Profiling;

namespace Pathfinding {
	using Pathfinding.Pooling;

	/// <summary>Represents a connection to another node</summary>
	public struct Connection {
		/// <summary>Node which this connection goes to</summary>
		public GraphNode node;

		/// <summary>
		/// Cost of moving along this connection.
		/// A cost of 1000 corresponds approximately to the cost of moving one world unit.
		/// </summary>
		public uint cost;

		/// <summary>
		/// Various metadata about the connection, such as the side of the node shape which this connection uses.
		///
		/// - Bits 0..1 represent <see cref="shapeEdge"/>.
		/// - Bits 2..3 represent <see cref="adjacentShapeEdge"/>.
		/// - Bit 4 represents <see cref="isIncoming"/>.
		/// - Bit 5 represents <see cref="isOutgoing"/>.
		/// - Bit 6 represents <see cref="edgesAreIdentical"/>.
		///
		/// Note: Due to alignment, the <see cref="node"/> and <see cref="cost"/> fields use 12 bytes which will be padded
		/// to 16 bytes when used in an array even if this field would be removed.
		/// So this field does not contribute to increased memory usage. We could even expand it to 32-bits if we need to in the future.
		/// </summary>
		public byte shapeEdgeInfo;

		/// <summary>
		/// The edge of the shape which this connection uses.
		/// This is an index into the shape's vertices.
		///
		/// A value of 0 corresponds to using the side for vertex 0 and vertex 1 on the node. 1 corresponds to vertex 1 and 2, etc.
		/// A value of 3 is invalid, and this will be the value if <see cref="isEdgeShared"/> is false.
		///
		/// See: <see cref="TriangleMeshNode"/>
		/// See: <see cref="MeshNode.AddPartialConnection"/>
		/// </summary>
		public int shapeEdge => shapeEdgeInfo & 0b11;

		/// <summary>
		/// The edge of the shape in the other node, which this connection represents.
		///
		/// See: <see cref="shapeEdge"/>
		/// </summary>
		public int adjacentShapeEdge => (shapeEdgeInfo >> 2) & 0b11;

		/// <summary>
		/// True if the two nodes share an identical edge.
		///
		/// This is only true if the connection is between two triangle mesh nodes and the nodes share the edge which this connection represents.
		///
		/// In contrast to <see cref="isEdgeShared"/>, this is true only if the triangle edge is identical (but reversed) in the other node.
		/// </summary>
		public bool edgesAreIdentical => (shapeEdgeInfo & IdenticalEdge) != 0;

		/// <summary>
		/// True if the two nodes share an edge.
		///
		/// This is only true if the connection is between two triangle mesh nodes and the nodes share the edge which this connection represents.
		/// Note that the edge does not need to be perfectly identical for this to be true, it is enough if the edge is very similar.
		/// </summary>
		public bool isEdgeShared => (shapeEdgeInfo & NoSharedEdge) != NoSharedEdge;

		/// <summary>
		/// True if the connection allows movement from this node to the other node.
		///
		/// A connection can be either outgoing, incoming, or both. Most connections are two-way, so both incoming and outgoing.
		/// </summary>
		public bool isOutgoing => (shapeEdgeInfo & OutgoingConnection) != 0;

		/// <summary>
		/// True if the connection allows movement from the other node to this node.
		///
		/// A connection can be either outgoing, incoming, or both. Most connections are two-way, so both incoming and outgoing.
		/// </summary>
		public bool isIncoming => (shapeEdgeInfo & IncomingConnection) != 0;


		public const byte NoSharedEdge = 0b1111;
		public const byte IncomingConnection = 1 << 4;
		public const byte OutgoingConnection = 1 << 5;
		public const byte IdenticalEdge = 1 << 6;

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public Connection (GraphNode node, uint cost, bool isOutgoing, bool isIncoming) {
			this.node = node;
			this.cost = cost;
			this.shapeEdgeInfo = PackShapeEdgeInfo(isOutgoing, isIncoming);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static byte PackShapeEdgeInfo(bool isOutgoing, bool isIncoming) => (byte)(NoSharedEdge | (isIncoming ? IncomingConnection : 0) | (isOutgoing ? OutgoingConnection : 0));

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static byte PackShapeEdgeInfo (byte shapeEdge, byte adjacentShapeEdge, bool areEdgesIdentical, bool isOutgoing, bool isIncoming) {
#if UNITY_EDITOR
			if (shapeEdge > 3) throw new System.ArgumentException("shapeEdge must be at most 3");
			if (adjacentShapeEdge > 3) throw new System.ArgumentException("adjacentShapeEdge must be at most 3");
#endif
			return (byte)(shapeEdge | (adjacentShapeEdge << 2) | (areEdgesIdentical ? IdenticalEdge : 0) | (isOutgoing ? OutgoingConnection : 0) | (isIncoming ? IncomingConnection : 0));
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public Connection (GraphNode node, uint cost, byte shapeEdgeInfo) {
			this.node = node;
			this.cost = cost;
			this.shapeEdgeInfo = shapeEdgeInfo;
		}

		public override int GetHashCode () {
			return node.GetHashCode() ^ (int)cost;
		}

		public override bool Equals (object obj) {
			if (!(obj is Connection)) return false;
			var conn = (Connection)obj;
			return conn.node == node && conn.cost == cost && conn.shapeEdgeInfo == shapeEdgeInfo;
		}
	}

	/// <summary>Base class for all nodes</summary>
	public abstract class GraphNode {
		/// <summary>Internal unique index. Also stores some bitpacked values such as <see cref="TemporaryFlag1"/> and <see cref="TemporaryFlag2"/>.</summary>
		private int nodeIndex;

		/// <summary>
		/// Bitpacked field holding several pieces of data.
		/// See: Walkable
		/// See: Area
		/// See: GraphIndex
		/// See: Tag
		/// </summary>
		protected uint flags;

#if !ASTAR_NO_PENALTY
		/// <summary>
		/// Penalty cost for walking on this node.
		/// This can be used to make it harder/slower to walk over certain nodes.
		///
		/// A penalty of 1000 (Int3.Precision) corresponds to the cost of walking one world unit.
		///
		/// See: graph-updates (view in online documentation for working links)
		/// </summary>
		private uint penalty;
#endif

		/// <summary>
		/// Graph which this node belongs to.
		///
		/// If you know the node belongs to a particular graph type, you can cast it to that type:
		/// <code>
		/// GraphNode node = ...;
		/// GridGraph graph = node.Graph as GridGraph;
		/// </code>
		///
		/// Will return null if the node has been destroyed.
		/// </summary>
		public NavGraph Graph => AstarData.GetGraph(this);

		/// <summary>
		/// Destroys the node.
		/// Cleans up any temporary pathfinding data used for this node.
		/// The graph is responsible for calling this method on nodes when they are destroyed, including when the whole graph is destoyed.
		/// Otherwise memory leaks might present themselves.
		///
		/// Once called the <see cref="Destroyed"/> property will return true and subsequent calls to this method will not do anything.
		///
		/// Note: Assumes the current active AstarPath instance is the same one that created this node.
		///
		/// Warning: Should only be called by graph classes on their own nodes
		/// </summary>
		public void Destroy () {
			if (Destroyed) return;

			ClearConnections(true);

			if (AstarPath.active != null) {
				AstarPath.active.DestroyNode(this);
			}
			NodeIndex = DestroyedNodeIndex;
		}

		public bool Destroyed {
			[IgnoredByDeepProfiler]
			get {
				return NodeIndex == DestroyedNodeIndex;
			}
		}

		// If anyone creates more than about 200 million nodes then things will not go so well, however at that point one will certainly have more pressing problems, such as having run out of RAM
		const uint NodeIndexMask = 0xFFFFFFF;
		public const uint DestroyedNodeIndex = NodeIndexMask - 1;
		public const int InvalidNodeIndex = 0;
		const int TemporaryFlag1Mask = 0x10000000;
		const int TemporaryFlag2Mask = 0x20000000;

		/// <summary>
		/// Internal unique index.
		/// Every node will get a unique index.
		/// This index is not necessarily correlated with e.g the position of the node in the graph.
		/// </summary>
		public uint NodeIndex {
			[IgnoredByDeepProfiler]
			get { return (uint)nodeIndex & NodeIndexMask; }
			[IgnoredByDeepProfiler]
			internal set { nodeIndex = (int)(((uint)nodeIndex & ~NodeIndexMask) | value); }
		}

		/// <summary>
		/// How many path node variants should be created for each node.
		///
		/// This should be a constant for each node type.
		///
		/// Typically this is 1, but for example the triangle mesh node type has 3 variants, one for each edge.
		///
		/// See: <see cref="Pathfinding.PathNode"/>
		/// </summary>
		internal virtual int PathNodeVariants => 1;

		/// <summary>
		/// Temporary flag for internal purposes.
		/// May only be used in the Unity thread. Must be reset to false after every use.
		/// </summary>
		internal bool TemporaryFlag1 { get { return (nodeIndex & TemporaryFlag1Mask) != 0; } set { nodeIndex = (nodeIndex & ~TemporaryFlag1Mask) | (value ? TemporaryFlag1Mask : 0); } }

		/// <summary>
		/// Temporary flag for internal purposes.
		/// May only be used in the Unity thread. Must be reset to false after every use.
		/// </summary>
		internal bool TemporaryFlag2 { get { return (nodeIndex & TemporaryFlag2Mask) != 0; } set { nodeIndex = (nodeIndex & ~TemporaryFlag2Mask) | (value ? TemporaryFlag2Mask : 0); } }

		/// <summary>
		/// Position of the node in world space.
		/// Note: The position is stored as an Int3, not a Vector3.
		/// You can convert an Int3 to a Vector3 using an explicit conversion.
		/// <code> var v3 = (Vector3)node.position; </code>
		/// </summary>
		public Int3 position;

		#region Constants
		/// <summary>Position of the walkable bit. See: <see cref="Walkable"/></summary>
		const int FlagsWalkableOffset = 0;
		/// <summary>Mask of the walkable bit. See: <see cref="Walkable"/></summary>
		const uint FlagsWalkableMask = 1 << FlagsWalkableOffset;

		/// <summary>Start of hierarchical node index bits. See: <see cref="HierarchicalNodeIndex"/></summary>
		const int FlagsHierarchicalIndexOffset = 1;
		/// <summary>Mask of hierarchical node index bits. See: <see cref="HierarchicalNodeIndex"/></summary>
		const uint HierarchicalIndexMask = (131072-1) << FlagsHierarchicalIndexOffset;

		/// <summary>Start of <see cref="IsHierarchicalNodeDirty"/> bits. See: <see cref="IsHierarchicalNodeDirty"/></summary>
		const int HierarchicalDirtyOffset = 18;

		/// <summary>Mask of the <see cref="IsHierarchicalNodeDirty"/> bit. See: <see cref="IsHierarchicalNodeDirty"/></summary>
		const uint HierarchicalDirtyMask = 1 << HierarchicalDirtyOffset;

		/// <summary>Start of graph index bits. See: <see cref="GraphIndex"/></summary>
		const int FlagsGraphOffset = 24;
		/// <summary>Mask of graph index bits. See: <see cref="GraphIndex"/></summary>
		const uint FlagsGraphMask = (256u-1) << FlagsGraphOffset;

		public const uint MaxHierarchicalNodeIndex = HierarchicalIndexMask >> FlagsHierarchicalIndexOffset;

		/// <summary>Max number of graphs-1</summary>
		public const uint MaxGraphIndex = (FlagsGraphMask-1) >> FlagsGraphOffset;
		public const uint InvalidGraphIndex = (FlagsGraphMask) >> FlagsGraphOffset;

		/// <summary>Start of tag bits. See: <see cref="Tag"/></summary>
		const int FlagsTagOffset = 19;
		/// <summary>Max number of tags - 1. Always a power of 2 minus one</summary>
		public const int MaxTagIndex = 32 - 1;
		/// <summary>Mask of tag bits. See: <see cref="Tag"/></summary>
		const uint FlagsTagMask = MaxTagIndex << FlagsTagOffset;

		#endregion

		#region Properties

		/// <summary>
		/// Holds various bitpacked variables.
		///
		/// Bit 0: <see cref="Walkable"/>
		/// Bits 1 through 17: <see cref="HierarchicalNodeIndex"/>
		/// Bit 18: <see cref="IsHierarchicalNodeDirty"/>
		/// Bits 19 through 23: <see cref="Tag"/>
		/// Bits 24 through 31: <see cref="GraphIndex"/>
		///
		/// Warning: You should pretty much never modify this property directly. Use the other properties instead.
		/// </summary>
		public uint Flags {
			get => flags;
			set => flags = value;
		}

		/// <summary>
		/// Penalty cost for walking on this node.
		/// This can be used to make it harder/slower to walk over specific nodes.
		/// A cost of 1000 (<see cref="Pathfinding.Int3.Precision"/>) corresponds to the cost of moving 1 world unit.
		///
		/// See: graph-updates (view in online documentation for working links)
		/// </summary>
		public uint Penalty {
#if !ASTAR_NO_PENALTY
			get => penalty;
			set {
				if (value > 0xFFFFFF)
					Debug.LogWarning("Very high penalty applied. Are you sure negative values haven't underflowed?\n" +
						"Penalty values this high could with long paths cause overflows and in some cases infinity loops because of that.\n" +
						"Penalty value applied: "+value);
				penalty = value;
			}
#else
			get => 0U;
			set {}
#endif
		}

		/// <summary>
		/// True if the node is traversable.
		///
		/// See: graph-updates (view in online documentation for working links)
		/// </summary>
		public bool Walkable {
			[IgnoredByDeepProfiler]
			get => (flags & FlagsWalkableMask) != 0;
			[IgnoredByDeepProfiler]
			set {
				flags = flags & ~FlagsWalkableMask | (value ? 1U : 0U) << FlagsWalkableOffset;
				AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
			}
		}

		/// <summary>
		/// Hierarchical Node that contains this node.
		/// The graph is divided into clusters of small hierarchical nodes in which there is a path from every node to every other node.
		/// This structure is used to speed up connected component calculations which is used to quickly determine if a node is reachable from another node.
		///
		/// See: <see cref="Pathfinding.HierarchicalGraph"/>
		///
		/// Warning: This is an internal property and you should most likely not change it.
		///
		/// Warning: This is only guaranteed to be valid outside of graph updates, and only for walkable nodes.
		/// </summary>
		internal int HierarchicalNodeIndex {
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)][IgnoredByDeepProfiler]
			get => (int)((flags & HierarchicalIndexMask) >> FlagsHierarchicalIndexOffset);
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)][IgnoredByDeepProfiler]
			set => flags = (flags & ~HierarchicalIndexMask) | (uint)(value << FlagsHierarchicalIndexOffset);
		}

		/// <summary>Some internal bookkeeping</summary>
		internal bool IsHierarchicalNodeDirty {
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)][IgnoredByDeepProfiler]
			get => (flags & HierarchicalDirtyMask) != 0;
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)][IgnoredByDeepProfiler]
			set => flags = flags & ~HierarchicalDirtyMask | (value ? 1U : 0U) << HierarchicalDirtyOffset;
		}

		/// <summary>
		/// Connected component that contains the node.
		/// This is visualized in the scene view as differently colored nodes (if the graph coloring mode is set to 'Areas').
		/// Each area represents a set of nodes such that there is no valid path between nodes of different colors.
		///
		/// See: https://en.wikipedia.org/wiki/Connected_component_(graph_theory)
		/// See: <see cref="Pathfinding.HierarchicalGraph"/>
		/// </summary>
		public uint Area => AstarPath.active.hierarchicalGraph.GetConnectedComponent(HierarchicalNodeIndex);

		/// <summary>
		/// Graph which contains this node.
		/// See: <see cref="Pathfinding.AstarData.graphs"/>
		/// See: <see cref="Graph"/>
		/// </summary>
		public uint GraphIndex {
			[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)][IgnoredByDeepProfiler]
			get => (flags & FlagsGraphMask) >> FlagsGraphOffset;
			set => flags = flags & ~FlagsGraphMask | value << FlagsGraphOffset;
		}

		/// <summary>
		/// Node tag.
		/// See: tags (view in online documentation for working links)
		/// See: graph-updates (view in online documentation for working links)
		/// </summary>
		public uint Tag {
			get => (flags & FlagsTagMask) >> FlagsTagOffset;
			set => flags = flags & ~FlagsTagMask | ((value << FlagsTagOffset) & FlagsTagMask);
		}

		#endregion

		/// <summary>
		/// Inform the system that the node's connectivity has changed.
		/// This is used for recalculating the connected components of the graph.
		///
		/// See: <see cref="Pathfinding.HierarchicalGraph"/>
		///
		/// You must call this method if you change the connectivity or walkability of the node without going through the high level methods
		/// such as the <see cref="Walkable"/> property or the <see cref="Connect"/> method. For example if your manually change the <see cref="Pathfinding.MeshNode.connections"/> array you need to call this method.
		/// </summary>
		public void SetConnectivityDirty () {
			AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
		}

		/// <summary>
		/// Calls the delegate with all connections from this node.
		///
		/// *
		/// <code>
		/// node.GetConnections(connectedTo => {
		///     Debug.DrawLine((Vector3)node.position, (Vector3)connectedTo.position, Color.red);
		/// });
		/// </code>
		///
		/// You can add all connected nodes to a list like this
		/// <code>
		/// var connections = new List<GraphNode>();
		/// node.GetConnections(connections.Add);
		/// </code>
		/// </summary>
		/// <param name="action">The delegate which will be called once for every connection.</param>
		/// <param name="connectionFilter">A bitmask of which connection types will be included. You may pass any combination of \reflink{Connection.OutgoingConnection} and \reflink{Connection.IncomingConnection}.
		///  Defaults to only outgoing connections. Unless one-way links are added to a graph, all connections will typically be bidirectional.</param>
		public virtual void GetConnections (System.Action<GraphNode> action, int connectionFilter = Connection.OutgoingConnection) {
			GetConnections((GraphNode node, ref System.Action<GraphNode> action) => action(node), ref action, connectionFilter);
		}

		/// <summary>
		/// Calls the delegate with all connections from or to this node, and passes a custom data value to the delegate.
		///
		/// <code>
		/// node.GetConnections(connectedTo => {
		///     Debug.DrawLine((Vector3)node.position, (Vector3)connectedTo.position, Color.red);
		/// });
		/// </code>
		///
		/// You can add all connected nodes to a list like this
		/// <code>
		/// var connections = new List<GraphNode>();
		/// node.GetConnections(connections.Add);
		/// </code>
		/// </summary>
		/// <param name="action">The delegate which will be called once for every connection.
		/// The first parameter to the delegate is the connection and the second parameter is the custom data passed to this method.</param>
		/// <param name="data">Custom data which will be passed to the delegate.</param>
		/// <param name="connectionFilter">A bitmask of which connection types will be included. A connection can either be incoming, outgoing, or both (bidirectional). You may pass any combination of \reflink{Connection.OutgoingConnection} and \reflink{Connection.IncomingConnection}.
		///  Defaults to only outgoing connections. Unless one-way links are added to a graph, all connections will typically be bidirectional.</param>
		public abstract void GetConnections<T>(GetConnectionsWithData<T> action, ref T data, int connectionFilter = Connection.OutgoingConnection);

		public delegate void GetConnectionsWithData<T>(GraphNode node, ref T data);

		/// <summary>
		/// Adds a connection between two nodes.
		///
		/// If the nodes already have a connection to each other, that connection will be updated with the new cost.
		///
		/// Note that some graphs have a special representation for some connections which is more efficient.
		/// For example grid graphs can represent connections to its 8 neighbours more efficiently.
		/// But to use that efficient representation you'll need to call <see cref="GridNode.SetConnectionInternal"/> instead of this method.
		///
		/// This is different from an off-mesh link. An off-mesh link contains more metadata about the connection and is in many cases preferable to use instead of this method.
		/// This is a much lower-level method which is used by off-mesh links internally.
		///
		/// Movement scripts such as the <see cref="RichAI"/> or <see cref="FollowerEntity"/> may get confused if they try to follow a connection made using this method
		/// as it does not contain any information about how to traverse the connection.
		///
		/// Internally, both nodes keep track of the connection to the other node, even for a one-way connection.
		/// This is done to make sure the connection can always be removed later on, if for example one of the nodes is destroyed.
		///
		/// <code>
		/// // Connect two nodes
		/// var node1 = AstarPath.active.GetNearest(transform.position, NNConstraint.None).node;
		/// var node2 = AstarPath.active.GetNearest(transform.position + Vector3.right, NNConstraint.None).node;
		/// var cost = (uint)(node2.position - node1.position).costMagnitude;
		///
		/// GraphNode.Connect(node1, node2, cost, OffMeshLinks.Directionality.TwoWay);
		/// </code>
		///
		/// See: <see cref="OffMeshLinks"/>
		/// See: <see cref="AddPartialConnection"/> which is a lower level method. But if you use it, you need to handle invariants yourself.
		/// </summary>
		/// <param name="lhs">First node to connect.</param>
		/// <param name="rhs">Second node to connect</param>
		/// <param name="cost">Cost of the connection. A cost of 1000 corresponds approximately to the cost of moving one world unit. See \reflink{Int3.Precision}.</param>
		/// <param name="directionality">Determines if both lhs->rhs and rhs->lhs connections will be created, or if only a connection from lhs->rhs should be created.</param>
		public static void Connect (GraphNode lhs, GraphNode rhs, uint cost, OffMeshLinks.Directionality directionality = OffMeshLinks.Directionality.TwoWay) {
			if (lhs.Destroyed || rhs.Destroyed) throw new System.ArgumentException("Cannot connect destroyed nodes");
			lhs.AddPartialConnection(rhs, cost, true, directionality == OffMeshLinks.Directionality.TwoWay);
			rhs.AddPartialConnection(lhs, cost, directionality == OffMeshLinks.Directionality.TwoWay, true);
		}

		/// <summary>
		/// Removes the connection between two nodes.
		///
		/// If no connection exists between the nodes, nothing will be done.
		///
		/// This will also handle special connections representations that some node types use. For example grid graphs represent
		/// the connections to their 8 grid neighbours differently from other connections.
		///
		/// See: <see cref="GraphNode.Connect"/>
		/// </summary>
		public static void Disconnect (GraphNode lhs, GraphNode rhs) {
			lhs.RemovePartialConnection(rhs);
			rhs.RemovePartialConnection(lhs);
		}

		/// <summary>
		/// Adds a connection to the given node.
		///
		/// Deprecated: Use the static <see cref="Connect"/> method instead, or <see cref="AddPartialConnection"/> if you really need to.
		/// </summary>
		[System.Obsolete("Use the static Connect method instead, or AddPartialConnection if you really need to")]
		public void AddConnection(GraphNode node, uint cost) => AddPartialConnection(node, cost, true, true);

		/// <summary>
		/// Removes a connection to the given node.
		///
		/// Deprecated: Use the static <see cref="Disconnect"/> method instead, or <see cref="RemovePartialConnection"/> if you really need to.
		/// </summary>

		[System.Obsolete("Use the static Disconnect method instead, or RemovePartialConnection if you really need to")]
		public void RemoveConnection(GraphNode node) => RemovePartialConnection(node);

		/// <summary>
		/// Add a connection from this node to the specified node.
		/// If the connection already exists, the cost will simply be updated and
		/// no extra connection added.
		///
		/// <code>
		/// AstarPath.active.AddWorkItem(new AstarWorkItem(ctx => {
		///     // Connect two nodes
		///     var node1 = AstarPath.active.GetNearest(transform.position, NNConstraint.None).node;
		///     var node2 = AstarPath.active.GetNearest(transform.position + Vector3.right, NNConstraint.None).node;
		///     var cost = (uint)(node2.position - node1.position).costMagnitude;
		///     node1.AddPartialConnection(node2, cost, true, true);
		///     node2.AddPartialConnection(node1, cost, true, true);
		///
		///     node1.ContainsOutgoingConnection(node2); // True
		///
		///     node1.RemovePartialConnection(node2);
		///     node2.RemovePartialConnection(node1);
		/// }));
		/// </code>
		///
		/// Warning: In almost all cases, you should be using the <see cref="Connect"/> method instead. If you use this method, you must ensure that you preserve the required invariants of connections.
		/// Notably: If a connection exists from A to B, then there must also exist a connection from B to A. And their outgoing and incoming connection flags must be set symmetrically.
		///
		/// Some graphs have a special representation for some connections which is more efficient.
		/// For example grid graphs can represent connections to its 8 neighbours more efficiently.
		/// But to use that efficient representation you'll need to call <see cref="GridNode.SetConnectionInternal"/> instead of this method.
		/// </summary>
		public abstract void AddPartialConnection(GraphNode node, uint cost, bool isOutgoing, bool isIncoming);

		/// <summary>
		/// Removes any connection from this node to the specified node.
		/// If no such connection exists, nothing will be done.
		///
		/// Warning: In almost all cases, you should be using the <see cref="Disconnect"/> method instead. If you use this method, you must ensure that you preserve the required invariants of connections.
		/// Notably: If a connection exists from A to B, then there must also exist a connection from B to A. And their outgoing and incoming connection flags must be set symmetrically.
		/// Graphs sometimes use this method directly to improve performance in some situations.
		///
		/// <code>
		/// AstarPath.active.AddWorkItem(new AstarWorkItem(ctx => {
		///     // Connect two nodes
		///     var node1 = AstarPath.active.GetNearest(transform.position, NNConstraint.None).node;
		///     var node2 = AstarPath.active.GetNearest(transform.position + Vector3.right, NNConstraint.None).node;
		///     var cost = (uint)(node2.position - node1.position).costMagnitude;
		///     node1.AddPartialConnection(node2, cost, true, true);
		///     node2.AddPartialConnection(node1, cost, true, true);
		///
		///     node1.ContainsOutgoingConnection(node2); // True
		///
		///     node1.RemovePartialConnection(node2);
		///     node2.RemovePartialConnection(node1);
		/// }));
		/// </code>
		/// </summary>
		public abstract void RemovePartialConnection(GraphNode node);

		/// <summary>
		/// Remove all connections between this node and other nodes.
		///
		/// Warning: If you pass false to the alsoReverse parameter, you must ensure that you preserve the required invariants of connections. See <see cref="RemovePartialConnection"/>.
		/// </summary>
		/// <param name="alsoReverse">if true, neighbours will be requested to remove connections to this node.</param>
		public abstract void ClearConnections(bool alsoReverse = true);

		/// <summary>
		/// True if this node contains a connection to the given node.
		///
		/// Deprecated: Use <see cref="ContainsOutgoingConnection"/> instead
		/// </summary>
		[System.Obsolete("Use ContainsOutgoingConnection instead")]
		public bool ContainsConnection(GraphNode node) => ContainsOutgoingConnection(node);

		/// <summary>
		/// True if this node contains a connection to the given node.
		///
		/// This will not return true if another node has a one-way connection to this node.
		///
		/// <code>
		/// AstarPath.active.AddWorkItem(new AstarWorkItem(ctx => {
		///     // Connect two nodes
		///     var node1 = AstarPath.active.GetNearest(transform.position, NNConstraint.None).node;
		///     var node2 = AstarPath.active.GetNearest(transform.position + Vector3.right, NNConstraint.None).node;
		///     var cost = (uint)(node2.position - node1.position).costMagnitude;
		///     node1.AddPartialConnection(node2, cost, true, true);
		///     node2.AddPartialConnection(node1, cost, true, true);
		///
		///     node1.ContainsOutgoingConnection(node2); // True
		///
		///     node1.RemovePartialConnection(node2);
		///     node2.RemovePartialConnection(node1);
		/// }));
		/// </code>
		/// </summary>
		public virtual bool ContainsOutgoingConnection (GraphNode node) {
			// Simple but slow default implementation
			bool contains = false;

			GetConnections((GraphNode neighbour, ref bool contains) => {
				contains |= neighbour == node;
			}, ref contains);
			return contains;
		}

		/// <summary>
		/// Add a portal from this node to the specified node.
		/// This function should add a portal to the left and right lists which is connecting the two nodes (this and other).
		///
		/// Returns: True if the call was deemed successful. False if some unknown case was encountered and no portal could be added.
		/// If both calls to node1.GetPortal (node2,...) and node2.GetPortal (node1,...) return false, the funnel modifier will fall back to adding to the path
		/// the positions of the node.
		///
		/// The default implementation simply returns false.
		///
		/// This function may add more than one portal if necessary.
		///
		/// See: http://digestingduck.blogspot.se/2010/03/simple-stupid-funnel-algorithm.html
		///
		/// Deprecated: Use GetPortal(GraphNode, out Vector3, out Vector3) instead
		/// </summary>
		/// <param name="other">The node which is on the other side of the portal (strictly speaking it does not actually have to be on the other side of the portal though).</param>
		/// <param name="left">List of portal points on the left side of the funnel</param>
		/// <param name="right">List of portal points on the right side of the funnel</param>
		/// <param name="backwards">If this is true, the call was made on a node with the other node as the node before this one in the path.
		/// In this case you may choose to do nothing since a similar call will be made to the other node with this node referenced as other (but then with backwards = true).
		/// You do not have to care about switching the left and right lists, that is done for you already.</param>
		[System.Obsolete("Use GetPortal(GraphNode, out Vector3, out Vector3) instead")]
		public bool GetPortal (GraphNode other, List<Vector3> left, List<Vector3> right, bool backwards) {
			if (!backwards && GetPortal(other, out var lp, out var rp)) {
				if (left != null) {
					left.Add(lp);
					right.Add(rp);
				}
				return true;
			} else {
				return false;
			}
		}

		/// <summary>
		/// Add a portal from this node to the specified node.
		/// This function returns a portal which connects this node to the given adjacenet node.
		///
		/// Returns: True if the call was deemed successful. False if some unknown case was encountered and no portal could be added.
		/// If both calls to node1.GetPortal (node2,...) and node2.GetPortal (node1,...) return false, the funnel modifier will fall back to adding to the path
		/// the positions of the node.
		///
		/// The default implementation simply returns false.
		///
		/// See: http://digestingduck.blogspot.se/2010/03/simple-stupid-funnel-algorithm.html
		/// </summary>
		/// <param name="other">The node which is on the other side of the portal.</param>
		/// <param name="left">Output left side of the portal.</param>
		/// <param name="right">Output right side of the portal.</param>
		public virtual bool GetPortal (GraphNode other, out Vector3 left, out Vector3 right) {
			left = Vector3.zero;
			right = Vector3.zero;
			return false;
		}

		/// <summary>
		/// Open the node.
		/// Used internally by the A* algorithm.
		/// </summary>
		public abstract void Open(Path path, uint pathNodeIndex, uint gScore);

		/// <summary>
		/// Open the node at a specific point.
		///
		/// Used internally by the A* algorithm.
		///
		/// Used when a path starts inside a node, or when an off-mesh link is used to move to a point inside this node.
		/// </summary>
		public abstract void OpenAtPoint(Path path, uint pathNodeIndex, Int3 position, uint gScore);

		/// <summary>
		/// The position of the path node during the search.
		///
		/// When an A* search on triangle nodes is carried out, each edge of the node is a separate path node variant.
		/// The search will additionally decide where on that edge the path node is located.
		/// This is encoded by the fractionAlongEdge variable.
		/// This function decodes the position of the path node.
		///
		/// Note: Most node types only have a single path node variant and does not use the fractionAlongEdge field.
		/// In those cases this function only returns the node <see cref="position"/> unchanged.
		/// </summary>
		public virtual Int3 DecodeVariantPosition(uint pathNodeIndex, uint fractionAlongEdge) => position;

		/// <summary>The surface area of the node in square world units</summary>
		public virtual float SurfaceArea() => 0;

		/// <summary>
		/// A random point on the surface of the node.
		/// For point nodes and other nodes which do not have a surface, this will always return the position of the node.
		/// </summary>
		public virtual Vector3 RandomPointOnSurface () {
			return (Vector3)position;
		}

		/// <summary>Closest point on the surface of this node to the point p</summary>
		public abstract Vector3 ClosestPointOnNode(Vector3 p);

		/// <summary>Checks if point is inside the node when seen from above</summary>
		public virtual bool ContainsPoint (Int3 point) {
			return ContainsPoint((Vector3)point);
		}

		/// <summary>Checks if point is inside the node when seen from above.</summary>
		public abstract bool ContainsPoint(Vector3 point);

		/// <summary>Checks if point is inside the node in graph space</summary>
		public abstract bool ContainsPointInGraphSpace(Int3 point);

		/// <summary>
		/// Hash code used for checking if the gizmos need to be updated.
		/// Will change when the gizmos for the node might change.
		/// </summary>
		public virtual int GetGizmoHashCode () {
			// Some hashing, the constants are just some arbitrary prime numbers. #flags contains the info for #Tag and #Walkable
			return position.GetHashCode() ^ (19 * (int)Penalty) ^ (41 * (int)(flags & ~(HierarchicalIndexMask | HierarchicalDirtyMask)));
		}

		/// <summary>Serialized the node data to a byte array</summary>
		public virtual void SerializeNode (GraphSerializationContext ctx) {
			// Write basic node data.
			ctx.writer.Write(Penalty);
			// Save all flags except the hierarchical node index and the dirty bit
			ctx.writer.Write(Flags & ~(HierarchicalIndexMask | HierarchicalDirtyMask));
		}

		/// <summary>Deserializes the node data from a byte array</summary>
		public virtual void DeserializeNode (GraphSerializationContext ctx) {
			Penalty = ctx.reader.ReadUInt32();
			// Load all flags except the hierarchical node index and the dirty bit (they aren't saved in newer versions and older data should just be cleared)
			// Note that the dirty bit needs to be preserved here because it may already be set (due to the node being created)
			Flags = (ctx.reader.ReadUInt32() & ~(HierarchicalIndexMask | HierarchicalDirtyMask)) | (Flags & (HierarchicalIndexMask | HierarchicalDirtyMask));

			// Set the correct graph index (which might have changed, e.g if loading additively)
			GraphIndex = ctx.graphIndex;
		}

		/// <summary>
		/// Used to serialize references to other nodes e.g connections.
		/// Use the GraphSerializationContext.GetNodeIdentifier and
		/// GraphSerializationContext.GetNodeFromIdentifier methods
		/// for serialization and deserialization respectively.
		///
		/// Nodes must override this method and serialize their connections.
		/// Graph generators do not need to call this method, it will be called automatically on all
		/// nodes at the correct time by the serializer.
		/// </summary>
		public virtual void SerializeReferences (GraphSerializationContext ctx) {
		}

		/// <summary>
		/// Used to deserialize references to other nodes e.g connections.
		/// Use the GraphSerializationContext.GetNodeIdentifier and
		/// GraphSerializationContext.GetNodeFromIdentifier methods
		/// for serialization and deserialization respectively.
		///
		/// Nodes must override this method and serialize their connections.
		/// Graph generators do not need to call this method, it will be called automatically on all
		/// nodes at the correct time by the serializer.
		/// </summary>
		public virtual void DeserializeReferences (GraphSerializationContext ctx) {
		}
	}

	public abstract class MeshNode : GraphNode {
		/// <summary>
		/// All connections from this node.
		/// See: <see cref="Connect"/>
		/// See: <see cref="Disconnect"/>
		///
		/// Note: If you modify this array or the contents of it you must call <see cref="SetConnectivityDirty"/>.
		///
		/// May be null if the node has no connections.
		/// </summary>
		public Connection[] connections;

		/// <summary>Get a vertex of this node.</summary>
		/// <param name="i">vertex index. Must be between 0 and #GetVertexCount (exclusive). Typically between 0 and 3.</param>
		public abstract Int3 GetVertex(int i);

		/// <summary>
		/// Number of corner vertices that this node has.
		/// For example for a triangle node this will return 3.
		/// </summary>
		public abstract int GetVertexCount();

		/// <summary>
		/// Closest point on the surface of this node when seen from above.
		/// This is usually very similar to <see cref="ClosestPointOnNode"/> but when the node is in a slope this can be significantly different.
		/// [Open online documentation to see images]
		/// When the blue point in the above image is used as an argument this method call will return the green point while the <see cref="ClosestPointOnNode"/> method will return the red point.
		/// </summary>
		public abstract Vector3 ClosestPointOnNodeXZ(Vector3 p);

		public override void ClearConnections (bool alsoReverse = true) {
			// Remove all connections to this node from our neighbours
			if (alsoReverse && connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					connections[i].node.RemovePartialConnection(this);
				}
			}

			ArrayPool<Connection>.Release(ref connections, true);
			AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
		}

		public override void GetConnections<T>(GetConnectionsWithData<T> action, ref T data, int connectionFilter = Connection.OutgoingConnection) {
			if (connections == null) return;
			for (int i = 0; i < connections.Length; i++) if ((connections[i].shapeEdgeInfo & connectionFilter) != 0) action(connections[i].node, ref data);
		}

		public override bool ContainsOutgoingConnection (GraphNode node) {
			if (connections != null) for (int i = 0; i < connections.Length; i++) if (connections[i].node == node && connections[i].isOutgoing) return true;
			return false;
		}

		public override void AddPartialConnection (GraphNode node, uint cost, bool isOutgoing, bool isIncoming) {
			AddPartialConnection(node, cost, Connection.PackShapeEdgeInfo(isOutgoing, isIncoming));
		}

		/// <summary>
		/// Add a connection from this node to the specified node.
		///
		/// If the connection already exists, the cost will simply be updated and
		/// no extra connection added.
		///
		/// Warning: In almost all cases, you should be using the <see cref="Connect"/> method instead. If you use this method, you must ensure that you preserve the required invariants of connections.
		/// Notably: If a connection exists from A to B, then there must also exist a connection from B to A. And their outgoing and incoming connection flags must be set symmetrically.
		/// </summary>
		/// <param name="node">Node to add a connection to</param>
		/// <param name="cost">Cost of traversing the connection. A cost of 1000 corresponds approximately to the cost of moving 1 world unit.</param>
		/// <param name="shapeEdgeInfo">Info about how the edge is which edge on the shape of this node to use or \reflink{Connection.NoSharedEdge} if no edge is used. See \reflink{Connection.PackShapeEdgeInfo(byte,byte,bool,bool,bool)}.</param>
		public void AddPartialConnection (GraphNode node, uint cost, byte shapeEdgeInfo) {
			if (node == null) throw new System.ArgumentNullException();

			// Check if we already have a connection to the node
			if (connections != null) {
				for (int i = 0; i < connections.Length; i++) {
					if (connections[i].node == node) {
						// Just update the cost for the existing connection
						connections[i].cost = cost;
						connections[i].shapeEdgeInfo = shapeEdgeInfo;
						return;
					}
				}
			}

			// Create new arrays which include the new connection
			int connLength = connections != null ? connections.Length : 0;

			var newconns = ArrayPool<Connection>.ClaimWithExactLength(connLength+1);
			for (int i = 0; i < connLength; i++) {
				newconns[i] = connections[i];
			}

			newconns[connLength] = new Connection(node, cost, shapeEdgeInfo);

			if (connections != null) {
				ArrayPool<Connection>.Release(ref connections, true);
			}

			connections = newconns;
			AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
		}

		public override void RemovePartialConnection (GraphNode node) {
			if (connections == null) return;

			// Iterate through all connections and check if there are any to the node
			for (int i = 0; i < connections.Length; i++) {
				if (connections[i].node == node) {
					// Create new arrays which have the specified node removed
					int connLength = connections.Length;

					var newconns = ArrayPool<Connection>.ClaimWithExactLength(connLength-1);
					for (int j = 0; j < i; j++) {
						newconns[j] = connections[j];
					}
					for (int j = i+1; j < connLength; j++) {
						newconns[j-1] = connections[j];
					}

					if (connections != null) {
						ArrayPool<Connection>.Release(ref connections, true);
					}

					connections = newconns;
					AstarPath.active.hierarchicalGraph.AddDirtyNode(this);
					return;
				}
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

		public override void SerializeReferences(GraphSerializationContext ctx) => ctx.SerializeConnections(connections, true);

		public override void DeserializeReferences(GraphSerializationContext ctx) => connections = ctx.DeserializeConnections(true);
	}
}
