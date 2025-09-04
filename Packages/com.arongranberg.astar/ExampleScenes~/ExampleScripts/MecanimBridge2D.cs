using UnityEngine;

namespace Pathfinding.Examples {
	/// <summary>
	/// Helper for adding animation to agents.
	///
	/// This script will forward the movement velocity to the animator component using the following animator parameters:
	///
	/// - InputMagnitude: Movement speed, normalized by the agent's natural speed. 1 if the agent is moving at its natural speed, and 0 if it is standing still.
	/// - X: Horizontal movement speed, normalized by the agent's natural speed.
	/// - Y: Vertical movement speed, normalized by the agent's natural speed.
	/// </summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/mecanimbridge2d.html")]
	public class MecanimBridge2D : VersionedMonoBehaviour {
		/// <summary>
		/// How much to smooth the velocity of the agent.
		///
		/// The velocity will be smoothed out over approximately this number of seconds.
		/// A value of zero indicates no smoothing.
		/// </summary>
		public float velocitySmoothing = 1;

		/// <summary>
		/// The natural movement speed is the speed that the animations are designed for.
		///
		/// One can for example configure the animator to speed up the animation if the agent moves faster than this, or slow it down if the agent moves slower than this.
		/// </summary>
		public float naturalSpeed = 5;

		/// <summary>
		/// How the agent's rotation is handled.
		///
		/// See: <see cref="RotationMode"/>
		/// </summary>
		public RotationMode rotationMode = RotationMode.Hide;

		public enum RotationMode {
			/// <summary>The agent's transform will rotate towards the movement direction</summary>
			RotateTransform,
			/// <summary>
			/// The agent will not visibly rotate.
			///
			/// This is useful if your animation changes the agent's sprite to show a rotation.
			/// Internally, the agent's rotation property will still return the true rotation of the agent.
			///
			/// This is implemented by setting <see cref="FollowerEntity.updateRotation"/> to false on the agent.
			/// </summary>
			Hide,
		}

		/// <summary>Cached reference to the movement script</summary>
		IAstarAI ai;

		/// <summary>Cached Animator component</summary>
		Animator anim;

		Vector2 smoothedVelocity;

		protected override void Awake () {
			base.Awake();
			ai = GetComponent<IAstarAI>();
			anim = GetComponentInChildren<Animator>();
		}

		void Update () {
			if (ai == null || anim == null) return;

			var updateRotation = rotationMode == RotationMode.RotateTransform;
			// TODO: Expose this property using an interface
			if (ai is AIBase aiBase) aiBase.updateRotation = updateRotation;
			else if (ai is AILerp aiLerp) aiLerp.updateRotation = updateRotation;
#if MODULE_ENTITIES
			else if (ai is FollowerEntity follower) follower.updateRotation = updateRotation;
#endif

			var desiredVelocity = naturalSpeed > 0 ? ai.desiredVelocity / naturalSpeed : ai.desiredVelocity;
			var movementPlane = ai.movementPlane;
			var desiredVelocity2D = (Vector2)movementPlane.ToPlane(desiredVelocity, out var _);
			anim.SetFloat("NormalizedSpeed", ai.reachedEndOfPath || desiredVelocity2D.magnitude < 0.1f ? 0f : desiredVelocity2D.magnitude);

			smoothedVelocity = Vector3.Lerp(smoothedVelocity, desiredVelocity2D, velocitySmoothing > 0 ? Time.deltaTime / velocitySmoothing : 1);
			if (smoothedVelocity.magnitude < 0.4f) {
				smoothedVelocity = smoothedVelocity.normalized * 0.4f;
			}

			anim.SetFloat("X", smoothedVelocity.x);
			anim.SetFloat("Y", smoothedVelocity.y);
		}
	}
}
