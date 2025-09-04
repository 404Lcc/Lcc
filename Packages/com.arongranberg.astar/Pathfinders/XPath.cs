using UnityEngine;

namespace Pathfinding {
	/// <summary>
	/// Extended Path.
	///
	/// This is the same as a standard path but it is possible to customize when the target should be considered reached.
	/// Can be used to for example signal a path as complete when it is within a specific distance from the target.
	///
	/// Note: More customizations does make it slower to calculate than an ABPath but not by very much.
	///
	/// See: Pathfinding.PathEndingCondition
	///
	/// Deprecated: Use an <see cref="ABPath"/> with the <see cref="ABPath.endingCondition"/> field instead.
	/// </summary>
	[System.Obsolete("Use an ABPath with the ABPath.endingCondition field instead")]
	public class XPath : ABPath {
		[System.Obsolete("Use ABPath.Construct instead")]
		public new static ABPath Construct (Vector3 start, Vector3 end, OnPathDelegate callback = null) {
			return ABPath.Construct(start, end, callback);
		}
	}

	/// <summary>
	/// Customized ending condition for a path.
	///
	/// If you want to create a path which needs a more complex ending condition than just reaching the end node, you can use this class.
	/// Inherit from this class and override the <see cref="TargetFound"/> function to implement you own ending condition logic.
	///
	/// For example, you might want to create an Ending Condition which stop searching when a node is close enough to a given point.
	/// Then what you do is that you create your own class, let's call it MyEndingCondition and override the function TargetFound to specify our own logic.
	/// We want to inherit from ABPathEndingCondition because only ABPaths have end points defined.
	///
	/// <code>
	/// public class MyEndingCondition : ABPathEndingCondition {
	///     // Maximum world distance to the target node before terminating the path
	///     public float maxDistance = 10;
	///
	///     // Reuse the constructor in the superclass
	///     public MyEndingCondition (ABPath p) : base(p) {}
	///
	///     public override bool TargetFound (GraphNode node, uint H, uint G) {
	///         return ((Vector3)node.position - abPath.originalEndPoint).sqrMagnitude <= maxDistance*maxDistance;
	///     }
	/// }
	/// </code>
	///
	/// The TargetReached method in the code above checks if the node that the path is currently searching is close enough to the target point for us to consider it a valid target node.
	/// If true is returned, the path will immediately terminate and return the path to that point.
	///
	/// To use a custom endition condition, you have to instantiate your class and then assign it to <see cref="ABPath.endingCondition"/> field.
	///
	/// <code>
	/// ABPath myPath = ABPath.Construct(startPoint, endPoint);
	/// var ec = new MyEndingCondition(myPath);
	/// ec.maxDistance = 100; // Or some other value
	/// myPath.endingCondition = ec;
	///
	/// // Calculate the path!
	/// seeker.StartPath(myPath);
	/// </code>
	///
	/// Where seeker is a <see cref="Seeker"/> component.
	///
	/// If ending conditions are used that are not centered around the endpoint of the path,
	/// then the heuristic (<see cref="AstarPath.heuristic"/>) must be set to None to guarantee that the path is still optimal.
	/// However, the performance impact of setting the heuristic to None is quite large, so you might want to try to run it with the default
	/// heuristic to see if the path is good enough for your use case anyway.
	///
	/// See: <see cref="ABPath"/>
	/// See: <see cref="ConstantPath"/>
	/// </summary>
	public abstract class PathEndingCondition {
		/// <summary>Path which this ending condition is used on</summary>
		protected Path path;

		protected PathEndingCondition () {}

		public PathEndingCondition (Path p) {
			if (p == null) throw new System.ArgumentNullException("p");
			this.path = p;
		}

		/// <summary>Has the ending condition been fulfilled.</summary>
		/// <param name="node">The current node.</param>
		/// <param name="H">Heuristic score. See Pathfinding.PathNode.H</param>
		/// <param name="G">Cost to reach this node. See Pathfinding.PathNode.G</param>
		public abstract bool TargetFound(GraphNode node, uint H, uint G);
	}

	/// <summary>Ending condition which emulates the default one for the ABPath</summary>
	public class ABPathEndingCondition : PathEndingCondition {
		/// <summary>
		/// Path which this ending condition is used on.
		/// Same as <see cref="path"/> but downcasted to ABPath
		/// </summary>
		protected ABPath abPath;

		public ABPathEndingCondition (ABPath p) {
			if (p == null) throw new System.ArgumentNullException("p");
			abPath = p;
			path = p;
		}

		/// <summary>
		/// Has the ending condition been fulfilled.
		///
		/// This is per default the same as asking if node == p.endNode
		/// </summary>
		/// <param name="node">The current node.</param>
		/// <param name="H">Heuristic score. See Pathfinding.PathNode.H</param>
		/// <param name="G">Cost to reach this node. See Pathfinding.PathNode.G</param>
		public override bool TargetFound (GraphNode node, uint H, uint G) {
			return node == abPath.endNode;
		}
	}

	/// <summary>Ending condition which stops a fixed distance from the target point</summary>
	public class EndingConditionProximity : ABPathEndingCondition {
		/// <summary>Maximum world distance to the target node before terminating the path</summary>
		public float maxDistance = 10;

		public EndingConditionProximity (ABPath p, float maxDistance) : base(p) {
			this.maxDistance = maxDistance;
		}

		public override bool TargetFound (GraphNode node, uint H, uint G) {
			return ((Vector3)node.position - abPath.originalEndPoint).sqrMagnitude <= maxDistance*maxDistance;
		}
	}
}
