using UnityEngine;
using Pathfinding.Pooling;

namespace Pathfinding {
	/// <summary>
	/// Restrict suitable nodes by if they have been searched by a FloodPath.
	///
	/// Suitable nodes are in addition to the basic contraints, only the nodes which return true on a FloodPath.HasPathTo (node) call.
	/// See: Pathfinding.FloodPath
	/// See: Pathfinding.FloodPathTracer
	/// </summary>
	public class FloodPathConstraint : NNConstraint {
		readonly FloodPath path;

		public FloodPathConstraint (FloodPath path) {
			if (path == null) { Debug.LogWarning("FloodPathConstraint should not be used with a NULL path"); }
			this.path = path;
		}

		public override bool Suitable (GraphNode node) {
			return base.Suitable(node) && path.HasPathTo(node);
		}
	}

	/// <summary>
	/// Traces a path created with the Pathfinding.FloodPath.
	///
	/// See Pathfinding.FloodPath for examples on how to use this path type
	///
	/// [Open online documentation to see images]
	/// </summary>
	public class FloodPathTracer : ABPath {
		/// <summary>Reference to the FloodPath which searched the path originally</summary>
		protected FloodPath flood;

		protected override bool hasEndPoint => false;

		/// <summary>
		/// Default constructor.
		/// Do not use this. Instead use the static Construct method which can handle path pooling.
		/// </summary>
		public FloodPathTracer () {}

		public static FloodPathTracer Construct (Vector3 start, FloodPath flood, OnPathDelegate callback = null) {
			var p = PathPool.GetPath<FloodPathTracer>();

			p.Setup(start, flood, callback);
			return p;
		}

		protected void Setup (Vector3 start, FloodPath flood, OnPathDelegate callback) {
			this.flood = flood;

			if (flood == null || flood.PipelineState < PathState.Returning) {
				throw new System.ArgumentException("You must supply a calculated FloodPath to the 'flood' argument");
			}

			base.Setup(start, flood.originalStartPoint, callback);
			nnConstraint = new FloodPathConstraint(flood);
		}

		protected override void Reset () {
			base.Reset();
			flood = null;
		}

		/// <summary>
		/// Initializes the path.
		/// Traces the path from the start node.
		/// </summary>
		protected override void Prepare () {
			if (!this.flood.IsValid(pathHandler.nodeStorage)) {
				FailWithError("The flood path is invalid because nodes have been destroyed since it was calculated. Please recalculate the flood path.");
				return;
			}

			base.Prepare();

			if (CompleteState == PathCompleteState.NotCalculated) {
				for (uint i = 0; i < pathHandler.numTemporaryNodes; i++) {
					var nodeIndex = pathHandler.temporaryNodeStartIndex + i;
					ref var tempNode = ref pathHandler.GetTemporaryNode(nodeIndex);
					if (tempNode.type == TemporaryNodeType.Start) {
						var node = pathHandler.GetNode(tempNode.associatedNode);

						// This is guaranteed by the FloodPathConstraint
						bool found = false;
						for (uint k = 0; k < node.PathNodeVariants; k++) {
							if (flood.GetParent(node.NodeIndex + k) != 0) {
								found = true;
								CompleteState = PathCompleteState.Complete;
								Trace(node.NodeIndex + k);
								break;
							}
						}
						if (!found) {
							FailWithError("The flood path did not contain any information about the end node. Have you modified the path's nnConstraint to an instance which does not subclass FloodPathConstraint?");
						}
						return;
					}
				}

				FailWithError("Could not find a valid start node");
			}
		}

		protected override void CalculateStep (long targetTick) {
			if (CompleteState != PathCompleteState.Complete) throw new System.Exception("Something went wrong. At this point the path should be completed");
		}

		/// <summary>
		/// Traces the calculated path from the start node to the end.
		/// This will build an array (<see cref="path)"/> of the nodes this path will pass through and also set the <see cref="vectorPath"/> array to the <see cref="path"/> arrays positions.
		/// This implementation will use the <see cref="flood"/> (FloodPath) to trace the path from precalculated data.
		/// </summary>
		protected override void Trace (uint fromPathNodeIndex) {
			uint pathNodeIndex = fromPathNodeIndex;
			int count = 0;
			GraphNode lastNode = null;

			while (pathNodeIndex != 0) {
				if ((pathNodeIndex & FloodPath.TemporaryNodeBit) != 0) {
					// Skip over temporary nodes
					pathNodeIndex = flood.GetParent(pathNodeIndex & ~FloodPath.TemporaryNodeBit);
				} else {
					var node = pathHandler.GetNode(pathNodeIndex);
					if (node == null) {
						FailWithError("A node in the path has been destroyed. The FloodPath needs to be recalculated before you can use a FloodPathTracer.");
						return;
					}

					// If a node has multiple variants (like the triangle mesh node), then we may visit
					// the same node multiple times in a sequence (but different variants of it).
					// In the final path we don't want the duplicates.
					if (node != lastNode) {
						if (!CanTraverse(node)) {
							FailWithError("A node in the path is no longer walkable. The FloodPath needs to be recalculated before you can use a FloodPathTracer.");
							return;
						}
						path.Add(node);
						lastNode = node;
						vectorPath.Add((Vector3)node.position);
					}
					var next = flood.GetParent(pathNodeIndex);
					if (next == pathNodeIndex) {
						break;
					}
					pathNodeIndex = next;
				}

				count++;
				if (count > 10000) {
					Debug.LogWarning("Infinite loop? >10000 node path. Remove this message if you really have that long paths");
					break;
				}
			}
		}
	}
}
