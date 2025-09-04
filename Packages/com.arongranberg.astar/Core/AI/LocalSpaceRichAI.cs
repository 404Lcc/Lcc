using UnityEngine;
namespace Pathfinding.Examples {
	using Pathfinding.Util;

	/// <summary>
	/// RichAI for local space (pathfinding on moving graphs).
	///
	/// What this script does is that it fakes graph movement.
	/// It can be seen in the example scene called 'Moving' where
	/// a character is pathfinding on top of a moving ship.
	/// The graph does not actually move in that example
	/// instead there is some 'cheating' going on.
	///
	/// When requesting a path, we first transform
	/// the start and end positions of the path request
	/// into local space for the object we are moving on
	/// (e.g the ship in the example scene), then when we get the
	/// path back, they will still be in these local coordinates.
	/// When following the path, we will every frame transform
	/// the coordinates of the waypoints in the path to global
	/// coordinates so that we can follow them.
	///
	/// At the start of the game (when the graph is scanned) the
	/// object we are moving on should be at a valid position on the graph and
	/// you should attach the <see cref="Pathfinding.LocalSpaceGraph"/> component to it. The <see cref="Pathfinding.LocalSpaceGraph"/>
	/// component will store the position and orientation of the object right there are the start
	/// and then we can use that information to transform coordinates back to that region of the graph
	/// as if the object had not moved at all.
	///
	/// This functionality is only implemented for the RichAI
	/// script, however it should not be hard to
	/// use the same approach for other movement scripts.
	/// </summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/localspacerichai.html")]
	public class LocalSpaceRichAI : RichAI {
		/// <summary>Root of the object we are moving on</summary>
		public LocalSpaceGraph graph;

		protected override Vector3 ClampPositionToGraph (Vector3 newPosition) {
			RefreshTransform();
			// Clamp the new position to the navmesh
			// First we need to transform the position to the same space that the graph is in though.
			var nearest = AstarPath.active != null? AstarPath.active.GetNearest(graph.transformation.InverseTransform(newPosition)) : new NNInfo();
			float elevation;

			movementPlane.ToPlane(newPosition, out elevation);
			return movementPlane.ToWorld(movementPlane.ToPlane(nearest.node != null ? graph.transformation.Transform(nearest.position) : newPosition), elevation);
		}

		void RefreshTransform () {
			graph.Refresh();
			richPath.transform = graph.transformation;
			movementPlane = graph.transformation.ToSimpleMovementPlane();
		}

		protected override void Start () {
			RefreshTransform();
			base.Start();
		}

		protected override void CalculatePathRequestEndpoints (out Vector3 start, out Vector3 end) {
			RefreshTransform();
			base.CalculatePathRequestEndpoints(out start, out end);
			start = graph.transformation.InverseTransform(start);
			end = graph.transformation.InverseTransform(end);
		}

		protected override void OnUpdate (float dt) {
			RefreshTransform();
			base.OnUpdate(dt);
		}
	}
}
