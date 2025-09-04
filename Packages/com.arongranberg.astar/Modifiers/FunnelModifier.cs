using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Pooling;

namespace Pathfinding {
	[AddComponentMenu("Pathfinding/Modifiers/Funnel Modifier")]
	[System.Serializable]
	/// <summary>
	/// Simplifies paths on navmesh graphs using the funnel algorithm.
	/// The funnel algorithm is an algorithm which can, given a path corridor with nodes in the path where the nodes have an area, like triangles, it can find the shortest path inside it.
	/// This makes paths on navmeshes look much cleaner and smoother.
	/// [Open online documentation to see images]
	///
	/// The funnel modifier also works on grid graphs using a different algorithm, but which yields visually similar results.
	/// See: <see cref="GridStringPulling"/>
	///
	/// Note: The <see cref="Pathfinding.RichAI"/> movement script has its own internal funnel modifier.
	/// You do not need to attach this component if you are using the RichAI movement script.
	///
	/// See: http://digestingduck.blogspot.se/2010/03/simple-stupid-funnel-algorithm.html
	/// </summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/funnelmodifier.html")]
	public class FunnelModifier : MonoModifier {
		/// <summary>
		/// Determines if funnel simplification is used.
		/// When using the low quality setting only the funnel algorithm is used
		/// but when the high quality setting an additional step is done to simplify the path even more.
		///
		/// On tiled recast/navmesh graphs, but sometimes on normal ones as well, it can be good to simplify
		/// the funnel as a post-processing step to make the paths straighter.
		///
		/// This has a moderate performance impact during frames when a path calculation is completed.
		/// This is why it is disabled by default. For any units that you want high
		/// quality movement for you should enable it.
		///
		/// [Open online documentation to see images]
		///
		/// See: <see cref="Funnel.Simplify"/>
		///
		/// Note: This is only used for recast/navmesh graphs. Not for grid graphs.
		/// </summary>
		public FunnelQuality quality = FunnelQuality.Medium;

		/// <summary>
		/// Insert a vertex every time the path crosses a portal instead of only at the corners of the path.
		/// The resulting path will have exactly one vertex per portal if this is enabled.
		/// This may introduce vertices with the same position in the output (esp. in corners where many portals meet).
		/// [Open online documentation to see images]
		///
		/// Note: This is only used for recast/navmesh graphs. Not for grid graphs.
		/// </summary>
		public bool splitAtEveryPortal;

		/// <summary>
		/// When using a grid graph, take penalties, tag penalties and <see cref="ITraversalProvider"/> penalties into account.
		/// Enabling this is quite slow. It can easily make the modifier take twice the amount of time to run.
		/// So unless you are using penalties/tags/ITraversalProvider penalties that you need to take into account when simplifying
		/// the path, you should leave this disabled.
		/// </summary>
		public bool accountForGridPenalties = false;

		public enum FunnelQuality {
			Medium,
			High,
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem("CONTEXT/Seeker/Add Funnel Modifier")]
		public static void AddComp (UnityEditor.MenuCommand command) {
			(command.context as Component).gameObject.AddComponent(typeof(FunnelModifier));
		}
#endif

		public override int Order { get { return 10; } }

		public override void Apply (Path p) {
			if (p.path == null || p.path.Count == 0 || p.vectorPath == null || p.vectorPath.Count == 0) {
				return;
			}

			List<Vector3> funnelPath = ListPool<Vector3>.Claim();

			// Split the path into different parts (separated by custom links)
			// and run the funnel algorithm on each of them in turn
			var parts = Funnel.SplitIntoParts(p);

			if (parts.Count == 0) {
				// As a really special case, it might happen that the path contained only a single node
				// and that node was part of a custom link (e.g added by the NodeLink2 component).
				// In that case the SplitIntoParts method will not know what to do with it because it is
				// neither a link (as only 1 of the 2 nodes of the link was part of the path) nor a normal
				// path part. So it will skip it. This will cause it to return an empty list.
				// In that case we want to simply keep the original path, which is just a single point.
				return;
			}

			if (quality == FunnelQuality.High) {
				System.Func<GraphNode, bool>  filter = p.CanTraverse;
				Funnel.Simplify(parts, ref p.path, filter);
			}

			for (int i = 0; i < parts.Count; i++) {
				var part = parts[i];
				if (part.type == Funnel.PartType.NodeSequence) {
					// If this is a grid graph (and not a hexagonal graph) then we can use a special
					// string pulling algorithm for grid graphs which works a lot better.
					if (p.path[part.startIndex].Graph is GridGraph gg && gg.neighbours != NumNeighbours.Six) {
						// TODO: Avoid dynamic allocations
						System.Func<GraphNode, uint>  traversalCost = null;
						if (accountForGridPenalties) {
							traversalCost = p.GetTraversalCost;
						}
						System.Func<GraphNode, bool>  filter = p.CanTraverse;
						var result = GridStringPulling.Calculate(p.path, part.startIndex, part.endIndex, part.startPoint, part.endPoint, traversalCost, filter, int.MaxValue);
						funnelPath.AddRange(result);
						ListPool<Vector3>.Release(ref result);
					} else {
						var portals = Funnel.ConstructFunnelPortals(p.path, part);
						var result = Funnel.Calculate(portals, splitAtEveryPortal);
						funnelPath.AddRange(result);
						ListPool<Vector3>.Release(ref portals.left);
						ListPool<Vector3>.Release(ref portals.right);
						ListPool<Vector3>.Release(ref result);
					}
				} else {
					// non-link parts will add the start/end points for the adjacent parts.
					// So if there is no non-link part before this one, then we need to add the start point of the link
					// and if there is no non-link part after this one, then we need to add the end point.
					if (i == 0 || parts[i-1].type == Funnel.PartType.OffMeshLink) {
						funnelPath.Add(part.startPoint);
					}
					if (i == parts.Count - 1 || parts[i+1].type == Funnel.PartType.OffMeshLink) {
						funnelPath.Add(part.endPoint);
					}
				}
			}

			UnityEngine.Assertions.Assert.IsTrue(funnelPath.Count >= 1);
			ListPool<Funnel.PathPart>.Release(ref parts);
			// Pool the previous vectorPath
			ListPool<Vector3>.Release(ref p.vectorPath);
			p.vectorPath = funnelPath;
		}
	}
}
