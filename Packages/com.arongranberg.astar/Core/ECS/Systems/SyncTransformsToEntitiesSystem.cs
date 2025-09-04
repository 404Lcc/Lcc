#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Profiling;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Jobs;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.Util;

	[UpdateBefore(typeof(TransformSystemGroup))]
	[UpdateBefore(typeof(AIMovementSystemGroup))]
	[UpdateInGroup(typeof(SimulationSystemGroup))]
	public partial struct SyncTransformsToEntitiesSystem : ISystem {
		public static readonly quaternion ZAxisForwardToYAxisForward = quaternion.Euler(math.PI / 2, 0, 0);
		public static readonly quaternion YAxisForwardToZAxisForward = quaternion.Euler(-math.PI / 2, 0, 0);

		public void OnUpdate (ref SystemState systemState) {
			int numComponents = BatchedEvents.GetComponents<FollowerEntity>(BatchedEvents.Event.None, out var transforms, out var components);
			if (numComponents > 0) {
				var entities = new NativeArray<Entity>(numComponents, Allocator.TempJob);

				for (int i = 0; i < numComponents; i++) entities[i] = components[i].entity;

				systemState.Dependency = new SyncTransformsToEntitiesJob {
					entities = entities,
					entityPositions = SystemAPI.GetComponentLookup<LocalTransform>(),
					syncPositionWithTransform = SystemAPI.GetComponentLookup<SyncPositionWithTransform>(true),
					syncRotationWithTransform = SystemAPI.GetComponentLookup<SyncRotationWithTransform>(true),
					orientationYAxisForward = SystemAPI.GetComponentLookup<OrientationYAxisForward>(true),
					movementState = SystemAPI.GetComponentLookup<MovementState>(true),
				}.Schedule(transforms, systemState.Dependency);
			}
		}

		[BurstCompile]
		struct SyncTransformsToEntitiesJob : IJobParallelForTransform {
			[ReadOnly]
			[DeallocateOnJobCompletion]
			public NativeArray<Entity> entities;

			// Safety: All entities are unique
			[NativeDisableParallelForRestriction]
			public ComponentLookup<LocalTransform> entityPositions;
			[ReadOnly]
			public ComponentLookup<SyncPositionWithTransform> syncPositionWithTransform;
			[ReadOnly]
			public ComponentLookup<SyncRotationWithTransform> syncRotationWithTransform;
			[ReadOnly]
			public ComponentLookup<OrientationYAxisForward> orientationYAxisForward;
			[ReadOnly]
			public ComponentLookup<MovementState> movementState;

			public void Execute (int index, TransformAccess transform) {
				var entity = entities[index];
				if (entityPositions.HasComponent(entity)) {
#if MODULE_ENTITIES_1_0_8_OR_NEWER
					ref var tr = ref entityPositions.GetRefRW(entity).ValueRW;
#else
					ref var tr = ref entityPositions.GetRefRW(entity, false).ValueRW;
#endif

					float3 offset = float3.zero;
					if (movementState.TryGetComponent(entity, out var ms)) {
						offset = ms.positionOffset;
					}

					if (syncPositionWithTransform.HasComponent(entity)) tr.Position = (float3)transform.position - offset;
					if (syncRotationWithTransform.HasComponent(entity)) {
						if (orientationYAxisForward.HasComponent(entity)) {
							tr.Rotation = math.mul(transform.rotation, YAxisForwardToZAxisForward);
						} else {
							// Z axis forward
							tr.Rotation = transform.rotation;
						}
					}
					tr.Scale = transform.localScale.y;
				}
			}
		}
	}
}
#endif
