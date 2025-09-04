using UnityEngine;
using System.Collections;
using System.Linq;
using Pathfinding;
using Pathfinding.Examples.RTS;

namespace Pathfinding.Examples.RTS {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rtsharvester.html")]
	public class RTSHarvester : MonoBehaviour {
		RTSUnit unit;
		Animator animator;

		void Awake () {
			unit = GetComponent<RTSUnit>();
			animator = GetComponent<Animator>();

			ctx = new BTContext {
				animator = animator,
				transform = transform,
				unit = unit
			};
		}

		BTNode behave;
		BTContext ctx;
		// Use this for initialization
		void Start () {
			StartCoroutine(StateMachine());
			behave = Behaviors.HarvestBehavior();
		}

		RTSHarvestableResource FindFreeResource () {
			/*var resources = FindObjectsOfType<RTSHarvestableResource>().Where(c => c.reservedBy == null).ToArray();
			RTSHarvestableResource closest = null;
			var dist = float.PositiveInfinity;
			var point = transform.position;
			for (int i = 0; i < resources.Length; i++) {
			    var d = (resources[i].transform.position - point).sqrMagnitude;
			    if (d < dist) {
			        dist = d;
			        closest = resources[i];
			    }
			}
			return closest;*/
			return null;
		}

		void OnDestroy () {
			behave.Terminate(ctx);
		}

		IEnumerator StateMachine () {
			yield break;
		}

		// Update is called once per frame
		void Update () {
			behave.Tick(ctx);
		}
	}
}
