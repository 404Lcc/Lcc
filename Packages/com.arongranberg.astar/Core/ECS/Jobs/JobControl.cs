#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Pathfinding.Drawing;
using Pathfinding.PID;
using Unity.Burst.Intrinsics;

namespace Pathfinding.ECS {
	[BurstCompile]
	[WithNone(typeof(AgentOffMeshLinkTraversal))]
	[WithAll(typeof(SimulateMovement), typeof(SimulateMovementControl))]
	public partial struct JobControl : IJobEntity, IJobEntityChunkBeginEnd {
		public float dt;
		public CommandBuilder draw;
		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NavmeshEdges.NavmeshBorderData navmeshEdgeData;

		[NativeDisableContainerSafetyRestriction]
		public NativeList<float2> edgesScratch;

		[NativeDisableContainerSafetyRestriction]
		public NativeList<int> indicesScratch;

		private static readonly ProfilerMarker MarkerConvertObstacles = new ProfilerMarker("ConvertObstacles");

		public static float3 ClampToNavmesh (float3 position, float3 closestOnNavmesh, in AgentCylinderShape shape, in AgentMovementPlane movementPlane) {
			// Don't clamp the elevation except to make sure it's not too far below the navmesh.
			var clamped2D = movementPlane.value.ToPlane(closestOnNavmesh, out float clampedElevation);
			movementPlane.value.ToPlane(position, out float currentElevation);
			currentElevation = math.max(currentElevation, clampedElevation - shape.height * 0.4f);
			position = movementPlane.value.ToWorld(clamped2D, currentElevation);
			return position;
		}

		public bool OnChunkBegin (in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask) {
			if (!edgesScratch.IsCreated) edgesScratch = new NativeList<float2>(64, Allocator.Temp);
			if (!indicesScratch.IsCreated) indicesScratch = new NativeList<int>(64, Allocator.Temp);
			return true;
		}

		public void OnChunkEnd (in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted) {}

		public void Execute (ref LocalTransform transform, ref MovementState state, in DestinationPoint destination, in AgentCylinderShape shape, in AgentMovementPlane movementPlane, in MovementSettings settings, in ResolvedMovement resolvedMovement, ref MovementControl controlOutput) {
			// Clamp the agent to the navmesh.
			var position = ClampToNavmesh(transform.Position, state.closestOnNavmesh, in shape, in movementPlane);

			edgesScratch.Clear();
			indicesScratch.Clear();

			var scale = math.abs(transform.Scale);
			var settingsTemp = settings.follower;
			// Scale the settings by the agent's scale
			settingsTemp.ScaleByAgentScale(scale);
			settingsTemp.desiredWallDistance *= resolvedMovement.turningRadiusMultiplier;

			if (state.isOnValidNode) {
				MarkerConvertObstacles.Begin();
				var localBounds = PIDMovement.InterestingEdgeBounds(ref settingsTemp, position, state.nextCorner, shape.height, movementPlane.value);
				navmeshEdgeData.GetEdgesInRange(state.hierarchicalNodeIndex, localBounds, edgesScratch, indicesScratch, movementPlane.value);
				MarkerConvertObstacles.End();
			}

			// To ensure we detect that the end of the path is reached robustly we make the agent move slightly closer.
			// to the destination than the stopDistance.
			const float FUZZ = 0.005f;
			// If we are moving towards an off-mesh link, then we want the agent to stop precisely at the off-mesh link.
			// TODO: Depending on the link, we may want the agent to move towards the link at full speed, instead of slowing down.
			var stopDistance = state.traversingLastPart ? math.max(0, settings.stopDistance - FUZZ) : 0f;
			var distanceToSteeringTarget = math.max(0, state.remainingDistanceToEndOfPart - stopDistance);
			var rotation = movementPlane.value.ToPlane(transform.Rotation) - state.rotationOffset - state.rotationOffset2;

			transform.Position = position;

			if (dt > 0.000001f) {
				if (!math.isfinite(distanceToSteeringTarget)) {
					// The agent has no path, just stay still
					controlOutput = new MovementControl {
						targetPoint = position,
						speed = 0,
						endOfPath = position,
						maxSpeed = settings.follower.speed,
						overrideLocalAvoidance = false,
						hierarchicalNodeIndex = state.hierarchicalNodeIndex,
						targetRotation = resolvedMovement.targetRotation,
						targetRotationHint = resolvedMovement.targetRotation,
						rotationSpeed = settings.follower.maxRotationSpeed,
						targetRotationOffset = state.rotationOffset, // May be modified by other systems
					};
				} else if (settings.isStopped) {
					// The user has requested that the agent slow down as quickly as possible.
					// TODO: If the agent is not clamped to the navmesh, it should still move towards the navmesh if it is outside it.
					controlOutput = new MovementControl {
						// Keep moving in the same direction as during the last frame, but slow down
						targetPoint = position + math.normalizesafe(resolvedMovement.targetPoint - position) * 10.0f,
						speed = settings.follower.Accelerate(resolvedMovement.speed, settings.follower.slowdownTime, -dt),
						endOfPath = state.endOfPath,
						maxSpeed = settings.follower.speed,
						overrideLocalAvoidance = false,
						hierarchicalNodeIndex = state.hierarchicalNodeIndex,
						targetRotation = rotation,
						targetRotationHint = rotation,
						rotationSpeed = settings.follower.maxRotationSpeed,
						targetRotationOffset = state.rotationOffset, // May be modified by other systems
					};
				} else {
					var controlParams = new PIDMovement.ControlParams {
						edges = edgesScratch.AsArray(),
						nextCorner = state.nextCorner,
						agentRadius = shape.radius,
						facingDirectionAtEndOfPath = destination.facingDirection,
						endOfPath = state.endOfPath,
						remainingDistance = distanceToSteeringTarget,
						closestOnNavmesh = state.closestOnNavmesh,
#if UNITY_EDITOR
						debugFlags = settings.debugFlags,
#else
						// Do not even try to draw debug info if we are not in the editor
						debugFlags = PIDMovement.DebugFlags.Nothing,
#endif
						p = position,
						rotation = rotation,
						maxDesiredWallDistance = state.followerState.maxDesiredWallDistance,
						speed = controlOutput.speed,
						movementPlane = movementPlane.value,
					};

					var control = PIDMovement.Control(ref settingsTemp, dt, ref controlParams, ref draw, out state.followerState.maxDesiredWallDistance);
					var positionDelta = movementPlane.value.ToWorld(control.positionDelta, 0);
					var speed = math.length(positionDelta) / dt;

					controlOutput = new MovementControl {
						targetPoint = position + math.normalizesafe(positionDelta) * distanceToSteeringTarget,
						speed = speed,
						endOfPath = state.endOfPath,
						maxSpeed = settingsTemp.speed * 1.1f,
						overrideLocalAvoidance = false,
						hierarchicalNodeIndex = state.hierarchicalNodeIndex,
						// It may seem sketchy to use a target rotation so close to the current rotation. One might think
						// there's risk of overshooting this target rotation if the frame rate is uneven.
						// But the TimeScaledRateManager ensures that this is not the case.
						// The cheap simulation's time (which is the one actually rotating the agent) is always guaranteed to be
						// behind (or precisely caught up with) the full simulation's time (that's the simulation which runs this system).
						targetRotation = rotation + control.rotationDelta,
						targetRotationHint = rotation + AstarMath.DeltaAngle(rotation, control.targetRotation),
						rotationSpeed = math.abs(control.rotationDelta / dt),
						targetRotationOffset = state.rotationOffset, // May be modified by other systems
					};
				}
			} else {
				controlOutput.hierarchicalNodeIndex = -1;
			}
		}
	}
}
#endif
