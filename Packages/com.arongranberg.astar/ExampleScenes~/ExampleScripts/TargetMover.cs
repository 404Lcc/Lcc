#pragma warning disable 649
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Util;

namespace Pathfinding {
	/// <summary>
	/// Moves the target in example scenes.
	/// This is a simple script which has the sole purpose
	/// of moving the target point of agents in the example
	/// scenes for the A* Pathfinding Project.
	///
	/// It is not meant to be pretty, but it does the job.
	/// </summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/targetmover.html")]
	public class TargetMover : VersionedMonoBehaviour {
		/// <summary>Mask for the raycast placement</summary>
		public LayerMask mask;

		public Transform target;

		/// <summary>Determines if the target position should be updated every frame or only on double-click</summary>
		bool onlyOnDoubleClick;
		public Trigger trigger;
		public GameObject clickEffect;
		public bool use2D;
		public PathUtilities.FormationMode formationMode = PathUtilities.FormationMode.SinglePoint;

		Camera cam;

		public enum Trigger {
			Continuously,
			SingleClick,
			DoubleClick
		}

		public void Start () {
			// Cache the Main Camera
			cam = Camera.main;
			useGUILayout = false;
		}

		public void OnGUI () {
			if (trigger != Trigger.Continuously && cam != null && Event.current.type == EventType.MouseDown) {
				if (Event.current.clickCount == (trigger == Trigger.DoubleClick ? 2 : 1)) {
					UpdateTargetPosition();
				}
			}
		}

		/// <summary>Update is called once per frame</summary>
		void Update () {
			if (trigger == Trigger.Continuously && cam != null) {
				UpdateTargetPosition();
			}
		}

		public void UpdateTargetPosition () {
			Vector3 newPosition = Vector3.zero;
			bool positionFound = false;
			Transform hitObject = null;

			// If the game view has never been rendered, the mouse position can be infinite
			if (!float.IsFinite(Input.mousePosition.x)) return;

			if (use2D) {
				newPosition = cam.ScreenToWorldPoint(Input.mousePosition);
				newPosition.z = 0;
				positionFound = true;
				var collider = Physics2D.OverlapPoint(newPosition, mask);
				if (collider != null) hitObject = collider.transform;
			} else {
				// Fire a ray through the scene at the mouse position and place the target where it hits
				if (cam.pixelRect.Contains(Input.mousePosition) && Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity, mask)) {
					newPosition = hit.point;
					hitObject = hit.transform;
					positionFound = true;
				}
			}

			if (positionFound) {
				if (target != null) target.position = newPosition;

				if (trigger != Trigger.Continuously) {
					// Slightly inefficient way of finding all AIs, but this is just an example script, so it doesn't matter much.
					// FindObjectsByType does not support interfaces unfortunately.
					var ais = UnityCompatibility.FindObjectsByTypeSorted<MonoBehaviour>().OfType<IAstarAI>().ToList();
					StopAllCoroutines();

					if (hitObject != null && hitObject.TryGetComponent<Pathfinding.Examples.Interactable>(out var interactable)) {
						// Pick the first AI to interact with the interactable
						if (ais.Count > 0) interactable.Interact(ais[0]);
					} else {
						if (clickEffect != null) {
							GameObject.Instantiate(clickEffect, newPosition, Quaternion.identity);
						}

						// This will calculate individual destinations for each agent, like in a formation pattern.
						// The simplest mode, FormationMode.SinglePoint, just assigns newPosition to all entries of the 'destinations' list.
						var destinations = PathUtilities.FormationDestinations(ais, newPosition, formationMode, 0.5f);
						for (int i = 0; i < ais.Count; i++) {
#if MODULE_ENTITIES
							var isFollowerEntity = ais[i] is FollowerEntity;
#else
							var isFollowerEntity = false;
#endif
							if (ais[i] != null) {
								ais[i].destination = destinations[i];

								// Make the agents recalculate their path immediately for slighly increased responsiveness.
								// The FollowerEntity is better at doing this automatically.
								if (!isFollowerEntity) ais[i].SearchPath();
							}
						}

						StartCoroutine(OptimizeFormationDestinations(ais, destinations));
					}
				}
			}
		}

		/// <summary>
		/// Swap the destinations of pairs of agents if it reduces the total distance they need to travel.
		///
		/// This is a simple optimization algorithm to make group movement smoother and more efficient.
		/// It is not perfect and may not always find the optimal solution, but it is very fast and works well in practice.
		/// It will not work great for large groups of agents, as the optimization becomes too hard for this simple algorithm.
		///
		/// See: https://en.wikipedia.org/wiki/Assignment_problem
		/// </summary>
		IEnumerator OptimizeFormationDestinations (List<IAstarAI> ais, List<Vector3> destinations) {
			// Prevent swapping the same agents multiple times.
			// This is because the distance measurement is only an approximation, and agents
			// may temporarily have to move away from their destination before they can move towards it.
			// Allowing multiple swaps could make the agents move back and forth indefinitely as the targets shift around.
			var alreadySwapped = new HashSet<(IAstarAI, IAstarAI)>();

			const int IterationsPerFrame = 4;

			while (true) {
				for (int i = 0; i < IterationsPerFrame; i++) {
					var a = Random.Range(0, ais.Count);
					var b = Random.Range(0, ais.Count);
					if (a == b) continue;
					if (b < a) Memory.Swap(ref a, ref b);
					var aiA = ais[a];
					var aiB = ais[b];

					if ((MonoBehaviour)aiA == null) continue;
					if ((MonoBehaviour)aiB == null) continue;

					if (alreadySwapped.Contains((aiA, aiB))) continue;

					var pA = aiA.position;
					var pB = aiB.position;
					var distA = (pA - destinations[a]).sqrMagnitude;
					var distB = (pB - destinations[b]).sqrMagnitude;

					var newDistA = (pA - destinations[b]).sqrMagnitude;
					var newDistB = (pB - destinations[a]).sqrMagnitude;
					var cost1 = distA + distB;
					var cost2 = newDistA + newDistB;
					if (cost2 < cost1 * 0.98f) {
						// Swap the destinations
						var tmp = destinations[a];
						destinations[a] = destinations[b];
						destinations[b] = tmp;

						aiA.destination = destinations[a];
						aiB.destination = destinations[b];

						alreadySwapped.Add((aiA, aiB));
					}
				}
				yield return null;
			}
		}

		protected override void OnUpgradeSerializedData (ref Serialization.Migrations migrations, bool unityThread) {
			if (migrations.TryMigrateFromLegacyFormat(out var legacyVersion)) {
				if (legacyVersion < 2) {
					trigger = onlyOnDoubleClick ? Trigger.DoubleClick : Trigger.Continuously;
				}
			}
			base.OnUpgradeSerializedData(ref migrations, unityThread);
		}
	}
}
