using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Pooling;
using Unity.Mathematics;

namespace Pathfinding {
	/// <summary>
	/// A path which searches from one point to a number of different targets in one search or from a number of different start points to a single target.
	///
	/// This is faster than searching with an ABPath for each target if pathsForAll is true.
	/// This path type can be used for example when you want an agent to find the closest target of a few different options.
	///
	/// When pathsForAll is true, it will calculate a path to each target point, but it can share a lot of calculations for the different paths so
	/// it is faster than requesting them separately.
	///
	/// When pathsForAll is false, it will perform a search using the heuristic set to None and stop as soon as it finds the first target.
	/// This may be faster or slower than requesting each path separately.
	/// It will run a Dijkstra search where it searches all nodes around the start point until the closest target is found.
	/// Note that this is usually faster if some target points are very close to the start point and some are very far away, but
	/// it can be slower if all target points are relatively far away because then it will have to search a much larger
	/// region since it will not use any heuristics.
	///
	/// See: Seeker.StartMultiTargetPath
	/// See: MultiTargetPathExample.cs (view in online documentation for working links) "Example of how to use multi-target-paths"
	///
	/// Version: Since 3.7.1 the vectorPath and path fields are always set to the shortest path even when pathsForAll is true.
	/// </summary>
	public class MultiTargetPath : ABPath {
		/// <summary>Callbacks to call for each individual path</summary>
		public OnPathDelegate[] callbacks;

		/// <summary>Nearest nodes to the <see cref="targetPoints"/></summary>
		public GraphNode[] targetNodes;

		/// <summary>Number of target nodes left to find</summary>
		protected int targetNodeCount;

		/// <summary>Indicates if the target has been found. Also true if the target cannot be reached (is in another area)</summary>
		public bool[] targetsFound;

		/// <summary>The cost of the calculated path for each target. Will be 0 if a path was not found.</summary>
		public uint[] targetPathCosts;

		/// <summary>Target points specified when creating the path. These are snapped to the nearest nodes</summary>
		public Vector3[] targetPoints;

		/// <summary>Target points specified when creating the path. These are not snapped to the nearest nodes</summary>
		public Vector3[] originalTargetPoints;

		/// <summary>Stores all vector paths to the targets. Elements are null if no path was found</summary>
		public List<Vector3>[] vectorPaths;

		/// <summary>Stores all paths to the targets. Elements are null if no path was found</summary>
		public List<GraphNode>[] nodePaths;

		/// <summary>If true, a path to all targets will be returned, otherwise just the one to the closest one.</summary>
		public bool pathsForAll = true;

		/// <summary>The closest target index (if any target was found)</summary>
		public int chosenTarget = -1;

		/// <summary>False if the path goes from one point to multiple targets. True if it goes from multiple start points to one target point</summary>
		public bool inverted { get; protected set; }

		public override bool endPointKnownBeforeCalculation => false;

		/// <summary>
		/// Default constructor.
		/// Do not use this. Instead use the static Construct method which can handle path pooling.
		/// </summary>
		public MultiTargetPath () {}

		public static MultiTargetPath Construct (Vector3[] startPoints, Vector3 target, OnPathDelegate[] callbackDelegates, OnPathDelegate callback = null) {
			MultiTargetPath p = Construct(target, startPoints, callbackDelegates, callback);

			p.inverted = true;
			return p;
		}

		public static MultiTargetPath Construct (Vector3 start, Vector3[] targets, OnPathDelegate[] callbackDelegates, OnPathDelegate callback = null) {
			var p = PathPool.GetPath<MultiTargetPath>();

			p.Setup(start, targets, callbackDelegates, callback);
			return p;
		}

		protected void Setup (Vector3 start, Vector3[] targets, OnPathDelegate[] callbackDelegates, OnPathDelegate callback) {
			inverted = false;
			this.callback = callback;
			callbacks = callbackDelegates;
			if (callbacks != null && callbacks.Length != targets.Length) throw new System.ArgumentException("The targets array must have the same length as the callbackDelegates array");
			targetPoints = targets;

			originalStartPoint = start;

			startPoint = start;

			if (targets.Length == 0) {
				FailWithError("No targets were assigned to the MultiTargetPath");
				return;
			}

			endPoint = targets[0];

			originalTargetPoints = new Vector3[targetPoints.Length];
			for (int i = 0; i < targetPoints.Length; i++) {
				originalTargetPoints[i] = targetPoints[i];
			}
		}

		protected override void Reset () {
			base.Reset();
			pathsForAll = true;
			chosenTarget = -1;
			inverted = true;
		}

		protected override void OnEnterPool () {
			if (vectorPaths != null)
				for (int i = 0; i < vectorPaths.Length; i++)
					if (vectorPaths[i] != null) ListPool<Vector3>.Release(vectorPaths[i]);

			vectorPaths = null;
			vectorPath = null;

			if (nodePaths != null)
				for (int i = 0; i < nodePaths.Length; i++)
					if (nodePaths[i] != null) ListPool<GraphNode>.Release(nodePaths[i]);

			nodePaths = null;
			path = null;
			callbacks = null;
			targetNodes = null;
			targetsFound = null;
			targetPathCosts = null;
			targetPoints = null;
			originalTargetPoints = null;

			base.OnEnterPool();
		}

		/// <summary>Set chosenTarget to the index of the shortest path</summary>
		void ChooseShortestPath () {
			// When pathsForAll is false there will be at most one non-null path
			chosenTarget = -1;
			if (nodePaths != null) {
				uint bestG = uint.MaxValue;
				for (int i = 0; i < nodePaths.Length; i++) {
					if (nodePaths[i] != null) {
						var g = targetPathCosts[i];
						if (g < bestG) {
							chosenTarget = i;
							bestG = g;
						}
					}
				}
			}
		}

		void SetPathParametersForReturn (int target) {
			path = nodePaths[target];
			vectorPath = vectorPaths[target];

			if (inverted) {
				startPoint = targetPoints[target];
				originalStartPoint = originalTargetPoints[target];
			} else {
				endPoint = targetPoints[target];
				originalEndPoint = originalTargetPoints[target];
			}
			cost = path != null ? targetPathCosts[target] : 0;
		}

		protected override void ReturnPath () {
			if (error) {
				// Call all callbacks
				if (callbacks != null) {
					for (int i = 0; i < callbacks.Length; i++)
						if (callbacks[i] != null) callbacks[i] (this);
				}

				if (callback != null) callback(this);

				return;
			}

			bool anySucceded = false;

			// Set the end point to the start point
			// since the path is reversed
			// (the start point will be set individually for each path)
			if (inverted) {
				endPoint = startPoint;
				originalEndPoint = originalStartPoint;
			}

			for (int i = 0; i < nodePaths.Length; i++) {
				if (nodePaths[i] != null) {
					// Note that we use the lowercase 'completeState' here.
					// The property (CompleteState) will ensure that the complete state is never
					// changed away from the error state but in this case we don't want that behaviour.
					completeState = PathCompleteState.Complete;
					anySucceded = true;
				} else {
					completeState = PathCompleteState.Error;
				}

				if (callbacks != null && callbacks[i] != null) {
					SetPathParametersForReturn(i);
					callbacks[i] (this);

					// In case a modifier changed the vectorPath, update the array of all vectorPaths
					vectorPaths[i] = vectorPath;
				}
			}

			if (anySucceded) {
				completeState = PathCompleteState.Complete;
				SetPathParametersForReturn(chosenTarget);
			} else {
				completeState = PathCompleteState.Error;
			}

			if (callback != null) {
				callback(this);
			}
		}

		protected void RebuildOpenList () {
			BinaryHeap heap = pathHandler.heap;
			if (heap.tieBreaking != BinaryHeap.TieBreaking.HScore) return;

			for (int i = 0; i < heap.numberOfItems; i++) {
				var pathNodeIndex = heap.GetPathNodeIndex(i);
				Int3 pos;
				if (pathHandler.IsTemporaryNode(pathNodeIndex)) {
					pos = pathHandler.GetTemporaryNode(pathNodeIndex).position;
				} else {
					pos = pathHandler.GetNode(pathNodeIndex).DecodeVariantPosition(pathNodeIndex, pathHandler.pathNodes[pathNodeIndex].fractionAlongEdge);
				}
				// Note: node index can be 0 here because the multi target path never uses the euclidean embedding
				var hScore = (uint)heuristicObjective.Calculate((int3)pos, 0);
				heap.SetH(i, hScore);
			}

			pathHandler.heap.Rebuild(pathHandler.pathNodes);
		}

		protected override void Prepare () {
			var startNNInfo = GetNearest(startPoint);
			var startNode = startNNInfo.node;

			if (endingCondition != null) {
				FailWithError("Multi target paths cannot use custom ending conditions");
				return;
			}

			if (startNode == null) {
				FailWithError("Could not find start node for multi target path");
				return;
			}

			if (!CanTraverse(startNode)) {
				FailWithError("The node closest to the start point could not be traversed");
				return;
			}

			// Tell the NNConstraint which node was found as the start node if it is a PathNNConstraint and not a normal NNConstraint
			if (nnConstraint is PathNNConstraint pathNNConstraint) {
				pathNNConstraint.SetStart(startNNInfo.node);
			}

			pathHandler.AddTemporaryNode(new TemporaryNode {
				associatedNode = startNNInfo.node.NodeIndex,
				position = (Int3)startNNInfo.position,
				type = TemporaryNodeType.Start,
			});

			vectorPaths = new List<Vector3>[targetPoints.Length];
			nodePaths = new List<GraphNode>[targetPoints.Length];
			targetNodes = new GraphNode[targetPoints.Length];
			targetsFound = new bool[targetPoints.Length];
			targetPathCosts = new uint[targetPoints.Length];
			targetNodeCount = 0;

			bool anyWalkable = false;
			bool anySameArea = false;
			bool anyNotNull = false;

			for (int i = 0; i < targetPoints.Length; i++) {
				var originalTarget = targetPoints[i];
				var endNNInfo = GetNearest(originalTarget);

				targetNodes[i] = endNNInfo.node;

				targetPoints[i] = endNNInfo.position;
				if (targetNodes[i] != null) {
					anyNotNull = true;
				}

				bool notReachable = false;

				if (endNNInfo.node != null && CanTraverse(endNNInfo.node)) {
					anyWalkable = true;
				} else {
					notReachable = true;
				}

				if (endNNInfo.node != null && endNNInfo.node.Area == startNode.Area) {
					anySameArea = true;
				} else {
					notReachable = true;
				}

				if (notReachable) {
					// Signal that the pathfinder should not look for this node because we have already found it
					targetsFound[i] = true;
				} else {
					targetNodeCount++;
#if !ASTAR_NO_GRID_GRAPH
					// Potentially we want to special case grid graphs a bit
					// to better support some kinds of games
					if (!EndPointGridGraphSpecialCase(endNNInfo.node, originalTarget, i))
#endif
					{
						pathHandler.AddTemporaryNode(new TemporaryNode {
							associatedNode = endNNInfo.node.NodeIndex,
							position = (Int3)endNNInfo.position,
							targetIndex = i,
							type = TemporaryNodeType.End,
						});
					}
				}
			}

			startPoint = startNNInfo.position;

			if (!anyNotNull) {
				FailWithError("Couldn't find a valid node close to the any of the end points");
				return;
			}

			if (!anyWalkable) {
				FailWithError("No target nodes could be traversed");
				return;
			}

			if (!anySameArea) {
				FailWithError("There's no valid path to any of the given targets");
				return;
			}

			MarkNodesAdjacentToTemporaryEndNodes();
			AddStartNodesToHeap();
			RecalculateHTarget();
		}

		void RecalculateHTarget () {
			if (pathsForAll) {
				// Sequentially go through all targets.
				// First we will find the path to the first target (or at least aim for it, we might find another one along the way),
				// then we will change the heuristic objective to the second target and so on.
				// This does not guarantee that we find the targets in order of closest to furthest,
				// but that is not required since we want to find all paths anyway.
				var target = FirstTemporaryEndNode();
				heuristicObjective = new HeuristicObjective(target, target, heuristic, heuristicScale, 0, null);
			} else {
				// Create a bounding box that contains all the end points,
				// and use that to calculate the heuristic.
				// This will ensure we find the closest target first.
				TemporaryEndNodesBoundingBox(out var mnTarget, out var mxTarget);
				heuristicObjective = new HeuristicObjective(mnTarget, mxTarget, heuristic, heuristicScale, 0, null);
			}

			// Rebuild the open list since all the H scores have changed
			RebuildOpenList();
		}

		protected override void Cleanup () {
			// Make sure that the shortest path is set
			// after the path has been calculated
			ChooseShortestPath();
			base.Cleanup();
		}

		protected override void OnHeapExhausted () {
			CompleteState = PathCompleteState.Complete;
		}

		protected override void OnFoundEndNode (uint pathNode, uint hScore, uint gScore) {
			if (!pathHandler.IsTemporaryNode(pathNode)) {
				FailWithError("Expected the end node to be a temporary node. Cannot determine which path it belongs to. This could happen if you are using a custom ending condition for the path.");
				return;
			}

			var targetIndex = pathHandler.GetTemporaryNode(pathNode).targetIndex;
			if (targetsFound[targetIndex]) throw new System.ArgumentException("This target has already been found");

			Trace(pathNode);
			vectorPaths[targetIndex] = vectorPath;
			nodePaths[targetIndex] = path;
			vectorPath = ListPool<Vector3>.Claim();
			path = ListPool<GraphNode>.Claim();

			targetsFound[targetIndex] = true;
			targetPathCosts[targetIndex] = gScore;

			targetNodeCount--;

			// Mark all end nodes for this target as ignored to avoid including them
			// in the H-score calculation and to avoid calling OnFoundEndNode for this
			// target index again.
			for (uint i = 0; i < pathHandler.numTemporaryNodes; i++) {
				var nodeIndex = pathHandler.temporaryNodeStartIndex + i;
				ref var node = ref pathHandler.GetTemporaryNode(nodeIndex);
				if (node.type == TemporaryNodeType.End && node.targetIndex == targetIndex) {
					node.type = TemporaryNodeType.Ignore;
				}
			}

			// When we find the first target, we can stop because it will be the closest one.
			if (!pathsForAll) {
				CompleteState = PathCompleteState.Complete;
				targetNodeCount = 0;
				return;
			}


			// If there are no more targets to find, return here and avoid calculating a new hTarget
			if (targetNodeCount <= 0) {
				CompleteState = PathCompleteState.Complete;
				return;
			}

			RecalculateHTarget();
		}

		protected override void Trace (uint pathNodeIndex) {
			base.Trace(pathNodeIndex, !inverted);
		}

		protected override string DebugString (PathLog logMode) {
			if (logMode == PathLog.None || (!error && logMode == PathLog.OnlyErrors)) {
				return "";
			}

			System.Text.StringBuilder text = pathHandler.DebugStringBuilder;
			text.Length = 0;

			DebugStringPrefix(logMode, text);

			if (!error) {
				text.Append("\nShortest path was ");
				text.Append(chosenTarget == -1 ? "undefined" : nodePaths[chosenTarget].Count.ToString());
				text.Append(" nodes long");

				if (logMode == PathLog.Heavy) {
					text.Append("\nPaths (").Append(targetsFound.Length).Append("):");
					for (int i = 0; i < targetsFound.Length; i++) {
						text.Append("\n\n	Path ").Append(i).Append(" Found: ").Append(targetsFound[i]);

						if (nodePaths[i] != null) {
							text.Append("\n		Length: ");
							text.Append(nodePaths[i].Count);
						}
					}

					text.Append("\nStart Node");
					text.Append("\n	Point: ");
					text.Append(((Vector3)endPoint).ToString());
					text.Append("\n	Graph: ");
					text.Append(startNode.GraphIndex);
					text.Append("\nBinary Heap size at completion: ");
					text.AppendLine((pathHandler.heap.numberOfItems-2).ToString());  // -2 because numberOfItems includes the next item to be added and item zero is not used
				}
			}

			DebugStringSuffix(logMode, text);

			return text.ToString();
		}
	}
}
