using UnityEngine;
using System.Collections;

namespace Pathfinding {
	/// <summary>
	/// Simple patrol behavior.
	/// This will set the destination on the agent so that it moves through the sequence of objects in the <see cref="targets"/> array.
	/// Upon reaching a target it will wait for <see cref="delay"/> seconds.
	///
	/// [Open online documentation to see videos]
	///
	/// See: <see cref="Pathfinding.AIDestinationSetter"/>
	/// See: <see cref="Pathfinding.AIPath"/>
	/// See: <see cref="Pathfinding.RichAI"/>
	/// See: <see cref="Pathfinding.AILerp"/>
	/// </summary>
	[UniqueComponent(tag = "ai.destination")]
	[AddComponentMenu("Pathfinding/AI/Behaviors/Patrol")]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/patrol.html")]
	public class Patrol : VersionedMonoBehaviour {
		/// <summary>Target points to move to in order</summary>
		public Transform[] targets;

		/// <summary>Time in seconds to wait at each target</summary>
		public float delay = 0;

		/// <summary>
		/// If true, the agent's destination will be updated every frame instead of only when switching targets.
		///
		/// This is good if you have moving targets, but is otherwise unnecessary and slightly slower.
		/// </summary>
		public bool updateDestinationEveryFrame = false;

		/// <summary>Current target index</summary>
		int index = -1;

		IAstarAI agent;
		float switchTime = float.NegativeInfinity;

		protected override void Awake () {
			base.Awake();
			agent = GetComponent<IAstarAI>();
		}

		/// <summary>Update is called once per frame</summary>
		void Update () {
			if (targets.Length == 0) return;

			// Note: using reachedEndOfPath and pathPending instead of reachedDestination here because
			// if the destination cannot be reached by the agent, we don't want it to get stuck, we just want it to get as close as possible and then move on.
			if (agent.reachedEndOfPath && !agent.pathPending && float.IsPositiveInfinity(switchTime)) {
				switchTime = Time.time + delay;
			}

			if (Time.time >= switchTime) {
				index++;
				switchTime = float.PositiveInfinity;

				index = index % targets.Length;
				agent.destination = targets[index].position;
				agent.SearchPath();
			} else if (updateDestinationEveryFrame) {
				index = index % targets.Length;
				agent.destination = targets[index].position;
			}
		}
	}
}
