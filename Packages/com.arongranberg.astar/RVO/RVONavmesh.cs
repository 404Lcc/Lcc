using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace Pathfinding.RVO {
	using Pathfinding.Util;

	/// <summary>
	/// Adds a navmesh as RVO obstacles.
	/// Add this to a scene in which has a navmesh or grid based graph, when scanning (or loading from cache) the graph
	/// it will be added as RVO obstacles to the RVOSimulator (which must exist in the scene).
	///
	/// Warning: You should only have a single instance of this script in the scene, otherwise it will add duplicate
	/// obstacles and thereby increasing the CPU usage.
	///
	/// If you update a graph during runtime the obstacles need to be recalculated which has a performance penalty.
	/// This can be quite significant for larger graphs.
	///
	/// In the screenshot the generated obstacles are visible in red.
	/// [Open online documentation to see images]
	///
	/// Deprecated: This component is deprecated. Local avoidance colliders never worked particularly well and the performance was poor. Modify the graphs instead so that pathfinding takes obstacles into account.
	/// </summary>
	[AddComponentMenu("")]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rvonavmesh.html")]
	public class RVONavmesh : GraphModifier {
		/// <summary>
		/// Height of the walls added for each obstacle edge.
		/// If a graph contains overlapping regions (e.g multiple floor in a building)
		/// you should set this low enough so that edges on different levels do not interfere,
		/// but high enough so that agents cannot move over them by mistake.
		/// </summary>
		public float wallHeight = 5;
	}
}
