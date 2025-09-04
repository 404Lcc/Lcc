using System.Collections.Generic;
using Pathfinding.Pooling;

namespace Pathfinding {
	/// <summary>
	/// Contains useful functions for updating graphs.
	/// This class works a lot with the GraphNode class, a useful function to get nodes is <see cref="AstarPath.GetNearest"/>.
	///
	/// See: <see cref="AstarPath.GetNearest"/>
	/// See: <see cref="Pathfinding.PathUtilities"/>
	/// </summary>
	public static class GraphUpdateUtilities {
		/// <summary>
		/// Updates graphs and checks if all nodes are still reachable from each other.
		/// Graphs are updated, then a check is made to see if the nodes are still reachable from each other.
		/// If they are not, the graphs are reverted to before the update and false is returned.
		/// This is slower than a normal graph update.
		/// All queued graph updates and thread safe callbacks will be flushed during this function.
		///
		/// Returns: True if the given nodes are still reachable from each other after the guo has been applied. False otherwise.
		///
		/// <code>
		/// var graphUpdate = new GraphUpdateObject(tower.GetComponent<Collider>().bounds);
		/// var spawnPointNode = AstarPath.active.GetNearest(spawnPoint.position).node;
		/// var goalNode = AstarPath.active.GetNearest(goalPoint.position).node;
		///
		/// if (GraphUpdateUtilities.UpdateGraphsNoBlock(graphUpdate, spawnPointNode, goalNode, false)) {
		///     // Valid tower position
		///     // Since the last parameter (which is called "alwaysRevert") in the method call was false
		///     // The graph is now updated and the game can just continue
		/// } else {
		///     // Invalid tower position. It blocks the path between the spawn point and the goal
		///     // The effect on the graph has been reverted
		///     Destroy(tower);
		/// }
		/// </code>
		///
		/// Warning: This will not work for recast graphs if <see cref="GraphUpdateObject.updatePhysics"/> is enabled (the default).
		/// </summary>
		/// <param name="guo">The GraphUpdateObject to update the graphs with</param>
		/// <param name="node1">Node which should have a valid path to node2. All nodes should be walkable or false will be returned.</param>
		/// <param name="node2">Node which should have a valid path to node1. All nodes should be walkable or false will be returned.</param>
		/// <param name="alwaysRevert">If true, reverts the graphs to the old state even if no blocking occurred</param>
		public static bool UpdateGraphsNoBlock (GraphUpdateObject guo, GraphNode node1, GraphNode node2, bool alwaysRevert = false) {
			List<GraphNode> buffer = ListPool<GraphNode>.Claim();

			buffer.Add(node1);
			buffer.Add(node2);

			bool worked = UpdateGraphsNoBlock(guo, buffer, alwaysRevert);
			ListPool<GraphNode>.Release(ref buffer);
			return worked;
		}

		/// <summary>
		/// Updates graphs and checks if all nodes are still reachable from each other.
		/// Graphs are updated, then a check is made to see if the nodes are still reachable from each other.
		/// If they are not, the graphs are reverted to before the update and false is returned.
		/// This is slower than a normal graph update.
		/// All queued graph updates will be flushed during this function.
		///
		/// Returns: True if the given nodes are still reachable from each other after the guo has been applied. False otherwise.
		/// </summary>
		/// <param name="guo">The GraphUpdateObject to update the graphs with</param>
		/// <param name="nodes">Nodes which should have valid paths between them. All nodes should be walkable or false will be returned.</param>
		/// <param name="alwaysRevert">If true, reverts the graphs to the old state even if no blocking occurred</param>
		public static bool UpdateGraphsNoBlock (GraphUpdateObject guo, List<GraphNode> nodes, bool alwaysRevert = false) {
			bool worked;

			// Pause pathfinding while modifying the graphs
			var graphLock = AstarPath.active.PausePathfinding();

			try {
				// Make sure any pending graph updates have been done before we start
				AstarPath.active.FlushGraphUpdates();

				// Make sure all nodes are walkable
				for (int i = 0; i < nodes.Count; i++) if (!nodes[i].Walkable) return false;

				// Create a snapshot to allow us to revert the graphs to their original state
				var snapshot = AstarPath.active.Snapshot(guo.bounds, guo.nnConstraint.graphMask);

				AstarPath.active.UpdateGraphs(guo);

				// Update the graphs immediately
				AstarPath.active.FlushGraphUpdates();

				// Check if all nodes are in the same area and that they are walkable, i.e that there are paths between all of them
				worked = PathUtilities.IsPathPossible(nodes);

				// If it did not work, revert the GUO
				if (!worked || alwaysRevert) {
					AstarPath.active.AddWorkItem(snapshot.Restore);
					AstarPath.active.FlushWorkItems();
				}
				snapshot.Dispose();
			} finally {
				graphLock.Release();
			}

			return worked;
		}
	}
}
