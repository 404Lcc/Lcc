using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Pathfinding.Pooling;

namespace Pathfinding {
	/// <summary>
	/// Basic path, finds the shortest path from A to B.
	///
	/// This is the most basic path object it will try to find the shortest path between two points.
	/// Many other path types inherit from this type.
	/// See: Seeker.StartPath
	/// See: calling-pathfinding (view in online documentation for working links)
	/// See: getstarted (view in online documentation for working links)
	/// </summary>
	public class ABPath : Path {
		/// <summary>Start node of the path</summary>
		public GraphNode startNode => path.Count > 0 ? path[0] : null;

		/// <summary>End node of the path</summary>
		public GraphNode endNode => path.Count > 0 ? path[path.Count-1] : null;

		/// <summary>Start Point exactly as in the path request</summary>
		public Vector3 originalStartPoint;

		/// <summary>End Point exactly as in the path request</summary>
		public Vector3 originalEndPoint;

		/// <summary>
		/// Start point of the path.
		/// This is the closest point on the <see cref="startNode"/> to <see cref="originalStartPoint"/>
		/// </summary>
		public Vector3 startPoint;

		/// <summary>
		/// End point of the path.
		/// This is the closest point on the <see cref="endNode"/> to <see cref="originalEndPoint"/>
		/// </summary>
		public Vector3 endPoint;

		/// <summary>
		/// Total cost of this path as used by the pathfinding algorithm.
		///
		/// The cost is influenced by both the length of the path, as well as any tags or penalties on the nodes.
		/// By default, the cost to move 1 world unit is <see cref="Int3.Precision"/>.
		///
		/// If the path failed, the cost will be set to zero.
		///
		/// See: tags (view in online documentation for working links)
		/// </summary>
		public uint cost;

		/// <summary>
		/// Determines if a search for an end node should be done.
		/// Set by different path types.
		/// Since: Added in 3.0.8.3
		/// </summary>
		protected virtual bool hasEndPoint => true;

		/// <summary>
		/// True if this path type has a well defined end point, even before calculation starts.
		///
		/// This is for example true for the <see cref="ABPath"/> type, but false for the <see cref="RandomPath"/> type.
		/// </summary>
		public virtual bool endPointKnownBeforeCalculation => true;

		/// <summary>
		/// Calculate partial path if the target node cannot be reached.
		/// If the target node cannot be reached, the node which was closest (given by heuristic) will be chosen as target node
		/// and a partial path will be returned.
		/// This only works if a heuristic is used (which is the default).
		/// If a partial path is found, CompleteState is set to Partial.
		/// Note: It is not required by other path types to respect this setting
		///
		/// The <see cref="endNode"/> and <see cref="endPoint"/> will be modified and be set to the node which ends up being closest to the target.
		///
		/// Warning: Using this may make path calculations significantly slower if you have a big graph. The reason is that
		/// when the target node cannot be reached, the path must search through every single other node that it can reach in order
		/// to determine which one is closest. This may be expensive, and is why this option is disabled by default.
		/// </summary>
		public bool calculatePartial;

		/// <summary>
		/// Current best target for the partial path.
		/// This is the node with the lowest H score.
		/// </summary>
		protected uint partialBestTargetPathNodeIndex = GraphNode.InvalidNodeIndex;
		protected uint partialBestTargetHScore = uint.MaxValue;
		protected uint partialBestTargetGScore = uint.MaxValue;

		/// <summary>
		/// Optional ending condition for the path.
		///
		/// The ending condition determines when the path has been completed.
		/// Can be used to for example mark a path as complete when it is within a specific distance from the target.
		///
		/// If ending conditions are used that are not centered around the endpoint of the path,
		/// then you should also set the <see cref="heuristic"/> to None to ensure the path is still optimal.
		/// The performance impact of setting the <see cref="heuristic"/> to None is quite large, so you might want to try to run it with the default
		/// heuristic to see if the path is good enough for your use case anyway.
		///
		/// If null, no custom ending condition will be used. This means that the path will end when the target node has been reached.
		///
		/// Note: If the ending condition returns false for all nodes, the path will just keep searching until it has searched the whole graph. This can be slow.
		///
		/// See: <see cref="PathEndingCondition"/>
		/// </summary>
		public PathEndingCondition endingCondition;

		/// <summary>@{ @name Constructors</summary>

		/// <summary>
		/// Default constructor.
		/// Do not use this. Instead use the static Construct method which can handle path pooling.
		/// </summary>
		public ABPath () {}

		/// <summary>
		/// Construct a path with a start and end point.
		/// The delegate will be called when the path has been calculated.
		/// Do not confuse it with the Seeker callback as they are sent at different times.
		/// If you are using a Seeker to start the path you can set callback to null.
		///
		/// Returns: The constructed path object
		/// </summary>
		public static ABPath Construct (Vector3 start, Vector3 end, OnPathDelegate callback = null) {
			var p = PathPool.GetPath<ABPath>();

			p.Setup(start, end, callback);
			return p;
		}

		protected void Setup (Vector3 start, Vector3 end, OnPathDelegate callbackDelegate) {
			callback = callbackDelegate;
			UpdateStartEnd(start, end);
		}

		/// <summary>
		/// Creates a fake path.
		/// Creates a path that looks almost exactly like it would if the pathfinding system had calculated it.
		///
		/// This is useful if you want your agents to follow some known path that cannot be calculated using the pathfinding system for some reason.
		///
		/// <code>
		/// var path = ABPath.FakePath(new List<Vector3> { new Vector3(1, 2, 3), new Vector3(4, 5, 6) });
		///
		/// ai.SetPath(path);
		/// </code>
		///
		/// You can use it to combine existing paths like this:
		///
		/// <code>
		/// var a = Vector3.zero;
		/// var b = new Vector3(1, 2, 3);
		/// var c = new Vector3(2, 3, 4);
		/// var path1 = ABPath.Construct(a, b);
		/// var path2 = ABPath.Construct(b, c);
		///
		/// AstarPath.StartPath(path1);
		/// AstarPath.StartPath(path2);
		/// path1.BlockUntilCalculated();
		/// path2.BlockUntilCalculated();
		///
		/// // Combine the paths
		/// // Note: Skip the first element in the second path as that will likely be the last element in the first path
		/// var newVectorPath = path1.vectorPath.Concat(path2.vectorPath.Skip(1)).ToList();
		/// var newNodePath = path1.path.Concat(path2.path.Skip(1)).ToList();
		/// var combinedPath = ABPath.FakePath(newVectorPath, newNodePath);
		/// </code>
		/// </summary>
		public static ABPath FakePath (List<Vector3> vectorPath, List<GraphNode> nodePath = null) {
			var path = PathPool.GetPath<ABPath>();

			for (int i = 0; i < vectorPath.Count; i++) path.vectorPath.Add(vectorPath[i]);

			path.completeState = PathCompleteState.Complete;
			((IPathInternals)path).AdvanceState(PathState.Returned);

			if (vectorPath.Count > 0) {
				path.UpdateStartEnd(vectorPath[0], vectorPath[vectorPath.Count - 1]);
			}

			if (nodePath != null) {
				for (int i = 0; i < nodePath.Count; i++) path.path.Add(nodePath[i]);
			}

			return path;
		}

		/// <summary>@}</summary>

		/// <summary>
		/// Sets the start and end points.
		/// Sets <see cref="originalStartPoint"/>, <see cref="originalEndPoint"/>, <see cref="startPoint"/>, <see cref="endPoint"/>
		/// </summary>
		protected void UpdateStartEnd (Vector3 start, Vector3 end) {
			originalStartPoint = start;
			originalEndPoint = end;

			startPoint = start;
			endPoint = end;
		}

		/// <summary>
		/// Reset all values to their default values.
		/// All inheriting path types must implement this function, resetting ALL their variables to enable recycling of paths.
		/// Call this base function in inheriting types with base.Reset ();
		/// </summary>
		protected override void Reset () {
			base.Reset();

			originalStartPoint = Vector3.zero;
			originalEndPoint = Vector3.zero;
			startPoint = Vector3.zero;
			endPoint = Vector3.zero;
			calculatePartial = false;
			partialBestTargetPathNodeIndex = GraphNode.InvalidNodeIndex;
			partialBestTargetHScore = uint.MaxValue;
			partialBestTargetGScore = uint.MaxValue;
			cost = 0;
			endingCondition = null;
		}

#if !ASTAR_NO_GRID_GRAPH
		/// <summary>Cached <see cref="Pathfinding.NNConstraint.None"/> to reduce allocations</summary>
		static readonly NNConstraint NNConstraintNone = NNConstraint.None;

		/// <summary>
		/// Applies a special case for grid nodes.
		///
		/// Assume the closest walkable node is a grid node.
		/// We will now apply a special case only for grid graphs.
		/// In tile based games, an obstacle often occupies a whole
		/// node. When a path is requested to the position of an obstacle
		/// (single unwalkable node) the closest walkable node will be
		/// one of the 8 nodes surrounding that unwalkable node
		/// but that node is not neccessarily the one that is most
		/// optimal to walk to so in this special case
		/// we mark all nodes around the unwalkable node as targets
		/// and when we search and find any one of them we simply exit
		/// and set that first node we found to be the 'real' end node
		/// because that will be the optimal node (this does not apply
		/// in general unless the heuristic is set to None, but
		/// for a single unwalkable node it does).
		/// This also applies if the nearest node cannot be traversed for
		/// some other reason like restricted tags.
		///
		/// Returns: True if the workaround was applied. If this happens, new temporary endpoints will have been added
		///
		/// Image below shows paths when this special case is applied. The path goes from the white sphere to the orange box.
		/// [Open online documentation to see images]
		///
		/// Image below shows paths when this special case has been disabled
		/// [Open online documentation to see images]
		/// </summary>
		protected virtual bool EndPointGridGraphSpecialCase (GraphNode closestWalkableEndNode, Vector3 originalEndPoint, int targetIndex) {
			var gridNode = closestWalkableEndNode as GridNode;

			if (gridNode != null) {
				var gridGraph = GridNode.GetGridGraph(gridNode.GraphIndex);

				// Find the closest node, not neccessarily walkable
				var endNNInfo2 = gridGraph.GetNearest(originalEndPoint, NNConstraintNone);
				var gridNode2 = endNNInfo2.node as GridNode;

				if (gridNode != gridNode2 && gridNode2 != null) {
					// Calculate the coordinates of the nodes
					var x1 = gridNode.NodeInGridIndex % gridGraph.width;
					var z1 = gridNode.NodeInGridIndex / gridGraph.width;

					var x2 = gridNode2.NodeInGridIndex % gridGraph.width;
					var z2 = gridNode2.NodeInGridIndex / gridGraph.width;

					bool wasClose = false;
					switch (gridGraph.neighbours) {
					case NumNeighbours.Four:
						if ((x1 == x2 && System.Math.Abs(z1-z2) == 1) || (z1 == z2 && System.Math.Abs(x1-x2) == 1)) {
							// If 'O' is gridNode2, then gridNode is one of the nodes marked with an 'x'
							//    x
							//  x O x
							//    x
							wasClose = true;
						}
						break;
					case NumNeighbours.Eight:
						if (System.Math.Abs(x1-x2) <= 1 && System.Math.Abs(z1-z2) <= 1) {
							// If 'O' is gridNode2, then gridNode is one of the nodes marked with an 'x'
							//  x x x
							//  x O x
							//  x x x
							wasClose = true;
						}
						break;
					case NumNeighbours.Six:
						// Hexagon graph
						for (int i = 0; i < 6; i++) {
							var nx = x2 + GridGraph.neighbourXOffsets[GridGraph.hexagonNeighbourIndices[i]];
							var nz = z2 + GridGraph.neighbourZOffsets[GridGraph.hexagonNeighbourIndices[i]];
							if (x1 == nx && z1 == nz) {
								// If 'O' is gridNode2, then gridNode is one of the nodes marked with an 'x'
								//    x x
								//  x O x
								//  x x
								wasClose = true;
								break;
							}
						}
						break;
					default:
						// Should not happen unless NumNeighbours is modified in the future
						throw new System.Exception("Unhandled NumNeighbours");
					}

					if (wasClose) {
						// We now need to find all nodes marked with an x to be able to mark them as targets
						AddEndpointsForSurroundingGridNodes(gridNode2, originalEndPoint, targetIndex);

						// hTargetNode is used for heuristic optimizations
						// (also known as euclidean embedding).
						// Even though the endNode is not walkable
						// we can use it for better heuristics since
						// there is a workaround added (EuclideanEmbedding.ApplyGridGraphEndpointSpecialCase)
						// which is there to support this case.
						// TODO
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>Helper method to add endpoints around a specific unwalkable grid node</summary>
		void AddEndpointsForSurroundingGridNodes (GridNode gridNode, Vector3 desiredPoint, int targetIndex) {
			// Loop through all adjacent grid nodes
			var gridGraph = GridNode.GetGridGraph(gridNode.GraphIndex);

			// Number of neighbours as an int
			int mxnum = gridGraph.neighbours == NumNeighbours.Four ? 4 : (gridGraph.neighbours == NumNeighbours.Eight ? 8 : 6);

			// Calculate the coordinates of the node
			var x = gridNode.NodeInGridIndex % gridGraph.width;
			var z = gridNode.NodeInGridIndex / gridGraph.width;

			for (int i = 0; i < mxnum; i++) {
				int nx, nz;
				if (gridGraph.neighbours == NumNeighbours.Six) {
					// Hexagon graph
					nx = x + GridGraph.neighbourXOffsets[GridGraph.hexagonNeighbourIndices[i]];
					nz = z + GridGraph.neighbourZOffsets[GridGraph.hexagonNeighbourIndices[i]];
				} else {
					nx = x + GridGraph.neighbourXOffsets[i];
					nz = z + GridGraph.neighbourZOffsets[i];
				}

				var adjacentNode = gridGraph.GetNode(nx, nz);
				// Check if the position is still inside the grid
				if (adjacentNode != null) {
					pathHandler.AddTemporaryNode(new TemporaryNode {
						type = TemporaryNodeType.End,
						position = (Int3)adjacentNode.ClosestPointOnNode(desiredPoint),
						associatedNode = adjacentNode.NodeIndex,
						targetIndex = targetIndex,
					});
				}
			}
		}
#endif

		/// <summary>Prepares the path. Searches for start and end nodes and does some simple checking if a path is at all possible</summary>
		protected override void Prepare () {
			var startNNInfo = GetNearest(startPoint);

			//Tell the NNConstraint which node was found as the start node if it is a PathNNConstraint and not a normal NNConstraint
			if (nnConstraint is PathNNConstraint pathNNConstraint) {
				pathNNConstraint.SetStart(startNNInfo.node);
			}

			startPoint = startNNInfo.position;

			if (startNNInfo.node == null) {
				FailWithError("Couldn't find a node close to the start point");
				return;
			}

			if (!CanTraverse(startNNInfo.node)) {
				FailWithError("The node closest to the start point could not be traversed");
				return;
			}

			pathHandler.AddTemporaryNode(new TemporaryNode {
				associatedNode = startNNInfo.node.NodeIndex,
				position = (Int3)startNNInfo.position,
				type = TemporaryNodeType.Start,
			});

			// If it is declared that this path type has an end point
			// Some path types might want to use most of the ABPath code, but will not have an explicit end point at this stage
			uint endNodeIndex = 0;
			if (hasEndPoint) {
				var endNNInfo = GetNearest(originalEndPoint);
				endPoint = endNNInfo.position;

				if (endNNInfo.node == null) {
					FailWithError("Couldn't find a node close to the end point");
					return;
				}

				// This should not trigger unless the user has modified the NNConstraint
				if (!CanTraverse(endNNInfo.node)) {
					FailWithError("The node closest to the end point could not be traversed");
					return;
				}

				// This should not trigger unless the user has modified the NNConstraint
				if (startNNInfo.node.Area != endNNInfo.node.Area) {
					FailWithError("There is no valid path to the target");
					return;
				}

				endNodeIndex = endNNInfo.node.NodeIndex;

#if !ASTAR_NO_GRID_GRAPH
				// Potentially we want to special case grid graphs a bit
				// to better support some kinds of games
				if (!EndPointGridGraphSpecialCase(endNNInfo.node, originalEndPoint, 0))
#endif
				{
					pathHandler.AddTemporaryNode(new TemporaryNode {
						associatedNode = endNNInfo.node.NodeIndex,
						position = (Int3)endNNInfo.position,
						type = TemporaryNodeType.End,
					});
				}
			}

			TemporaryEndNodesBoundingBox(out var mnTarget, out var mxTarget);
			heuristicObjective = new HeuristicObjective(mnTarget, mxTarget, heuristic, heuristicScale, endNodeIndex, AstarPath.active.euclideanEmbedding);
			MarkNodesAdjacentToTemporaryEndNodes();
			AddStartNodesToHeap();
		}

		void CompletePartial () {
			CompleteState = PathCompleteState.Partial;
			// TODO: Add unit test for this
			endPoint = pathHandler.GetNode(partialBestTargetPathNodeIndex).ClosestPointOnNode(originalEndPoint);
			cost = partialBestTargetGScore;
			Trace(partialBestTargetPathNodeIndex);
		}

		protected override void OnHeapExhausted () {
			if (calculatePartial && partialBestTargetPathNodeIndex != GraphNode.InvalidNodeIndex) {
				CompletePartial();
			} else {
				FailWithError("Searched all reachable nodes, but could not find target. This can happen if you have nodes with a different tag blocking the way to the goal. You can enable path.calculatePartial to handle that case as a workaround (though this comes with a performance cost).");
			}
		}

		protected override void OnFoundEndNode (uint pathNode, uint hScore, uint gScore) {
			if (pathHandler.IsTemporaryNode(pathNode)) {
				// Common case, a temporary node is used to represent the target.
				// However, it may not be clamped to the closest point on the node.
				// In particular the grid graph special case will not clamp it.
				// So we clamp it here instead.
				var tempNode = pathHandler.GetTemporaryNode(pathNode);
				var associatedNode = pathHandler.GetNode(tempNode.associatedNode);
				if (endingCondition != null && !endingCondition.TargetFound(associatedNode, partialBestTargetHScore, gScore)) {
					// The ending condition is not fulfilled, so we should not stop here.
					// This is a weird situation where the ending condition doesn't consider the closest node to the destination
					// as a valid end node. It can be useful in rare cases, though.
					return;
				}
				endPoint = (Vector3)tempNode.position;
				endPoint = associatedNode.ClosestPointOnNode(endPoint);
			} else {
				// The target node is a normal node. We use the center of the node as the end point.
				// This can happen when using a custom ending condition.
				var node = pathHandler.GetNode(pathNode);
				endPoint = (Vector3)node.position;
			}
			cost = gScore;
			CompleteState = PathCompleteState.Complete;
			Trace(pathNode);
		}

		public override void OnVisitNode (uint pathNode, uint hScore, uint gScore) {
			// This method may be called multiple times without checking if the path is complete yet.
			if (CompleteState != PathCompleteState.NotCalculated) return;

			if (endingCondition != null) {
				var node = pathHandler.GetNode(pathNode);
				if (endingCondition.TargetFound(node, hScore, gScore)) {
					OnFoundEndNode(pathNode, hScore, gScore);
					if (CompleteState == PathCompleteState.Complete) {
						return;
					}
				}
			}

			if (hScore < partialBestTargetHScore) {
				partialBestTargetPathNodeIndex = pathNode;
				partialBestTargetHScore = hScore;
				partialBestTargetGScore = gScore;
			}
		}

		/// <summary>Returns a debug string for this path.</summary>
		protected override string DebugString (PathLog logMode) {
			if (logMode == PathLog.None || (!error && logMode == PathLog.OnlyErrors)) {
				return "";
			}

			var text = new System.Text.StringBuilder();

			DebugStringPrefix(logMode, text);
			if (!error) {
				text.Append(" Path Cost: ");
				text.Append(cost);
			}

			if (!error && logMode == PathLog.Heavy) {
				if (hasEndPoint && endNode != null) {
					// text.Append("\nEnd Node\n	G: ");
					// text.Append(nodeR.G);
					// text.Append("\n	H: ");
					// text.Append(nodeR.H);
					// text.Append("\n	F: ");
					// text.Append(nodeR.F);
					text.Append("\n	Point: ");
					text.Append(((Vector3)endPoint).ToString());
					text.Append("\n	Graph: ");
					text.Append(endNode.GraphIndex);
				}

				text.Append("\nStart Node");
				text.Append("\n	Point: ");
				text.Append(((Vector3)startPoint).ToString());
				text.Append("\n	Graph: ");
				if (startNode != null) text.Append(startNode.GraphIndex);
				else text.Append("< null startNode >");
			}

			DebugStringSuffix(logMode, text);

			return text.ToString();
		}
	}
}
