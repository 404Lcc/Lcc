using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Serialization;
using Pathfinding.Pooling;

namespace Pathfinding {
	using Pathfinding.Drawing;
	using Unity.Jobs;

	/// <summary>
	/// Graph consisting of a set of points.
	///
	/// [Open online documentation to see images]
	///
	/// The point graph is the most basic graph structure, it consists of a number of interconnected points in space called nodes or waypoints.
	/// The point graph takes a Transform object as "root", this Transform will be searched for child objects, every child object will be treated as a node.
	/// If <see cref="recursive"/> is enabled, it will also search the child objects of the children recursively.
	/// It will then check if any connections between the nodes can be made, first it will check if the distance between the nodes isn't too large (<see cref="maxDistance)"/>
	/// and then it will check if the axis aligned distance isn't too high. The axis aligned distance, named <see cref="limits"/>,
	/// is useful because usually an AI cannot climb very high, but linking nodes far away from each other,
	/// but on the same Y level should still be possible. <see cref="limits"/> and <see cref="maxDistance"/> are treated as being set to infinity if they are set to 0 (zero).
	/// Lastly it will check if there are any obstructions between the nodes using
	/// <a href="http://unity3d.com/support/documentation/ScriptReference/Physics.Raycast.html">raycasting</a> which can optionally be thick.
	/// One thing to think about when using raycasting is to either place the nodes a small
	/// distance above the ground in your scene or to make sure that the ground is not in the raycast mask to avoid the raycast from hitting the ground.
	///
	/// Alternatively, a tag can be used to search for nodes.
	/// See: http://docs.unity3d.com/Manual/Tags.html
	///
	/// For larger graphs, it can take quite some time to scan the graph with the default settings.
	/// You can enable <see cref="optimizeForSparseGraph"/> which will in most cases reduce the calculation times drastically.
	///
	/// Note: Does not support linecast because the nodes do not have a surface.
	///
	/// See: get-started-point (view in online documentation for working links)
	/// See: graphTypes (view in online documentation for working links)
	///
	/// \section pointgraph-inspector Inspector
	/// [Open online documentation to see images]
	///
	/// \inspectorField{Root, root}
	/// \inspectorField{Recursive, recursive}
	/// \inspectorField{Tag, searchTag}
	/// \inspectorField{Max Distance, maxDistance}
	/// \inspectorField{Max Distance (axis aligned), limits}
	/// \inspectorField{Raycast, raycast}
	/// \inspectorField{Raycast → Use 2D Physics, use2DPhysics}
	/// \inspectorField{Raycast → Thick Raycast, thickRaycast}
	/// \inspectorField{Raycast → Thick Raycast → Radius, thickRaycastRadius}
	/// \inspectorField{Raycast → Mask, mask}
	/// \inspectorField{Optimize For Sparse Graph, optimizeForSparseGraph}
	/// \inspectorField{Nearest Node Queries Find Closest, nearestNodeDistanceMode}
	/// \inspectorField{Initial Penalty, initialPenalty}
	/// </summary>
	[JsonOptIn]
	[Pathfinding.Util.Preserve]
	public class PointGraph : NavGraph
		, IUpdatableGraph {
		/// <summary>
		/// Children of this transform are treated as nodes.
		///
		/// If null, the <see cref="searchTag"/> will be used instead.
		/// </summary>
		[JsonMember]
		public Transform root;

		/// <summary>If no <see cref="root"/> is set, all nodes with the tag is used as nodes</summary>
		[JsonMember]
		public string searchTag;

		/// <summary>
		/// Max distance for a connection to be valid.
		/// The value 0 (zero) will be read as infinity and thus all nodes not restricted by
		/// other constraints will be added as connections.
		///
		/// A negative value will disable any neighbours to be added.
		/// It will completely stop the connection processing to be done, so it can save you processing
		/// power if you don't these connections.
		/// </summary>
		[JsonMember]
		public float maxDistance;

		/// <summary>Max distance along the axis for a connection to be valid. 0 = infinity</summary>
		[JsonMember]
		public Vector3 limits;

		/// <summary>
		/// Use raycasts to filter connections.
		///
		/// If a hit is detected between two nodes, the connection will not be created.
		/// </summary>
		[JsonMember]
		public bool raycast = true;

		/// <summary>Use the 2D Physics API</summary>
		[JsonMember]
		public bool use2DPhysics;

		/// <summary>
		/// Use thick raycast.
		///
		/// If enabled, the collision check shape will not be a line segment, but a capsule with a radius of <see cref="thickRaycastRadius"/>.
		/// </summary>
		[JsonMember]
		public bool thickRaycast;

		/// <summary>
		/// Thick raycast radius.
		///
		/// See: <see cref="thickRaycast"/>
		/// </summary>
		[JsonMember]
		public float thickRaycastRadius = 1;

		/// <summary>
		/// Recursively search for child nodes to the <see cref="root"/>.
		///
		/// If false, all direct children of <see cref="root"/> will be used as nodes.
		/// If true, all children of <see cref="root"/> and their children (recursively) will be used as nodes.
		/// </summary>
		[JsonMember]
		public bool recursive = true;

		/// <summary>
		/// Layer mask to use for raycasting.
		///
		/// All objects included in this layer mask will be treated as obstacles.
		///
		/// See: <see cref="raycast"/>
		/// </summary>
		[JsonMember]
		public LayerMask mask;

		/// <summary>
		/// Optimizes the graph for sparse graphs.
		///
		/// This can reduce calculation times for both scanning and for normal path requests by huge amounts.
		/// It reduces the number of node-node checks that need to be done during scan, and can also optimize getting the nearest node from the graph (such as when querying for a path).
		///
		/// Try enabling and disabling this option, check the scan times logged when you scan the graph to see if your graph is suited for this optimization
		/// or if it makes it slower.
		///
		/// The gain of using this optimization increases with larger graphs, the default scan algorithm is brute force and requires O(n^2) checks, this optimization
		/// along with a graph suited for it, requires only O(n) checks during scan (assuming the connection distance limits are reasonable).
		///
		/// Warning:
		/// When you have this enabled, you will not be able to move nodes around using scripting unless you recalculate the lookup structure at the same time.
		/// See: <see cref="RebuildNodeLookup"/>
		///
		/// If you enable this during runtime, you need to call <see cref="RebuildNodeLookup"/> to make this take effect.
		/// If you are going to scan the graph afterwards then you do not need to do this.
		/// </summary>
		[JsonMember]
		public bool optimizeForSparseGraph;

		PointKDTree lookupTree = new PointKDTree();

		/// <summary>
		/// Longest known connection.
		/// In squared Int3 units.
		///
		/// See: <see cref="RegisterConnectionLength"/>
		/// </summary>
		long maximumConnectionLength = 0;

		/// <summary>
		/// All nodes in this graph.
		/// Note that only the first <see cref="nodeCount"/> will be non-null.
		///
		/// You can also use the GetNodes method to get all nodes.
		///
		/// The order of the nodes is unspecified, and may change when nodes are added or removed.
		/// </summary>
		public PointNode[] nodes;

		/// <summary>
		/// \copydoc Pathfinding::PointGraph::NodeDistanceMode
		///
		/// See: <see cref="NodeDistanceMode"/>
		///
		/// If you enable this during runtime, you will need to call <see cref="RebuildConnectionDistanceLookup"/> to make sure some cache data is properly recalculated.
		/// If the graph doesn't have any nodes yet or if you are going to scan the graph afterwards then you do not need to do this.
		/// </summary>
		[JsonMember]
		public NodeDistanceMode nearestNodeDistanceMode;

		/// <summary>Number of nodes in this graph</summary>
		public int nodeCount { get; protected set; }

		public override bool isScanned => nodes != null;

		/// <summary>
		/// Distance query mode.
		/// [Open online documentation to see images]
		///
		/// In the image above there are a few red nodes. Assume the agent is the orange circle. Using the Node mode the closest point on the graph that would be found would be the node at the bottom center which
		/// may not be what you want. Using the Connection mode it will find the closest point on the connection between the two nodes in the top half of the image.
		///
		/// When using the Connection option you may also want to use the Connection option for the Seeker's Start End Modifier snapping options.
		/// This is not strictly necessary, but it most cases it is what you want.
		///
		/// See: <see cref="Pathfinding.StartEndModifier.exactEndPoint"/>
		/// </summary>
		public enum NodeDistanceMode {
			/// <summary>
			/// All nearest node queries find the closest node center.
			/// This is the fastest option but it may not be what you want if you have long connections.
			/// </summary>
			Node,
			/// <summary>
			/// All nearest node queries find the closest point on edges between nodes.
			/// This is useful if you have long connections where the agent might be closer to some unrelated node if it is standing on a long connection between two nodes.
			/// This mode is however slower than the Node mode.
			/// </summary>
			Connection,
		}

		public override int CountNodes () {
			return nodeCount;
		}

		public override void GetNodes (System.Action<GraphNode> action) {
			if (nodes == null) return;
			var count = nodeCount;
			for (int i = 0; i < count; i++) action(nodes[i]);
		}

		public override NNInfo GetNearest (Vector3 position, NNConstraint constraint, float maxDistanceSqr) {
			if (nodes == null) return NNInfo.Empty;
			var iposition = (Int3)position;

			if ((lookupTree != null) != optimizeForSparseGraph) {
				Debug.LogWarning("Lookup tree is not in the correct state. Have you changed PointGraph.optimizeForSparseGraph without calling RebuildNodeLookup?");
			}

			if (lookupTree != null) {
				if (nearestNodeDistanceMode == NodeDistanceMode.Node) {
					var minDistSqr = maxDistanceSqr;
					var closestNode = lookupTree.GetNearest(iposition, constraint, ref minDistSqr);
					return closestNode == null ? NNInfo.Empty : new NNInfo(closestNode, (Vector3)closestNode.position, minDistSqr);
				} else {
					var closestNode = lookupTree.GetNearestConnection(iposition, constraint, maximumConnectionLength);
					return closestNode == null ? NNInfo.Empty : FindClosestConnectionPoint(closestNode as PointNode, position, maxDistanceSqr);
				}
			}

			PointNode minNode = null;
			long minDist = AstarMath.SaturatingConvertFloatToLong(maxDistanceSqr * Int3.FloatPrecision * Int3.FloatPrecision);

			for (int i = 0; i < nodeCount; i++) {
				PointNode node = nodes[i];
				long dist = (iposition - node.position).sqrMagnitudeLong;

				if (dist < minDist && (constraint == null || constraint.Suitable(node))) {
					minDist = dist;
					minNode = node;
				}
			}

			float distSqr = Int3.PrecisionFactor*Int3.PrecisionFactor*minDist;
			// Do a final distance check here just to make sure we don't exceed the max distance due to rounding errors when converting between longs and floats
			return distSqr < maxDistanceSqr && minNode != null ? new NNInfo(minNode, (Vector3)minNode.position, Int3.PrecisionFactor*Int3.PrecisionFactor*minDist) : NNInfo.Empty;
		}

		NNInfo FindClosestConnectionPoint (PointNode node, Vector3 position, float maxDistanceSqr) {
			var closestConnectionPoint = (Vector3)node.position;
			var conns = node.connections;
			var nodePos = (Vector3)node.position;

			if (conns != null) {
				for (int i = 0; i < conns.Length; i++) {
					var connectionMidpoint = ((UnityEngine.Vector3)conns[i].node.position + nodePos) * 0.5f;
					var closestPoint = VectorMath.ClosestPointOnSegment(nodePos, connectionMidpoint, position);
					var dist = (closestPoint - position).sqrMagnitude;
					if (dist < maxDistanceSqr) {
						maxDistanceSqr = dist;
						closestConnectionPoint = closestPoint;
					}
				}
			}

			return new NNInfo(node, closestConnectionPoint, maxDistanceSqr);
		}

		public override NNInfo RandomPointOnSurface (NNConstraint nnConstraint = null, bool highQuality = true) {
			if (!isScanned || nodes.Length == 0) return NNInfo.Empty;

			// All nodes have the same surface area, so just pick a random node
			for (int i = 0; i < 10; i++) {
				var node = this.nodes[UnityEngine.Random.Range(0, this.nodes.Length)];
				if (node != null && (nnConstraint == null || nnConstraint.Suitable(node))) {
					return new NNInfo(node, node.RandomPointOnSurface(), 0);
				}
			}

			// If a valid node was not found after a few tries, the graph likely contains a lot of unwalkable/unsuitable nodes.
			// Fall back to the base method which will try to find a valid node by checking all nodes.
			return base.RandomPointOnSurface(nnConstraint, highQuality);
		}

		/// <summary>
		/// Add a node to the graph at the specified position.
		/// Note: Vector3 can be casted to Int3 using (Int3)myVector.
		///
		/// Note: This needs to be called when it is safe to update nodes, which is
		/// - when scanning
		/// - during a graph update
		/// - inside a callback registered using AstarPath.AddWorkItem
		///
		/// <code>
		/// AstarPath.active.AddWorkItem(() => {
		///     var graph = AstarPath.active.data.pointGraph;
		///     // Add 2 nodes and connect them
		///     var node1 = graph.AddNode((Int3)transform.position);
		///     var node2 = graph.AddNode((Int3)(transform.position + Vector3.right));
		///     var cost = (uint)(node2.position - node1.position).costMagnitude;
		///     GraphNode.Connect(node1, node2, cost);
		/// });
		/// </code>
		///
		/// See: runtime-graphs (view in online documentation for working links)
		/// See: creating-point-nodes (view in online documentation for working links)
		/// </summary>
		public PointNode AddNode (Int3 position) {
			return AddNode(new PointNode(active), position);
		}

		/// <summary>
		/// Add a node with the specified type to the graph at the specified position.
		///
		/// Note: Vector3 can be casted to Int3 using (Int3)myVector.
		///
		/// Note: This needs to be called when it is safe to update nodes, which is
		/// - when scanning
		/// - during a graph update
		/// - inside a callback registered using AstarPath.AddWorkItem
		///
		/// See: <see cref="AstarPath.AddWorkItem"/>
		/// See: runtime-graphs (view in online documentation for working links)
		/// See: creating-point-nodes (view in online documentation for working links)
		/// </summary>
		/// <param name="node">This must be a node created using T(AstarPath.active) right before the call to this method.
		/// The node parameter is only there because there is no new(AstarPath) constraint on
		/// generic type parameters.</param>
		/// <param name="position">The node will be set to this position.</param>
		public T AddNode<T>(T node, Int3 position) where T : PointNode {
			AssertSafeToUpdateGraph();
			if (nodes == null || nodeCount == nodes.Length) {
				var newNodes = new PointNode[nodes != null ? System.Math.Max(nodes.Length+4, nodes.Length*2) : 4];
				if (nodes != null) nodes.CopyTo(newNodes, 0);
				nodes = newNodes;
				RebuildNodeLookup();
			}

			node.position = position;
			node.GraphIndex = graphIndex;
			node.Walkable = true;

			nodes[nodeCount] = node;
			nodeCount++;

			if (lookupTree != null) lookupTree.Add(node);

			return node;
		}

		/// <summary>
		/// Removes a node from the graph.
		///
		/// <code>
		/// // Make sure we only modify the graph when all pathfinding threads are paused
		/// AstarPath.active.AddWorkItem(() => {
		///     // Find the node closest to some point
		///     var nearest = AstarPath.active.GetNearest(new Vector3(1, 2, 3));
		///
		///     // Check if it is a PointNode
		///     if (nearest.node is PointNode pnode) {
		///         // Remove the node. Assuming it belongs to the first point graph in the scene
		///         AstarPath.active.data.pointGraph.RemoveNode(pnode);
		///     }
		/// });
		/// </code>
		///
		/// Note: For larger graphs, this operation can be slow, as it is linear in the number of nodes in the graph.
		///
		/// See: <see cref="AddNode"/>
		/// See: creating-point-nodes (view in online documentation for working links)
		/// </summary>
		public void RemoveNode (PointNode node) {
			AssertSafeToUpdateGraph();
			if (node.Destroyed) throw new System.ArgumentException("The node has already been destroyed");
			if (node.GraphIndex != graphIndex) throw new System.ArgumentException("The node does not belong to this graph");
			if (!isScanned) throw new System.InvalidOperationException("Graph contains no nodes");

			// Remove and swap with the last node
			// We can do this because we do not guarantee the order of the nodes
			var idx = System.Array.IndexOf(nodes, node);
			if (idx == -1) throw new System.ArgumentException("The node is not in the graph");

			nodeCount--;
			nodes[idx] = nodes[nodeCount];
			nodes[nodeCount] = null;
			node.Destroy();

			if (lookupTree != null) {
				lookupTree.Remove(node);
			}
		}

		/// <summary>Recursively counds children of a transform</summary>
		protected static int CountChildren (Transform tr) {
			int c = 0;

			foreach (Transform child in tr) {
				c++;
				c += CountChildren(child);
			}
			return c;
		}

		/// <summary>Recursively adds childrens of a transform as nodes</summary>
		protected static void AddChildren (PointNode[] nodes, ref int c, Transform tr) {
			foreach (Transform child in tr) {
				nodes[c].position = (Int3)child.position;
				nodes[c].Walkable = true;
				nodes[c].gameObject = child.gameObject;

				c++;
				AddChildren(nodes, ref c, child);
			}
		}

		/// <summary>
		/// Rebuilds the lookup structure for nodes.
		///
		/// This is used when <see cref="optimizeForSparseGraph"/> is enabled.
		///
		/// You should call this method every time you move a node in the graph manually and
		/// you are using <see cref="optimizeForSparseGraph"/>, otherwise pathfinding might not work correctly.
		///
		/// You may also call this after you have added many nodes using the
		/// <see cref="AddNode"/> method. When adding nodes using the <see cref="AddNode"/> method they
		/// will be added to the lookup structure. The lookup structure will
		/// rebalance itself when it gets too unbalanced. But if you are
		/// sure you won't be adding any more nodes in the short term, you can
		/// make sure it is perfectly balanced and thus squeeze out the last
		/// bit of performance by calling this method. This can improve the
		/// performance of the <see cref="GetNearest"/> method slightly. The improvements
		/// are on the order of 10-20%.
		/// </summary>
		public void RebuildNodeLookup () {
			lookupTree = BuildNodeLookup(nodes, nodeCount, optimizeForSparseGraph);
			RebuildConnectionDistanceLookup();
		}

		static PointKDTree BuildNodeLookup (PointNode[] nodes, int nodeCount, bool optimizeForSparseGraph) {
			if (optimizeForSparseGraph && nodes != null) {
				var lookupTree = new PointKDTree();
				lookupTree.Rebuild(nodes, 0, nodeCount);
				return lookupTree;
			} else {
				return null;
			}
		}

		/// <summary>Rebuilds a cache used when <see cref="nearestNodeDistanceMode"/> = <see cref="NodeDistanceMode"/>.Connection</summary>
		public void RebuildConnectionDistanceLookup () {
			if (nearestNodeDistanceMode == NodeDistanceMode.Connection) {
				maximumConnectionLength = LongestConnectionLength(nodes, nodeCount);
			} else {
				maximumConnectionLength = 0;
			}
		}

		static long LongestConnectionLength (PointNode[] nodes, int nodeCount) {
			long maximumConnectionLength = 0;
			for (int j = 0; j < nodeCount; j++) {
				var node = nodes[j];
				var conns = node.connections;
				if (conns != null) {
					for (int i = 0; i < conns.Length; i++) {
						var distSqr = (node.position - conns[i].node.position).sqrMagnitudeLong;
						maximumConnectionLength = System.Math.Max(maximumConnectionLength, distSqr);
					}
				}
			}
			return maximumConnectionLength;
		}

		/// <summary>
		/// Ensures the graph knows that there is a connection with this length.
		/// This is used when the nearest node distance mode is set to ToConnection.
		/// If you are modifying node connections yourself (i.e. manipulating the PointNode.connections array) then you must call this function
		/// when you add any connections.
		///
		/// When using GraphNode.Connect this is done automatically.
		/// It is also done for all nodes when <see cref="RebuildNodeLookup"/> is called.
		/// </summary>
		/// <param name="sqrLength">The length of the connection in squared Int3 units. This can be calculated using (node1.position - node2.position).sqrMagnitudeLong.</param>
		public void RegisterConnectionLength (long sqrLength) {
			maximumConnectionLength = System.Math.Max(maximumConnectionLength, sqrLength);
		}

		protected virtual PointNode[] CreateNodes (int count) {
			var nodes = new PointNode[count];

			for (int i = 0; i < count; i++) nodes[i] = new PointNode(active);
			return nodes;
		}

		class PointGraphScanPromise : IGraphUpdatePromise {
			public PointGraph graph;
			PointKDTree lookupTree;
			PointNode[] nodes;

			public IEnumerator<JobHandle> Prepare () {
				var root = graph.root;
				if (root == null) {
					// If there is no root object, try to find nodes with the specified tag instead
					GameObject[] gos = graph.searchTag != null? GameObject.FindGameObjectsWithTag(graph.searchTag) : null;

					if (gos == null) {
						nodes = new PointNode[0];
					} else {
						// Create all the nodes
						nodes = graph.CreateNodes(gos.Length);

						for (int i = 0; i < gos.Length; i++) {
							var node = nodes[i];
							node.position = (Int3)gos[i].transform.position;
							node.Walkable = true;
							node.gameObject = gos[i].gameObject;
						}
					}
				} else {
					// Search the root for children and create nodes for them
					if (!graph.recursive) {
						var nodeCount = root.childCount;
						nodes = graph.CreateNodes(nodeCount);

						int c = 0;
						foreach (Transform child in root) {
							var node = nodes[c];
							node.position = (Int3)child.position;
							node.Walkable = true;
							node.gameObject = child.gameObject;
							c++;
						}
					} else {
						var nodeCount = CountChildren(root);
						nodes = graph.CreateNodes(nodeCount);

						int nodeIndex = 0;
						AddChildren(nodes, ref nodeIndex, root);
						UnityEngine.Assertions.Assert.AreEqual(nodeIndex, nodeCount);
					}
				}

				yield return default;
				lookupTree = BuildNodeLookup(nodes, nodes.Length, graph.optimizeForSparseGraph);

				foreach (var progress in ConnectNodesAsync(nodes, nodes.Length, lookupTree, graph.maxDistance, graph.limits, graph)) yield return default;
			}

			public void Apply (IGraphUpdateContext ctx) {
				// Destroy all previous nodes (if any)
				graph.DestroyAllNodes();
				// Assign the new node data
				graph.lookupTree = lookupTree;
				graph.nodes = nodes;
				graph.nodeCount = nodes.Length;
				graph.maximumConnectionLength = graph.nearestNodeDistanceMode == NodeDistanceMode.Connection ? LongestConnectionLength(nodes, nodes.Length) : 0;
			}
		}

		protected override void DestroyAllNodes () {
			base.DestroyAllNodes();
			nodes = null;
			lookupTree = null;
		}

		protected override IGraphUpdatePromise ScanInternal () => new PointGraphScanPromise { graph = this };

		/// <summary>
		/// Recalculates connections for all nodes in the graph.
		/// This is useful if you have created nodes manually using <see cref="AddNode"/> and then want to connect them in the same way as the point graph normally connects nodes.
		/// </summary>
		public void ConnectNodes () {
			AssertSafeToUpdateGraph();
			var ie = ConnectNodesAsync(nodes, nodeCount, lookupTree, maxDistance, limits, this).GetEnumerator();

			while (ie.MoveNext()) {}

			RebuildConnectionDistanceLookup();
		}

		/// <summary>
		/// Calculates connections for all nodes in the graph.
		/// This is an IEnumerable, you can iterate through it using e.g foreach to get progress information.
		/// </summary>
		static IEnumerable<float> ConnectNodesAsync (PointNode[] nodes, int nodeCount, PointKDTree lookupTree, float maxDistance, Vector3 limits, PointGraph graph) {
			if (maxDistance >= 0) {
				// To avoid too many allocations, these lists are reused for each node
				var connections = new List<Connection>();
				var candidateConnections = new List<GraphNode>();

				long maxSquaredRange;
				// Max possible squared length of a connection between two nodes
				// This is used to speed up the calculations by skipping a lot of nodes that do not need to be checked
				if (maxDistance == 0 && (limits.x == 0 || limits.y == 0 || limits.z == 0)) {
					maxSquaredRange = long.MaxValue;
				} else {
					maxSquaredRange = (long)(Mathf.Max(limits.x, Mathf.Max(limits.y, Mathf.Max(limits.z, maxDistance))) * Int3.Precision) + 1;
					maxSquaredRange *= maxSquaredRange;
				}

				// Report progress every N nodes
				const int YieldEveryNNodes = 512;

				// Loop through all nodes and add connections to other nodes
				for (int i = 0; i < nodeCount; i++) {
					if (i % YieldEveryNNodes == 0) {
						yield return i/(float)nodeCount;
					}

					connections.Clear();
					var node = nodes[i];
					if (lookupTree != null) {
						candidateConnections.Clear();
						lookupTree.GetInRange(node.position, maxSquaredRange, candidateConnections);
						for (int j = 0; j < candidateConnections.Count; j++) {
							var other = candidateConnections[j] as PointNode;
							if (other != node && graph.IsValidConnection(node, other, out var dist)) {
								connections.Add(new Connection(
									other,
									/// <summary>TODO: Is this equal to .costMagnitude</summary>
									(uint)Mathf.RoundToInt(dist*Int3.FloatPrecision),
									true,
									true
									));
							}
						}
					} else {
						// brute force
						for (int j = 0; j < nodeCount; j++) {
							if (i == j) continue;

							PointNode other = nodes[j];
							if (graph.IsValidConnection(node, other, out var dist)) {
								connections.Add(new Connection(
									other,
									/// <summary>TODO: Is this equal to .costMagnitude</summary>
									(uint)Mathf.RoundToInt(dist*Int3.FloatPrecision),
									true,
									true
									));
							}
						}
					}
					node.connections = connections.ToArray();
					node.SetConnectivityDirty();
				}
			}
		}

		/// <summary>
		/// Returns if the connection between a and b is valid.
		/// Checks for obstructions using raycasts (if enabled) and checks for height differences.
		/// As a bonus, it outputs the distance between the nodes too if the connection is valid.
		///
		/// Note: This is not the same as checking if node a is connected to node b.
		/// That should be done using a.ContainsOutgoingConnection(b)
		/// </summary>
		public virtual bool IsValidConnection (GraphNode a, GraphNode b, out float dist) {
			dist = 0;

			if (!a.Walkable || !b.Walkable) return false;

			var dir = (Vector3)(b.position-a.position);

			if (
				(!Mathf.Approximately(limits.x, 0) && Mathf.Abs(dir.x) > limits.x) ||
				(!Mathf.Approximately(limits.y, 0) && Mathf.Abs(dir.y) > limits.y) ||
				(!Mathf.Approximately(limits.z, 0) && Mathf.Abs(dir.z) > limits.z)) {
				return false;
			}

			dist = dir.magnitude;
			if (maxDistance == 0 || dist < maxDistance) {
				if (raycast) {
					var ray = new Ray((Vector3)a.position, dir);
					var invertRay = new Ray((Vector3)b.position, -dir);

					if (use2DPhysics) {
						if (thickRaycast) {
							return !Physics2D.CircleCast(ray.origin, thickRaycastRadius, ray.direction, dist, mask) && !Physics2D.CircleCast(invertRay.origin, thickRaycastRadius, invertRay.direction, dist, mask);
						} else {
							return !Physics2D.Linecast((Vector2)(Vector3)a.position, (Vector2)(Vector3)b.position, mask) && !Physics2D.Linecast((Vector2)(Vector3)b.position, (Vector2)(Vector3)a.position, mask);
						}
					} else {
						if (thickRaycast) {
							return !Physics.SphereCast(ray, thickRaycastRadius, dist, mask) && !Physics.SphereCast(invertRay, thickRaycastRadius, dist, mask);
						} else {
							return !Physics.Linecast((Vector3)a.position, (Vector3)b.position, mask) && !Physics.Linecast((Vector3)b.position, (Vector3)a.position, mask);
						}
					}
				} else {
					return true;
				}
			}
			return false;
		}

		class PointGraphUpdatePromise : IGraphUpdatePromise {
			public PointGraph graph;
			public List<GraphUpdateObject> graphUpdates;

			public void Apply (IGraphUpdateContext ctx) {
				var nodes = graph.nodes;
				for (int u = 0; u < graphUpdates.Count; u++) {
					var guo = graphUpdates[u];
					for (int i = 0; i < graph.nodeCount; i++) {
						var node = nodes[i];
						if (guo.bounds.Contains((Vector3)node.position)) {
							guo.WillUpdateNode(node);
							guo.Apply(node);
						}
					}

					if (guo.updatePhysics) {
						// Use a copy of the bounding box, we should not change the GUO's bounding box since it might be used for other graph updates
						Bounds bounds = guo.bounds;

						if (graph.thickRaycast) {
							// Expand the bounding box to account for the thick raycast
							bounds.Expand(graph.thickRaycastRadius*2);
						}

						// Create a temporary list used for holding connection data
						List<Connection> tmpList = Pathfinding.Pooling.ListPool<Connection>.Claim();

						for (int i = 0; i < graph.nodeCount; i++) {
							PointNode node = graph.nodes[i];
							var nodePos = (Vector3)node.position;

							List<Connection> conn = null;

							for (int j = 0; j < graph.nodeCount; j++) {
								if (j == i) continue;

								var otherNodePos = (Vector3)nodes[j].position;
								// Check if this connection intersects the bounding box.
								// If it does we need to recalculate that connection.
								if (VectorMath.SegmentIntersectsBounds(bounds, nodePos, otherNodePos)) {
									float dist;
									PointNode other = nodes[j];
									bool contains = node.ContainsOutgoingConnection(other);
									bool validConnection = graph.IsValidConnection(node, other, out dist);

									// Fill the 'conn' list when we need to change a connection
									if (conn == null && (contains != validConnection)) {
										tmpList.Clear();
										conn = tmpList;
										if (node.connections != null) conn.AddRange(node.connections);
									}

									if (!contains && validConnection) {
										// A new connection should be added
										uint cost = (uint)Mathf.RoundToInt(dist*Int3.FloatPrecision);
										conn.Add(new Connection(other, cost, true, true));
										graph.RegisterConnectionLength((other.position - node.position).sqrMagnitudeLong);
									} else if (contains && !validConnection) {
										// A connection should be removed
										for (int q = 0; q < conn.Count; q++) {
											if (conn[q].node == other) {
												conn.RemoveAt(q);
												break;
											}
										}
									}
								}
							}

							// Save the new connections if any were changed
							if (conn != null) {
								node.connections = conn.ToArray();
								node.SetConnectivityDirty();
							}
						}

						// Release buffers back to the pool
						ListPool<Connection>.Release(ref tmpList);
						ctx.DirtyBounds(guo.bounds);
					}
				}

				ListPool<GraphUpdateObject>.Release(ref graphUpdates);
			}
		}

		/// <summary>
		/// Updates an area in the list graph.
		/// Recalculates possibly affected connections, i.e all connectionlines passing trough the bounds of the guo will be recalculated
		/// </summary>
		IGraphUpdatePromise IUpdatableGraph.ScheduleGraphUpdates (List<GraphUpdateObject> graphUpdates) {
			if (!isScanned) return null;

			return new PointGraphUpdatePromise {
					   graph = this,
					   graphUpdates = graphUpdates
			};
		}

#if UNITY_EDITOR
		static readonly Color NodeColor = new Color(0.161f, 0.341f, 1f, 0.5f);

		public override void OnDrawGizmos (DrawingData gizmos, bool drawNodes, RedrawScope redrawScope) {
			base.OnDrawGizmos(gizmos, drawNodes, redrawScope);

			if (!drawNodes) return;

			using (var draw = gizmos.GetBuilder()) {
				using (draw.WithColor(NodeColor)) {
					if (this.isScanned) {
						for (int i = 0; i < nodeCount; i++) {
							var pos = (Vector3)nodes[i].position;
							draw.SolidBox(pos, Vector3.one*UnityEditor.HandleUtility.GetHandleSize(pos)*0.1F);
						}
					} else {
						// When not scanned, draw the source data
						if (root != null) {
							DrawChildren(draw, this, root);
						} else if (!string.IsNullOrEmpty(searchTag)) {
							GameObject[] gos = GameObject.FindGameObjectsWithTag(searchTag);
							for (int i = 0; i < gos.Length; i++) {
								draw.SolidBox(gos[i].transform.position, Vector3.one*UnityEditor.HandleUtility.GetHandleSize(gos[i].transform.position)*0.1F);
							}
						}
					}
				}
			}
		}

		static void DrawChildren (CommandBuilder draw, PointGraph graph, Transform tr) {
			foreach (Transform child in tr) {
				draw.SolidBox(child.position, Vector3.one*UnityEditor.HandleUtility.GetHandleSize(child.position)*0.1F);
				if (graph.recursive) DrawChildren(draw, graph, child);
			}
		}
#endif

		protected override void PostDeserialization (GraphSerializationContext ctx) {
			RebuildNodeLookup();
		}

		public override void RelocateNodes (Matrix4x4 deltaMatrix) {
			base.RelocateNodes(deltaMatrix);
			RebuildNodeLookup();
		}

		protected override void SerializeExtraInfo (GraphSerializationContext ctx) {
			// Serialize node data

			if (nodes == null) ctx.writer.Write(-1);

			// Length prefixed array of nodes
			ctx.writer.Write(nodeCount);
			for (int i = 0; i < nodeCount; i++) {
				// -1 indicates a null field
				if (nodes[i] == null) ctx.writer.Write(-1);
				else {
					ctx.writer.Write(0);
					nodes[i].SerializeNode(ctx);
				}
			}
		}

		protected override void DeserializeExtraInfo (GraphSerializationContext ctx) {
			int count = ctx.reader.ReadInt32();

			if (count == -1) {
				nodes = null;
				return;
			}

			nodes = new PointNode[count];
			nodeCount = count;

			for (int i = 0; i < nodes.Length; i++) {
				if (ctx.reader.ReadInt32() == -1) continue;
				nodes[i] = new PointNode(active);
				nodes[i].DeserializeNode(ctx);
			}
		}
	}
}
