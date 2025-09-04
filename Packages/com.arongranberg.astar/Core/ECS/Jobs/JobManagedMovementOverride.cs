#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Transforms;

namespace Pathfinding.ECS {
	public partial struct JobManagedMovementOverrideBeforeControl : IJobEntity {
		public float dt;

		public void Execute (ManagedMovementOverrideBeforeControl managedOverride, Entity entity, ref LocalTransform localTransform, ref AgentCylinderShape shape, ref AgentMovementPlane movementPlane, ref DestinationPoint destination, ref MovementState movementState, ref MovementSettings movementSettings) {
			if (managedOverride.callback != null) {
				managedOverride.callback(entity, dt, ref localTransform, ref shape, ref movementPlane, ref destination, ref movementState, ref movementSettings);
				// The callback may have modified the movement state, so we need to reset the path tracer version to indicate that the movement state is not up to date.
				// This will cause the repair job to avoid optimizing some updates away.
				movementState.pathTracerVersion--;
			}
		}
	}

	public partial struct JobManagedMovementOverrideAfterControl : IJobEntity {
		public float dt;

		public void Execute (ManagedMovementOverrideAfterControl managedOverride, Entity entity, ref LocalTransform localTransform, ref AgentCylinderShape shape, ref AgentMovementPlane movementPlane, ref DestinationPoint destination, ref MovementState movementState, ref MovementSettings movementSettings, ref MovementControl movementControl) {
			if (managedOverride.callback != null) {
				managedOverride.callback(entity, dt, ref localTransform, ref shape, ref movementPlane, ref destination, ref movementState, ref movementSettings, ref movementControl);
				// The callback may have modified the movement state, so we need to reset the path tracer version to indicate that the movement state is not up to date.
				// This will cause the repair job to avoid optimizing some updates away.
				movementState.pathTracerVersion--;
			}
		}
	}

	public partial struct JobManagedMovementOverrideBeforeMovement : IJobEntity {
		public float dt;

		public void Execute (ManagedMovementOverrideBeforeMovement managedOverride, Entity entity, ref LocalTransform localTransform, ref AgentCylinderShape shape, ref AgentMovementPlane movementPlane, ref DestinationPoint destination, ref MovementState movementState, ref MovementSettings movementSettings, ref MovementControl movementControl, ref ResolvedMovement resolvedMovement) {
			if (managedOverride.callback != null) {
				managedOverride.callback(entity, dt, ref localTransform, ref shape, ref movementPlane, ref destination, ref movementState, ref movementSettings, ref movementControl, ref resolvedMovement);
				// The callback may have modified the movement state, so we need to reset the path tracer version to indicate that the movement state is not up to date.
				// This will cause the repair job to avoid optimizing some updates away.
				movementState.pathTracerVersion--;
			}
		}
	}
}
#endif
