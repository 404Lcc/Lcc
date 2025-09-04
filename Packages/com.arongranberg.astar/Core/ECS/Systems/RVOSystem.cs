#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Mathematics;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using GCHandle = System.Runtime.InteropServices.GCHandle;

namespace Pathfinding.ECS.RVO {
	using Pathfinding.RVO;
	using Unity.Jobs;

	/// <summary>
	/// Simulates local avoidance in an ECS context.
	///
	/// All agent entities must have the following ECS components:
	/// - LocalTransform
	/// - <see cref="AgentCylinderShape"/>
	/// - <see cref="AgentMovementPlane"/>
	/// - <see cref="RVOAgent"/>
	/// - <see cref="MovementControl"/>: where you store how you want the agent to move
	/// - <see cref="ResolvedMovement"/>: where this system will output how the agent should move, when using RVO
	///
	/// The system will use the data from <see cref="MovementControl"/>, and output the following fields to <see cref="ResolvedMovement"/>:
	///
	/// <see cref="ResolvedMovement.targetPoint"/>: Where the agent should move to.
	/// <see cref="ResolvedMovement.speed"/>: At what speed the agent should move, in world units.
	/// <see cref="ResolvedMovement.turningRadiusMultiplier"/>: This will go up if its more crowded, to indicate that the agent should try to take wider turns to improve crowd flow.
	///
	/// The <see cref="AgentIndex"/> component will be added to the agent automatically by this system. You do not need to care about it.
	/// </summary>
	[BurstCompile]
	[UpdateAfter(typeof(FollowerControlSystem))]
	[UpdateInGroup(typeof(AIMovementSystemGroup))]
	public partial struct RVOSystem : ISystem {
		/// <summary>
		/// Keeps track of the last simulator that this RVOSystem saw.
		/// This is a weak GCHandle to allow it to be stored in an ISystem.
		/// </summary>
		GCHandle lastSimulator;
		EntityQuery withAgentIndex;
		EntityQuery shouldBeAddedToSimulation;
		EntityQuery shouldBeRemovedFromSimulation;
		ComponentLookup<AgentOffMeshLinkTraversal> agentOffMeshLinkTraversalLookup;

		public void OnCreate (ref SystemState state) {
			withAgentIndex = state.GetEntityQuery(
				ComponentType.ReadWrite<AgentIndex>()
				);
			shouldBeAddedToSimulation = state.GetEntityQuery(
				ComponentType.ReadOnly<RVOAgent>(),
				ComponentType.Exclude<AgentIndex>()
				);
			shouldBeRemovedFromSimulation = state.GetEntityQuery(
				ComponentType.ReadOnly<AgentIndex>(),
				ComponentType.Exclude<RVOAgent>()
				);
			lastSimulator = GCHandle.Alloc(null, System.Runtime.InteropServices.GCHandleType.Weak);
			agentOffMeshLinkTraversalLookup = state.GetComponentLookup<AgentOffMeshLinkTraversal>(true);
		}

		public void OnDestroy (ref SystemState state) {
			lastSimulator.Free();
		}

		public void OnUpdate (ref SystemState systemState) {
			var simulator = RVOSimulator.active?.GetSimulator();

			if (simulator != lastSimulator.Target) {
				// If the simulator has been destroyed, we need to remove all AgentIndex components
				RemoveAllAgentsFromSimulation(ref systemState);
				lastSimulator.Target = simulator;
			}
			if (simulator == null) return;

			AddAndRemoveAgentsFromSimulation(ref systemState, simulator);

			// The full movement calculations do not necessarily need to be done every frame if the fps is high
			if (AIMovementSystemGroup.TimeScaledRateManager.CheapSimulationOnly) {
				return;
			}

			CopyFromEntitiesToRVOSimulator(ref systemState, simulator, SystemAPI.Time.DeltaTime);

			// Schedule RVO update
			simulator.Update(
				systemState.Dependency,
				SystemAPI.Time.DeltaTime,
				AIMovementSystemGroup.TimeScaledRateManager.IsLastSubstep,
				systemState.WorldUpdateAllocator
				);

			CopyFromRVOSimulatorToEntities(ref systemState, simulator);
		}

		void RemoveAllAgentsFromSimulation (ref SystemState systemState) {
			var buffer = new EntityCommandBuffer(Allocator.Temp);
			var entities = withAgentIndex.ToEntityArray(systemState.WorldUpdateAllocator);
			buffer.RemoveComponent<AgentIndex>(entities);
			buffer.Playback(systemState.EntityManager);
			buffer.Dispose();
		}

		void AddAndRemoveAgentsFromSimulation (ref SystemState systemState, SimulatorBurst simulator) {
			// Remove all agents from the simulation that do not have an RVOAgent component, but have an AgentIndex
			var indicesToRemove = shouldBeRemovedFromSimulation.ToComponentDataArray<AgentIndex>(systemState.WorldUpdateAllocator);
			// Add all agents to the simulation that have an RVOAgent component, but not AgentIndex component
			var entitiesToAdd = shouldBeAddedToSimulation.ToEntityArray(systemState.WorldUpdateAllocator);
			// Avoid a sync point in the common case
			if (indicesToRemove.Length > 0 || entitiesToAdd.Length > 0) {
				var buffer = new EntityCommandBuffer(Allocator.Temp);
#if MODULE_ENTITIES_1_0_8_OR_NEWER
				buffer.RemoveComponent<AgentIndex>(shouldBeRemovedFromSimulation, EntityQueryCaptureMode.AtPlayback);
#else
				buffer.RemoveComponent<AgentIndex>(shouldBeRemovedFromSimulation);
#endif
				for (int i = 0; i < indicesToRemove.Length; i++) {
					simulator.RemoveAgent(indicesToRemove[i]);
				}
				for (int i = 0; i < entitiesToAdd.Length; i++) {
					buffer.AddComponent<AgentIndex>(entitiesToAdd[i], simulator.AddAgentBurst(UnityEngine.Vector3.zero));
				}

				buffer.Playback(systemState.EntityManager);
				buffer.Dispose();
			}
		}

		void CopyFromEntitiesToRVOSimulator (ref SystemState systemState, SimulatorBurst simulator, float dt) {
			agentOffMeshLinkTraversalLookup.Update(ref systemState);
			var writeLock = simulator.LockSimulationDataReadWrite();
			systemState.Dependency = new JobCopyFromEntitiesToRVOSimulator {
				agentData = simulator.simulationData,
				agentOutputData = simulator.outputData,
				movementPlaneMode = simulator.movementPlane,
				agentOffMeshLinkTraversalLookup = agentOffMeshLinkTraversalLookup,
				dt = dt,
			}.ScheduleParallel(JobHandle.CombineDependencies(writeLock.dependency, systemState.Dependency));
			writeLock.UnlockAfter(systemState.Dependency);
		}

		void CopyFromRVOSimulatorToEntities (ref SystemState systemState, SimulatorBurst simulator) {
			var writeLock = simulator.LockSimulationDataReadWrite();
			systemState.Dependency = new JobCopyFromRVOSimulatorToEntities {
				quadtree = simulator.quadtree,
				agentDataVersions = simulator.simulationData.version,
				agentOutputData = simulator.outputData,
			}.ScheduleParallel(JobHandle.CombineDependencies(writeLock.dependency, systemState.Dependency));
			writeLock.UnlockAfter(systemState.Dependency);
		}

		[BurstCompile]
		public partial struct JobCopyFromEntitiesToRVOSimulator : IJobEntity {
			[NativeDisableParallelForRestriction]
			public SimulatorBurst.AgentData agentData;
			[ReadOnly]
			public SimulatorBurst.AgentOutputData agentOutputData;
			public MovementPlane movementPlaneMode;
			[ReadOnly]
			public ComponentLookup<AgentOffMeshLinkTraversal> agentOffMeshLinkTraversalLookup;
			public float dt;

			public void Execute (Entity entity, in LocalTransform transform, in AgentCylinderShape shape, in AgentMovementPlane movementPlane, in AgentIndex agentIndex, in RVOAgent controller, in MovementControl target) {
				var scale = math.abs(transform.Scale);
				if (!agentIndex.TryGetIndex(ref agentData, out var index)) throw new System.InvalidOperationException("RVOAgent has an invalid entity index");

				// Actual infinity is not handled well by some algorithms, but very large values are ok.
				// This should be larger than any reasonable value a user might want to use.
				const float VERY_LARGE = 100000;

				// Copy all fields to the rvo simulator, and clamp them to reasonable values
				agentData.radius[index] = math.clamp(shape.radius * scale, 0.001f, VERY_LARGE);
				agentData.agentTimeHorizon[index] = math.clamp(controller.agentTimeHorizon, 0, VERY_LARGE);
				agentData.obstacleTimeHorizon[index] = math.clamp(controller.obstacleTimeHorizon, 0, VERY_LARGE);
				agentData.locked[index] = controller.locked;
				agentData.maxNeighbours[index] = math.max(controller.maxNeighbours, 0);
				agentData.debugFlags[index] = controller.debug;
				agentData.layer[index] = controller.layer;
				agentData.collidesWith[index] = controller.collidesWith;
				agentData.targetPoint[index] = target.targetPoint;
				agentData.desiredSpeed[index] = math.clamp(target.speed, 0, VERY_LARGE);
				agentData.maxSpeed[index] = math.clamp(target.maxSpeed, 0, VERY_LARGE);
				agentData.manuallyControlled[index] = target.overrideLocalAvoidance;
				agentData.endOfPath[index] = target.endOfPath;
				agentData.hierarchicalNodeIndex[index] = target.hierarchicalNodeIndex;
				agentData.movementPlane[index] = movementPlane.value;

				// Use the position from the movement script if one is attached
				// as the movement script's position may not be the same as the transform's position
				// (in particular if IAstarAI.updatePosition is false).
				var pos = movementPlane.value.ToPlane(transform.Position, out float elevation);
				if (movementPlaneMode == MovementPlane.XY) {
					// In 2D it is assumed the Z coordinate differences of agents is ignored.
					agentData.height[index] = 1;
					agentData.position[index] = movementPlane.value.ToWorld(pos, 0);
				} else {
					var center = 0.5f * shape.height;
					agentData.height[index] = math.clamp(shape.height * scale, 0, VERY_LARGE);
					agentData.position[index] = movementPlane.value.ToWorld(pos, elevation + (center - 0.5f * shape.height) * scale);
				}


				// TODO: Move this to a separate file
				var reached = agentOutputData.effectivelyReachedDestination[index];
				var prio = math.clamp(controller.priority * controller.priorityMultiplier, 0, VERY_LARGE);
				var flow = math.clamp(controller.flowFollowingStrength, 0, 1);
				// TODO: This is gettting overriden every frame, right?
				if (reached == ReachedEndOfPath.Reached) {
					// Override flow following strength and make it go towards 1
					flow = math.lerp(agentData.flowFollowingStrength[index], 1.0f, 6.0f * dt);
					prio *= 0.3f;
				} else if (reached == ReachedEndOfPath.ReachedSoon) {
					// Override flow following strength and make it go towards 1
					flow = math.lerp(agentData.flowFollowingStrength[index], 1.0f, 6.0f * dt);
					prio *= 0.45f;
				}
				agentData.priority[index] = prio;
				agentData.flowFollowingStrength[index] = flow;

				if (agentOffMeshLinkTraversalLookup.HasComponent(entity)) {
					// Agents traversing off-mesh links should not avoid other agents,
					// but other agents may still avoid them.
					agentData.manuallyControlled[index] = true;
				}
			}
		}

		[BurstCompile]
		public partial struct JobCopyFromRVOSimulatorToEntities : IJobEntity {
			[ReadOnly]
			public NativeArray<AgentIndex> agentDataVersions;
			[ReadOnly]
			public RVOQuadtreeBurst quadtree;
			[ReadOnly]
			public SimulatorBurst.AgentOutputData agentOutputData;

			/// <summary>See https://en.wikipedia.org/wiki/Circle_packing</summary>
			const float MaximumCirclePackingDensity = 0.9069f;

			public void Execute (in LocalTransform transform, in AgentCylinderShape shape, in AgentIndex agentIndex, in RVOAgent controller, in MovementControl control, ref ResolvedMovement resolved) {
				if (!agentIndex.TryGetIndex(ref agentDataVersions, out var index)) return;

				var scale = math.abs(transform.Scale);
				var r = shape.radius * scale * 3f;
				var area = quadtree.QueryArea(transform.Position, r);
				var density = area / (MaximumCirclePackingDensity * math.PI * r * r);

				resolved.targetPoint = agentOutputData.targetPoint[index];
				resolved.speed = agentOutputData.speed[index];
				var rnd = 1.0f; // (agentIndex.Index % 1024) / 1024f;
				resolved.turningRadiusMultiplier = math.max(1f, math.pow(density * 2.0f, 4.0f) * rnd);
			}
		}
	}
}
#endif
