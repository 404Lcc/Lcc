using UnityEngine;

namespace Pathfinding.Examples {
	/// <summary>Helper script in the example scene 'Turn Based'</summary>
	[RequireComponent(typeof(Animator))]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/hexagontrigger.html")]
	public class HexagonTrigger : MonoBehaviour {
		Animator anim;
		bool visible;

		void Awake () {
			anim = GetComponent<Animator>();
		}

		/// <summary>[OnTriggerEnter]</summary>
		void OnTriggerEnter (Collider coll) {
			var unit = coll.GetComponentInParent<TurnBasedAI>();
			var node = AstarPath.active.GetNearest(transform.position).node;

			// Check if it is an agent and the agent is headed for this node
			if (unit != null && unit.targetNode == node) {
				visible = true;
				anim.CrossFade("show", 0.1f);
			}
		}
		/// <summary>[OnTriggerEnter]</summary>

		void OnTriggerExit (Collider coll) {
			if (coll.GetComponentInParent<TurnBasedAI>() != null && visible) {
				anim.CrossFade("hide", 0.1f);
			}
		}
	}
}
