using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding.Examples {
	/// <summary>
	/// Animation helper specifically made for the spider robot in the example scenes.
	/// The spider robot (or mine-bot) which has been copied from the Unity Example Project
	/// can have this script attached to be able to pathfind around with animations working properly.
	///
	/// This script should be attached to a parent GameObject however since the original bot has Z+ as up.
	/// This component requires Z+ to be forward and Y+ to be up.
	///
	/// A movement script (e.g AIPath) must also be attached to the same GameObject to actually move the unit.
	///
	/// This script will forward the movement speed to the animator component (<see cref="anim)"/> using the following animator parameter:
	/// - NormalizedSpeed: Movement speed in world units, divided by <see cref="MineBotAnimation.naturalSpeed"/> and the character's scale. This will be 1 when the agent is moving at the natural speed, and 0 when it is standing still.
	///
	/// When the end of path is reached, if the <see cref="endOfPathEffect"/> is not null, it will be instantiated at the current position. However, a check will be
	/// done so that it won't spawn effects too close to the previous spawn-point.
	/// [Open online documentation to see images]
	/// </summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/minebotanimation.html")]
	public class MineBotAnimation : VersionedMonoBehaviour {
		/// <summary>Animator component</summary>
		public Animator anim;

		/// <summary>
		/// Effect which will be instantiated when end of path is reached.
		/// See: <see cref="OnTargetReached"/>
		/// </summary>
		public GameObject endOfPathEffect;

		/// <summary>
		/// The natural movement speed is the speed that the animations are designed for.
		///
		/// One can for example configure the animator to speed up the animation if the agent moves faster than this, or slow it down if the agent moves slower than this.
		/// </summary>
		public float naturalSpeed = 5f;

		bool isAtEndOfPath;

		IAstarAI ai;
		Transform tr;

		const string NormalizedSpeedKey = "NormalizedSpeed";
		static int NormalizedSpeedKeyHash = Animator.StringToHash(NormalizedSpeedKey);

		protected override void Awake () {
			base.Awake();
			ai = GetComponent<IAstarAI>();
			tr = GetComponent<Transform>();
			if (anim != null && !HasParameter(anim, NormalizedSpeedKey)) {
				Debug.LogError($"No '{NormalizedSpeedKey}' parameter found on the animator. The animator must have a float parameter called '{NormalizedSpeedKey}'", this);
				enabled = false;
			}
		}

		static bool HasParameter (Animator animator, string paramName) {
			foreach (AnimatorControllerParameter param in animator.parameters) if (param.name == paramName) return true;
			return false;
		}

		/// <summary>Point for the last spawn of <see cref="endOfPathEffect"/></summary>
		protected Vector3 lastTarget;

		/// <summary>
		/// Called when the end of path has been reached.
		/// An effect (<see cref="endOfPathEffect)"/> is spawned when this function is called
		/// However, since paths are recalculated quite often, we only spawn the effect
		/// when the current position is some distance away from the previous spawn-point
		/// </summary>
		void OnTargetReached () {
			if (endOfPathEffect != null && Vector3.Distance(tr.position, lastTarget) > 1) {
				GameObject.Instantiate(endOfPathEffect, tr.position, tr.rotation);
				lastTarget = tr.position;
			}
		}

		void OnEnable () {
			// Process all components in a batched fashion to avoid Unity overhead
			// See https://blog.unity.com/engine-platform/10000-update-calls
			BatchedEvents.Add(this, BatchedEvents.Event.Update, OnUpdate);
		}

		void OnDisable () {
			BatchedEvents.Remove(this);
		}

		static void OnUpdate (MineBotAnimation[] components, int count) {
			for (int i = 0; i < count; i++) components[i].OnUpdate();
		}

		void OnUpdate () {
			if (ai == null) return;

			if (ai.reachedEndOfPath) {
				if (!isAtEndOfPath) OnTargetReached();
				isAtEndOfPath = true;
			} else isAtEndOfPath = false;

			// Calculate the velocity relative to this transform's orientation
			Vector3 relVelocity = tr.InverseTransformDirection(ai.velocity);
			relVelocity.y = 0;

			// Speed relative to the character size
			anim.SetFloat(NormalizedSpeedKeyHash, relVelocity.magnitude / (naturalSpeed * anim.transform.lossyScale.x));
		}
	}
}
