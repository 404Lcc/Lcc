#if MODULE_ENTITIES
using Pathfinding.Drawing;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Pathfinding.ECS {
	[BurstCompile]
	public partial struct JobPrepareAgentRaycasts : IJobEntity {
		public NativeArray<RaycastCommand> raycastCommands;
		public QueryParameters raycastQueryParameters;
		public CommandBuilder draw;
		public float dt;
		public float gravity;

		void ApplyGravity (ref GravityState gravityState) {
			gravityState.verticalVelocity += gravity * dt;
		}

		RaycastCommand CalculateRaycastCommands (ref LocalTransform transform, in AgentCylinderShape shape, in AgentMovementPlane movementPlane, in MovementSettings movementSettings, ref GravityState gravityState, PhysicsScene physicsScene) {
			// TODO: Might be more performant to convert the movement plane to two matrices

			movementPlane.value.ToPlane(transform.Position, out var lastElevation);

			var elevationDelta = gravityState.verticalVelocity * dt;
			var localPosition = movementPlane.value.ToPlane(transform.Position, out var elevation);
			var rayStartElevation = math.max(elevation + elevationDelta, lastElevation) + shape.height * 0.5f;
			var rayStopElevation = math.min(elevation + elevationDelta, lastElevation);
			float rayLength = rayStartElevation - rayStopElevation; // TODO: Multiply by scale
			var down = movementPlane.value.ToWorld(0, -1);
			raycastQueryParameters.layerMask = movementSettings.groundMask;
			return new RaycastCommand(physicsScene, movementPlane.value.ToWorld(localPosition, rayStartElevation), down, raycastQueryParameters, rayLength);
		}

		public void Execute (ref LocalTransform transform, in AgentCylinderShape shape, in AgentMovementPlane movementPlane, ref MovementState state, in MovementSettings movementSettings, ref ResolvedMovement resolvedMovement, ref MovementStatistics movementStatistics, ref GravityState gravityState, in PhysicsSceneRef physicsWorldIndex, [Unity.Entities.EntityIndexInQuery] int entityIndexInQuery) {
			// Move only along the movement plane
			// JobMoveAgent.MoveAgent(ref transform, in shape, in movementPlane, ref state, in movementSettings, in resolvedMovement, ref movementStatistics, dt);

			ApplyGravity(ref gravityState);
			raycastCommands[entityIndexInQuery] = CalculateRaycastCommands(ref transform, in shape, in movementPlane, in movementSettings, ref gravityState, physicsWorldIndex.physicsScene);
		}
	}
}
#endif
