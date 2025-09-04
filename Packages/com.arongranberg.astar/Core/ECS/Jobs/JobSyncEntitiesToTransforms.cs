#if MODULE_ENTITIES
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Jobs;

namespace Pathfinding.ECS {
	[BurstCompile]
	struct JobSyncEntitiesToTransforms : IJobParallelForTransform {
		[ReadOnly]
		public NativeArray<Entity> entities;
		[ReadOnly]
		public ComponentLookup<LocalTransform> entityPositions;
		[ReadOnly]
		public ComponentLookup<MovementState> movementState;
		[ReadOnly]
		public ComponentLookup<SyncPositionWithTransform> syncPositionWithTransform;
		[ReadOnly]
		public ComponentLookup<SyncRotationWithTransform> syncRotationWithTransform;
		[ReadOnly]
		public ComponentLookup<OrientationYAxisForward> orientationYAxisForward;

		public void Execute (int index, TransformAccess transform) {
			var entity = entities[index];
			if (!entityPositions.HasComponent(entity)) return;
			float3 offset = float3.zero;
			if (movementState.TryGetComponent(entity, out var ms)) {
				offset = ms.positionOffset;
			}

			var tr = entityPositions.GetRefRO(entity);
			if (syncPositionWithTransform.HasComponent(entity)) transform.position = tr.ValueRO.Position + offset;
			if (syncRotationWithTransform.HasComponent(entity)) {
				if (orientationYAxisForward.HasComponent(entity)) {
					// Y axis forward
					transform.rotation = math.mul(tr.ValueRO.Rotation, SyncTransformsToEntitiesSystem.ZAxisForwardToYAxisForward);
				} else {
					// Z axis forward
					transform.rotation = tr.ValueRO.Rotation;
				}
			}
		}
	}
}
#endif
