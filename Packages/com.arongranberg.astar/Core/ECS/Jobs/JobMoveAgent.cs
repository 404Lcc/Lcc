#if MODULE_ENTITIES
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pathfinding.ECS {
	[BurstCompile]
	[WithAll(typeof(SimulateMovement), typeof(SimulateMovementFinalize))]
	[WithNone(typeof(AgentOffMeshLinkMovementDisabled))]
	public partial struct JobMoveAgent : IJobEntity {
		public float dt;

		static void UpdateVelocityEstimate (ref LocalTransform transform, ref MovementStatistics movementStatistics, float dt) {
			if (dt > 0.000001f) {
				movementStatistics.estimatedVelocity = (transform.Position - movementStatistics.lastPosition) / dt;
			}
		}

		static void ResolveRotation (ref LocalTransform transform, ref MovementState state, in ResolvedMovement resolvedMovement, in MovementSettings movementSettings, in AgentMovementPlane movementPlane, float dt) {
			var currentRotation = movementPlane.value.ToPlane(transform.Rotation);
			var currentInternalRotation = currentRotation - state.rotationOffset - state.rotationOffset2;
			var deltaRotation = math.clamp(AstarMath.DeltaAngle(currentInternalRotation, resolvedMovement.targetRotation), -resolvedMovement.rotationSpeed * dt, resolvedMovement.rotationSpeed * dt);
			var extraRotationSpeed = math.radians(movementSettings.follower.maxRotationSpeed) * 0.5f;
			var deltaExtraRotation = math.clamp(AstarMath.DeltaAngle(state.rotationOffset, resolvedMovement.targetRotationOffset), -extraRotationSpeed * dt, extraRotationSpeed * dt);
			var currentUnsmoothedRotation = currentInternalRotation + state.rotationOffset;
			var newInternalRotation = currentInternalRotation + deltaRotation;
			// Keep track of how much extra rotation we are applying. This is done so that
			// the movement calculation can separate this out when doing its movement calculations.
			state.rotationOffset += deltaExtraRotation;
			// Make sure the rotation offset is between -pi/2 and pi/2 radians
			state.rotationOffset = AstarMath.DeltaAngle(0, state.rotationOffset);
			var newUnsmoothedRotation = newInternalRotation + state.rotationOffset;

			if (movementSettings.rotationSmoothing > 0) {
				// Apply compensation to rotationOffset2 to precisely cancel out the agent's rotation during this frame
				state.rotationOffset2 += currentUnsmoothedRotation - newUnsmoothedRotation;

				state.rotationOffset2 = AstarMath.DeltaAngle(0, state.rotationOffset2);

				// Decay the rotationOffset2. This implicitly adds an exponential moving average to the visual rotation
				var decay = math.abs(AstarMath.DeltaAngle(currentRotation, resolvedMovement.targetRotationHint)) / movementSettings.rotationSmoothing;
				var exponentialDecay = decay*dt;

				// In addition to an exponential decay, we also add a linear decay.
				// This is important to relatively quickly zero out the error when the agent is almost
				// facing the right direction. With an exponential decay, it would take far too long to look good.
				const float LINEAR_DECAY_AMOUNT = 0.1f;
				var linearDecay = (LINEAR_DECAY_AMOUNT/movementSettings.rotationSmoothing)*dt;

				if (math.abs(state.rotationOffset2) > 0) state.rotationOffset2 *= math.max(0, 1 - exponentialDecay - linearDecay/math.abs(state.rotationOffset2));
			} else if (state.rotationOffset2 != 0) {
				// Rotation smoothing is disabled, decay the rotation offset very quickly, but still avoid jarring changes
				state.rotationOffset2 += math.clamp(-state.rotationOffset2, -extraRotationSpeed * dt, extraRotationSpeed * dt);
			}

			transform.Rotation = movementPlane.value.ToWorldRotation(newInternalRotation + state.rotationOffset + state.rotationOffset2);
		}

		public static float3 MoveWithoutGravity (ref LocalTransform transform, in ResolvedMovement resolvedMovement, in AgentMovementPlane movementPlane, float dt) {
			UnityEngine.Assertions.Assert.IsTrue(math.all(math.isfinite(resolvedMovement.targetPoint)));
			// Move only along the movement plane
			var localDir = movementPlane.value.ToPlane(resolvedMovement.targetPoint - transform.Position);
			var magn = math.length(localDir);
			var localDelta = math.select(localDir, localDir * math.clamp(resolvedMovement.speed * dt / magn, 0, 1.0f), magn > 0.0001f);
			var delta = movementPlane.value.ToWorld(localDelta, 0);
			return delta;
		}

		public static void ResolvePositionSmoothing (float3 movementDelta, ref MovementState state, in MovementSettings movementSettings, float dt) {
			if (movementSettings.positionSmoothing > 0) {
				state.positionOffset -= movementDelta;
				var exponentialDecay = 1f/movementSettings.positionSmoothing*dt;
				var linearDecay = 0.1f/movementSettings.positionSmoothing*dt;
				var positionOffsetMagnitude = math.length(state.positionOffset);
				if (positionOffsetMagnitude > 0) state.positionOffset *= math.max(0, 1 - exponentialDecay - linearDecay/positionOffsetMagnitude);
			} else {
				state.positionOffset = float3.zero;
			}
		}

		public void Execute (ref LocalTransform transform, in AgentCylinderShape shape, in AgentMovementPlane movementPlane, ref MovementState state, in MovementSettings movementSettings, in ResolvedMovement resolvedMovement, ref MovementStatistics movementStatistics) {
			MoveAgent(ref transform, in shape, in movementPlane, ref state, in movementSettings, in resolvedMovement, ref movementStatistics, dt);
		}

		public static void MoveAgent (ref LocalTransform transform, in AgentCylinderShape shape, in AgentMovementPlane movementPlane, ref MovementState state, in MovementSettings movementSettings, in ResolvedMovement resolvedMovement, ref MovementStatistics movementStatistics, float dt) {
			var delta = MoveWithoutGravity(ref transform, in resolvedMovement, in movementPlane, dt);
			UnityEngine.Assertions.Assert.IsTrue(math.all(math.isfinite(delta)), "Refusing to set the agent's position to a non-finite vector");
			transform.Position += delta;
			// In 2D games, the agent may move slightly in the Z direction, due to floating point errors.
			// Some users get confused about this, so if the Z coordinate is very close to zero, just set it to zero.
			if (math.abs(transform.Position.z) < 0.00001f) transform.Position.z = 0;

			ResolvePositionSmoothing(delta, ref state, in movementSettings, dt);
			ResolveRotation(ref transform, ref state, in resolvedMovement, in movementSettings, in movementPlane, dt);
			UpdateVelocityEstimate(ref transform, ref movementStatistics, dt);
			movementStatistics.lastPosition = transform.Position;
		}
	}
}
#endif
