#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using GCHandle = System.Runtime.InteropServices.GCHandle;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.ECS.RVO;
	using Pathfinding.Drawing;
	using Pathfinding.Util;
	using Unity.Profiling;
	using UnityEngine.Profiling;

	[BurstCompile]
	[UpdateAfter(typeof(FollowerControlSystem))]
	[UpdateAfter(typeof(RVOSystem))]
	[UpdateAfter(typeof(FallbackResolveMovementSystem))]
	[UpdateInGroup(typeof(AIMovementSystemGroup))]
	[RequireMatchingQueriesForUpdate]
	public partial struct AIMoveSystem : ISystem {
		EntityQuery entityQueryPrepareMovement;
		EntityQuery entityQueryWithGravity;
		EntityQuery entityQueryGizmos;
		EntityQuery entityQueryMovementOverride;
		JobRepairPath.Scheduler jobRepairPathScheduler;
		ComponentTypeHandle<MovementState> MovementStateTypeHandleRO;
		ComponentTypeHandle<ResolvedMovement> ResolvedMovementHandleRO;

		public void OnCreate (ref SystemState state) {
			jobRepairPathScheduler = new JobRepairPath.Scheduler(ref state);
			MovementStateTypeHandleRO = state.GetComponentTypeHandle<MovementState>(true);
			ResolvedMovementHandleRO = state.GetComponentTypeHandle<ResolvedMovement>(true);

			entityQueryWithGravity = state.GetEntityQuery(
				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadOnly<AgentCylinderShape>(),
				ComponentType.ReadWrite<AgentMovementPlane>(),
				ComponentType.ReadWrite<MovementState>(),
				ComponentType.ReadOnly<MovementSettings>(),
				ComponentType.ReadWrite<ResolvedMovement>(),
				ComponentType.ReadWrite<MovementStatistics>(),
				ComponentType.ReadOnly<MovementControl>(),
				ComponentType.ReadWrite<GravityState>(),
				ComponentType.ReadOnly<PhysicsSceneRef>(),

				// When in 2D mode, gravity is always disabled
				ComponentType.Exclude<OrientationYAxisForward>(),

				ComponentType.Exclude<AgentOffMeshLinkMovementDisabled>(),

				ComponentType.ReadOnly<AgentMovementPlaneSource>(),
				ComponentType.ReadOnly<SimulateMovement>(),
				ComponentType.ReadOnly<SimulateMovementFinalize>()
				);

			entityQueryPrepareMovement = jobRepairPathScheduler.GetEntityQuery(Allocator.Temp).WithAll<SimulateMovement, SimulateMovementRepair>().Build(ref state);

			entityQueryGizmos = state.GetEntityQuery(
				ComponentType.ReadOnly<LocalTransform>(),
				ComponentType.ReadOnly<AgentCylinderShape>(),
				ComponentType.ReadOnly<MovementSettings>(),
				ComponentType.ReadOnly<AgentMovementPlane>(),
				ComponentType.ReadOnly<ManagedState>(),
				ComponentType.ReadOnly<MovementState>(),
				ComponentType.ReadOnly<ResolvedMovement>(),

				ComponentType.ReadOnly<SimulateMovement>()
				);

			entityQueryMovementOverride = state.GetEntityQuery(
				ComponentType.ReadWrite<ManagedMovementOverrideBeforeMovement>(),

				ComponentType.ReadWrite<LocalTransform>(),
				ComponentType.ReadWrite<AgentCylinderShape>(),
				ComponentType.ReadWrite<AgentMovementPlane>(),
				ComponentType.ReadWrite<DestinationPoint>(),
				ComponentType.ReadWrite<MovementState>(),
				ComponentType.ReadWrite<MovementStatistics>(),
				ComponentType.ReadWrite<ManagedState>(),
				ComponentType.ReadWrite<MovementSettings>(),
				ComponentType.ReadWrite<ResolvedMovement>(),
				ComponentType.ReadWrite<MovementControl>(),

				ComponentType.Exclude<AgentOffMeshLinkTraversal>(),
				ComponentType.ReadOnly<SimulateMovement>(),
				ComponentType.ReadOnly<SimulateMovementControl>()
				);
		}

		static readonly ProfilerMarker MarkerMovementOverride = new ProfilerMarker("MovementOverrideBeforeMovement");

		public void OnDestroy (ref SystemState state) {
			jobRepairPathScheduler.Dispose();
		}

		public void OnUpdate (ref SystemState systemState) {
			var draw = DrawingManager.GetBuilder();

			// This system is executed at least every frame to make sure the agent is moving smoothly even at high fps.
			// The control loop and local avoidance may be running less often.
			// So this is designated a "cheap" system, and we use the corresponding delta time for that.
			var dt = AIMovementSystemGroup.TimeScaledRateManager.CheapStepDeltaTime;

			systemState.Dependency = new JobAlignAgentWithMovementDirection {
				dt = dt,
			}.Schedule(systemState.Dependency);

			RunMovementOverrideBeforeMovement(ref systemState, dt);

			// Move all agents which do not have a GravityState component
			systemState.Dependency = new JobMoveAgent {
				dt = dt,
			}.ScheduleParallel(systemState.Dependency);

			ScheduleApplyGravity(ref systemState, draw, dt);
			var gizmosDependency = systemState.Dependency;

			UpdateTypeHandles(ref systemState);

			systemState.Dependency = ScheduleRepairPaths(ref systemState, systemState.Dependency);

			// Draw gizmos only in the editor, and at most once per frame.
			// The movement calculations may run multiple times per frame when using high time-scales,
			// but rendering gizmos more than once would just lead to clutter.
			if (Application.isEditor && AIMovementSystemGroup.TimeScaledRateManager.IsLastSubstep) {
				gizmosDependency = ScheduleDrawGizmos(draw, systemState.Dependency);
			}

			// Render gizmos as soon as all relevant jobs are done
			draw.DisposeAfter(gizmosDependency);
			systemState.Dependency = ScheduleSyncEntitiesToTransforms(ref systemState, systemState.Dependency);
			systemState.Dependency = JobHandle.CombineDependencies(systemState.Dependency, gizmosDependency);
			systemState.Dependency = new JobClearTemporaryData().Schedule(systemState.Dependency);
		}

		void ScheduleApplyGravity (ref SystemState systemState, CommandBuilder draw, float dt) {
			Profiler.BeginSample("Gravity");
			// Note: We cannot use CalculateEntityCountWithoutFiltering here, because the GravityState component can be disabled
			var count = entityQueryWithGravity.CalculateEntityCount();
			var raycastCommands = CollectionHelper.CreateNativeArray<RaycastCommand>(count, systemState.WorldUpdateAllocator, NativeArrayOptions.UninitializedMemory);
			var raycastHits = CollectionHelper.CreateNativeArray<RaycastHit>(count, systemState.WorldUpdateAllocator, NativeArrayOptions.UninitializedMemory);

			// Prepare raycasts for all entities that have a GravityState component
			systemState.Dependency = new JobPrepareAgentRaycasts {
				raycastQueryParameters = new QueryParameters(-1, false, QueryTriggerInteraction.Ignore, false),
				raycastCommands = raycastCommands,
				draw = draw,
				dt = dt,
				gravity = Physics.gravity.y,
			}.ScheduleParallel(entityQueryWithGravity, systemState.Dependency);

			var raycastJob = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, 32, 1, systemState.Dependency);

			// Apply gravity and move all agents that have a GravityState component
			systemState.Dependency = new JobApplyGravity {
				raycastHits = raycastHits,
				raycastCommands = raycastCommands,
				draw = draw,
				dt = dt,
			}.ScheduleParallel(entityQueryWithGravity, JobHandle.CombineDependencies(systemState.Dependency, raycastJob));

			Profiler.EndSample();
		}

		void RunMovementOverrideBeforeMovement (ref SystemState systemState, float dt) {
			if (!entityQueryMovementOverride.IsEmptyIgnoreFilter) {
				MarkerMovementOverride.Begin();
				// The movement overrides always run on the main thread.
				// This adds a sync point, but only if people actually add a movement override (which is rare).
				systemState.CompleteDependency();
				new JobManagedMovementOverrideBeforeMovement {
					dt = dt,
					// TODO: Add unit test to make sure it fires/not fires when it should
				}.Run(entityQueryMovementOverride);
				MarkerMovementOverride.End();
			}
		}

		void UpdateTypeHandles (ref SystemState systemState) {
			MovementStateTypeHandleRO.Update(ref systemState);
			ResolvedMovementHandleRO.Update(ref systemState);
		}

		JobHandle ScheduleRepairPaths (ref SystemState systemState, JobHandle dependency) {
			Profiler.BeginSample("RepairPaths");
			// This job accesses graph data, but this is safe because the AIMovementSystemGroup
			// holds a read lock on the graph data while its subsystems are running.
			dependency = jobRepairPathScheduler.ScheduleParallel(ref systemState, entityQueryPrepareMovement, dependency);
			Profiler.EndSample();
			return dependency;
		}

		JobHandle ScheduleDrawGizmos (CommandBuilder commandBuilder, JobHandle dependency) {
			// Note: The ScheduleRepairPaths job runs right before this, so those handles are still valid
			return new JobDrawFollowerGizmos {
					   draw = commandBuilder,
					   entityManagerHandle = jobRepairPathScheduler.entityManagerHandle,
					   LocalTransformTypeHandleRO = jobRepairPathScheduler.LocalTransformTypeHandleRO,
					   AgentCylinderShapeHandleRO = jobRepairPathScheduler.AgentCylinderShapeTypeHandleRO,
					   MovementSettingsHandleRO = jobRepairPathScheduler.MovementSettingsTypeHandleRO,
					   AgentMovementPlaneHandleRO = jobRepairPathScheduler.AgentMovementPlaneTypeHandleRO,
					   ManagedStateHandleRW = jobRepairPathScheduler.ManagedStateTypeHandleRW,
					   MovementStateHandleRO = MovementStateTypeHandleRO,
					   ResolvedMovementHandleRO = ResolvedMovementHandleRO,
			}.ScheduleParallel(entityQueryGizmos, dependency);
		}

		JobHandle ScheduleSyncEntitiesToTransforms (ref SystemState systemState, JobHandle dependency) {
			Profiler.BeginSample("SyncEntitiesToTransforms");
			int numComponents = BatchedEvents.GetComponents<FollowerEntity>(BatchedEvents.Event.None, out var transforms, out var components);
			if (numComponents == 0) {
				Profiler.EndSample();
				return dependency;
			}

			var entities = CollectionHelper.CreateNativeArray<Entity>(numComponents, systemState.WorldUpdateAllocator);
			for (int i = 0; i < numComponents; i++) entities[i] = components[i].entity;

			dependency = new JobSyncEntitiesToTransforms {
				entities = entities,
				syncPositionWithTransform = SystemAPI.GetComponentLookup<SyncPositionWithTransform>(true),
				syncRotationWithTransform = SystemAPI.GetComponentLookup<SyncRotationWithTransform>(true),
				orientationYAxisForward = SystemAPI.GetComponentLookup<OrientationYAxisForward>(true),
				entityPositions = SystemAPI.GetComponentLookup<LocalTransform>(true),
				movementState = SystemAPI.GetComponentLookup<MovementState>(true),
			}.Schedule(transforms, dependency);
			Profiler.EndSample();
			return dependency;
		}
	}
}
#endif
