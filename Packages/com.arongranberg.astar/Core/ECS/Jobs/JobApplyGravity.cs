#if MODULE_ENTITIES
using Pathfinding.Drawing;
using Pathfinding.Util;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Pathfinding.ECS {
	[BurstCompile]
	public partial struct JobApplyGravity : IJobEntity {
		[ReadOnly]
		public NativeArray<RaycastHit> raycastHits;
		[ReadOnly]
		public NativeArray<RaycastCommand> raycastCommands;
		public CommandBuilder draw;
		public float dt;

		void ResolveGravity (RaycastHit hit, bool grounded, ref LocalTransform transform, in AgentMovementPlane movementPlane, ref GravityState gravityState) {
			var localPosition = movementPlane.value.ToPlane(transform.Position, out var currentElevation);
			if (grounded) {
				// Grounded
				// Make the vertical velocity fall off exponentially. This is reasonable from a physical standpoint as characters
				// are not completely stiff and touching the ground will not immediately negate all velocity downwards. The AI will
				// stop moving completely due to the raycast penetration test but it will still *try* to move downwards. This helps
				// significantly when moving down along slopes, because if the vertical velocity would be set to zero when the character
				// was grounded it would lead to a kind of 'bouncing' behavior (try it, it's hard to explain). Ideally this should
				// use a more physically correct formula but this is a good approximation and is much more performant. The constant
				// CONVERGENCE_SPEED in the expression below determines how quickly it converges but high values can lead to too much noise.
				const float CONVERGENCE_SPEED = 5f;
				gravityState.verticalVelocity *= math.max(0, 1 - CONVERGENCE_SPEED * dt);

				movementPlane.value.ToPlane(hit.point, out var hitElevation);
				var elevationDelta = gravityState.verticalVelocity * dt;
				const float VERTICAL_COLLISION_ADJUSTMENT_SPEED = 6f;
				if (hitElevation > currentElevation) {
					// Already below ground, only allow upwards movement
					currentElevation = Mathf.MoveTowards(currentElevation, hitElevation, VERTICAL_COLLISION_ADJUSTMENT_SPEED * math.sqrt(math.abs(hitElevation - currentElevation)) * dt);
				} else {
					// Was above the ground, allow downwards movement until we are at the ground
					currentElevation = math.max(hitElevation, currentElevation + elevationDelta);
				}
			} else {
				var elevationDelta = gravityState.verticalVelocity * dt;
				currentElevation += elevationDelta;
			}
			transform.Position = movementPlane.value.ToWorld(localPosition, currentElevation);
		}

		public void Execute (ref LocalTransform transform, in MovementSettings movementSettings, ref AgentMovementPlane movementPlane, ref GravityState gravityState, in AgentMovementPlaneSource movementPlaneSource, [Unity.Entities.EntityIndexInQuery] int entityIndexInQuery) {
			var hit = raycastHits[entityIndexInQuery];
			var hitAnything = math.any((float3)hit.normal != 0f);
			if (hitAnything && movementPlaneSource.value == MovementPlaneSource.Raycast) {
				movementPlane.value = movementPlane.value.MatchUpDirection(hit.normal);
			}
			ResolveGravity(hit, hitAnything, ref transform, in movementPlane, ref gravityState);
		}
	}
}
#endif
