using UnityEngine;
namespace Pathfinding {
	/// <summary>
	/// Provides additional traversal information to a path request.
	///
	/// Example implementation:
	/// <code>
	/// public class MyCustomTraversalProvider : ITraversalProvider {
	///     public bool CanTraverse (Path path, GraphNode node) {
	///         // Make sure that the node is walkable and that the 'enabledTags' bitmask
	///         // includes the node's tag.
	///         return node.Walkable && (path.enabledTags >> (int)node.Tag & 0x1) != 0;
	///         // alternatively:
	///         // return DefaultITraversalProvider.CanTraverse(path, node);
	///     }
	///
	///     /** [CanTraverseDefault] */
	///     public bool CanTraverse (Path path, GraphNode from, GraphNode to) {
	///         return CanTraverse(path, to);
	///     }
	///     /** [CanTraverseDefault] */
	///
	///     public uint GetTraversalCost (Path path, GraphNode node) {
	///         // The traversal cost is the sum of the penalty of the node's tag and the node's penalty
	///         return path.GetTagPenalty((int)node.Tag) + node.Penalty;
	///         // alternatively:
	///         // return DefaultITraversalProvider.GetTraversalCost(path, node);
	///     }
	///
	///     // This can be omitted in Unity 2021.3 and newer because a default implementation (returning true) can be used there.
	///     public bool filterDiagonalGridConnections {
	///         get {
	///             return true;
	///         }
	///     }
	/// }
	/// </code>
	///
	/// See: traversal_provider (view in online documentation for working links)
	/// </summary>
	public interface ITraversalProvider {
		/// <summary>
		/// Filter diagonal connections using <see cref="GridGraph.cutCorners"/> for effects applied by this ITraversalProvider.
		/// This includes tags and other effects that this ITraversalProvider controls.
		///
		/// This only has an effect if <see cref="GridGraph.cutCorners"/> is set to false and your grid has <see cref="GridGraph.neighbours"/> set to Eight.
		///
		/// Take this example, the grid is completely walkable, but an ITraversalProvider is used to make the nodes marked with '#'
		/// as unwalkable. The agent 'S' is in the middle.
		///
		/// <code>
		/// ..........
		/// ....#.....
		/// ...<see cref="S"/>#....
		/// ....#.....
		/// ..........
		/// </code>
		///
		/// If filterDiagonalGridConnections is false the agent will be free to use the diagonal connections to move away from that spot.
		/// However, if filterDiagonalGridConnections is true (the default) then the diagonal connections will be disabled and the agent will be stuck.
		///
		/// Typically, there are a few common use cases:
		/// - If your ITraversalProvider makes walls and obstacles and you want it to behave identically to obstacles included in the original grid graph scan, then this should be true.
		/// - If your ITraversalProvider is used for agent to agent avoidance and you want them to be able to move around each other more freely, then this should be false.
		///
		/// See: <see cref="GridNode"/>
		/// </summary>
		bool filterDiagonalGridConnections => true;

		/// <summary>True if node should be able to be traversed by the path.</summary>
		bool CanTraverse(Path path, GraphNode node) => DefaultITraversalProvider.CanTraverse(path, node);

		/// <summary>
		/// True if the path can traverse a link between from and to and if to can be traversed itself.
		/// If this method returns true then a call to CanTraverse(path,to) must also return true.
		/// Thus this method is a more flexible version of <see cref="CanTraverse(Path,GraphNode)"/>.
		///
		/// If you only need the functionality for <see cref="CanTraverse(Path,GraphNode)"/> then you may implement this method by just forwarding it to <see cref="CanTraverse(Path,GraphNode)"/>
		///
		/// <code>
		/// public bool CanTraverse (Path path, GraphNode from, GraphNode to) {
		///     return CanTraverse(path, to);
		/// }
		/// </code>
		/// </summary>
		bool CanTraverse(Path path, GraphNode from, GraphNode to) => CanTraverse(path, to);

		/// <summary>
		/// Cost of traversing a given node.
		/// Should return the additional cost for traversing this node. By default if no tags or penalties
		/// are used then the traversal cost is zero. A cost of 1000 corresponds roughly to the cost of moving 1 world unit.
		/// </summary>
		uint GetTraversalCost(Path path, GraphNode node) => DefaultITraversalProvider.GetTraversalCost(path, node);
	}

	/// <summary>Convenience class to access the default implementation of the ITraversalProvider</summary>
	public static class DefaultITraversalProvider {
		public static bool CanTraverse (Path path, GraphNode node) {
			return node.Walkable && (path == null || (path.enabledTags >> (int)node.Tag & 0x1) != 0);
		}

		public static uint GetTraversalCost (Path path, GraphNode node) {
			return node.Penalty + (path != null ? path.GetTagPenalty((int)node.Tag) : 0);
		}
	}
}
