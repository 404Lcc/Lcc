using UnityEngine;

namespace Pathfinding.Examples {
#if MODULE_ENTITIES
	using Pathfinding.ECS;
	using Unity.Entities;
	using Unity.Mathematics;
	using Unity.Transforms;
#endif

	/// <summary>
	/// Example of how to use Mecanim with the included movement scripts.
	///
	/// This script will use Mecanim to apply root motion to move the character
	/// instead of allowing the movement script to do the movement.
	///
	/// It assumes that the Mecanim controller uses 3 input variables
	/// - InputMagnitude which is simply 1 when the character should be moving and 0 when it should stop. Or, for the FollowerEntity component, 1 when it is moving at its natural speed, and less than 1 when it is moving slower.
	/// - X which is component of the desired movement along the left/right axis. For the AIPath and RichAI movement scripts, this will be a velocity in meters/second, while for the FollowerEntity movement script, this will be an angular velocity in radians/second.
	/// - Y which is component of the desired movement direction along the forward/backward axis. This is a velocity in meters/second.
	///
	/// It works with the <see cref="AIPath"/>, <see cref="RichAI"/> and <see cref="FollowerEntity"/> movement scripts.
	///
	/// See: https://docs.unity3d.com/Manual/RootMotion.html
	/// See: <see cref="IAstarAI"/>
	/// See: <see cref="AIPath"/>
	/// See: <see cref="RichAI"/>
	/// See: <see cref="FollowerEntity"/>
	/// </summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/mecanimbridge.html")]
	public class MecanimBridge : VersionedMonoBehaviour {
		/// <summary>Smoothing factor for the velocity, in seconds.</summary>
		public float velocitySmoothing = 1;

		/// <summary>
		/// Smoothing factor for the angular velocity, in seconds.
		///
		/// Note: This is only used with the <see cref="FollowerEntity"/> movement script.
		/// </summary>
		public float angularVelocitySmoothing = 1;
		public float naturalSpeed = 5;
#if MODULE_ENTITIES
		float smoothedRotationSpeed;
#endif

		/// <summary>Cached reference to the movement script</summary>
		IAstarAI ai;

		/// <summary>Cached Animator component</summary>
		Animator anim;

		/// <summary>Cached Transform component</summary>
		Transform tr;

		Vector3 smoothedVelocity;

		/// <summary>Position of the left and right feet during the previous frame</summary>
		Vector3[] prevFootPos = new Vector3[2];

		/// <summary>Cached reference to the left and right feet</summary>
		Transform[] footTransforms;

		const string InputMagnitudeKey = "InputMagnitude";
		static int InputMagnitudeKeyHash = Animator.StringToHash(InputMagnitudeKey);

		const string NormalizedSpeedKey = "NormalizedSpeed";
		static int NormalizedSpeedKeyHash = Animator.StringToHash(NormalizedSpeedKey);

		const string XAxisKey = "X";
		static int XAxisKeyHash = Animator.StringToHash(XAxisKey);

		const string YAxisKey = "Y";
		static int YAxisKeyHash = Animator.StringToHash(YAxisKey);

		protected override void Awake () {
			base.Awake();
			ai = GetComponent<IAstarAI>();
			anim = GetComponent<Animator>();
			tr = transform;

			// Find the feet of the character
			footTransforms = new [] { anim.GetBoneTransform(HumanBodyBones.LeftFoot), anim.GetBoneTransform(HumanBodyBones.RightFoot) };

			if (anim != null) {
				if (!HasParameter(anim, InputMagnitudeKey)) {
					Debug.LogError($"No '{InputMagnitudeKey}' parameter found on the animator. The animator must have a float parameter called '{InputMagnitudeKey}'", this);
					enabled = false;
				}

				if (!HasParameter(anim, XAxisKey)) {
					Debug.LogError($"No '{XAxisKey}' parameter found on the animator. The animator must have a float parameter called '{XAxisKey}'", this);
					enabled = false;
				}

				if (!HasParameter(anim, YAxisKey)) {
					Debug.LogError($"No '{YAxisKey}' parameter found on the animator. The animator must have a float parameter called '{YAxisKey}'", this);
					enabled = false;
				}
			}
		}

		static bool HasParameter (Animator animator, string paramName) {
			foreach (AnimatorControllerParameter param in animator.parameters) if (param.name == paramName) return true;
			return false;
		}

#if MODULE_ENTITIES
		void OnEnable () {
			if (ai is FollowerEntity followerEntity) {
				followerEntity.movementOverrides.AddBeforeMovementCallback(MovementOverride);
			}
		}

		void OnDisable () {
			if (ai is FollowerEntity followerEntity) {
				followerEntity.movementOverrides.RemoveBeforeMovementCallback(MovementOverride);
			}
		}

		void MovementOverride (Entity entity, float dt, ref LocalTransform localTransform, ref AgentCylinderShape shape, ref AgentMovementPlane movementPlane, ref DestinationPoint destination, ref MovementState movementState, ref MovementSettings movementSettings, ref MovementControl movementControl, ref ResolvedMovement resolvedMovement) {
			var desiredVelocity = math.normalizesafe(resolvedMovement.targetPoint - localTransform.Position) * resolvedMovement.speed;
			var currentRotation = movementPlane.value.ToPlane(localTransform.Rotation);

			var deltaRotationSpeed = AstarMath.DeltaAngle(currentRotation, resolvedMovement.targetRotation);
			deltaRotationSpeed = Mathf.Sign(deltaRotationSpeed) * Mathf.Clamp01(Mathf.Abs(deltaRotationSpeed) / math.max(0.001f, dt * resolvedMovement.rotationSpeed));
			deltaRotationSpeed = -deltaRotationSpeed * resolvedMovement.rotationSpeed;

			smoothedRotationSpeed = Mathf.Lerp(smoothedRotationSpeed, deltaRotationSpeed, angularVelocitySmoothing > 0 ? dt / angularVelocitySmoothing : 1);

			// Calculate the desired velocity relative to the character (+Z = forward, +X = right)
			var localDesiredVelocity = localTransform.InverseTransformDirection(desiredVelocity);
			localDesiredVelocity.y = 0;

			smoothedVelocity = Vector3.Lerp(smoothedVelocity, localDesiredVelocity, velocitySmoothing > 0 ? dt / velocitySmoothing : 1);
			if (smoothedVelocity.magnitude < 0.4f) {
				smoothedVelocity = smoothedVelocity.normalized * 0.4f;
			}

			var normalizedRotationSpeed = movementSettings.follower.maxRotationSpeed > 0 ? Mathf.Rad2Deg * Mathf.Abs(resolvedMovement.rotationSpeed) / movementSettings.follower.maxRotationSpeed : 0;
			var normalizedSpeed = movementSettings.follower.speed * naturalSpeed > 0 ? resolvedMovement.speed / naturalSpeed : 0;

			// Combine the normalized rotation speed and normalized speed such that either of them being large, results in the input magnitude being large.
			// This is to ensure that even if the agent wants to almost rotate on the spot, the input magnitude will still be large.
			var inputMagnitude = Mathf.Min(1, Mathf.Sqrt(normalizedSpeed*normalizedSpeed + normalizedRotationSpeed*normalizedRotationSpeed));
			anim.SetFloat(InputMagnitudeKeyHash, inputMagnitude);
			anim.SetFloat(XAxisKeyHash, smoothedRotationSpeed);
			anim.SetFloat(YAxisKeyHash, smoothedVelocity.z);

			// Calculate how much the agent should rotate during this frame
			var nextPosition = localTransform.Position;
			var nextRotation = localTransform.Rotation;

			// Apply rotational root motion
			nextRotation = anim.deltaRotation * nextRotation;

			nextPosition += (float3)anim.deltaPosition;

			resolvedMovement.targetPoint = nextPosition;
			resolvedMovement.targetRotation = movementPlane.value.ToPlane(nextRotation);
			// target rotation speed?
			resolvedMovement.speed = math.length(nextPosition - localTransform.Position) / math.max(0.001f, dt);
		}
#endif

		/// <summary>Update is called once per frame</summary>
		void Update () {
			if (ai is AIBase aiBase) {
				aiBase.canMove = false;
				// aiBase.updatePosition = false;
				// aiBase.updateRotation = false;
			}
		}

		/// <summary>Calculate position of the currently grounded foot</summary>
		Vector3 CalculateBlendPoint () {
			// Fall back to rotating around the transform position if no feet could be found
			if (footTransforms[0] == null || footTransforms[1] == null) return tr.position;

			var leftFootPos = footTransforms[0].position;
			var rightFootPos = footTransforms[1].position;

			// This is the same calculation that Unity uses for
			// Animator.pivotWeight and Animator.pivotPosition
			// but those properties do not work for all animations apparently.
			var footVelocity1 = (leftFootPos - prevFootPos[0]) / Time.deltaTime;
			var footVelocity2 = (rightFootPos - prevFootPos[1]) / Time.deltaTime;
			float denominator = footVelocity1.magnitude + footVelocity2.magnitude;
			var pivotWeight = denominator > 0 ? footVelocity1.magnitude / denominator : 0.5f;
			prevFootPos[0] = leftFootPos;
			prevFootPos[1] = rightFootPos;
			var pivotPosition = Vector3.Lerp(leftFootPos, rightFootPos, pivotWeight);
			return pivotPosition;
		}

		void OnAnimatorMove () {
#if MODULE_ENTITIES
			if (ai is FollowerEntity) return;
#endif

			Vector3 nextPosition;
			Quaternion nextRotation;

			ai.MovementUpdate(Time.deltaTime, out nextPosition, out nextRotation);

			//var desiredVelocity = (ai.steeringTarget - tr.position).normalized * 2;//ai.desiredVelocity;
			var desiredVelocity = ai.desiredVelocity;

			// Calculate the desired velocity relative to the character (+Z = forward, +X = right)
			var localDesiredVelocity = tr.InverseTransformDirection(desiredVelocity);
			localDesiredVelocity.y = 0;
			var desiredVelocityWithoutGrav = tr.TransformDirection(localDesiredVelocity);

			anim.SetFloat(InputMagnitudeKeyHash, ai.reachedEndOfPath || localDesiredVelocity.magnitude < 0.1f ? 0f : 1f);

			smoothedVelocity = Vector3.Lerp(smoothedVelocity, localDesiredVelocity, velocitySmoothing > 0 ? Time.deltaTime / velocitySmoothing : 1);
			if (smoothedVelocity.magnitude < 0.4f) {
				smoothedVelocity = smoothedVelocity.normalized * 0.4f;
			}

			anim.SetFloat(XAxisKeyHash, smoothedVelocity.x);
			anim.SetFloat(YAxisKeyHash, smoothedVelocity.z);

			// The IAstarAI interface doesn't expose rotation speeds right now, so we have to do this ugly thing.
			// In case this is an unknown movement script, we fall back to a reasonable value.
			var rotationSpeed = 360f;
			if (ai is AIPath aipath) {
				rotationSpeed = aipath.rotationSpeed;
			} else if (ai is RichAI richai) {
				rotationSpeed = richai.rotationSpeed;
			}

			// Calculate how much the agent should rotate during this frame
			var newRot = RotateTowards(desiredVelocityWithoutGrav, Time.deltaTime * rotationSpeed);
			// Rotate the character around the currently grounded foot to prevent foot sliding
			nextPosition = ai.position;
			nextRotation = ai.rotation;

			nextPosition = RotatePointAround(nextPosition, CalculateBlendPoint(), newRot * Quaternion.Inverse(nextRotation));
			nextRotation = newRot;

			// Apply rotational root motion
			nextRotation = anim.deltaRotation * nextRotation;

			// Use gravity from the movement script, not from animation
			var deltaPos = anim.deltaPosition;
			deltaPos.y = desiredVelocity.y * Time.deltaTime;
			nextPosition += deltaPos;

			// Call the movement script to perform the final movement
			ai.FinalizeMovement(nextPosition, nextRotation);
		}

		static Vector3 RotatePointAround (Vector3 point, Vector3 around, Quaternion rotation) {
			return rotation * (point - around) + around;
		}

		/// <summary>
		/// Calculates a rotation closer to the desired direction.
		/// Returns: The new rotation for the character
		/// </summary>
		/// <param name="direction">Direction in the movement plane to rotate toward.</param>
		/// <param name="maxDegrees">Maximum number of degrees to rotate this frame.</param>
		protected virtual Quaternion RotateTowards (Vector3 direction, float maxDegrees) {
			if (direction != Vector3.zero) {
				Quaternion targetRotation = Quaternion.LookRotation(direction);
				return Quaternion.RotateTowards(tr.rotation, targetRotation, maxDegrees);
			} else {
				return tr.rotation;
			}
		}
	}
}
