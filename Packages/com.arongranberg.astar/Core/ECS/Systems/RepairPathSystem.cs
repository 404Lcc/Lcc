#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Entities;
using UnityEngine.Profiling;
using Unity.Transforms;
using Unity.Burst;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.ECS.RVO;
	using Pathfinding.RVO;
	using Unity.Burst.Intrinsics;
	using Unity.Collections;
	using Unity.Mathematics;

	[UpdateInGroup(typeof(AIMovementSystemGroup))]
	[UpdateBefore(typeof(FollowerControlSystem))]
	[BurstCompile]
	public partial struct RepairPathSystem : ISystem {
		EntityQuery entityQueryPrepare;
		EntityQuery entityQueryOffMeshLink;
		EntityQuery entityQueryOffMeshLinkCleanup;
		public JobRepairPath.Scheduler jobRepairPathScheduler;

		public void OnCreate (ref SystemState state) {
			jobRepairPathScheduler = new JobRepairPath.Scheduler(ref state);

			entityQueryPrepare = jobRepairPathScheduler.GetEntityQuery(Unity.Collections.Allocator.Temp).WithAll<SimulateMovement, SimulateMovementRepair>().Build(ref state);

			entityQueryOffMeshLink = state.GetEntityQuery(new EntityQueryDesc {
				All = new ComponentType[] {
					ComponentType.ReadWrite<LocalTransform>(),
					ComponentType.ReadOnly<AgentCylinderShape>(),
					ComponentType.ReadWrite<AgentMovementPlane>(),
					ComponentType.ReadOnly<DestinationPoint>(),
					ComponentType.ReadWrite<MovementState>(),
					ComponentType.ReadOnly<MovementStatistics>(),
					ComponentType.ReadWrite<ManagedState>(),
					ComponentType.ReadWrite<MovementSettings>(),
					ComponentType.ReadOnly<ResolvedMovement>(),
					ComponentType.ReadWrite<MovementControl>(),
					ComponentType.ReadWrite<AgentOffMeshLinkTraversal>(),
					ComponentType.ReadWrite<ManagedAgentOffMeshLinkTraversal>(),
					ComponentType.ReadOnly<SimulateMovement>(),
				},
				Present = new ComponentType[] {
					ComponentType.ReadWrite<AgentOffMeshLinkMovementDisabled>()
				}
			});

			entityQueryOffMeshLinkCleanup = state.GetEntityQuery(
				// ManagedAgentOffMeshLinkTraversal is a cleanup component.
				// If it exists, but the AgentOffMeshLinkTraversal does not exist,
				// then the agent must have been destroyed while traversing the off-mesh link.
				ComponentType.ReadWrite<ManagedAgentOffMeshLinkTraversal>(),
				ComponentType.Exclude<AgentOffMeshLinkTraversal>()
				);
		}

		public void OnDestroy (ref SystemState state) {
			jobRepairPathScheduler.Dispose();
		}

		public void OnUpdate (ref SystemState systemState) {
			if (AstarPath.active == null) return;

			var commandBuffer = new EntityCommandBuffer(systemState.WorldUpdateAllocator);

			if (AIMovementSystemGroup.TimeScaledRateManager.IsFirstSubstep) {
				// We don't care about syncing these more often than once per frame,
				// as the source cannot typically change between simulations steps anyway
				SyncLocalAvoidanceComponents(ref systemState, commandBuffer);

				// While the agent can technically discover that the path is stale during a simulation step,
				// only scheduling paths during the first substep is typically good enough.
				SchedulePaths(ref systemState);
			}
			StartOffMeshLinkTraversal(ref systemState, commandBuffer);

			commandBuffer.Playback(systemState.EntityManager);
			commandBuffer.Dispose();

			ProcessActiveOffMeshLinkTraversal(ref systemState);
			RepairPaths(ref systemState);
		}

		void SyncLocalAvoidanceComponents (ref SystemState systemState, EntityCommandBuffer commandBuffer) {
			var simulator = RVOSimulator.active?.GetSimulator();
			// First check if we have a simulator. If not, we can skip handling RVO components
			if (simulator == null) return;

			Profiler.BeginSample("AddRVOComponents");
			foreach (var(managedState, entity) in SystemAPI.Query<ManagedState>().WithNone<RVOAgent>().WithEntityAccess()) {
				if (managedState.enableLocalAvoidance) {
					commandBuffer.AddComponent<RVOAgent>(entity, managedState.rvoSettings);
				}
			}
			Profiler.EndSample();
			Profiler.BeginSample("CopyRVOSettings");
			foreach (var(managedState, rvoAgent, entity) in SystemAPI.Query<ManagedState, RefRW<RVOAgent> >().WithEntityAccess()) {
				rvoAgent.ValueRW = managedState.rvoSettings;
				if (!managedState.enableLocalAvoidance) {
					commandBuffer.RemoveComponent<RVOAgent>(entity);
				}
			}

			Profiler.EndSample();
		}

		void RepairPaths (ref SystemState systemState) {
			Profiler.BeginSample("RepairPaths");
			// This job accesses managed component data in a somewhat unsafe way.
			// It should be safe to run it in parallel with other systems, but I'm not 100% sure.
			// This job also accesses graph data, but this is safe because the AIMovementSystemGroup
			// holds a read lock on the graph data while its subsystems are running.
			systemState.Dependency = jobRepairPathScheduler.ScheduleParallel(ref systemState, entityQueryPrepare, systemState.Dependency);
			Profiler.EndSample();
		}

		[WithAbsent(typeof(ManagedAgentOffMeshLinkTraversal))] // Do not recalculate the path of agents that are currently traversing an off-mesh link.
		partial struct JobCheckStaleness : IJobEntity, IJobEntityChunkBeginEnd {
			public NativeBitArray isPathStale;
			int index;

			public void Execute (ManagedState state) {
				isPathStale.Set(index++, state.pathTracer.isStale);
			}

			public bool OnChunkBegin (in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
				if (index + chunk.Count > isPathStale.Length) isPathStale.Resize(math.ceilpow2(index + chunk.Count), NativeArrayOptions.ClearMemory);
				return true;
			}

			public void OnChunkEnd (in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted) {}
		}


		[BurstCompile]
		[WithAbsent(typeof(ManagedAgentOffMeshLinkTraversal))] // Do not recalculate the path of agents that are currently traversing an off-mesh link.
		partial struct JobShouldRecalculatePaths : IJobEntity {
			public float time;
			public NativeBitArray shouldRecalculatePath;
			int index;

			public void Execute (ref ECS.AutoRepathPolicy autoRepathPolicy, in LocalTransform transform, in AgentCylinderShape shape, in DestinationPoint destination) {
				var isPathStale = shouldRecalculatePath.IsSet(index);
				shouldRecalculatePath.Set(index++, autoRepathPolicy.ShouldRecalculatePath(transform.Position, shape.radius, destination.destination, time, isPathStale));
			}
		}

		[WithAbsent(typeof(ManagedAgentOffMeshLinkTraversal))] // Do not recalculate the path of agents that are currently traversing an off-mesh link.
		public partial struct JobRecalculatePaths : IJobEntity {
			public float time;
			public NativeBitArray shouldRecalculatePath;
			int index;

			public void Execute (ManagedState state, ref ECS.AutoRepathPolicy autoRepathPolicy, ref LocalTransform transform, ref DestinationPoint destination, ref AgentMovementPlane movementPlane) {
				MaybeRecalculatePath(state, ref autoRepathPolicy, ref transform, ref destination, ref movementPlane, time, shouldRecalculatePath.IsSet(index++));
			}

			public static void MaybeRecalculatePath (ManagedState state, ref ECS.AutoRepathPolicy autoRepathPolicy, ref LocalTransform transform, ref DestinationPoint destination, ref AgentMovementPlane movementPlane, float time, bool wantsToRecalculatePath) {
				if (wantsToRecalculatePath && state.pendingPath == null) {
					var path = ABPath.Construct(transform.Position, destination.destination, null);
					path.UseSettings(state.pathfindingSettings);
					path.nnConstraint.distanceMetric = DistanceMetric.ClosestAsSeenFromAboveSoft(movementPlane.value.up);
					ManagedState.SetPath(path, state, in movementPlane, ref destination);
					autoRepathPolicy.OnScheduledPathRecalculation(destination.destination, time);
				}
			}
		}

		void SchedulePaths (ref SystemState systemState) {
			Profiler.BeginSample("Schedule search");
			var bits = new NativeBitArray(512, Allocator.TempJob);
			systemState.CompleteDependency();

			// Block the pathfinding threads from starting new path calculations while this loop is running.
			// This is done to reduce lock contention and significantly improve performance.
			// If we did not do this, all pathfinding threads would immediately wake up when a path was pushed to the queue.
			// Immediately when they wake up they will try to acquire a lock on the path queue.
			// If we are scheduling a lot of paths, this causes significant contention, and can make this loop take 100 times
			// longer to complete, compared to if we block the pathfinding threads.
			// TODO: Switch to a lock-free queue to avoid this issue altogether.
			var pathfindingLock = AstarPath.active.PausePathfindingSoon();

			// Propagate staleness
			new JobCheckStaleness {
				isPathStale = bits,
			}.Run();
			// Calculate which agents want to recalculate their path (using burst)
			new JobShouldRecalculatePaths {
				time = (float)SystemAPI.Time.ElapsedTime,
				shouldRecalculatePath = bits,
			}.Run();
			// Schedule the path calculations
			new JobRecalculatePaths {
				time = (float)SystemAPI.Time.ElapsedTime,
				shouldRecalculatePath = bits,
			}.Run();
			pathfindingLock.Release();
			bits.Dispose();
			Profiler.EndSample();
		}

		void StartOffMeshLinkTraversal (ref SystemState systemState, EntityCommandBuffer commandBuffer) {
			Profiler.BeginSample("Start off-mesh link traversal");
			foreach (var(state, entity) in SystemAPI.Query<ManagedState>().WithAll<ReadyToTraverseOffMeshLink>()
					 .WithEntityAccess()
			         // Do not try to add another off-mesh link component to agents that already have one.
					 .WithNone<AgentOffMeshLinkTraversal>()) {
				// UnityEngine.Assertions.Assert.IsTrue(movementState.ValueRO.reachedEndOfPart && state.pathTracer.isNextPartValidLink);
				var linkInfo = NextLinkToTraverse(state);
				var ctx = new AgentOffMeshLinkTraversalContext(linkInfo.link);
				// Add the AgentOffMeshLinkTraversal and ManagedAgentOffMeshLinkTraversal components when the agent should start traversing an off-mesh link.
				commandBuffer.AddComponent(entity, new AgentOffMeshLinkTraversal(linkInfo));
				commandBuffer.AddComponent(entity, new ManagedAgentOffMeshLinkTraversal(ctx, ResolveOffMeshLinkHandler(state, ctx)));
				commandBuffer.AddComponent(entity, new AgentOffMeshLinkMovementDisabled());
			}
			Profiler.EndSample();
		}

		public static OffMeshLinks.OffMeshLinkTracer NextLinkToTraverse (ManagedState state) {
			return state.pathTracer.GetLinkInfo(1);
		}

		public static IOffMeshLinkHandler ResolveOffMeshLinkHandler (ManagedState state, AgentOffMeshLinkTraversalContext ctx) {
			var handler = state.onTraverseOffMeshLink ?? ctx.concreteLink.handler;
			return handler;
		}

		void ProcessActiveOffMeshLinkTraversal (ref SystemState systemState) {
			var commandBuffer = new EntityCommandBuffer(systemState.WorldUpdateAllocator);
			systemState.CompleteDependency();
			new JobManagedOffMeshLinkTransition {
				commandBuffer = commandBuffer,
				deltaTime = AIMovementSystemGroup.TimeScaledRateManager.CheapStepDeltaTime,
			}.Run();

			new JobManagedOffMeshLinkTransitionCleanup().Run(entityQueryOffMeshLinkCleanup);
#if MODULE_ENTITIES_1_0_8_OR_NEWER
			commandBuffer.RemoveComponent<ManagedAgentOffMeshLinkTraversal>(entityQueryOffMeshLinkCleanup, EntityQueryCaptureMode.AtPlayback);
			commandBuffer.RemoveComponent<AgentOffMeshLinkMovementDisabled>(entityQueryOffMeshLinkCleanup, EntityQueryCaptureMode.AtPlayback);
#else
			commandBuffer.RemoveComponent<ManagedAgentOffMeshLinkTraversal>(entityQueryOffMeshLinkCleanup);
			commandBuffer.RemoveComponent<AgentOffMeshLinkMovementDisabled>(entityQueryOffMeshLinkCleanup);
#endif
			commandBuffer.Playback(systemState.EntityManager);
			commandBuffer.Dispose();
		}
	}
}
#endif
