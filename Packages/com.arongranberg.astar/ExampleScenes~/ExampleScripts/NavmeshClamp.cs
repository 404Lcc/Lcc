using UnityEngine;

namespace Pathfinding {
	/// <summary>
	/// Attach to any GameObject and the object will be clamped to the navmesh.
	/// If a GameObject has this component attached, one or more graph linecasts will be carried out every frame to ensure that the object
	/// does not leave the navmesh area.
	///
	/// It can be used with grid graphs, layered grid graphs, navmesh graphs and recast graphs.
	/// </summary>
	[AddComponentMenu("Pathfinding/Navmesh Clamp")]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/navmeshclamp.html")]
	public class NavmeshClamp : MonoBehaviour {
		GraphNode prevNode;
		Vector3 prevPos;

		// Update is called once per frame
		void LateUpdate () {
			if (prevNode == null || prevNode.Destroyed) {
				var nninfo = AstarPath.active.GetNearest(transform.position);
				prevNode = nninfo.node;
				prevPos = transform.position;
			}

			if (prevNode == null) {
				return;
			}

			if (prevNode != null) {
				var graph = AstarData.GetGraph(prevNode) as IRaycastableGraph;
				if (graph != null) {
					GraphHitInfo hit;
					if (graph.Linecast(prevPos, transform.position, out hit) && hit.node != null) {
						hit.point.y = transform.position.y;
						Vector3 closest = VectorMath.ClosestPointOnLine(hit.tangentOrigin, hit.tangentOrigin+hit.tangent, transform.position);
						Vector3 ohit = hit.point;
						ohit = ohit + Vector3.ClampMagnitude((Vector3)hit.node.position-ohit, 0.008f);
						if (graph.Linecast(ohit, closest, out hit)) {
							hit.point.y = transform.position.y;
							transform.position = hit.point;
						} else {
							closest.y = transform.position.y;

							transform.position = closest;
						}
					}
					prevNode = hit.node;
				}
			}

			prevPos = transform.position;
		}
	}
}
