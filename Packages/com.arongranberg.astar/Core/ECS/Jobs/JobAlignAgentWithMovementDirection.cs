#if MODULE_ENTITIES
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Pathfinding.ECS {
	[BurstCompile]
	[WithAll(typeof(SimulateMovement), typeof(SimulateMovementFinalize))]
	public partial struct JobAlignAgentWithMovementDirection : IJobEntity {
		public float dt;

		public void Execute (ref LocalTransform transform, in MovementSettings movementSettings, in MovementState movementState, in AgentCylinderShape shape, in AgentMovementPlane movementPlane, in MovementControl movementControl, ref ResolvedMovement resolvedMovement) {
			if (math.lengthsq(movementControl.targetPoint - resolvedMovement.targetPoint) > 0.001f && resolvedMovement.speed > movementSettings.follower.speed * 0.1f) {
				// If the agent is moving, align it with the movement direction
				var desiredDirection = movementPlane.value.ToPlane(movementControl.targetPoint - transform.Position);
				var actualDirection = movementPlane.value.ToPlane(resolvedMovement.targetPoint - transform.Position);

				float desiredAngle;
				if (math.lengthsq(desiredDirection) > math.pow(movementSettings.follower.speed * 0.1f, 2)) {
					desiredAngle = math.atan2(desiredDirection.y, desiredDirection.x);
				} else {
					// If the agent did not desire to move at all, use the agent's current rotation
					desiredAngle = movementPlane.value.ToPlane(transform.Rotation) + math.PI*0.5f;
				}

				// The agent only moves if the actual movement direction is non-zero
				if (math.lengthsq(actualDirection) > math.pow(movementSettings.follower.speed * 0.1f, 2)) {
					var actualAngle = math.atan2(actualDirection.y, actualDirection.x);
					resolvedMovement.targetRotationOffset = AstarMath.DeltaAngle(desiredAngle, actualAngle);
					return;
				}
			}

			{
				// Decay the rotation offset
				// var da = AstarMath.DeltaAngle(movementState.rotationOffset, 0);
				// resolvedMovement.targetRotationOffset += da * dt * 2.0f;
				resolvedMovement.targetRotationOffset = AstarMath.DeltaAngle(0, resolvedMovement.targetRotationOffset) * (1 - dt * 2.0f);
			}
		}
	}
}
#endif
