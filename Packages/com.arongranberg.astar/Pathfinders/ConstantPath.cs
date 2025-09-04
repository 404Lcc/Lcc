
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Pathfinding.Pooling;

namespace Pathfinding {
	/// <summary>
	/// Finds all nodes within a specified distance from the start.
	/// This class will search outwards from the start point and find all nodes which it costs less than <see cref="EndingConditionDistance.maxGScore"/> to reach, this is usually the same as the distance to them multiplied with 1000.
	///
	/// The path can be called like:
	/// <code>
	/// // Here you create a new path and set how far it should search.
	/// ConstantPath cpath = ConstantPath.Construct(transform.position, 20000, null);
	/// AstarPath.StartPath(cpath);
	///
	/// // Block until the path has been calculated. You can also calculate it asynchronously
	/// // by providing a callback in the constructor above.
	/// cpath.BlockUntilCalculated();
	///
	/// // Draw a line upwards from all nodes within range
	/// for (int i = 0; i < cpath.allNodes.Count; i++) {
	///     Debug.DrawRay((Vector3)cpath.allNodes[i].position, Vector3.up, Color.red, 2f);
	/// }
	/// </code>
	///
	/// When the path has been calculated, all nodes it searched will be stored in the variable <see cref="ConstantPath.allNodes"/> (remember that you need to cast it from Path to ConstantPath first to get the variable).
	///
	/// This list will be sorted by the cost to reach that node (more specifically the G score if you are familiar with the terminology for search algorithms).
	/// [Open online documentation to see images]
	/// </summary>
	public class ConstantPath : Path {
		public GraphNode startNode;
		public Vector3 startPoint;
		public Vector3 originalStartPoint;

		/// <summary>
		/// Contains all nodes the path found.
		/// This list will be sorted by G score (cost/distance to reach the node).
		/// </summary>
		public List<GraphNode> allNodes;

		/// <summary>
		/// Determines when the path calculation should stop.
		/// This is set up automatically in the constructor to an instance of the Pathfinding.EndingConditionDistance class with a maxGScore is specified in the constructor.
		///
		/// See: Pathfinding.PathEndingCondition for examples
		/// </summary>
		public PathEndingCondition endingCondition;

		/// <summary>
		/// Constructs a ConstantPath starting from the specified point.
		///
		/// Searching will be stopped when a node has a G score (cost to reach it) greater or equal to maxGScore
		/// in order words it will search all nodes with a cost to get there less than maxGScore.
		/// </summary>
		/// <param name="start">From where the path will be started from (the closest node to that point will be used).</param>
		/// <param name="maxGScore">Searching will be stopped when a node has a G score (cost to reach the node) greater than this.</param>
		/// <param name="callback">Will be called when the path has been calculated, leave as null if you use a Seeker component.</param>
		public static ConstantPath Construct (Vector3 start, int maxGScore, OnPathDelegate callback = null) {
			var p = PathPool.GetPath<ConstantPath>();

			p.Setup(start, maxGScore, callback);
			return p;
		}

		/// <summary>Sets up a ConstantPath starting from the specified point</summary>
		protected void Setup (Vector3 start, int maxGScore, OnPathDelegate callback) {
			this.callback = callback;
			startPoint = start;
			originalStartPoint = startPoint;

			endingCondition = new EndingConditionDistance(this, maxGScore);
		}

		protected override void OnEnterPool () {
			base.OnEnterPool();
			if (allNodes != null) Pooling.ListPool<GraphNode>.Release(ref allNodes);
		}

		/// <summary>
		/// Reset the path to default values.
		/// Clears the <see cref="allNodes"/> list.
		/// Note: This does not reset the <see cref="endingCondition"/>.
		///
		/// Also sets <see cref="heuristic"/> to Heuristic.None as it is the default value for this path type
		/// </summary>
		protected override void Reset () {
			base.Reset();
			allNodes = Pooling.ListPool<GraphNode>.Claim();
			endingCondition = null;
			originalStartPoint = Vector3.zero;
			startPoint = Vector3.zero;
			startNode = null;
			heuristic = Heuristic.None;
		}

		protected override void Prepare () {
			var startNNInfo = GetNearest(startPoint);

			startNode = startNNInfo.node;
			if (startNode == null) {
				FailWithError("Could not find close node to the start point");
				return;
			}

			pathHandler.AddTemporaryNode(new TemporaryNode {
				type = TemporaryNodeType.Start,
				position = (Int3)startNNInfo.position,
				associatedNode = startNode.NodeIndex,
			});
			heuristicObjective = new HeuristicObjective(int3.zero, Heuristic.None, 0.0f);
			AddStartNodesToHeap();
		}

		protected override void OnHeapExhausted () {
			CompleteState = PathCompleteState.Complete;
		}

		protected override void OnFoundEndNode (uint pathNode, uint hScore, uint gScore) {
			throw new System.InvalidOperationException("ConstantPaths do not have any end nodes");
		}

		public override void OnVisitNode (uint pathNode, uint hScore, uint gScore) {
			var node = pathHandler.GetNode(pathNode);
			if (endingCondition.TargetFound(node, hScore, gScore)) {
				CompleteState = PathCompleteState.Complete;
			} else {
				allNodes.Add(node);
			}
		}
	}

	/// <summary>
	/// Target is found when the path is longer than a specified value.
	/// Actually this is defined as when the current node's G score is >= a specified amount (EndingConditionDistance.maxGScore).
	/// The G score is the cost from the start node to the current node, so an area with a higher penalty (weight) will add more to the G score.
	/// However the G score is usually just the shortest distance from the start to the current node.
	///
	/// See: Pathfinding.ConstantPath which uses this ending condition
	/// </summary>
	public class EndingConditionDistance : PathEndingCondition {
		/// <summary>Max G score a node may have</summary>
		public int maxGScore = 100;

		public EndingConditionDistance (Path p, int maxGScore) : base(p) {
			this.maxGScore = maxGScore;
		}

		public override bool TargetFound (GraphNode node, uint H, uint G) {
			return (int)G >= maxGScore;
		}
	}
}
