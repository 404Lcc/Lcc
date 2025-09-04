using UnityEngine;
using System.Collections;
using Pathfinding;

namespace Pathfinding.Examples {
	/// <summary>
	/// Helper editor script to snap an object to the closest node.
	/// Used in the "Turn Based" example scene for snapping obstacles to the hexagon grid.
	/// </summary>
	[ExecuteInEditMode]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/snaptonode.html")]
	public class SnapToNode : MonoBehaviour {
		/// <summary>[Update]</summary>
		void Update () {
			if (transform.hasChanged && AstarPath.active != null) {
				var node = AstarPath.active.GetNearest(transform.position, NNConstraint.None).node;
				if (node != null) {
					transform.position = (Vector3)node.position;
					transform.hasChanged = false;
				}
			}
		}
		/// <summary>[Update]</summary>
	}
}
