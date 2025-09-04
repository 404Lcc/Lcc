using UnityEngine;

namespace Pathfinding.Graphs.Navmesh {
	using Pathfinding.Drawing;
	using Pathfinding.Util;

	/// <summary>
	/// Pruning of recast navmesh regions.
	/// A RelevantGraphSurface component placed in the scene specifies that
	/// the navmesh region it is inside should be included in the navmesh.
	///
	/// See: Pathfinding.RecastGraph.relevantGraphSurfaceMode
	/// </summary>
	[AddComponentMenu("Pathfinding/Navmesh/RelevantGraphSurface")]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/relevantgraphsurface.html")]
	public class RelevantGraphSurface : VersionedMonoBehaviour {
		private static RelevantGraphSurface root;

		public float maxRange = 1;

		private RelevantGraphSurface prev;
		private RelevantGraphSurface next;
		private Vector3 position;

		public Vector3 Position {
			get { return position; }
		}

		public RelevantGraphSurface Next {
			get { return next; }
		}

		public RelevantGraphSurface Prev {
			get { return prev; }
		}

		public static RelevantGraphSurface Root {
			get { return root; }
		}

		public void UpdatePosition () {
			position = transform.position;
		}

		void OnEnable () {
			UpdatePosition();
			if (root == null) {
				root = this;
			} else {
				next = root;
				root.prev = this;
				root = this;
			}
		}

		void OnDisable () {
			if (root == this) {
				root = next;
				if (root != null) root.prev = null;
			} else {
				if (prev != null) prev.next = next;
				if (next != null) next.prev = prev;
			}
			prev = null;
			next = null;
		}

		/// <summary>
		/// Updates the positions of all relevant graph surface components.
		/// Required to be able to use the position property reliably.
		/// </summary>
		public static void UpdateAllPositions () {
			RelevantGraphSurface c = root;

			while (c != null) { c.UpdatePosition(); c = c.Next; }
		}

		public static void FindAllGraphSurfaces () {
			var srf = UnityCompatibility.FindObjectsByTypeUnsorted<RelevantGraphSurface>();

			for (int i = 0; i < srf.Length; i++) {
				srf[i].OnDisable();
				srf[i].OnEnable();
			}
		}

		public override void DrawGizmos () {
			var color = new Color(57/255f, 211/255f, 46/255f);

			if (!GizmoContext.InActiveSelection(this)) color.a *= 0.4f;
			Draw.Line(transform.position - Vector3.up*maxRange, transform.position + Vector3.up*maxRange, color);
		}
	}
}
