using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pathfinding {
	using Pathfinding.Util;
	using Pathfinding.Drawing;

	/// <summary>
	/// Connects two nodes with a direct connection.
	/// It is not possible to detect this link when following a path (which may be good or bad), for that you can use NodeLink2.
	///
	/// See: editing-graphs (view in online documentation for working links)
	/// </summary>
	[AddComponentMenu("Pathfinding/Link")]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/nodelink.html")]
	public class NodeLink : GraphModifier {
		/// <summary>End position of the link</summary>
		public Transform end;

		/// <summary>
		/// The connection will be this times harder/slower to traverse.
		/// Note that values lower than one will not always make the pathfinder choose this path instead of another path even though this one should
		/// lead to a lower total cost unless you also adjust the Heuristic Scale in A* Inspector -> Settings -> Pathfinding or disable the heuristic altogether.
		/// </summary>
		public float costFactor = 1.0f;

		/// <summary>Make a one-way connection</summary>
		public bool oneWay = false;

		/// <summary>Delete existing connection instead of adding one</summary>
		public bool deleteConnection = false;

		public Transform Start {
			get { return transform; }
		}

		public Transform End {
			get { return end; }
		}

		public override void OnGraphsPostUpdateBeforeAreaRecalculation () {
			Apply();
		}

		public static void DrawArch (Vector3 a, Vector3 b, Vector3 up, Color color) {
			Vector3 dir = b - a;

			if (dir == Vector3.zero) return;

			var normal = Vector3.Cross(up, dir);
			var normalUp = Vector3.Cross(dir, normal).normalized * dir.magnitude * 0.1f;

			Draw.Bezier(a, a + normalUp, b + normalUp, b, color);
		}

		/// <summary>
		/// Connects the start and end points using a link or refreshes the existing link.
		///
		/// If you have moved the link or otherwise modified it you need to call this method.
		///
		/// Warning: This must only be done when it is safe to update the graph structure.
		/// The easiest is to do it inside a work item. See <see cref="AstarPath.AddWorkItem"/>.
		/// </summary>
		public virtual void Apply () {
			if (Start == null || End == null || AstarPath.active == null) return;

			GraphNode startNode = AstarPath.active.GetNearest(Start.position).node;
			GraphNode endNode = AstarPath.active.GetNearest(End.position).node;

			if (startNode == null || endNode == null) return;


			if (deleteConnection) {
				GraphNode.Disconnect(startNode, endNode);
			} else {
				uint cost = (uint)System.Math.Round((startNode.position-endNode.position).costMagnitude*costFactor);

				GraphNode.Connect(startNode, endNode, cost, oneWay ? OffMeshLinks.Directionality.OneWay : OffMeshLinks.Directionality.TwoWay);
			}
		}

		public override void DrawGizmos () {
			if (Start == null || End == null) return;

			NodeLink.DrawArch(Start.position, End.position, Vector3.up, deleteConnection ? Color.red : Color.green);
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem("Edit/Pathfinding/Link Pair %&l")]
		public static void LinkObjects () {
			Transform[] tfs = Selection.transforms;
			if (tfs.Length == 2) {
				LinkObjects(tfs[0], tfs[1], false);
			}
			SceneView.RepaintAll();
		}

		[UnityEditor.MenuItem("Edit/Pathfinding/Unlink Pair %&u")]
		public static void UnlinkObjects () {
			Transform[] tfs = Selection.transforms;
			if (tfs.Length == 2) {
				LinkObjects(tfs[0], tfs[1], true);
			}
			SceneView.RepaintAll();
		}

		[UnityEditor.MenuItem("Edit/Pathfinding/Delete Links on Selected %&b")]
		public static void DeleteLinks () {
			Transform[] tfs = Selection.transforms;
			for (int i = 0; i < tfs.Length; i++) {
				NodeLink[] conns = tfs[i].GetComponents<NodeLink>();
				for (int j = 0; j < conns.Length; j++) DestroyImmediate(conns[j]);
			}
			SceneView.RepaintAll();
		}

		public static void LinkObjects (Transform a, Transform b, bool removeConnection) {
			NodeLink connecting = null;

			NodeLink[] conns = a.GetComponents<NodeLink>();
			for (int i = 0; i < conns.Length; i++) {
				if (conns[i].end == b) {
					connecting = conns[i];
					break;
				}
			}

			conns = b.GetComponents<NodeLink>();
			for (int i = 0; i < conns.Length; i++) {
				if (conns[i].end == a) {
					connecting = conns[i];
					break;
				}
			}

			if (removeConnection) {
				if (connecting != null) DestroyImmediate(connecting);
			} else {
				if (connecting == null) {
					connecting = a.gameObject.AddComponent<NodeLink>();
					connecting.end = b;
				} else {
					connecting.deleteConnection = !connecting.deleteConnection;
				}
			}
		}
#endif
	}
}
