using UnityEngine;
using System.Collections;

namespace Pathfinding.Examples {
	using Pathfinding.RVO;

	/// <summary>
	/// Player controlled character which RVO agents will avoid.
	/// This script is intended to show how you can make NPCs avoid
	/// a player controlled (or otherwise externally controlled) character.
	///
	/// See: Pathfinding.RVO.RVOController
	/// </summary>
	[RequireComponent(typeof(RVOController))]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/manualrvoagent.html")]
	public class ManualRVOAgent : MonoBehaviour {
		RVOController rvo;

		public float speed = 1;

		void Awake () {
			rvo = GetComponent<RVOController>();
		}

		/// <summary>[ManualRVOVelocity]</summary>
		void Update () {
			var x = Input.GetAxis("Horizontal");
			var y = Input.GetAxis("Vertical");

			var v = new Vector3(x, 0, y) * speed;

			// Override the RVOController's velocity. This will disable local avoidance calculations for one simulation step.
			rvo.velocity = v;
			transform.position += v * Time.deltaTime;
		}
		/// <summary>[ManualRVOVelocity]</summary>
	}
}
