#pragma warning disable CS0282 // "There is no defined ordering between fields in multiple declarations of partial struct"
#if MODULE_ENTITIES
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Profiling;
using Unity.Entities;
using Unity.Transforms;
using Unity.Profiling;

namespace Pathfinding {
	using Pathfinding.Drawing;
	using Pathfinding.Util;
	using Palette = Pathfinding.Drawing.Palette.Colorbrewer.Set1;
	using System;
	using Pathfinding.PID;
	using Pathfinding.ECS.RVO;
	using Pathfinding.ECS;
	using Pathfinding.Collections;
	using UnityEngine.Assertions;

	/// <summary>
	/// Movement script that uses ECS.
	///
	/// This script is a replacement for the <see cref="AIPath"/> and <see cref="RichAI"/> scripts.
	///
	/// This script is a movement script. It takes care of moving an agent along a path, updating the path, and so on.
	///
	/// \section followerentity-contents Contents
	/// \toc
	///
	/// \section followerentity-intro Introduction
	///
	/// The intended way to use this script is to use these two components:
	/// - <see cref="FollowerEntity"/>
	/// - <see cref="AIDestinationSetter"/> (optional, you can instead set the <see cref="destination"/> property manually)
	///
	/// Of note is that this component shouldn't be used with a <see cref="Seeker"/> component.
	/// It has its own settings for pathfinding instead, which are stored in the <see cref="pathfindingSettings"/> field.
	///
	/// When used with local avoidance, it also has its own settings for local avoidance which are stored in the <see cref="rvoSettings"/> field,
	/// instead of using a separate <see cref="RVOController"/> component.
	///
	/// Other than that, you can use it just like the other movement scripts in this package.
	///
	/// \section followerentity-features Features
	///
	/// - Uses Unity's ECS (Entity Component System) to move the agent. This means it is highly-performant and is able to utilize multiple threads.
	///   You do not need to know anything about ECS to be able to use it.
	/// - Supports local avoidance (see local-avoidance) (view in online documentation for working links).
	/// - Supports movement in both 2D and 3D games.
	/// - Supports movement on spherical on non-planar worlds (see spherical) (view in online documentation for working links).
	/// - Supports movement on grid graphs as well as navmesh/recast graphs.
	/// - Not recommended on hexagonal graphs at the moment (though it does mostly work).
	/// - Does <b>not</b> support movement on point graphs at the moment. This may be added in a future update.
	/// - Supports time-scales greater than 1. The agent will automatically run multiple simulation steps per frame if the time-scale is greater than 1, to ensure stability.
	/// - Supports off-mesh links. See offmeshlinks (view in online documentation for working links) for more info.
	/// - Knows which node it is traversing at all times (see <see cref="currentNode)"/>.
	/// - Automatically stops when trying to reach a crowded destination when using local avoidance.
	/// - Clamps the agent to the navmesh at all times.
	/// - Follows paths very smoothly.
	/// - Can keep a desired distance to walls.
	/// - Can approach its destination with a desired facing direction.
	///
	/// \section followerentity-inspector Inspector
	///
	/// <b>Shape</b>
	/// \inspectorField{Radius, radius}
	/// \inspectorField{Height, height}
	/// \inspectorField{Orientation, orientation}
	///
	/// <b>Movement</b>
	/// \inspectorField{Speed, maxSpeed}
	/// \inspectorField{Rotation Speed, rotationSpeed}
	/// \inspectorField{Max Rotation Speed, maxRotationSpeed}
	/// \inspectorField{Allow Rotating On The Spot, movementSettings.follower.allowRotatingOnSpot}
	/// \inspectorField{Max On Spot Rotation Speed, movementSettings.follower.maxOnSpotRotationSpeed}
	/// \inspectorField{Slowdown Time When Turning On Spot, movementSettings.follower.slowdownTimeWhenTurningOnSpot}
	/// \inspectorField{Position Smoothing, positionSmoothing}
	/// \inspectorField{Rotation Smoothing, rotationSmoothing}
	/// \inspectorField{Slowdown Time, movementSettings.follower.slowdownTime}
	/// \inspectorField{Stop Distance, movementSettings.stopDistance}
	/// \inspectorField{Lead In Radius When Approaching Destination, movementSettings.follower.leadInRadiusWhenApproachingDestination}
	/// \inspectorField{Desired Wall Distance, movementSettings.follower.desiredWallDistance}
	/// \inspectorField{Gravity, enableGravity}
	/// \inspectorField{Raycast Ground Mask, groundMask}
	/// \inspectorField{Movement Plane Source, movementPlaneSource}
	///
	/// <b>%Pathfinding</b>
	/// \inspectorField{Traversable Graphs, pathfindingSettings.graphMask}
	/// \inspectorField{Tags/Penalty, pathfindingSettings.tagPenalties}
	/// \inspectorField{Tags/Traversable, pathfindingSettings.traversableTags}
	/// \inspectorField{Recalculate Paths Automatically, autoRepath.mode}
	/// \inspectorField{Period, autoRepath.period}
	///
	/// <b>Local Avoidance</b>
	/// \inspectorField{Enable Local Avoidance, enableLocalAvoidance}
	/// \inspectorField{Agent Time Horizon, rvoSettings.agentTimeHorizon}
	/// \inspectorField{Obstacle Time Horizon, rvoSettings.obstacleTimeHorizon}
	/// \inspectorField{Max Neighbours, rvoSettings.maxNeighbours}
	/// \inspectorField{Layer, rvoSettings.layer}
	/// \inspectorField{Collides With, rvoSettings.collidesWith}
	/// \inspectorField{Priority, rvoSettings.priority}
	/// \inspectorField{Locked, rvoSettings.locked}
	///
	/// <b>Debug</b>
	/// \inspectorField{Movement Debug Rendering, debugFlags}
	/// \inspectorField{Local Avoidance Debug Rendering, rvoSettings.debug}
	/// \inspectorField{Reached Destination, reachedDestination}
	/// \inspectorField{Reached End Of Path, reachedEndOfPath}
	/// \inspectorField{Has Path, hasPath}
	/// \inspectorField{Path Pending, pathPending}
	/// \inspectorField{Destination, destination}
	/// \inspectorField{Remaining Distance, remainingDistance}
	/// \inspectorField{Speed, velocity}
	///
	/// \section followerentity-ecs ECS
	///
	/// This script uses Unity's ECS (Entity Component System) to move the agent. This means it is highly-performant and is able to utilize multiple threads.
	/// Internally, an entity is created for the agent with the following components:
	///
	/// - LocalTransform
	/// - <see cref="MovementState"/>
	/// - <see cref="MovementSettings"/>
	/// - <see cref="MovementControl"/>
	/// - <see cref="ManagedState"/>
	/// - <see cref="SearchState"/>
	/// - <see cref="MovementStatistics"/>
	/// - <see cref="AgentCylinderShape"/>
	/// - <see cref="ResolvedMovement"/>
	/// - <see cref="GravityState"/>
	/// - <see cref="DestinationPoint"/>
	/// - <see cref="AgentMovementPlane"/>
	/// - <see cref="SimulateMovement"/> - tag component (if <see cref="canMove"/> is enabled)
	/// - <see cref="SimulateMovementRepair"/> - tag component
	/// - <see cref="SimulateMovementControl"/> - tag component
	/// - <see cref="SimulateMovementFinalize"/> - tag component
	/// - <see cref="SyncPositionWithTransform"/> - tag component (if <see cref="updatePosition"/> is enabled)
	/// - <see cref="SyncRotationWithTransform"/> - tag component (if <see cref="updateRotation"/> is enabled)
	/// - <see cref="OrientationYAxisForward"/> - tag component (if <see cref="orientation"/> is <see cref="OrientationMode"/>.YAxisForward)
	/// - <see cref="ECS.RVO.RVOAgent"/> (if local avoidance is enabled)
	/// - <see cref="PhysicsSceneRef"/> - shared component
	///
	/// Then this script barely does anything by itself. It is a thin wrapper around the ECS components.
	/// Instead, actual movement calculations are carried out by the following systems:
	///
	/// - <see cref="SyncTransformsToEntitiesSystem"/> - Updates the entity's transform from the GameObject.
	/// - <see cref="MovementPlaneFromGraphSystem"/> - Updates the agent's movement plane.
	/// - <see cref="SyncDestinationTransformSystem"/> - Updates the destination point if the destination transform moves.
	/// - <see cref="RepairPathSystem"/> - Keeps the agent's path up to date.
	/// - <see cref="FollowerControlSystem"/> - Calculates how the agent wants to move.
	/// - <see cref="RVOSystem"/> - Local avoidance calculations.
	/// - <see cref="FallbackResolveMovementSystem"/> - NOOP system for if local avoidance is disabled.
	/// - <see cref="AIMoveSystem"/> - Performs the actual movement.
	///
	/// In fact, as long as you create the appropriate ECS components, you do not even need this script. You can use the systems directly.
	///
	/// This is <b>not</b> a baked component. That is, this script will continue to work even in standalone games. It is designed to be easily used
	/// without having to care too much about the underlying ECS implementation.
	///
	/// \section followerentity-differences Differences compared to AIPath and RichAI
	///
	/// This movement script has been written to remedy several inconsistency issues with other movement scrips, to provide very smooth movement,
	/// and "just work" for most games.
	///
	/// For example, it goes to great lengths to ensure
	/// that the <see cref="reachedDestination"/> and <see cref="reachedEndOfPath"/> properties are as accurate as possible at all times, even before it has had time to recalculate its path to account for a new <see cref="destination"/>.
	/// It does this by locally repairing the path (if possible) immediately when the destination changes instead of waiting for a path recalculation.
	/// This also has a bonus effect that the agent can often work just fine with moving targets, even if it almost never recalculates its path (though the repaired path may not always be optimal),
	/// and it leads to very responsive movement.
	///
	/// In contrast to other movement scripts, this movement script does not use path modifiers at all.
	/// Instead, this script contains its own internal <see cref="FunnelModifier"/> which it uses to simplify the path before it follows it.
	/// In also doesn't use a separate <see cref="RVOController"/> component for local avoidance, but instead it stores local avoidance settings in <see cref="rvoSettings"/>.
	///
	/// \section followerentity-bestpractices Best practices for good performance
	///
	/// Here are some tips for how to improve performance when using this script.
	/// As always, make sure to profile your game first, to see what is actually causing performance problems.
	///
	/// <b>Disable unused features</b>
	/// This script has some optional parts. Local avoidance, for example. Local avoidance is used to make sure that agents do not overlap with each other.
	/// However, if you do not need it, you can disable it to improve performance.
	///
	/// <b>Don't change the destination unnecessarily</b>
	/// Repairing the path each frame can be a significant part of the movement calculation time. The FollowerEntity will perform better
	/// if the <see cref="destination"/> is static, or moves seldom. For example, updating the destination every 10 frames will be faster than updating it every frame,
	/// but to the player, both will look basically the same.
	///
	/// Note: Repairing the path is different from recalculating it from scratch. The agent will recalculate the path from scratch relatively seldom,
	/// but it will repair it every frame, if necessary, to account for small changes in the agent's position and destination.
	///
	/// <b>Disable debug rendering</b>
	/// Debug rendering has some performance costs in the Unity Editor. Disable all <see cref="debugFlags"/> and <see cref="rvoSettings.debug"/> to improve performance.
	/// However, in standalone builds, these are automatically disabled and have no cost.
	///
	/// <b>Be aware of property access costs</b>
	/// Using ECS components has some downsides. Accessing properties on this script is significantly slower compared to accessing properties on other movement scripts.
	/// This is because on each property access, the script has to make sure no jobs are running concurrently, which is a relatively expensive operation.
	/// Slow is a relative term, though. This only starts to matter if you have lots of agents, maybe a hundred or so. So don't be scared of using it.
	///
	/// But if you have a lot of agents, it is recommended to not access properties on this script more often than required. Avoid setting fields to the same value over and over again every frame, for example.
	/// If you have a moving target, try to use the <see cref="AIDestinationSetter"/> component instead of setting the <see cref="destination"/> property manually, as that is faster than setting the <see cref="destination"/> property every frame.
	///
	/// You can instead write custom ECS systems to access the properties on the ECS components directly. This is much faster.
	/// For example, if you want to make the agent follow a particular entity, you could create a new DestinationEntity component which just holds an entity reference,
	/// and then create a system that every frame copies that entity's position to the <see cref="DestinationPoint.destination"/> field (a component that this entity will always have).
	///
	/// \section followerentity-timescale Time scaling
	/// This component will automatically run multiple simulation steps per frame if the time scale is greater than 1.
	/// This is done to ensure that the movement remains stable even at high time scales.
	/// One case when this happens is when fast-forwarding games, which is common in some types of city builders and other types of simulation games.
	/// This will impact performance at high time scales, but it is necessary to ensure that the movement remains stable.
	/// </summary>
	[AddComponentMenu("Pathfinding/AI/Follower Entity (2D,3D)")]
	[UniqueComponent(tag = "ai")]
	[UniqueComponent(tag = "rvo")]
	[DisallowMultipleComponent]
	public sealed partial class FollowerEntity : VersionedMonoBehaviour, IAstarAI, ISerializationCallbackReceiver {
		[SerializeField]
		AgentCylinderShape shape = new AgentCylinderShape {
			height = 2,
			radius = 0.5f,
		};
		[SerializeField]
		MovementSettings movement = new MovementSettings {
			follower = new PIDMovement {
				rotationSpeed = 600,
				speed = 5,
				maxRotationSpeed = 720,
				maxOnSpotRotationSpeed = 720,
				slowdownTime = 0.5f,
				desiredWallDistance = 0.5f,
				allowRotatingOnSpot = true,
				leadInRadiusWhenApproachingDestination = 1f,
			},
			stopDistance = 0.2f,
			rotationSmoothing = 0f,
			groundMask = -1,
			isStopped = false,
			debugFlags = PIDMovement.DebugFlags.Path,
		};

		[SerializeField]
		ManagedState managedState = new ManagedState {
			enableLocalAvoidance = false,
			pathfindingSettings = PathRequestSettings.Default,
		};

		[SerializeField]
		ECS.AutoRepathPolicy autoRepathBacking = ECS.AutoRepathPolicy.Default;

		/// <summary>
		/// Determines which direction the agent moves in.
		///
		/// See: <see cref="orientation"/>
		/// </summary>
		[SerializeField]
		OrientationMode orientationBacking;
		[SerializeField]
		MovementPlaneSource movementPlaneSourceBacking = MovementPlaneSource.Graph;

		/// <summary>\copydocref{updatePosition}</summary>
		[SerializeField]
		bool syncPosition = true;

		/// <summary>\copydocref{updateRotation}</summary>
		[SerializeField]
		bool syncRotation = true;

		/// <summary>Cached transform component</summary>
		Transform tr;

		/// <summary>
		/// Entity which this movement script represents.
		///
		/// An entity will be created when this script is enabled, and destroyed when this script is disabled.
		///
		/// Check the class documentation to see which components it usually has, and what systems typically affect it.
		/// </summary>
		public Entity entity { [IgnoredByDeepProfiler] get; private set; }

		static EntityAccess<DestinationPoint> destinationPointAccessRW = new EntityAccess<DestinationPoint>(false);
		static EntityAccess<DestinationPoint> destinationPointAccessRO = new EntityAccess<DestinationPoint>(true);
		static EntityAccess<AgentMovementPlane> movementPlaneAccessRW = new EntityAccess<AgentMovementPlane>(false);
		static EntityAccess<AgentMovementPlane> movementPlaneAccessRO = new EntityAccess<AgentMovementPlane>(false);
		static EntityAccess<MovementState> movementStateAccessRW = new EntityAccess<MovementState>(false);
		static EntityAccess<MovementState> movementStateAccessRO = new EntityAccess<MovementState>(true);
		static EntityAccess<MovementStatistics> movementOutputAccessRW = new EntityAccess<MovementStatistics>(false);
		static EntityAccess<ResolvedMovement> resolvedMovementAccessRO = new EntityAccess<ResolvedMovement>(true);
		static EntityAccess<ResolvedMovement> resolvedMovementAccessRW = new EntityAccess<ResolvedMovement>(false);
		static EntityAccess<MovementControl> movementControlAccessRO = new EntityAccess<MovementControl>(true);
		static EntityAccess<MovementControl> movementControlAccessRW = new EntityAccess<MovementControl>(false);
		static EntityAccess<MovementStatistics> movementStatisticsAccessRW = new EntityAccess<MovementStatistics>(false);
		static ManagedEntityAccess<ManagedState> managedStateAccessRO = new ManagedEntityAccess<ManagedState>(true);
		static ManagedEntityAccess<ManagedState> managedStateAccessRW = new ManagedEntityAccess<ManagedState>(false);
		static EntityAccess<ECS.AutoRepathPolicy> autoRepathPolicyRW = new EntityAccess<ECS.AutoRepathPolicy>(false);
		static EntityAccess<LocalTransform> localTransformAccessRO = new EntityAccess<LocalTransform>(true);
		static EntityAccess<LocalTransform> localTransformAccessRW = new EntityAccess<LocalTransform>(false);
		static EntityAccess<AgentCylinderShape> agentCylinderShapeAccessRO = new EntityAccess<AgentCylinderShape>(true);
		static EntityAccess<AgentCylinderShape> agentCylinderShapeAccessRW = new EntityAccess<AgentCylinderShape>(false);
		static EntityAccess<MovementSettings> movementSettingsAccessRO = new EntityAccess<MovementSettings>(true);
		static EntityAccess<MovementSettings> movementSettingsAccessRW = new EntityAccess<MovementSettings>(false);
		static EntityAccess<AgentOffMeshLinkTraversal> agentOffMeshLinkTraversalRO = new EntityAccess<AgentOffMeshLinkTraversal>(true);
		static EntityAccess<ReadyToTraverseOffMeshLink> readyToTraverseOffMeshLinkRW = new EntityAccess<ReadyToTraverseOffMeshLink>(false);
		static EntityStorageCache entityStorageCache;

		static EntityArchetype archetype;
		static World achetypeWorld;

#if !UNITY_2023_1_OR_NEWER
		bool didStart;
#endif

		void OnEnable () {
			scratchReferenceCount++;
			FindComponents();
			entity = CreateEntity(tr.position, tr.rotation, tr.localScale.x, ref shape, ref movement, ref autoRepathBacking, managedState, orientationBacking, movementPlaneSourceBacking, syncPosition, syncRotation, PhysicsSceneExtensions.GetPhysicsScene(gameObject.scene));

			// Register with the BatchedEvents system
			// This is used not for the events, but because it keeps track of a TransformAccessArray
			// of all components. This is then used by the SyncTransformsToEntitiesSystem.
			BatchedEvents.Add(this, BatchedEvents.Event.None, (components, ev) => {});

			var runtimeBakers = GetComponents<IRuntimeBaker>();
			for (int i = 0; i < runtimeBakers.Length; i++) if (((MonoBehaviour)runtimeBakers[i]).enabled) runtimeBakers[i].OnCreatedEntity(World.DefaultGameObjectInjectionWorld, entity);

			// Make sure Start runs every time after OnEnable.
			// When the game starts we don't want it to run immediately, because the graphs may not be scanned.
			// But if the component is enabled at some later point in the game, it can run at the same time as OnEnable.
			if (didStart) Start();
		}

		/// <summary>
		/// Creates an entity with the given data.
		///
		/// If you don't want to use the FollowerEntity MonoBehaviour, you can use this method to create an equivalent entity directly.
		/// </summary>
		public static Entity CreateEntity (float3 position, quaternion rotation, float scale, ref AgentCylinderShape shape, ref MovementSettings movement, ref ECS.AutoRepathPolicy autoRepath, ManagedState managedState, OrientationMode orientation, MovementPlaneSource movementPlaneSource, bool updatePosition, bool updateRotation, PhysicsScene physicsScene) {
			var world = World.DefaultGameObjectInjectionWorld;
			if (!archetype.Valid || achetypeWorld != world) {
				if (world == null) throw new Exception("World.DefaultGameObjectInjectionWorld is null. Has the world been destroyed?");
				achetypeWorld = world;
				archetype = world.EntityManager.CreateArchetype(
					typeof(LocalTransform),
					typeof(MovementState),
					typeof(MovementSettings),
					typeof(ECS.AutoRepathPolicy),
					typeof(MovementControl),
					typeof(ManagedState),
					typeof(SearchState),
					typeof(MovementStatistics),
					typeof(AgentCylinderShape),
					typeof(ResolvedMovement),
					typeof(DestinationPoint),
					typeof(AgentMovementPlane),
					typeof(GravityState),
					typeof(SimulateMovement),
					typeof(SimulateMovementRepair),
					typeof(SimulateMovementControl),
					typeof(SimulateMovementFinalize),
					typeof(SyncPositionWithTransform),
					typeof(SyncRotationWithTransform),
					typeof(ReadyToTraverseOffMeshLink),
					typeof(AgentMovementPlaneSource),
					typeof(PhysicsSceneRef)
					);
			}

			if (orientation == OrientationMode.YAxisForward) rotation = math.mul(rotation, SyncTransformsToEntitiesSystem.YAxisForwardToZAxisForward);

			var entityManager = world.EntityManager;
			var entity = entityManager.CreateEntity(archetype);
			// This GameObject may be in a hierarchy, but the entity will not be. So we copy the world orientation to the entity's local transform component
			entityManager.SetComponentData(entity, LocalTransform.FromPositionRotationScale(position, rotation, scale));
			entityManager.SetComponentData(entity, new MovementState(position));
#if UNITY_EDITOR
			entityManager.SetName(entity, "Follower Entity");
#endif
			entityManager.SetComponentData(entity, new DestinationPoint {
				destination = new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity),
			});
			autoRepath.Reset();
			entityManager.SetComponentData(entity, autoRepath);
			entityManager.SetComponentData(entity, movement);
			if (!managedState.pathTracer.isCreated) {
				managedState.pathTracer = new PathTracer(Allocator.Persistent);
			}
			entityManager.SetComponentData(entity, managedState);
			entityManager.SetComponentData(entity, new MovementStatistics {
				estimatedVelocity = float3.zero,
				lastPosition = position,
			});
			entityManager.SetComponentData(entity, shape);
			entityManager.SetComponentEnabled<GravityState>(entity, managedState.enableGravity);
			if (orientation == OrientationMode.YAxisForward) {
				entityManager.AddComponent<OrientationYAxisForward>(entity);
			}
			entityManager.SetComponentEnabled<ReadyToTraverseOffMeshLink>(entity, false);
			entityManager.SetSharedComponent(entity, new AgentMovementPlaneSource { value = movementPlaneSource });
			entityManager.SetSharedComponent(entity, new PhysicsSceneRef { physicsScene = physicsScene });
			ToggleComponent<SyncPositionWithTransform>(entity, updatePosition, true);
			ToggleComponent<SyncRotationWithTransform>(entity, updateRotation, true);

			ResolvedMovement resolvedMovement = default;
			MovementControl movementControl = default;
			AgentMovementPlane movementPlane = new AgentMovementPlane(rotation);
			ResetControl(ref resolvedMovement, ref movementControl, ref movementPlane, position, rotation, position);

			// Set the initial movement plane. This will be overriden before the first simulation loop runs.
			entityManager.SetComponentData(entity, movementPlane);

			entityManager.SetComponentData(entity, resolvedMovement);
			entityManager.SetComponentData(entity, movementControl);

			return entity;
		}

		internal void RegisterRuntimeBaker (IRuntimeBaker baker) {
			if (entityExists) baker.OnCreatedEntity(World.DefaultGameObjectInjectionWorld, entity);
		}

		/// <summary>Cached NNConstraint, to avoid allocations</summary>
		static NNConstraint ScratchNNConstraint = NNConstraint.Walkable;

		void Start () {
#if !UNITY_2023_1_OR_NEWER
			didStart = true;
#endif
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			managedStateAccessRW.Update(entityManager);
			movementPlaneAccessRW.Update(entityManager);
			resolvedMovementAccessRW.Update(entityManager);
			movementControlAccessRW.Update(entityManager);
			if (!managedState.pathTracer.hasPath && AstarPath.active != null) {
				// Since we haven't calculated a path yet,
				ScratchNNConstraint.UseSettings(managedState.pathfindingSettings);
				var nearest = AstarPath.active.GetNearest(position, ScratchNNConstraint);

				if (nearest.node != null) {
					var storage = entityManager.GetStorageInfo(entity);
					ref var movementPlane = ref movementPlaneAccessRW[storage];
					ref var resolvedMovement = ref resolvedMovementAccessRW[storage];
					ref var movementControl = ref movementControlAccessRW[storage];

					// If we are using the graph's natural movement plane, we need to update our movement plane from the graph
					// before we start repairing the path. Otherwise the agent can get snapped to a weird point on the navmesh,
					// if its initial rotation was not aligned with the graph.
					if (movementPlaneSource == MovementPlaneSource.Graph) {
						// The target rotations are relative to the movement plane, so we need to patch it, to make sure it stays constant in world space.
						// This is important when the agent starts with isStopped=true, because the targetRotation will not be recalculated every frame.
						// TODO: Alternatively we could make sure to always make the new movement plane as similar as possible to the old one,
						// but this has a minor performance impact every frame.
						var targetRotation = movementPlane.value.ToWorldRotation(resolvedMovement.targetRotation);
						var targetRotationHint = movementPlane.value.ToWorldRotation(resolvedMovement.targetRotationHint);
						var targetRotation2 = movementPlane.value.ToWorldRotation(movementControl.targetRotation);
						var targetRotationHint2 = movementPlane.value.ToWorldRotation(movementControl.targetRotationHint);

						movementPlane = new AgentMovementPlane(MovementPlaneFromGraphSystem.MovementPlaneFromGraph(nearest.node.Graph));
						// TODO: Do we need to do a similar thing for the raycast and navmesh normal cases?

						resolvedMovement.targetRotation = movementPlane.value.ToPlane(targetRotation);
						resolvedMovement.targetRotationHint = movementPlane.value.ToPlane(targetRotationHint);
						movementControl.targetRotation = movementPlane.value.ToPlane(targetRotation2);
						movementControl.targetRotationHint = movementPlane.value.ToPlane(targetRotationHint2);
					}

					// Make the agent's path consist of a single node at the current position.
					// This is temporary and will be replaced by the actual path when it is calculated.
					// This allows it to be clamped to the navmesh immediately, instead of waiting for a destination to be set and a path to be calculated.
					managedState.pathTracer.SetFromSingleNode(nearest.node, nearest.position, movementPlane.value, managedState.pathfindingSettings);
					// Make the end of the path be unset
					managedState.pathTracer.UpdateEnd(Vector3.positiveInfinity, PathTracer.RepairQuality.High, movementPlane.value, null, null);
				}
			}
		}

		/// <summary>
		/// Called when the component is disabled or about to be destroyed.
		///
		/// This is also called by Unity when an undo/redo event is performed. This means that
		/// when an undo event happens the entity will get destroyed and then re-created.
		/// </summary>
		void OnDisable () {
			scratchReferenceCount--;
			if (scratchReferenceCount == 0) {
				if (indicesScratch.IsCreated) indicesScratch.Dispose();
				if (nextCornersScratch.IsCreated) nextCornersScratch.Dispose();
			}

			BatchedEvents.Remove(this);
			CancelCurrentPathRequest();
			if (World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.IsCreated) World.DefaultGameObjectInjectionWorld.EntityManager.DestroyEntity(entity);
			// Make sure the managed state gets disposed, even if no entity exists. If an entity exists, this will be automatically called.
			managedState.Dispose();

			// Note: The entity itself may actually live for another frame to handle cleanup components.
			// But we want to completely forget about it here.
			entity = default;
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::radius</summary>
		public float radius {
			get => shape.radius;
			set {
				this.shape.radius = value;
				if (entityStorageCache.GetComponentData(entity, ref agentCylinderShapeAccessRW, out var shape)) {
					shape.value.radius = value;
				}
			}
		}

		/// <summary>
		/// Height of the agent in world units.
		/// This is visualized in the scene view as a yellow cylinder around the character.
		///
		/// This value is used for various heuristics, and for visualization purposes.
		/// For example, the destination is only considered reached if the destination is not above the agent's head, and it's not more than half the agent's height below its feet.
		///
		/// If local lavoidance is enabled, this is also used to filter out collisions with agents and obstacles that are too far above or below the agent.
		/// </summary>
		public float height {
			get => shape.height;
			set {
				this.shape.height = value;
				if (entityStorageCache.GetComponentData(entity, ref agentCylinderShapeAccessRW, out var shape)) {
					shape.value.height = value;
				}
			}
		}

		/// <summary>
		/// %Pathfinding settings.
		///
		/// The settings in this struct controls how the agent calculates paths to its destination.
		///
		/// This is analogous to the <see cref="Seeker"/> component used for other movement scripts.
		///
		/// See: <see cref="PathRequestSettings"/>
		/// </summary>
		public ref PathRequestSettings pathfindingSettings {
			get {
				// Complete any job dependencies
				// Need RW because this getter has a ref return.
				entityStorageCache.GetComponentData(entity, ref movementStateAccessRW, out var _);
				return ref managedState.pathfindingSettings;
			}
		}

		/// <summary>Local avoidance settings</summary>
		public ref RVOAgent rvoSettings {
			get {
				// Complete any job dependencies
				// Need RW because this getter has a ref return.
				entityStorageCache.GetComponentData(entity, ref movementStateAccessRW, out var _);
				return ref managedState.rvoSettings;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::position</summary>
		public Vector3 position {
			get {
				// Make sure we are not waiting for a job to update the world position
				if (entityStorageCache.GetComponentData(entity, ref localTransformAccessRO, out var localTransform)) {
					return localTransform.value.Position;
				} else {
					return transform.position;
				}
			}
			set {
				if (entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out var entityManager, out var storage)) {
					// Update path and other properties using our new position
					if (entityManager.HasComponent<SyncPositionWithTransform>(entity)) {
						transform.position = value;
					}
					movementStateAccessRW.Update(entityManager);
					managedStateAccessRW.Update(entityManager);
					agentCylinderShapeAccessRO.Update(entityManager);
					movementSettingsAccessRO.Update(entityManager);
					destinationPointAccessRO.Update(entityManager);
					movementPlaneAccessRO.Update(entityManager);
					localTransformAccessRW.Update(entityManager);
					readyToTraverseOffMeshLinkRW.Update(entityManager);
					autoRepathPolicyRW.Update(entityManager);

					ref var localTransform = ref localTransformAccessRW[storage];
					localTransform.Position = value;
					ref var movementState = ref movementStateAccessRW[storage];
					movementState.positionOffset = float3.zero;
					if (managedState.pathTracer.hasPath) {
						Profiler.BeginSample("RepairStart");
						ref var movementPlane = ref movementPlaneAccessRO[storage];
						var oldVersion = managedState.pathTracer.version;
						managedState.pathTracer.UpdateStart(value, PathTracer.RepairQuality.High, movementPlane.value, managedState.pathfindingSettings.traversalProvider, managedState.activePath);
						Profiler.EndSample();
						if (managedState.pathTracer.version != oldVersion) {
							Profiler.BeginSample("EstimateNative");
							ref var shape = ref agentCylinderShapeAccessRO[storage];
							ref var movementSettings = ref movementSettingsAccessRO[storage];
							ref var destinationPoint = ref destinationPointAccessRO[storage];
							ref var autoRepath = ref autoRepathPolicyRW[storage];
							var readyToTraverseOffMeshLink = storage.Chunk.GetEnabledMask(ref readyToTraverseOffMeshLinkRW.handle).GetEnabledRefRW<ReadyToTraverseOffMeshLink>(storage.IndexInChunk);
							if (!nextCornersScratch.IsCreated) nextCornersScratch = new NativeList<float3>(4, Allocator.Persistent);
							JobRepairPath.Execute(
								ref localTransform,
								ref movementState,
								ref shape,
								ref movementPlane,
								ref autoRepath,
								ref destinationPoint,
								readyToTraverseOffMeshLink,
								managedState,
								in movementSettings,
								nextCornersScratch,
								ref indicesScratch,
								Allocator.Persistent,
								false
								);
							Profiler.EndSample();
						}
					}
				} else {
					transform.position = value;
				}
			}
		}

		/// <summary>
		/// True if the agent is currently traversing an off-mesh link.
		///
		/// See: offmeshlinks (view in online documentation for working links)
		/// See: <see cref="onTraverseOffMeshLink"/>
		/// See: <see cref="offMeshLink"/>
		/// </summary>
		public bool isTraversingOffMeshLink {
			get {
				if (!entityExists) return false;

				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				return entityManager.HasComponent<AgentOffMeshLinkTraversal>(entity);
			}
		}

		/// <summary>
		/// The off-mesh link that the agent is currently traversing.
		///
		/// This will be a default <see cref="OffMeshLinks.OffMeshLinkTracer"/> if the agent is not traversing an off-mesh link (the <see cref="OffMeshLinks.OffMeshLinkTracer.link"/> field will be null).
		///
		/// Note: If the off-mesh link is destroyed while the agent is traversing it, this property will still return the link.
		/// But be careful about accessing properties like <see cref="OffMeshLinkSource.gameObject"/>, as that may refer to a destroyed gameObject.
		///
		/// See: offmeshlinks (view in online documentation for working links)
		/// See: <see cref="onTraverseOffMeshLink"/>
		/// See: <see cref="isTraversingOffMeshLink"/>
		/// </summary>
		public OffMeshLinks.OffMeshLinkTracer offMeshLink {
			get {
				if (entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out var entityManager, out var storage) && entityManager.HasComponent<AgentOffMeshLinkTraversal>(entity)) {
					agentOffMeshLinkTraversalRO.Update(entityManager);
					var linkTraversal = agentOffMeshLinkTraversalRO[storage];
					var linkTraversalManaged = entityManager.GetComponentData<ManagedAgentOffMeshLinkTraversal>(entity);
					return new OffMeshLinks.OffMeshLinkTracer(linkTraversalManaged.context.concreteLink, linkTraversal.relativeStart, linkTraversal.relativeEnd, linkTraversal.isReverse);
				} else {
					return default;
				}
			}
		}

		/// <summary>
		/// Callback to be called when an agent starts traversing an off-mesh link.
		///
		/// The handler will be called when the agent starts traversing an off-mesh link.
		/// It allows you to to control the agent for the full duration of the link traversal.
		///
		/// Use the passed context struct to get information about the link and to control the agent.
		///
		/// <code>
		/// using UnityEngine;
		/// using Pathfinding;
		/// using System.Collections;
		/// using Pathfinding.ECS;
		///
		/// namespace Pathfinding.Examples {
		///     public class FollowerJumpLink : MonoBehaviour, IOffMeshLinkHandler, IOffMeshLinkStateMachine {
		///         // Register this class as the handler for off-mesh links when the component is enabled
		///         void OnEnable() => GetComponent<NodeLink2>().onTraverseOffMeshLink = this;
		///         void OnDisable() => GetComponent<NodeLink2>().onTraverseOffMeshLink = null;
		///
		///         IOffMeshLinkStateMachine IOffMeshLinkHandler.GetOffMeshLinkStateMachine(AgentOffMeshLinkTraversalContext context) => this;
		///
		///         void IOffMeshLinkStateMachine.OnFinishTraversingOffMeshLink (AgentOffMeshLinkTraversalContext context) {
		///             Debug.Log("An agent finished traversing an off-mesh link");
		///         }
		///
		///         void IOffMeshLinkStateMachine.OnAbortTraversingOffMeshLink () {
		///             Debug.Log("An agent aborted traversing an off-mesh link");
		///         }
		///
		///         IEnumerable IOffMeshLinkStateMachine.OnTraverseOffMeshLink (AgentOffMeshLinkTraversalContext ctx) {
		///             var start = (Vector3)ctx.link.relativeStart;
		///             var end = (Vector3)ctx.link.relativeEnd;
		///             var dir = end - start;
		///
		///             // Disable local avoidance while traversing the off-mesh link.
		///             // If it was enabled, it will be automatically re-enabled when the agent finishes traversing the link.
		///             ctx.DisableLocalAvoidance();
		///
		///             // Move and rotate the agent to face the other side of the link.
		///             // When reaching the off-mesh link, the agent may be facing the wrong direction.
		///             while (!ctx.MoveTowards(
		///                 position: start,
		///                 rotation: Quaternion.LookRotation(dir, ctx.movementPlane.up),
		///                 gravity: true,
		///                 slowdown: true).reached) {
		///                 yield return null;
		///             }
		///
		///             var bezierP0 = start;
		///             var bezierP1 = start + Vector3.up*5;
		///             var bezierP2 = end + Vector3.up*5;
		///             var bezierP3 = end;
		///             var jumpDuration = 1.0f;
		///
		///             // Animate the AI to jump from the start to the end of the link
		///             for (float t = 0; t < jumpDuration; t += ctx.deltaTime) {
		///                 ctx.transform.Position = AstarSplines.CubicBezier(bezierP0, bezierP1, bezierP2, bezierP3, Mathf.SmoothStep(0, 1, t / jumpDuration));
		///                 yield return null;
		///             }
		///         }
		///     }
		/// }
		/// </code>
		///
		/// Warning: Off-mesh links can be destroyed or disabled at any moment. The built-in code will attempt to make the agent continue following the link even if it is destroyed,
		/// but if you write your own traversal code, you should be aware of this.
		///
		/// You can alternatively set the corresponding property property on the off-mesh link (<see cref="NodeLink2.onTraverseOffMeshLink"/>) to specify a callback for a specific off-mesh link.
		///
		/// Note: The agent's off-mesh link handler takes precedence over the link's off-mesh link handler, if both are set.
		///
		/// See: offmeshlinks (view in online documentation for working links) for more details and example code
		/// See: <see cref="isTraversingOffMeshLink"/>
		/// </summary>
		public IOffMeshLinkHandler onTraverseOffMeshLink {
			get => managedState.onTraverseOffMeshLink;
			set {
				// Complete any job dependencies
				entityStorageCache.GetComponentData(entity, ref movementStateAccessRW, out var _);
				managedState.onTraverseOffMeshLink = value;
			}
		}

		/// <summary>
		/// Node which the agent is currently traversing.
		///
		/// You can, for example, use this to make the agent use a different animation when traversing nodes with a specific tag.
		///
		/// Note: Will be null if the agent does not have a path, or if the node under the agent has just been destroyed by a graph update.
		///
		/// When traversing an off-mesh link, this will return the final non-link node in the path before the agent started traversing the link.
		/// </summary>
		public GraphNode currentNode {
			get {
				if (!entityExists) return null;

				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				// Complete any job dependencies
				managedStateAccessRO.Update(entityManager);
				var node = managedState.pathTracer.startNode;
				if (node == null || node.Destroyed) return null;
				return node;
			}
		}

		/// <summary>
		/// Rotation of the agent.
		/// In world space.
		///
		/// The entity internally always treats the Z axis as forward, but this property respects the <see cref="orientation"/> field. So it
		/// will return either a rotation with the Y axis as forward, or Z axis as forward, depending on the <see cref="orientation"/> field.
		///
		/// Note: if <see cref="updateRotation"/> is true (which is the default), this will also set the transform's rotation.
		/// If <see cref="updateRotation"/> is false, only the agent's internal rotation will be set.
		///
		/// This will return the agent's rotation even if <see cref="updateRotation"/> is false.
		///
		/// See: <see cref="position"/>
		/// </summary>
		public Quaternion rotation {
			get {
				if (entityStorageCache.GetComponentData(entity, ref localTransformAccessRO, out var localTransform)) {
					var r = localTransform.value.Rotation;
					if (orientation == OrientationMode.YAxisForward) r = math.mul(r, SyncTransformsToEntitiesSystem.ZAxisForwardToYAxisForward);
					return r;
				} else {
					return transform.rotation;
				}
			}
			set {
				if (entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out var entityManager, out var storage)) {
					// Update path and other properties using our new position
					if (entityManager.HasComponent<SyncRotationWithTransform>(entity)) {
						transform.rotation = value;
					}

					if (orientation == OrientationMode.YAxisForward) value = math.mul(value, SyncTransformsToEntitiesSystem.YAxisForwardToZAxisForward);
					localTransformAccessRW.Update(entityManager);
					localTransformAccessRW[storage].Rotation = value;
				} else {
					if (updateRotation) {
						transform.rotation = value;
					} else {
						Debug.LogWarning("Cannot set agent rotation because updateRotation is false and the FollowerEntity component has not been enabled yet. Therefore, the internal entity does not exist, and there's no rotation to set.", this);
					}
				}
			}
		}

		/// <summary>
		/// How to calculate which direction is "up" for the agent.
		///
		/// In almost all cases, you should use the Graph option. This will make the agent use the graph's natural "up" direction.
		/// However, if you are using a spherical world, or a world with some other strange shape, then you may want to use the NavmeshNormal or Raycast options.
		///
		/// See: spherical (view in online documentation for working links)
		/// </summary>
		public MovementPlaneSource movementPlaneSource {
			get => movementPlaneSourceBacking;
			set {
				movementPlaneSourceBacking = value;
				if (entityExists) {
					var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
					entityManager.SetSharedComponent(entity, new AgentMovementPlaneSource { value = value });
				}
			}
		}

		/// <summary>
		/// Determines which layers the agent will stand on.
		///
		/// The agent will use a raycast each frame to check if it should stop falling.
		///
		/// This layer mask should ideally not contain the agent's own layer, if the agent has a collider,
		/// as this may cause it to try to stand on top of itself.
		/// </summary>
		public LayerMask groundMask {
			get => movement.groundMask;
			set {
				movement.groundMask = value;
				if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
					movementSettings.value.groundMask = value;
				}
			}
		}

		/// <summary>
		/// Enables or disables debug drawing for this agent.
		///
		/// This is a bitmask with multiple flags so that you can choose exactly what you want to debug.
		///
		/// See: <see cref="PIDMovement.DebugFlags"/>
		/// See: bitmasks (view in online documentation for working links)
		/// </summary>
		public PIDMovement.DebugFlags debugFlags {
			get => movement.debugFlags;
			set {
				movement.debugFlags = value;
				if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
					movementSettings.value.debugFlags = value;
				}
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::maxSpeed</summary>
		public float maxSpeed {
			get => movement.follower.speed;
			set {
				movement.follower.speed = value;
				if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
					movementSettings.value.follower.speed = value;
				}
			}
		}

		/// <summary>\copydocref{PIDMovement.rotationSpeed}</summary>
		public float rotationSpeed {
			get => movement.follower.rotationSpeed;
			set {
				movement.follower.rotationSpeed = value;
				if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
					movementSettings.value.follower.rotationSpeed = value;
				}
			}
		}

		/// <summary>\copydocref{PIDMovement.maxRotationSpeed}</summary>
		public float maxRotationSpeed {
			get => movement.follower.maxRotationSpeed;
			set {
				movement.follower.maxRotationSpeed = value;
				if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
					movementSettings.value.follower.maxRotationSpeed = value;
				}
			}
		}

		/// <summary>
		/// Actual velocity that the agent is moving with.
		/// In world units per second.
		///
		/// This is useful for, for example, selecting which animations to play, and at what speeds.
		///
		/// Note: Any value set here will be overriden during the next simulation step. Nevertheless, it can be useful to set this value if you have disabled the agent's movement logic using e.g. <see cref="canMove"/>.
		/// This value is only an output statistic. It is not used to control the agent's movement.
		///
		/// See: <see cref="desiredVelocity"/>
		/// </summary>
		public Vector3 velocity {
			get {
				return entityExists ? (Vector3)World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<MovementStatistics>(entity).estimatedVelocity : Vector3.zero;
			}
			set {
				if (entityStorageCache.GetComponentData(entity, ref movementStatisticsAccessRW, out var statistics)) {
					statistics.value.estimatedVelocity = (float3)value;
				}
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::desiredVelocity</summary>
		public Vector3 desiredVelocity {
			get {
				if (entityStorageCache.GetComponentData(entity, ref resolvedMovementAccessRO, out var resolvedMovement)) {
					var dt = Mathf.Max(Time.deltaTime, 0.0001f);
					return Vector3.ClampMagnitude((Vector3)resolvedMovement.value.targetPoint - position, dt * resolvedMovement.value.speed) / dt;
				} else {
					return Vector3.zero;
				}
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::desiredVelocityWithoutLocalAvoidance</summary>
		public Vector3 desiredVelocityWithoutLocalAvoidance {
			get {
				if (entityStorageCache.GetComponentData(entity, ref movementControlAccessRO, out var movementControl)) {
					var dt = Mathf.Max(Time.deltaTime, 0.0001f);
					return Vector3.ClampMagnitude((Vector3)movementControl.value.targetPoint - position, dt * movementControl.value.speed) / dt;
				} else {
					return Vector3.zero;
				}
			}
			set => throw new NotImplementedException("The FollowerEntity does not support setting this property. If you want to override the movement, you'll need to write a custom entity component system.");
		}

		/// <summary>
		/// Approximate remaining distance along the current path to the end of the path.
		///
		/// The agent does not know the true distance at all times, so this is an approximation.
		/// It tends to be a bit lower than the true distance.
		///
		/// Note: This is the distance to the end of the path, which may or may not be the same as the destination.
		/// If the character cannot reach the destination it will try to move as close as possible to it.
		///
		/// This value will update immediately if the <see cref="destination"/> property is changed, or if the agent is moved using the <see cref="position"/> property or the <see cref="Teleport"/> method.
		///
		/// If the agent has no path, or if the current path is stale (e.g. if the graph has been updated close to the agent, and it hasn't had time to recalculate its path), this will return positive infinity.
		///
		/// See: <see cref="reachedDestination"/>
		/// See: <see cref="reachedEndOfPath"/>
		/// See: <see cref="pathPending"/>
		/// </summary>
		public float remainingDistance {
			get {
				if (!entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out var entityManager, out var storage)) return float.PositiveInfinity;

				movementStateAccessRO.Update(entityManager);
				managedStateAccessRO.Update(entityManager);
				// TODO: Should this perhaps only check if the start/end points are stale, and ignore the case when the graph is updated and some nodes are destroyed?
				if (managedState.pathTracer.hasPath && !managedState.pathTracer.isStale) {
					ref var movementState = ref movementStateAccessRO[storage];
					return movementState.remainingDistanceToEndOfPart + Vector3.Distance(managedState.pathTracer.endPointOfFirstPart, managedState.pathTracer.endPoint);
				} else {
					return float.PositiveInfinity;
				}
			}
		}

		/// <summary>\copydocref{MovementSettings.stopDistance}</summary>
		public float stopDistance {
			get => movement.stopDistance;
			set {
				if (movement.stopDistance != value) {
					movement.stopDistance = value;
					if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
						movementSettings.value.stopDistance = value;
					}
				}
			}
		}

		/// <summary>\copydocref{MovementSettings.positionSmoothing}</summary>
		public float positionSmoothing {
			get => movement.positionSmoothing;
			set {
				if (movement.positionSmoothing != value) {
					movement.positionSmoothing = value;
					if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
						movementSettings.value.positionSmoothing = value;
					}
				}
			}
		}

		/// <summary>\copydocref{MovementSettings.rotationSmoothing}</summary>
		public float rotationSmoothing {
			get => movement.rotationSmoothing;
			set {
				if (movement.rotationSmoothing != value) {
					movement.rotationSmoothing = value;
					if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
						movementSettings.value.rotationSmoothing = value;
					}
				}
			}
		}

		/// <summary>
		/// True if the ai has reached the <see cref="destination"/>.
		///
		/// The agent considers the destination reached when it is within <see cref="stopDistance"/> world units from the <see cref="destination"/>.
		/// Additionally, the destination must not be above the agent's head, and it must not be more than half the agent's height below its feet.
		///
		/// If a facing direction was specified when setting the destination, this will only return true once the agent is approximately facing the correct orientation.
		///
		/// This value will be updated immediately when the <see cref="destination"/> is changed.
		///
		/// <code>
		/// IEnumerator Start () {
		///     ai.destination = somePoint;
		///     // Start to search for a path to the destination immediately
		///     ai.SearchPath();
		///     // Wait until the agent has reached the destination
		///     while (!ai.reachedDestination) {
		///         yield return null;
		///     }
		///     // The agent has reached the destination now
		/// }
		/// </code>
		///
		/// Note: The agent may not be able to reach the destination. In that case this property may never become true. Sometimes <see cref="reachedEndOfPath"/> is more appropriate.
		///
		/// See: <see cref="stopDistance"/>
		/// See: <see cref="remainingDistance"/>
		/// See: <see cref="reachedEndOfPath"/>
		/// </summary>
		public bool reachedDestination => entityStorageCache.GetComponentData(entity, ref movementStateAccessRW, out var movementState) ? movementState.value.reachedDestinationAndOrientation : false;

		/// <summary>
		/// True if the agent has reached the end of the current path.
		///
		/// The agent considers the end of the path reached when it is within <see cref="stopDistance"/> world units from the end of the path.
		/// Additionally, the end of the path must not be above the agent's head, and it must not be more than half the agent's height below its feet.
		///
		/// If a facing direction was specified when setting the destination, this will only return true once the agent is approximately facing the correct orientation.
		///
		/// This value will be updated immediately when the <see cref="destination"/> is changed.
		///
		/// Note: Reaching the end of the path does not imply that it has reached its desired destination, as the destination may not even be possible to reach.
		/// Sometimes <see cref="reachedDestination"/> is more appropriate.
		///
		/// See: <see cref="remainingDistance"/>
		/// See: <see cref="reachedDestination"/>
		/// </summary>
		public bool reachedEndOfPath => entityStorageCache.GetComponentData(entity, ref movementStateAccessRW, out var movementState) ? movementState.value.reachedEndOfPathAndOrientation : false;

		/// <summary>
		/// Like <see cref="reachedEndOfPath"/>, but will also return true if the end of the path is crowded, and this agent has stopped because it cannot get closer.
		///
		/// This is only relevant if the agent is using local avoidance. Otherwise, this will be identical to <see cref="reachedEndOfPath"/>.
		///
		/// If the agent has a stale path (e.g. because the destination changed significantly, or a graph update happened near the agent), false will be returned
		/// until the path has been recalculated (typically in the next one or two frames).
		///
		/// You can see a visualization of this state by enabling "Reached State" in the <see cref="rvoSettings.debug;Local Avoidance Debug Rendering"/> field.
		///
		/// Note: The agent may not be completely stopped when this is true. It knows that there are other agents in the way, but it might still be able to slowly make some progress.
		/// Check the <see cref="velocity"/> property to see if the agent is actually moving.
		///
		/// In the video below, the agents will get a red ring around them when this property is true.
		///
		/// [Open online documentation to see videos]
		///
		/// See: local-avoidance (view in online documentation for working links).
		/// See: <see cref="ReachedEndOfPath"/>
		/// </summary>
		public bool reachedCrowdedEndOfPath {
			get {
				if (reachedEndOfPath) return true;
				if (!hasPath) return false;

				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				if (RVO.RVOSimulator.active != null && entityManager.HasComponent<AgentIndex>(entity)) {
					var agentIndex = entityManager.GetComponentData<AgentIndex>(entity);
					var simulator = RVO.RVOSimulator.active.GetSimulator();
					if (agentIndex.TryGetIndex(ref simulator.simulationData, out var index)) {
						var effectivelyReachedDestination = simulator.outputData.effectivelyReachedDestination[index];
						if (effectivelyReachedDestination == RVO.ReachedEndOfPath.Reached) {
							managedStateAccessRO.Update(entityManager);

							// Check if the RVO simulator state is roughly in sync with the path tracer
							var rvoEndOfPath = (Vector3)simulator.simulationData.endOfPath[index];
							var endOfPath = managedState.pathTracer.endPoint;
							const float MaxChangeRadians = 0.1f;
							if ((rvoEndOfPath - endOfPath).sqrMagnitude < (endOfPath - position).sqrMagnitude*MaxChangeRadians*MaxChangeRadians) {
								return true;
							} else {
								// The RVO simulator has a different end of path than the path tracer.
								// This can happen if the destination has just changed, but the rvo simulation has not yet run another iteration.
								// In that case we should not consider it reached.
							}
						}
					}
				}
				return false;
			}
		}

		/// <summary>
		/// End point of path the agent is currently following.
		/// If the agent has no path (or if it's not calculated yet), this will return the <see cref="destination"/> instead.
		/// If the agent has no destination it will return the agent's current position.
		///
		/// The end of the path is usually identical or very close to the <see cref="destination"/>, but it may differ
		/// if the path for example was blocked by a wall, so that the agent couldn't get any closer.
		///
		/// See: <see cref="GetRemainingPath"/>
		/// </summary>
		public Vector3 endOfPath {
			get {
				if (entityExists) {
					// Make sure we block to ensure no managed state changes are made in jobs while we are reading from it
					managedStateAccessRO.Update(World.DefaultGameObjectInjectionWorld.EntityManager);
					if (hasPath) return managedState.pathTracer.endPoint;
					var d = destination;
					if (float.IsFinite(d.x)) return d;
				}
				return position;
			}
		}

		static NativeList<float3> nextCornersScratch;
		static NativeArray<int> indicesScratch;
		static int scratchReferenceCount = 0;

		/// <summary>
		/// Position in the world that this agent should move to.
		///
		/// If no destination has been set yet, then (+infinity, +infinity, +infinity) will be returned.
		///
		/// Setting this property will immediately try to repair the path if the agent already has a path.
		/// This will also immediately update properties like <see cref="reachedDestination"/>, <see cref="reachedEndOfPath"/> and <see cref="remainingDistance"/>.
		///
		/// The agent may do a full path recalculation if the local repair was not sufficient,
		/// but this will at earliest happen in the next simulation step.
		///
		/// <code>
		/// IEnumerator Start () {
		///     ai.destination = somePoint;
		///     // Wait until the AI has reached the destination
		///     while (!ai.reachedEndOfPath) {
		///         yield return null;
		///     }
		///     // The agent has reached the destination now
		/// }
		/// </code>
		///
		/// See: <see cref="SetDestination"/>, which also allows you to set a facing direction for the agent.
		/// </summary>
		public Vector3 destination {
			get => entityStorageCache.GetComponentData(entity, ref destinationPointAccessRO, out var destination) ? (Vector3)destination.value.destination : Vector3.positiveInfinity;
			set => SetDestination(value, default);
		}

		/// <summary>
		/// Direction the agent will try to face when it reaches the destination.
		///
		/// If this is zero, the agent will not try to face any particular direction.
		///
		/// The following video shows three agents, one with no facing direction set, and then two agents with varying values of the <see cref="PIDMovement.leadInRadiusWhenApproachingDestination;lead in radius"/>.
		/// [Open online documentation to see videos]
		///
		/// See: <see cref="MovementSettings.follower.leadInRadiusWhenApproachingDestination"/>
		/// See: <see cref="SetDestination"/>
		/// </summary>
		Vector3 destinationFacingDirection {
			get => entityStorageCache.GetComponentData(entity, ref destinationPointAccessRO, out var destination) ? (Vector3)destination.value.facingDirection : Vector3.zero;
		}

		/// <summary>
		/// Set the position in the world that this agent should move to.
		///
		/// This method will immediately try to repair the path if the agent already has a path.
		/// This will also immediately update properties like <see cref="reachedDestination"/>, <see cref="reachedEndOfPath"/> and <see cref="remainingDistance"/>.
		/// The agent may do a full path recalculation if the local repair was not sufficient,
		/// but this will at earliest happen in the next simulation step.
		///
		/// If you are setting a destination and want to know when the agent has reached that destination,
		/// then you could use either <see cref="reachedDestination"/> or <see cref="reachedEndOfPath"/>.
		///
		/// You may also set a facing direction for the agent. If set, the agent will try to approach the destination point
		/// with the given heading. <see cref="reachedDestination"/> and <see cref="reachedEndOfPath"/> will only return true once the agent is approximately facing the correct direction.
		/// The <see cref="MovementSettings.follower.leadInRadiusWhenApproachingDestination"/> field controls how wide an arc the agent will try to use when approaching the destination.
		///
		/// The following video shows three agents, one with no facing direction set, and then two agents with varying values of the <see cref="PIDMovement.leadInRadiusWhenApproachingDestination;lead in radius"/>.
		/// [Open online documentation to see videos]
		///
		/// <code>
		/// IEnumerator Start () {
		///     ai.SetDestination(somePoint, Vector3.right);
		///     // Wait until the AI has reached the destination and is rotated to the right in world space
		///     while (!ai.reachedEndOfPath) {
		///         yield return null;
		///     }
		///     // The agent has reached the destination now
		/// }
		/// </code>
		///
		/// See: <see cref="destination"/>
		/// </summary>
		public void SetDestination (float3 destination, float3 facingDirection = default) {
			AssertEntityExists();
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			movementStateAccessRW.Update(entityManager);
			managedStateAccessRW.Update(entityManager);
			agentCylinderShapeAccessRO.Update(entityManager);
			movementSettingsAccessRO.Update(entityManager);
			localTransformAccessRO.Update(entityManager);
			autoRepathPolicyRW.Update(entityManager);
			destinationPointAccessRW.Update(entityManager);
			movementPlaneAccessRO.Update(entityManager);
			readyToTraverseOffMeshLinkRW.Update(entityManager);

			var storage = entityManager.GetStorageInfo(entity);
			destinationPointAccessRW[storage] = new DestinationPoint {
				destination = destination,
				facingDirection = facingDirection,
			};

			// If we already have a path, we try to repair it immediately.
			// This ensures that the #reachedDestination and #reachedEndOfPath flags are as up to date as possible.
			if (managedState.pathTracer.hasPath) {
				Profiler.BeginSample("RepairEnd");
				ref var movementPlane = ref movementPlaneAccessRO[storage];
				managedState.pathTracer.UpdateEnd(destination, PathTracer.RepairQuality.High, movementPlane.value, managedState.pathfindingSettings.traversalProvider, managedState.activePath);
				Profiler.EndSample();
				ref var movementState = ref movementStateAccessRW[storage];
				if (movementState.pathTracerVersion != managedState.pathTracer.version) {
					Profiler.BeginSample("EstimateNative");
					ref var shape = ref agentCylinderShapeAccessRO[storage];
					ref var movementSettings = ref movementSettingsAccessRO[storage];
					ref var localTransform = ref localTransformAccessRO[storage];
					ref var autoRepath = ref autoRepathPolicyRW[storage];
					ref var destinationPoint = ref destinationPointAccessRW[storage];
					var readyToTraverseOffMeshLink = storage.Chunk.GetEnabledMask(ref readyToTraverseOffMeshLinkRW.handle).GetEnabledRefRW<ReadyToTraverseOffMeshLink>(storage.IndexInChunk);
					if (!nextCornersScratch.IsCreated) nextCornersScratch = new NativeList<float3>(4, Allocator.Persistent);
					JobRepairPath.Execute(
						ref localTransform,
						ref movementState,
						ref shape,
						ref movementPlane,
						ref autoRepath,
						ref destinationPoint,
						readyToTraverseOffMeshLink,
						managedState,
						in movementSettings,
						nextCornersScratch,
						ref indicesScratch,
						Allocator.Persistent,
						false
						);
					Profiler.EndSample();
				}
			}
		}

		/// <summary>
		/// Policy for when the agent recalculates its path.
		///
		/// See: <see cref="ECS.AutoRepathPolicy"/>
		/// </summary>
		public ECS.AutoRepathPolicy autoRepath {
			get {
				return autoRepathBacking;
			}
			set {
				autoRepathBacking = value;
				if (entityStorageCache.GetComponentData(entity, ref autoRepathPolicyRW, out var component)) {
					component.value = value;
				}
			}
		}

		/// <summary>
		/// \copydoc Pathfinding::IAstarAI::canSearch
		/// Deprecated: This has been superseded by <see cref="autoRepath.mode"/>.
		/// </summary>
		[System.Obsolete("This has been superseded by autoRepath.mode")]
		public bool canSearch {
			get {
				return autoRepathBacking.mode != AutoRepathPolicy.Mode.Never;
			}
			set {
				if (value) {
					if (autoRepathBacking.mode == AutoRepathPolicy.Mode.Never) {
						autoRepathBacking.mode = AutoRepathPolicy.Mode.EveryNSeconds;
					}
				} else {
					autoRepathBacking.mode = AutoRepathPolicy.Mode.Never;
				}
				// Ensure the entity date is up to date
				autoRepath = autoRepathBacking;
			}
		}

		/// <summary>
		/// Enables or disables movement completely.
		/// If you want the agent to stand still, but still react to local avoidance and use gravity: use <see cref="isStopped"/> instead.
		///
		/// Disabling this will remove the <see cref="SimulateMovement"/> component from the entity, which prevents
		/// most systems from running for this entity.
		///
		/// When disabled, the <see cref="velocity"/> property will no longer update.
		///
		/// See: <see cref="autoRepath"/>
		/// See: <see cref="isStopped"/>
		/// </summary>
		public bool canMove {
			get {
				if (!entityExists) return true;

				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				return entityManager.HasComponent<SimulateMovement>(entity);
			}
			set => ToggleComponent<SimulateMovement>(entity, value, true);
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::movementPlane</summary>
		public NativeMovementPlane movementPlane => entityStorageCache.GetComponentData(entity, ref movementPlaneAccessRO, out var movementPlane) ? movementPlane.value.value : new NativeMovementPlane(rotation);

		/// <summary>
		/// Enables or disables gravity.
		///
		/// If gravity is enabled, the agent will accelerate downwards, and use a raycast to check if it should stop falling.
		///
		/// This has no effect if the agent's <see cref="orientation"/> is set to YAxisForward (2D mode).
		/// Gravity does not really make sense for top-down 2D games. The gravity setting is also hidden from the inspector in this mode.
		///
		/// See: <see cref="groundMask"/>
		/// </summary>
		public bool enableGravity {
			get {
				if (!entityExists) return managedState.enableGravity;

				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				return entityManager.HasComponent<GravityState>(entity);
			}
			set {
				if (managedState.enableGravity != value) {
					managedState.enableGravity = value;
					ToggleComponentEnabled<GravityState>(entity, value, false);
				}
			}
		}

		/// <summary>
		/// \copydocref{ManagedState.enableLocalAvoidance}
		///
		/// See: <see cref="rvoSettings"/>
		/// </summary>
		public bool enableLocalAvoidance {
			get => managedState.enableLocalAvoidance;
			set => managedState.enableLocalAvoidance = value;
		}

		/// <summary>
		/// Determines if the character's position should be coupled to the Transform's position.
		/// If false then all movement calculations will happen as usual, but the GameObject that this component is attached to will not move.
		/// Instead, only the <see cref="position"/> property and the internal entity's position will change.
		///
		/// This is useful if you want to control the movement of the character using some other means, such
		/// as root motion, but still want the AI to move freely.
		///
		/// See: <see cref="canMove"/> which in contrast to this field will disable all movement calculations.
		/// See: <see cref="updateRotation"/>
		/// </summary>
		public bool updatePosition {
			get => syncPosition;
			set {
				syncPosition = value;
				ToggleComponent<SyncPositionWithTransform>(entity, value, false);
			}
		}

		/// <summary>
		/// Determines if the character's rotation should be coupled to the Transform's rotation.
		/// If false then all movement calculations will happen as usual, but the GameObject that this component is attached to will not rotate.
		/// Instead, only the <see cref="rotation"/> property and the internal entity's rotation will change.
		///
		/// This is particularly useful for 2D games where you want the Transform to stay in the same orientation, and instead swap out the displayed
		/// sprite to indicate the direction the character is facing.
		///
		/// You can enable <see cref="PIDMovement.DebugFlags"/>.Rotation in <see cref="debugFlags"/> to draw a gizmos arrow in the scene view to indicate the agent's internal rotation.
		///
		/// See: <see cref="updatePosition"/>
		/// See: <see cref="rotation"/>
		/// See: <see cref="orientation"/>
		/// </summary>
		public bool updateRotation {
			get => syncRotation;
			set {
				syncRotation = value;
				ToggleComponent<SyncRotationWithTransform>(entity, value, false);
			}
		}

		/// <summary>
		/// Determines which direction the agent moves in.
		/// For 3D games you most likely want the ZAxisIsForward option as that is the convention for 3D games.
		/// For 2D games you most likely want the YAxisIsForward option as that is the convention for 2D games.
		///
		/// When using ZAxisForard, the +Z axis will be the forward direction of the agent, +Y will be upwards, and +X will be the right direction.
		/// When using YAxisForward, the +Y axis will be the forward direction of the agent, +Z will be upwards, and +X will be the right direction.
		///
		/// [Open online documentation to see images]
		/// </summary>
		public OrientationMode orientation {
			get => orientationBacking;
			set {
				if (orientationBacking != value) {
					orientationBacking = value;
					ToggleComponent<OrientationYAxisForward>(entity, value == OrientationMode.YAxisForward, false);
				}
			}
		}

		/// <summary>Adds or removes a component from an entity</summary>
		static void ToggleComponent<T>(Entity entity, bool enabled, bool mustExist) where T : struct, IComponentData {
			var world = World.DefaultGameObjectInjectionWorld;
			if (world == null || !world.EntityManager.Exists(entity)) {
				if (!mustExist) throw new System.InvalidOperationException("Entity does not exist. You can only access this if the component is active and enabled.");
				return;
			}
			if (enabled) {
				world.EntityManager.AddComponent<T>(entity);
			} else {
				world.EntityManager.RemoveComponent<T>(entity);
			}
		}

		/// <summary>Enables or disables a component on an entity</summary>
		static void ToggleComponentEnabled<T>(Entity entity, bool enabled, bool mustExist) where T : struct, IComponentData, IEnableableComponent {
			var world = World.DefaultGameObjectInjectionWorld;
			if (world == null || !world.EntityManager.Exists(entity)) {
				if (!mustExist) throw new System.InvalidOperationException("Entity does not exist. You can only access this if the component is active and enabled.");
				return;
			}
			world.EntityManager.SetComponentEnabled<T>(entity, enabled);
		}

		/// <summary>
		/// True if this agent currently has a valid path that it follows.
		///
		/// This is true if the agent has a path and the path is not stale.
		///
		/// A path may become stale if the graph is updated close to the agent and it hasn't had time to recalculate its path yet.
		/// </summary>
		public bool hasPath {
			get {
				// Ensure no jobs are writing to the managed state while we are reading from it
				if (entityExists) managedStateAccessRO.Update(World.DefaultGameObjectInjectionWorld.EntityManager);
				return !managedState.pathTracer.isStale;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::pathPending</summary>
		public bool pathPending {
			get {
				if (!entityExists) return false;
				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				managedStateAccessRO.Update(entityManager);
				return managedState.pendingPath != null;
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::isStopped</summary>
		public bool isStopped {
			get => movement.isStopped;
			set {
				if (movement.isStopped != value) {
					movement.isStopped = value;
					if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
						movementSettings.value.isStopped = value;
					}
				}
			}
		}

		/// <summary>
		/// Various movement settings.
		///
		/// Some of these settings are exposed on the FollowerEntity directly. For example <see cref="maxSpeed"/>.
		///
		/// Note: The return value is a struct. If you want to change some settings, you'll need to modify the returned struct and then assign it back to this property.
		/// </summary>
		public MovementSettings movementSettings {
			get => movement;
			set {
				movement = value;
				if (entityStorageCache.GetComponentData(entity, ref movementSettingsAccessRW, out var movementSettings)) {
					movementSettings.value = value;
				}
			}
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::steeringTarget</summary>
		public Vector3 steeringTarget => entityStorageCache.GetComponentData(entity, ref movementStateAccessRO, out var movementState) ? (Vector3)movementState.value.nextCorner : position;

		/// <summary>\copydoc Pathfinding::IAstarAI::onSearchPath</summary>
		Action IAstarAI.onSearchPath {
			get => null;
			set => throw new NotImplementedException("The FollowerEntity does not support this property.");
		}

		/// <summary>
		/// Provides callbacks during various parts of the movement calculations.
		///
		/// With this property you can register callbacks that will be called during various parts of the movement calculations.
		/// These can be used to modify movement of the agent.
		///
		/// The following example demonstrates how one can hook into one of the available phases and modify the agent's movement.
		/// In this case, the movement is modified to become wavy.
		///
		/// [Open online documentation to see videos]
		///
		/// <code>
		/// using Pathfinding;
		/// using Pathfinding.ECS;
		/// using Unity.Entities;
		/// using Unity.Mathematics;
		/// using Unity.Transforms;
		/// using UnityEngine;
		///
		/// public class MovementModifierNoise : MonoBehaviour {
		///     /** How much noise to apply */
		///     public float strength = 1;
		///     /** How fast the noise should change */
		///     public float frequency = 1;
		///     float phase;
		///
		///     public void Start () {
		///         // Register a callback to modify the movement.
		///         // This will be called during every simulation step for the agent.
		///         // This may be called multiple times per frame if the time scale is high or fps is low,
		///         // or less than once per frame, if the fps is very high.
		///         GetComponent<FollowerEntity>().movementOverrides.AddBeforeControlCallback(MovementOverride);
		///
		///         // Randomize a phase, to make different agents behave differently
		///         phase = UnityEngine.Random.value * 1000;
		///     }
		///
		///     public void OnDisable () {
		///         // Remove the callback when the component is disabled
		///         GetComponent<FollowerEntity>().movementOverrides.RemoveBeforeControlCallback(MovementOverride);
		///     }
		///
		///     public void MovementOverride (Entity entity, float dt, ref LocalTransform localTransform, ref AgentCylinderShape shape, ref AgentMovementPlane movementPlane, ref DestinationPoint destination, ref MovementState movementState, ref MovementSettings movementSettings) {
		///         // Rotate the next corner the agent is moving towards around the agent by a random angle.
		///         // This will make the agent appear to move in a drunken fashion.
		///
		///         // Don't modify the movement as much if we are very close to the end of the path
		///         var strengthMultiplier = Mathf.Min(1, movementState.remainingDistanceToEndOfPart / Mathf.Max(shape.radius, movementSettings.follower.slowdownTime * movementSettings.follower.speed));
		///         strengthMultiplier *= strengthMultiplier;
		///
		///         // Generate a smoothly varying rotation angle
		///         var rotationAngleRad = strength * strengthMultiplier * (Mathf.PerlinNoise1D(Time.time * frequency + phase) - 0.5f);
		///         // Clamp it to at most plus or minus 90 degrees
		///         rotationAngleRad = Mathf.Clamp(rotationAngleRad, -math.PI*0.5f, math.PI*0.5f);
		///
		///         // Convert the rotation angle to a world-space quaternion.
		///         // We use the movement plane to rotate around the agent's up axis,
		///         // making this code work in both 2D and 3D games.
		///         var rotation = movementPlane.value.ToWorldRotation(rotationAngleRad);
		///
		///         // Rotate the direction to the next corner around the agent
		///         movementState.nextCorner = localTransform.Position + math.mul(rotation, movementState.nextCorner - localTransform.Position);
		///     }
		/// }
		/// </code>
		///
		/// There are a few different phases that you can register callbacks for:
		///
		/// - BeforeControl: Called before the agent's movement is calculated. At this point, the agent has a valid path, and the next corner that is moving towards has been calculated.
		/// - AfterControl: Called after the agent's desired movement is calculated. The agent has stored its desired movement in the <see cref="MovementControl"/> component. Local avoidance has not yet run.
		/// - BeforeMovement: Called right before the agent's movement is applied. At this point the agent's final movement (including local avoidance) is stored in the <see cref="ResolvedMovement"/> component, which you may modify.
		///
		/// Warning: If any agent has a callback registered here, a sync point will be created for all agents when the callback runs.
		/// This can make the simulation not able to utilize multiple threads as effectively. If you have a lot of agents, consider using a custom entity component system instead.
		/// But as always, profile first to see if this is actually a problem for your game.
		///
		/// The callbacks may be called multiple times per frame, if the fps is low, or if the time scale is high.
		/// It may also be called less than once per frame if the fps is very high.
		/// Each callback is provided with a dt parameter, which is the time in seconds since the last simulation step. You should prefer using this instead of Time.deltaTime.
		///
		/// See: <see cref="canMove"/>
		/// See: <see cref="updatePosition"/>
		/// See: <see cref="updateRotation"/>
		///
		/// Note: This API is unstable. It may change in future versions.
		/// </summary>
		public ManagedMovementOverrides movementOverrides => new ManagedMovementOverrides(entity, World.DefaultGameObjectInjectionWorld);

		/// <summary>\copydoc Pathfinding::IAstarAI::FinalizeMovement</summary>
		void IAstarAI.FinalizeMovement (Vector3 nextPosition, Quaternion nextRotation) {
			throw new InvalidOperationException("The FollowerEntity component does not support FinalizeMovement. Use an ECS system to override movement instead, or use the movementOverrides property. If you just want to move the agent to a position, set ai.position or call ai.Teleport.");
		}

		/// <summary>
		/// Fills buffer with the remaining path.
		///
		/// If the agent traverses off-mesh links, the buffer will still contain the whole path. Off-mesh links will be represented by a single line segment.
		/// You can use the <see cref="GetRemainingPath(List<Vector3>,List<PathPartWithLinkInfo>,bool)"/> overload to get more detailed information about the different parts of the path.
		///
		/// <code>
		/// var buffer = new List<Vector3>();
		///
		/// ai.GetRemainingPath(buffer, out bool stale);
		/// for (int i = 0; i < buffer.Count - 1; i++) {
		///     Debug.DrawLine(buffer[i], buffer[i+1], Color.red);
		/// }
		/// </code>
		/// [Open online documentation to see images]
		/// </summary>
		/// <param name="buffer">The buffer will be cleared and replaced with the path. The first point is the current position of the agent.</param>
		/// <param name="stale">May be true if the path is invalid in some way. For example if the agent has no path or if the agent has detected that some nodes in the path have been destroyed.</param>
		public void GetRemainingPath (List<Vector3> buffer, out bool stale) {
			GetRemainingPath(buffer, null, out stale);
		}

		/// <summary>
		/// Fills buffer with the remaining path.
		///
		/// <code>
		/// var buffer = new List<Vector3>();
		/// var parts = new List<PathPartWithLinkInfo>();
		///
		/// ai.GetRemainingPath(buffer, parts, out bool stale);
		/// foreach (var part in parts) {
		///     for (int i = part.startIndex; i < part.endIndex; i++) {
		///         Debug.DrawLine(buffer[i], buffer[i+1], part.type == Funnel.PartType.NodeSequence ? Color.red : Color.green);
		///     }
		/// }
		/// </code>
		/// [Open online documentation to see images]
		///
		/// Note: The <see cref="FollowerEntity"/> simplifies its path continuously as it moves along it. This means that the agent may not follow this exact path, if it manages to simplify the path later.
		/// Furthermore, the agent will apply a steering behavior on top of this path, to make its movement smoother.
		/// </summary>
		/// <param name="buffer">The buffer will be cleared and replaced with the path. The first point is the current position of the agent.</param>
		/// <param name="partsBuffer">If not null, this list will be filled with information about the different parts of the path. A part is a sequence of nodes or an off-mesh link.</param>
		/// <param name="stale">May be true if the path is invalid in some way. For example if the agent has no path or if the agent has detected that some nodes in the path have been destroyed.</param>
		public void GetRemainingPath (List<Vector3> buffer, List<PathPartWithLinkInfo> partsBuffer, out bool stale) {
			buffer.Clear();
			if (partsBuffer != null) partsBuffer.Clear();
			if (!entityExists) {
				buffer.Add(position);
				if (partsBuffer != null) partsBuffer.Add(new PathPartWithLinkInfo { startIndex = 0, endIndex = 0 });
				stale = true;
				return;
			}

			var ms = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<ManagedState>(entity);
			stale = false;
			if (ms.pathTracer.hasPath) {
				var nativeBuffer = new NativeList<float3>(Allocator.Temp);
				var scratch = new NativeArray<int>(8, Allocator.Temp);
				ms.pathTracer.GetNextCorners(nativeBuffer, int.MaxValue, ref scratch, Allocator.Temp, ms.pathfindingSettings.traversalProvider, ms.activePath);
				if (partsBuffer != null) partsBuffer.Add(new PathPartWithLinkInfo(0, nativeBuffer.Length - 1));

				if (ms.pathTracer.partCount > 1) {
					// There are more parts in the path. We need to create a new PathTracer to get the other parts.
					// This can be comparatively expensive, since it needs to generate all the other types from scratch.
					var pathTracer = ms.pathTracer.Clone();
					while (pathTracer.partCount > 1) {
						pathTracer.PopParts(1, ms.pathfindingSettings.traversalProvider, ms.activePath);
						var startIndex = nativeBuffer.Length;
						if (pathTracer.GetPartType() == Funnel.PartType.NodeSequence) {
							pathTracer.GetNextCorners(nativeBuffer, int.MaxValue, ref scratch, Allocator.Temp, ms.pathfindingSettings.traversalProvider, ms.activePath);
							if (partsBuffer != null) partsBuffer.Add(new PathPartWithLinkInfo(startIndex, nativeBuffer.Length - 1));
						} else {
							// If the link contains destroyed nodes, we cannot get a valid link object.
							// In that case, we stop here and mark the path as stale.
							if (pathTracer.PartContainsDestroyedNodes()) {
								stale = true;
								break;
							}
							// Note: startIndex will refer to the last point in the previous part, and endIndex will refer to the first point in the next part
							Assert.IsTrue(startIndex > 0);
							if (partsBuffer != null) partsBuffer.Add(new PathPartWithLinkInfo(startIndex - 1, startIndex, pathTracer.GetLinkInfo()));
						}
						// We need to check if the path is stale after each part because the path tracer may have realized that some nodes are destroyed
						stale |= pathTracer.isStale;
					}
					pathTracer.Dispose();
				}

				nativeBuffer.AsUnsafeSpan().Reinterpret<Vector3>().CopyTo(buffer);
			} else {
				buffer.Add(position);
				if (partsBuffer != null) partsBuffer.Add(new PathPartWithLinkInfo { startIndex = 0, endIndex = 0 });
			}
			stale |= ms.pathTracer.isStale;
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::Move</summary>
		public void Move (Vector3 deltaPosition) {
			position += deltaPosition;
		}

		void IAstarAI.MovementUpdate (float deltaTime, out Vector3 nextPosition, out Quaternion nextRotation) {
			throw new InvalidOperationException("The FollowerEntity component does not support MovementUpdate. Use an ECS system to override movement instead, or use the movementOverrides property");
		}

		/// <summary>\copydoc Pathfinding::IAstarAI::SearchPath</summary>
		public void SearchPath () {
			var dest = destination;
			if (!float.IsFinite(dest.x)) return;

			var movementPlane = this.movementPlane;
			var path = ABPath.Construct(position, dest, null);
			path.UseSettings(managedState.pathfindingSettings);
			SetPath(path, false);
		}

		void AssertEntityExists () {
			if (World.DefaultGameObjectInjectionWorld == null || !World.DefaultGameObjectInjectionWorld.EntityManager.Exists(entity)) throw new System.InvalidOperationException("Entity does not exist. You can only access this if the component is active and enabled.");
		}

		/// <summary>
		/// True if this component's entity exists.
		///
		/// This is typically true if the component is active and enabled and the game is running.
		///
		/// See: <see cref="entity"/>
		/// </summary>
		public bool entityExists => World.DefaultGameObjectInjectionWorld != null && World.DefaultGameObjectInjectionWorld.EntityManager.Exists(entity);

		void CancelCurrentPathRequest () {
			if (entityExists) {
				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				managedStateAccessRW.Update(entityManager);
				managedState.CancelCurrentPathRequest();
			}
		}

		void ClearPath() => ClearPath(entity);

		static void ClearPath (Entity entity) {
			if (entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out var entityManager, out var storage)) {
				agentOffMeshLinkTraversalRO.Update(entityManager);

				if (agentOffMeshLinkTraversalRO.HasComponent(storage)) {
					// Agent is traversing an off-mesh link. We must abort this link traversal.
					var managedInfo = entityManager.GetComponentData<ManagedAgentOffMeshLinkTraversal>(entity);
					if (managedInfo.stateMachine != null) managedInfo.stateMachine.OnAbortTraversingOffMeshLink();
					managedInfo.context.Restore();
					entityManager.RemoveComponent<AgentOffMeshLinkTraversal>(entity);
					entityManager.RemoveComponent<ManagedAgentOffMeshLinkTraversal>(entity);
					// We need to get the storage info again, because the entity will have been moved to another chunk
					entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out entityManager, out storage);
				}

				entityManager.SetComponentEnabled<ReadyToTraverseOffMeshLink>(entity, false);

				managedStateAccessRW.Update(entityManager);
				movementStateAccessRW.Update(entityManager);
				localTransformAccessRO.Update(entityManager);
				movementPlaneAccessRO.Update(entityManager);
				resolvedMovementAccessRW.Update(entityManager);
				movementControlAccessRW.Update(entityManager);

				ref var movementState = ref movementStateAccessRW[storage];
				ref var localTransform = ref localTransformAccessRO[storage];
				ref var movementPlane = ref movementPlaneAccessRO[storage];
				ref var resolvedMovement = ref resolvedMovementAccessRW[storage];
				ref var controlOutput = ref movementControlAccessRW[storage];
				var managedState = managedStateAccessRW[storage];

				managedState.ClearPath();
				managedState.CancelCurrentPathRequest();
				movementState.SetPathIsEmpty(localTransform.Position);

				// This emulates what JobControl does when the agent has no path.
				// This ensures that properties like #desiredVelocity return the correct value immediately after the path has been cleared.
				ResetControl(ref resolvedMovement, ref controlOutput, ref movementPlane, localTransform.Position, localTransform.Rotation, movementState.endOfPath);
			}
		}

		static void ResetControl (ref ResolvedMovement resolvedMovement, ref MovementControl controlOutput, ref AgentMovementPlane movementPlane, float3 position, quaternion rotation, float3 endOfPath) {
			resolvedMovement.targetPoint = position;
			resolvedMovement.speed = 0;
			resolvedMovement.targetRotation = resolvedMovement.targetRotationHint = controlOutput.targetRotation = controlOutput.targetRotationHint = movementPlane.value.ToPlane(rotation);
			controlOutput.endOfPath = endOfPath;
			controlOutput.speed = 0f;
			controlOutput.targetPoint = position;
		}

		/// <summary>
		/// Make the AI follow the specified path.
		///
		/// In case the path has not been calculated, the script will schedule the path to be calculated.
		/// This means the AI may not actually start to follow the path until in a few frames when the path has been calculated.
		/// The <see cref="pathPending"/> field will, as usual, return true while the path is being calculated.
		///
		/// In case the path has already been calculated, it will immediately replace the current path the AI is following.
		///
		/// If you pass null path, then the current path will be cleared and the agent will stop moving.
		/// Note than unless you have also disabled <see cref="canSearch"/>, then the agent will soon recalculate its path and start moving again.
		///
		/// Note: Stopping the agent by passing a null path works. But this will stop the agent instantly, and it will not be able to use local avoidance or know its place on the navmesh.
		/// Usually it's better to set <see cref="isStopped"/> to false, which will make the agent slow down smoothly.
		///
		/// You can disable the automatic path recalculation by setting the <see cref="canSearch"/> field to false.
		///
		/// Note: This call will be ignored if the agent is currently traversing an off-mesh link.
		/// Furthermore, when an agent starts traversing an off-mesh link, the current path request will be canceled (if one is currently in progress).
		///
		/// <code>
		/// IEnumerator Start () {
		///     var pointToAvoid = enemy.position;
		///     // Make the AI flee from an enemy.
		///     // The path will be about 20 world units long (the default cost of moving 1 world unit is 1000).
		///     var path = FleePath.Construct(ai.position, pointToAvoid, 1000 * 20);
		///
		///     // Make the path use the same traversable tags and other pathfinding settings as set in the FollowerEntity inspector
		///     path.UseSettings(ai.pathfindingSettings);
		///
		///     ai.SetPath(path);
		///
		///     while (!ai.reachedEndOfPath) {
		///         yield return null;
		///     }
		/// }
		/// </code>
		///
		/// See: calling-pathfinding (view in online documentation for working links)
		/// See: example_path_types (view in online documentation for working links)
		/// </summary>
		/// <param name="path">The path to follow.</param>
		/// <param name="updateDestinationFromPath">If true, the \reflink{destination} property will be set to the end point of the path. If false, the previous destination value will be kept.
		///     If you pass a path which has no well defined destination before it is calculated (e.g. a MultiTargetPath or RandomPath), then the destination will be first be cleared, but once the path has been calculated, it will be set to the end point of the path.</param>
		public void SetPath(Path path, bool updateDestinationFromPath = true) => SetPath(entity, path, updateDestinationFromPath);

		/// <summary>
		/// \copydocref{SetPath(Path,bool)}
		///
		/// Note: This static method is used if you only have an entity reference. If you are working with a GameObject, you can use the instance method instead.
		/// </summary>
		public static void SetPath (Entity entity, Path path, bool updateDestinationFromPath = true) {
			var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			if (!entityManager.Exists(entity)) throw new System.InvalidOperationException("Entity does not exist. You can only assign a path if the component is active and enabled.");

			managedStateAccessRW.Update(entityManager);
			movementPlaneAccessRO.Update(entityManager);
			agentOffMeshLinkTraversalRO.Update(entityManager);
			movementStateAccessRW.Update(entityManager);
			localTransformAccessRO.Update(entityManager);
			destinationPointAccessRW.Update(entityManager);
			autoRepathPolicyRW.Update(entityManager);

			var storage = entityManager.GetStorageInfo(entity);

			bool isTraversingOffMeshLink = agentOffMeshLinkTraversalRO.HasComponent(storage);
			if (isTraversingOffMeshLink) {
				// Agent is traversing an off-mesh link. We ignore any path updates during this time.
				// TODO: Race condition when adding off mesh link component?
				// TODO: If passing null, should we clear the whole path after the off-mesh link?
				return;
			}

			if (path == null) {
				ClearPath(entity);
				return;
			}

			var managedState = managedStateAccessRW[storage];
			ref var movementPlane = ref movementPlaneAccessRO[storage];
			ref var movementState = ref movementStateAccessRW[storage];
			ref var localTransform = ref localTransformAccessRO[storage];
			ref var destination = ref destinationPointAccessRW[storage];
			ref var autoRepathPolicy = ref autoRepathPolicyRW[storage];

			if (updateDestinationFromPath && path is ABPath abPath) {
				// If the user supplies a new ABPath manually, they probably want the agent to move to that point.
				// So by default we update the destination to match the path.
				if (abPath.endPointKnownBeforeCalculation || abPath.IsDone()) {
					destination = new DestinationPoint { destination = abPath.originalEndPoint, facingDirection = default };
				} else {
					// If the destination is not known, we set it to positive infinity.
					// This is the case for MultiTargetPath and RandomPath, for example.
					destination = new DestinationPoint { destination = Vector3.positiveInfinity, facingDirection = default };
				}
			}

			// The FollowerEntity works best with a ClosestAsSeenFromAboveSoft distance metric
			path.nnConstraint.distanceMetric = DistanceMetric.ClosestAsSeenFromAboveSoft(movementPlane.value.up);

			autoRepathPolicy.OnScheduledPathRecalculation(destination.destination, (float)World.DefaultGameObjectInjectionWorld.Time.ElapsedTime);
			if (path.IsDone()) autoRepathPolicy.OnPathCalculated(path.error);
			ManagedState.SetPath(path, managedState, in movementPlane, ref destination);

			// Check if we have started to follow the path.
			// If it wasn't calculated yet, it will have just been scheduled to be calculated, and will be applied later.
			if (managedState.activePath == path) {
				agentCylinderShapeAccessRO.Update(entityManager);
				movementSettingsAccessRO.Update(entityManager);
				readyToTraverseOffMeshLinkRW.Update(entityManager);

				// This remaining part ensures that the path tracer is fully up to date immediately after the path has been assigned.
				// So that things like GetRemainingPath, and various properties like reachedDestination are up to date immediately.
				managedState.pathTracer.UpdateStart(localTransform.Position, PathTracer.RepairQuality.High, movementPlane.value, managedState.pathfindingSettings.traversalProvider, managedState.activePath);
				managedState.pathTracer.UpdateEnd(destination.destination, PathTracer.RepairQuality.High, movementPlane.value, managedState.pathfindingSettings.traversalProvider, managedState.activePath);

				if (movementState.pathTracerVersion != managedState.pathTracer.version) {
					if (!nextCornersScratch.IsCreated) nextCornersScratch = new NativeList<float3>(4, Allocator.Persistent);
					ref var shape = ref agentCylinderShapeAccessRO[storage];
					ref var movementSettings = ref movementSettingsAccessRO[storage];
					var readyToTraverseOffMeshLink = storage.Chunk.GetEnabledMask(ref readyToTraverseOffMeshLinkRW.handle).GetEnabledRefRW<ReadyToTraverseOffMeshLink>(storage.IndexInChunk);
					JobRepairPath.Execute(
						ref localTransform,
						ref movementState,
						ref shape,
						ref movementPlane,
						ref autoRepathPolicy,
						ref destination,
						readyToTraverseOffMeshLink,
						managedState,
						in movementSettings,
						nextCornersScratch,
						ref indicesScratch,
						Allocator.Persistent,
						false
						);
				}
			}
		}

		/// <summary>
		/// Instantly move the agent to a new position.
		///
		/// The current path will be cleared by default.
		///
		/// This method is preferred for long distance teleports. If you only move the agent a very small distance (so that it is reasonable that it can keep its current path),
		/// then setting the <see cref="position"/> property is preferred.
		/// Setting the <see cref="position"/> property very far away from the agent could cause the agent to fail to move the full distance, as it can get blocked by the navmesh.
		///
		/// See: Works similarly to Unity's NavmeshAgent.Warp.
		/// See: <see cref="position"/>
		/// See: <see cref="SearchPath"/>
		/// </summary>
		public void Teleport (Vector3 newPosition, bool clearPath = true) {
			if (clearPath) ClearPath();

			if (entityExists) {
				var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
				movementOutputAccessRW.Update(entityManager);
				managedStateAccessRW.Update(entityManager);
				movementPlaneAccessRO.Update(entityManager);
				var storage = entityManager.GetStorageInfo(entity);
				ref var movementOutput = ref movementOutputAccessRW[storage];
				movementOutput.lastPosition = newPosition;
				managedState.CancelCurrentPathRequest();
				if (AstarPath.active != null) {
					// TODO: Should we use the from-above distance metric here?
					// This would fail when used on a spherical world and the agent was teleported
					// to another part of the sphere.
					ScratchNNConstraint.UseSettings(managedState.pathfindingSettings);
					var nearest = AstarPath.active.GetNearest(newPosition, ScratchNNConstraint);
					if (nearest.node != null) {
						var movementPlane = movementPlaneAccessRO[storage];
						managedState.pathTracer.SetFromSingleNode(nearest.node, nearest.position, movementPlane.value, managedState.pathfindingSettings);
					}
				}

				// Note: Since we are starting from a completely new path,
				// setting the position will also cause the path tracer to repair the destination.
				// Therefore we don't have to also set the destination here.
				position = newPosition;
			} else {
				position = newPosition;
			}
		}

		void FindComponents () {
			tr = transform;
		}

		static readonly Color ShapeGizmoColor = new Color(240/255f, 213/255f, 30/255f);

		public override void DrawGizmos () {
			// This may be called before the component has been enabled.
			// For example outside of play mode, or even in play mode when in prefab isolation mode
			if (tr == null) FindComponents();

			var color = ShapeGizmoColor;
			var destination = this.destination;
			var rotation = this.rotation;
			var localScale = tr.localScale;
			var radius = shape.radius * math.abs(localScale.x);

			if (orientation == OrientationMode.YAxisForward) {
				Draw.Circle(position, rotation * Vector3.forward, radius, color);
			} else {
				Draw.WireCylinder(position, rotation * Vector3.up, localScale.y * shape.height, radius, color);
			}

			if (!updateRotation) {
				Draw.ArrowheadArc(position, rotation * Vector3.forward, radius * 1.05f, color);
			}

			if (!float.IsPositiveInfinity(destination.x) && Application.isPlaying) {
				var dir = destinationFacingDirection;
				if (dir != Vector3.zero) {
					Draw.xz.ArrowheadArc(destination, dir, 0.25f, Color.blue);
				}
				Draw.xz.Circle(destination, 0.2f, Color.blue);
			}
		}

		[System.Flags]
		enum FollowerEntityMigrations {
			MigratePathfindingSettings = 1 << 0,
			MigrateMovementPlaneSource = 1 << 1,
			MigrateAutoRepathPolicy = 1 << 2,
		}

		protected override void OnUpgradeSerializedData (ref Serialization.Migrations migrations, bool unityThread) {
			if (migrations.TryMigrateFromLegacyFormat(out var _legacyVersion)) {
				// Only 1 version of the previous version format existed for this component
				if (this.pathfindingSettings.tagPenalties.Length != 0) migrations.MarkMigrationFinished((int)FollowerEntityMigrations.MigratePathfindingSettings);
			}

			if (migrations.AddAndMaybeRunMigration((int)FollowerEntityMigrations.MigratePathfindingSettings, unityThread)) {
				if (TryGetComponent<Seeker>(out var seeker)) {
					this.pathfindingSettings = new PathRequestSettings {
						graphMask = seeker.graphMask,
						traversableTags = seeker.traversableTags,
						tagPenalties = seeker.tagPenalties,
					};
					UnityEngine.Object.DestroyImmediate(seeker);
				} else {
					this.pathfindingSettings = PathRequestSettings.Default;
				}
			}
			// Old migrations that cannot run anymore
			migrations.AddAndMaybeRunMigration((int)FollowerEntityMigrations.MigrateMovementPlaneSource);
			migrations.AddAndMaybeRunMigration((int)FollowerEntityMigrations.MigrateAutoRepathPolicy);
		}

#if UNITY_EDITOR
		/// <summary>\cond IGNORE_IN_DOCS</summary>

		/// <summary>
		/// Copies all settings from this component to the entity's components.
		///
		/// Note: This is an internal method and you should never need to use it yourself.
		/// Typically it is used by the editor to keep the entity's state in sync with the component's state.
		/// </summary>
		public void SyncWithEntity () {
			if (!entityStorageCache.Update(World.DefaultGameObjectInjectionWorld, entity, out var entityManager, out var storage)) return;

			this.position = this.position;
			this.autoRepath = this.autoRepath;
			movementSettingsAccessRW.Update(entityManager);
			managedStateAccessRW.Update(entityManager);
			agentCylinderShapeAccessRW.Update(entityManager);

			SyncWithEntity(managedStateAccessRW[storage], ref agentCylinderShapeAccessRW[storage], ref movementSettingsAccessRW[storage]);

			// Structural changes
			ToggleComponent<GravityState>(entity, managedState.enableGravity, false);
			ToggleComponent<OrientationYAxisForward>(entity, orientation == OrientationMode.YAxisForward, false);
			this.updatePosition = this.updatePosition;
			this.updateRotation = this.updateRotation;
			this.movementPlaneSource = this.movementPlaneSource;
		}

		/// <summary>
		/// Copies all settings from this component to the entity's components.
		///
		/// Note: This is an internal method and you should never need to use it yourself.
		/// </summary>
		public void SyncWithEntity (ManagedState managedState, ref AgentCylinderShape shape, ref MovementSettings movementSettings) {
			movementSettings = this.movement;
			shape = this.shape;
			// Copy all serialized fields to the managed state object.
			// This excludes the PathTracer, onTraverseOffMeshLink and pathfindingSettings.traversalProvider, since they are not serialized
			var traversalProvider = managedState.pathfindingSettings.traversalProvider;
			managedState.rvoSettings = this.managedState.rvoSettings;
			managedState.enableLocalAvoidance = this.managedState.enableLocalAvoidance;
			managedState.pathfindingSettings = this.managedState.pathfindingSettings;
			managedState.pathfindingSettings.traversalProvider = traversalProvider;
			// Replace this instance of the managed state with the entity component
			this.managedState = managedState;
			// Note: RVO settings are copied every frame automatically before local avoidance simulations
		}

		static List<FollowerEntity> needsSyncWithEntityList = new List<FollowerEntity>();

		void ISerializationCallbackReceiver.OnBeforeSerialize () {}

		void ISerializationCallbackReceiver.OnAfterDeserialize () {
			UpgradeSerializedData(false);

			// This is (among other times) called after an undo or redo event has happened.
			// In that case, the entity's state might be out of sync with this component's state,
			// so we need to sync the two together. Unfortunately this method is called
			// from Unity's separate serialization thread, so we cannot access the entity directly.
			// Instead we add this component to a list and make sure to process them in the next
			// editor update.
			lock (needsSyncWithEntityList) {
				needsSyncWithEntityList.Add(this);
				if (needsSyncWithEntityList.Count == 1) {
					UnityEditor.EditorApplication.update += SyncWithEntities;
				}
			}
		}

		static void SyncWithEntities () {
			lock (needsSyncWithEntityList) {
				for (int i = 0; i < needsSyncWithEntityList.Count; i++) {
					needsSyncWithEntityList[i].SyncWithEntity();
				}
				needsSyncWithEntityList.Clear();
				UnityEditor.EditorApplication.update -= SyncWithEntities;
			}
		}

		/// <summary>\endcond</summary>
#endif
	}
}
#else
namespace Pathfinding {
	public sealed partial class FollowerEntity : VersionedMonoBehaviour {
		public void Start () {
			UnityEngine.Debug.LogError("The FollowerEntity component requires at least version 1.0 of the 'Entities' package to be installed. You can install it using the Unity package manager.");
		}

		protected override void OnUpgradeSerializedData (ref Serialization.Migrations migrations, bool unityThread) {
			// Since most of the code for this component is stripped out, we should just preserve the current state,
			// and not try to migrate anything.
			// If we don't do this, then the base code will log an error about an unknown migration already being done.
			migrations.IgnoreMigrationAttempt();
		}
	}
}
#endif
