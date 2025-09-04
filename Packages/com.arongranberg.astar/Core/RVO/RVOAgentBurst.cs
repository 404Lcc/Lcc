using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding.RVO {
	using Pathfinding;
	using Pathfinding.Util;
	using Unity.Burst;
	using Unity.Jobs;
	using Unity.Mathematics;
	using Unity.Collections;
	using Pathfinding.Collections;
	using Pathfinding.Drawing;
	using Pathfinding.ECS.RVO;
	using static Unity.Burst.CompilerServices.Aliasing;
	using Unity.Profiling;
	using System.Diagnostics;

	[BurstCompile(CompileSynchronously = false, FloatMode = FloatMode.Fast)]
	public struct JobRVOPreprocess<MovementPlaneWrapper> : IJob where MovementPlaneWrapper : struct, IMovementPlaneWrapper {
		[ReadOnly]
		public SimulatorBurst.AgentData agentData;

		[ReadOnly]
		public SimulatorBurst.AgentOutputData previousOutput;

		[WriteOnly]
		public SimulatorBurst.TemporaryAgentData temporaryAgentData;

		public int startIndex;
		public int endIndex;

		public void Execute () {
			for (int i = startIndex; i < endIndex; i++) {
				if (!agentData.version[i].Valid) continue;

				// Manually controlled overrides the agent being locked.
				// If one for some reason uses them at the same time.
				var locked = agentData.locked[i] & !agentData.manuallyControlled[i];

				if (locked) {
					temporaryAgentData.desiredTargetPointInVelocitySpace[i] = float2.zero;
					temporaryAgentData.desiredVelocity[i] = float3.zero;
					temporaryAgentData.currentVelocity[i] = float3.zero;
				} else {
					MovementPlaneWrapper movementPlane = default;
					movementPlane.Set(agentData.movementPlane[i]);

					var desiredTargetPointInVelocitySpace = movementPlane.ToPlane(agentData.targetPoint[i] - agentData.position[i]);
					temporaryAgentData.desiredTargetPointInVelocitySpace[i] = desiredTargetPointInVelocitySpace;

					// Estimate our current velocity
					// This is necessary because other agents need to know
					// how this agent is moving to be able to avoid it
					var currentVelocity = math.normalizesafe(previousOutput.targetPoint[i] - agentData.position[i]) * previousOutput.speed[i];

					// Calculate the desired velocity from the point we want to reach
					temporaryAgentData.desiredVelocity[i] = movementPlane.ToWorld(math.normalizesafe(desiredTargetPointInVelocitySpace) * agentData.desiredSpeed[i], 0);

					var collisionNormal = math.normalizesafe(agentData.collisionNormal[i]);
					// Check if the velocity is going into the wall
					// If so: remove that component from the velocity
					// Note: if the collisionNormal is zero then the dot prodct will produce a zero as well and nothing will happen.
					float dot = math.dot(currentVelocity, collisionNormal);
					currentVelocity -= math.min(0, dot) * collisionNormal;
					temporaryAgentData.currentVelocity[i] = currentVelocity;
				}
			}
		}
	}

	/// <summary>
	/// Inspired by StarCraft 2's avoidance of locked units.
	/// See: http://www.gdcvault.com/play/1014514/AI-Navigation-It-s-Not
	/// </summary>
	[BurstCompile(FloatMode = FloatMode.Fast)]
	public struct JobHorizonAvoidancePhase1<MovementPlaneWrapper> : Pathfinding.Jobs.IJobParallelForBatched where MovementPlaneWrapper : struct, IMovementPlaneWrapper {
		[ReadOnly]
		public SimulatorBurst.AgentData agentData;

		[ReadOnly]
		public NativeArray<float2> desiredTargetPointInVelocitySpace;

		[ReadOnly]
		public NativeArray<int> neighbours;

		public SimulatorBurst.HorizonAgentData horizonAgentData;

		public CommandBuilder draw;

		public bool allowBoundsChecks { get { return true; } }

		/// <summary>
		/// Super simple bubble sort.
		/// TODO: This will be replaced by a better implementation from the Unity.Collections library when that is stable.
		/// </summary>
		static void Sort<T>(NativeSlice<T> arr, NativeSlice<float> keys) where T : struct {
			bool changed = true;

			while (changed) {
				changed = false;
				for (int i = 0; i < arr.Length - 1; i++) {
					if (keys[i] > keys[i+1]) {
						var tmp = keys[i];
						var tmp2 = arr[i];
						keys[i] = keys[i+1];
						keys[i+1] = tmp;
						arr[i] = arr[i+1];
						arr[i+1] = tmp2;
						changed = true;
					}
				}
			}
		}


		/// <summary>Calculates the shortest difference between two given angles given in radians.</summary>
		public static float DeltaAngle (float current, float target) {
			float num = Mathf.Repeat(target - current, math.PI*2);

			if (num > math.PI) {
				num -= math.PI*2;
			}
			return num;
		}

		public void Execute (int startIndex, int count) {
			NativeArray<float> angles = new NativeArray<float>(SimulatorBurst.MaxNeighbourCount*2, Allocator.Temp);
			NativeArray<int> deltas = new NativeArray<int>(SimulatorBurst.MaxNeighbourCount*2, Allocator.Temp);

			for (int i = startIndex; i < startIndex + count; i++) {
				if (!agentData.version[i].Valid) continue;

				if (agentData.locked[i] || agentData.manuallyControlled[i]) {
					horizonAgentData.horizonSide[i] = 0;
					horizonAgentData.horizonMinAngle[i] = 0;
					horizonAgentData.horizonMaxAngle[i] = 0;
					continue;
				}

				float desiredAngle = math.atan2(desiredTargetPointInVelocitySpace[i].y, desiredTargetPointInVelocitySpace[i].x);

				int eventCount = 0;

				int inside = 0;

				float radius = agentData.radius[i];

				var position = agentData.position[i];
				MovementPlaneWrapper movementPlane = default;
				movementPlane.Set(agentData.movementPlane[i]);

				var distSqToEndOfPath = math.all(math.isfinite(agentData.endOfPath[i])) ? math.lengthsq(agentData.endOfPath[i] - position) : float.PositiveInfinity;

				var agentNeighbours = neighbours.Slice(i*SimulatorBurst.MaxNeighbourCount, SimulatorBurst.MaxNeighbourCount);
				for (int j = 0; j < agentNeighbours.Length && agentNeighbours[j] != -1; j++) {
					var other = agentNeighbours[j];
					if (!agentData.locked[other] && !agentData.manuallyControlled[other]) continue;

					var relativePosition = movementPlane.ToPlane(agentData.position[other] - position);
					float dist = math.length(relativePosition);
					var otherRadius = agentData.radius[other];

					var distanceUntilCollision = dist - (radius + otherRadius);
					if (distanceUntilCollision*distanceUntilCollision > distSqToEndOfPath) continue;

					float angle = math.atan2(relativePosition.y, relativePosition.x) - desiredAngle;
					float deltaAngle;

					if (distanceUntilCollision <= 0) {
						// Collision
						deltaAngle = math.PI * 0.49f;
					} else {
						// One degree
						const float AngleMargin = math.PI / 180f;
						deltaAngle = math.asin((radius + otherRadius)/dist) + AngleMargin;
					}

					float aMin = DeltaAngle(0, angle - deltaAngle);
					float aMax = aMin + DeltaAngle(aMin, angle + deltaAngle);

					if (aMin < 0 && aMax > 0) inside++;

					angles[eventCount] = aMin;
					deltas[eventCount] = 1;
					eventCount++;
					angles[eventCount] = aMax;
					deltas[eventCount] = -1;
					eventCount++;
				}

				// If no angle range includes angle 0 then we are already done
				if (inside == 0) {
					horizonAgentData.horizonSide[i] = 0;
					horizonAgentData.horizonMinAngle[i] = 0;
					horizonAgentData.horizonMaxAngle[i] = 0;
					continue;
				}

				// Sort the events by their angle in ascending order
				Sort(deltas.Slice(0, eventCount), angles.Slice(0, eventCount));

				// Find the first index for which the angle is positive
				int firstPositiveIndex = 0;
				for (; firstPositiveIndex < eventCount; firstPositiveIndex++) if (angles[firstPositiveIndex] > 0) break;

				// Walk in the positive direction from angle 0 until the end of the group of angle ranges that include angle 0
				int tmpInside = inside;
				int tmpIndex = firstPositiveIndex;
				for (; tmpIndex < eventCount; tmpIndex++) {
					tmpInside += deltas[tmpIndex];
					if (tmpInside == 0) break;
				}
				var maxAngle = tmpIndex == eventCount ? math.PI : angles[tmpIndex];

				// Walk in the negative direction from angle 0 until the end of the group of angle ranges that include angle 0
				tmpInside = inside;
				tmpIndex = firstPositiveIndex - 1;
				for (; tmpIndex >= 0; tmpIndex--) {
					tmpInside -= deltas[tmpIndex];
					if (tmpInside == 0) break;
				}
				var minAngle = tmpIndex == -1 ? -math.PI : angles[tmpIndex];

				//horizonBias = -(minAngle + maxAngle);

				// Indicates that a new side should be chosen. The "best" one will be chosen later.
				if (horizonAgentData.horizonSide[i] == 0) horizonAgentData.horizonSide[i] = 2;
				//else horizonBias = math.PI * horizonSide;

				horizonAgentData.horizonMinAngle[i] = minAngle + desiredAngle;
				horizonAgentData.horizonMaxAngle[i] = maxAngle + desiredAngle;
			}
		}
	}

	/// <summary>
	/// Inspired by StarCraft 2's avoidance of locked units.
	/// See: http://www.gdcvault.com/play/1014514/AI-Navigation-It-s-Not
	/// </summary>
	[BurstCompile(FloatMode = FloatMode.Fast)]
	public struct JobHorizonAvoidancePhase2<MovementPlaneWrapper> : Pathfinding.Jobs.IJobParallelForBatched where MovementPlaneWrapper : struct, IMovementPlaneWrapper {
		[ReadOnly]
		public NativeArray<int> neighbours;
		[ReadOnly]
		public NativeArray<AgentIndex> versions;
		public NativeArray<float3> desiredVelocity;
		public NativeArray<float2> desiredTargetPointInVelocitySpace;

		[ReadOnly]
		public NativeArray<NativeMovementPlane> movementPlane;

		public SimulatorBurst.HorizonAgentData horizonAgentData;

		public bool allowBoundsChecks => false;

		public void Execute (int startIndex, int count) {
			for (int i = startIndex; i < startIndex + count; i++) {
				if (!versions[i].Valid) continue;

				// Note: Assumes this code is run synchronous (i.e not included in the double buffering part)
				//offsetVelocity = (position - Position) / simulator.DeltaTime;

				if (horizonAgentData.horizonSide[i] == 0) {
					continue;
				}

				if (horizonAgentData.horizonSide[i] == 2) {
					float sum = 0;
					var agentNeighbours = neighbours.Slice(i*SimulatorBurst.MaxNeighbourCount, SimulatorBurst.MaxNeighbourCount);
					for (int j = 0; j < agentNeighbours.Length && agentNeighbours[j] != -1; j++) {
						var other = agentNeighbours[j];
						var otherHorizonBias = -(horizonAgentData.horizonMinAngle[other] + horizonAgentData.horizonMaxAngle[other]);
						sum += otherHorizonBias;
					}
					var horizonBias = -(horizonAgentData.horizonMinAngle[i] + horizonAgentData.horizonMaxAngle[i]);
					sum += horizonBias;

					horizonAgentData.horizonSide[i] = sum < 0 ? -1 : 1;
				}

				float bestAngle = horizonAgentData.horizonSide[i] < 0 ? horizonAgentData.horizonMinAngle[i] : horizonAgentData.horizonMaxAngle[i];
				float2 desiredDirection;
				math.sincos(bestAngle, out desiredDirection.y, out desiredDirection.x);

				MovementPlaneWrapper movementPlane = default;
				movementPlane.Set(this.movementPlane[i]);

				desiredVelocity[i] = movementPlane.ToWorld(math.length(desiredVelocity[i]) * desiredDirection, 0);
				desiredTargetPointInVelocitySpace[i] = math.length(desiredTargetPointInVelocitySpace[i]) * desiredDirection;
			}
		}
	}

	[BurstCompile(FloatMode = FloatMode.Fast)]
	public struct JobHardCollisions<MovementPlaneWrapper> : Pathfinding.Jobs.IJobParallelForBatched where MovementPlaneWrapper : struct, IMovementPlaneWrapper {
		[ReadOnly]
		public SimulatorBurst.AgentData agentData;
		[ReadOnly]
		public NativeArray<int> neighbours;
		[WriteOnly]
		public NativeArray<float2> collisionVelocityOffsets;

		public float deltaTime;
		public bool enabled;

		/// <summary>
		/// How aggressively hard collisions are resolved.
		/// Should be a value between 0 and 1.
		/// </summary>
		const float CollisionStrength = 0.8f;

		public bool allowBoundsChecks => false;

		public void Execute (int startIndex, int count) {
			if (!enabled) {
				for (int i = startIndex; i < startIndex + count; i++) {
					collisionVelocityOffsets[i] = float2.zero;
				}
				return;
			}

			for (int i = startIndex; i < startIndex + count; i++) {
				if (!agentData.version[i].Valid || agentData.locked[i]) {
					collisionVelocityOffsets[i] = float2.zero;
					continue;
				}

				var agentNeighbours = neighbours.Slice(i*SimulatorBurst.MaxNeighbourCount, SimulatorBurst.MaxNeighbourCount);
				var radius = agentData.radius[i];
				var totalOffset = float2.zero;
				float totalWeight = 0;

				var position = agentData.position[i];
				var movementPlane = new MovementPlaneWrapper();
				movementPlane.Set(agentData.movementPlane[i]);

				for (int j = 0; j < agentNeighbours.Length && agentNeighbours[j] != -1; j++) {
					var other = agentNeighbours[j];
					var relativePosition = movementPlane.ToPlane(position - agentData.position[other]);

					var dirSqrLength = math.lengthsq(relativePosition);
					var combinedRadius = agentData.radius[other] + radius;
					if (dirSqrLength < combinedRadius*combinedRadius && dirSqrLength > 0.00000001f) {
						// Collision
						var dirLength = math.sqrt(dirSqrLength);
						var normalizedDir = relativePosition * (1.0f / dirLength);

						// Overlap amount
						var weight = combinedRadius - dirLength;

						// Position offset required to make the agents not collide anymore
						var offset = normalizedDir * weight;
						// In a later step a weighted average will be taken so that the average offset is extracted
						var weightedOffset = offset * weight;

						totalOffset += weightedOffset;
						totalWeight += weight;
					}
				}

				var offsetVelocity = totalOffset * (1.0f / (0.0001f + totalWeight));
				offsetVelocity *= (CollisionStrength * 0.5f) / deltaTime;

				collisionVelocityOffsets[i] = offsetVelocity;
			}
		}
	}

	[BurstCompile(CompileSynchronously = false, FloatMode = FloatMode.Fast)]
	public struct JobRVOCalculateNeighbours<MovementPlaneWrapper> : Pathfinding.Jobs.IJobParallelForBatched where MovementPlaneWrapper : struct, IMovementPlaneWrapper {
		[ReadOnly]
		public SimulatorBurst.AgentData agentData;

		[ReadOnly]
		public RVOQuadtreeBurst quadtree;

		public NativeArray<int> outNeighbours;

		[WriteOnly]
		public SimulatorBurst.AgentOutputData output;

		public bool allowBoundsChecks { get { return false; } }

		public void Execute (int startIndex, int count) {
			NativeArray<float> neighbourDistances = new NativeArray<float>(SimulatorBurst.MaxNeighbourCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

			for (int i = startIndex; i < startIndex + count; i++) {
				if (!agentData.version[i].Valid) continue;
				CalculateNeighbours(i, outNeighbours, neighbourDistances);
			}
		}

		void CalculateNeighbours (int agentIndex, NativeArray<int> neighbours, NativeArray<float> neighbourDistances) {
			int maxNeighbourCount = math.min(SimulatorBurst.MaxNeighbourCount, agentData.maxNeighbours[agentIndex]);
			// Write the output starting at this index in the neighbours array
			var outputIndex = agentIndex * SimulatorBurst.MaxNeighbourCount;

			int numNeighbours = quadtree.QueryKNearest(new RVOQuadtreeBurst.QuadtreeQuery {
				position = agentData.position[agentIndex],
				speed = agentData.maxSpeed[agentIndex],
				agentRadius = agentData.radius[agentIndex],
				timeHorizon = agentData.agentTimeHorizon[agentIndex],
				outputStartIndex = outputIndex,
				maxCount = maxNeighbourCount,
				result = neighbours,
				layerMask = agentData.collidesWith[agentIndex],
				layers = agentData.layer,
				resultDistances = neighbourDistances,
			});

			output.numNeighbours[agentIndex] = numNeighbours;

			MovementPlaneWrapper movementPlane = default;
			movementPlane.Set(agentData.movementPlane[agentIndex]);

			movementPlane.ToPlane(agentData.position[agentIndex], out float localElevation);

			// Filter out invalid neighbours
			for (int i = 0; i < numNeighbours; i++) {
				int otherIndex = neighbours[outputIndex + i];
				if (otherIndex == -1) throw new System.Exception("Invalid neighbour index");
				// Interval along the y axis in which the agents overlap
				movementPlane.ToPlane(agentData.position[otherIndex], out float otherElevation);
				float maxY = math.min(localElevation + agentData.height[agentIndex], otherElevation + agentData.height[otherIndex]);
				float minY = math.max(localElevation, otherElevation);

				// The agents cannot collide if they are on different y-levels.
				// Also do not avoid the agent itself.
				// Use binary OR to reduce branching.
				if ((maxY < minY) | (otherIndex == agentIndex)) {
					numNeighbours--;
					neighbours[outputIndex + i] = neighbours[outputIndex + numNeighbours];
					i--;
				}
			}

			// Add a token indicating the size of the neighbours list
			if (numNeighbours < SimulatorBurst.MaxNeighbourCount) neighbours[outputIndex + numNeighbours] = -1;
		}
	}

	/// <summary>
	/// Calculates if the agent has reached the end of its path and if its blocked from further progress towards it.
	///
	/// If many agents have the same destination they can often end up crowded around a single point.
	/// It is often desirable to detect this and mark all agents around that destination as having at least
	/// partially reached the end of their paths.
	///
	/// This job uses the following heuristics to determine this:
	///
	/// 1. If an agent wants to move in a particular direction, but there's another agent in the way that makes it have to reduce its velocity,
	///     the other agent is considered to be "blocking" the current agent.
	/// 2. If the agent is within a small distance of the destination
	///        THEN it is considered to have reached the end of its path.
	/// 3. If the agent is blocked by another agent,
	///        AND the other agent is blocked by this agent in turn,
	///        AND if the destination is between the two agents,
	///        THEN the the agent is considered to have reached the end of its path.
	/// 4. If the agent is blocked by another agent which has reached the end of its path,
	///        AND this agent is is moving slowly
	///        AND this agent cannot move furter forward than 50% of its radius.
	///        THEN the agent is considered to have reached the end of its path.
	///
	/// Heuristics 2 and 3 are calculated initially, and then using heuristic 4 the set of agents which have reached their destinations expands outwards.
	///
	/// These heuristics are robust enough that they can be used even if for example the agents are stuck in a winding maze
	/// and only one agent is actually able to reach the destination.
	///
	/// This job doesn't affect the movement of the agents by itself.
	/// However, it is built with the intention that the FlowFollowingStrength parameter will be set
	/// elsewhere to 1 for agents which have reached the end of their paths. This will make the agents stop gracefully
	/// when the end of their paths is crowded instead of continuing to try to desperately reach the destination.
	/// </summary>
	[BurstCompile(CompileSynchronously = false, FloatMode = FloatMode.Fast)]
	public struct JobDestinationReached<MovementPlaneWrapper>: IJob where MovementPlaneWrapper : struct, IMovementPlaneWrapper {
		[ReadOnly]
		public SimulatorBurst.AgentData agentData;

		[ReadOnly]
		public SimulatorBurst.TemporaryAgentData temporaryAgentData;

		public SimulatorBurst.AgentOutputData output;
		public int numAgents;
		public CommandBuilder draw;

		private static readonly ProfilerMarker MarkerInvert = new ProfilerMarker("InvertArrows");
		private static readonly ProfilerMarker MarkerAlloc = new ProfilerMarker("Alloc");
		private static readonly ProfilerMarker MarkerFirstPass = new ProfilerMarker("FirstPass");

		struct TempAgentData {
			public bool blockedAndSlow;
			public float distToEndSq;
		}

		public void Execute () {
			MarkerAlloc.Begin();
			for (int agentIndex = 0; agentIndex < numAgents; agentIndex++) {
				output.effectivelyReachedDestination[agentIndex] = ReachedEndOfPath.NotReached;
			}

			// For each agent, store which agents it blocks
			var inArrows = new NativeArray<int>(agentData.position.Length*SimulatorBurst.MaxBlockingAgentCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			// Number of agents that each agent blocks
			var inArrowCounts = new NativeArray<int>(agentData.position.Length, Allocator.Temp, NativeArrayOptions.ClearMemory);
			var que = new NativeCircularBuffer<int>(16, Allocator.Temp);
			// True for an agent if it is in the queue, or if it should never be queued again
			var queued = new NativeArray<bool>(numAgents, Allocator.Temp, NativeArrayOptions.ClearMemory);
			var tempData = new NativeArray<TempAgentData>(numAgents, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			MarkerAlloc.End();
			MarkerInvert.Begin();

			for (int agentIndex = 0; agentIndex < numAgents; agentIndex++) {
				if (!agentData.version[agentIndex].Valid) continue;
				for (int i = 0; i < SimulatorBurst.MaxBlockingAgentCount; i++) {
					var blockingAgentIndex = output.blockedByAgents[agentIndex*SimulatorBurst.MaxBlockingAgentCount + i];
					if (blockingAgentIndex == -1) break;
					var count = inArrowCounts[blockingAgentIndex];
					if (count >= SimulatorBurst.MaxBlockingAgentCount) continue;
					inArrows[blockingAgentIndex*SimulatorBurst.MaxBlockingAgentCount + count] = agentIndex;
					inArrowCounts[blockingAgentIndex] = count+1;
				}
			}
			MarkerInvert.End();

			MarkerFirstPass.Begin();
			for (int agentIndex = 0; agentIndex < numAgents; agentIndex++) {
				if (!agentData.version[agentIndex].Valid) continue;

				var position = agentData.position[agentIndex];
				MovementPlaneWrapper movementPlane = default;
				movementPlane.Set(agentData.movementPlane[agentIndex]);

				var ourSpeed = output.speed[agentIndex];
				var ourEndOfPath = agentData.endOfPath[agentIndex];

				// Ignore if destination is not set
				if (!math.isfinite(ourEndOfPath.x)) continue;

				var distToEndSq = math.lengthsq(movementPlane.ToPlane(ourEndOfPath - position, out float endOfPathElevationDifference));
				var ourHeight = agentData.height[agentIndex];
				var reachedEndOfPath = false;
				var flowFollowing = false;
				var ourRadius = agentData.radius[agentIndex];
				var forwardClearance = output.forwardClearance[agentIndex];

				// Heuristic 2
				if (distToEndSq < ourRadius*ourRadius*(0.5f*0.5f) && endOfPathElevationDifference < ourHeight && endOfPathElevationDifference > -ourHeight*0.5f) {
					reachedEndOfPath = true;
				}

				var closeToBlocked = forwardClearance < ourRadius*0.5f;
				var slowish = ourSpeed*ourSpeed < math.max(0.01f*0.01f, math.lengthsq(temporaryAgentData.desiredVelocity[agentIndex])*0.25f);
				var blockedAndSlow = closeToBlocked && slowish;
				tempData[agentIndex] = new TempAgentData {
					blockedAndSlow = blockedAndSlow,
					distToEndSq = distToEndSq
				};

				// Heuristic 3
				for (int i = 0; i < SimulatorBurst.MaxBlockingAgentCount; i++) {
					var blockingAgentIndex = output.blockedByAgents[agentIndex*SimulatorBurst.MaxBlockingAgentCount + i];
					if (blockingAgentIndex == -1) break;

					var otherPosition = agentData.position[blockingAgentIndex];
					var distBetweenAgentsSq = math.lengthsq(movementPlane.ToPlane(position - otherPosition));
					var circleRadius = (math.sqrt(distBetweenAgentsSq) + ourRadius + agentData.radius[blockingAgentIndex])*0.5f;
					var endWithinCircle = math.lengthsq(movementPlane.ToPlane(ourEndOfPath - 0.5f*(position + otherPosition))) < circleRadius*circleRadius;
					if (endWithinCircle) {
						// Check if the other agent has an arrow pointing to this agent (i.e. it is blocked by this agent)
						var loop = false;
						for (int j = 0; j < SimulatorBurst.MaxBlockingAgentCount; j++) {
							var arrowFromAgent = inArrows[agentIndex*SimulatorBurst.MaxBlockingAgentCount + j];
							if (arrowFromAgent == -1) break;
							if (arrowFromAgent == blockingAgentIndex) {
								loop = true;
								break;
							}
						}

						if (loop) {
							flowFollowing = true;

							if (blockedAndSlow) {
								reachedEndOfPath = true;
							}
						}
					}
				}

				var effectivelyReached = reachedEndOfPath ? ReachedEndOfPath.Reached : (flowFollowing ? ReachedEndOfPath.ReachedSoon : ReachedEndOfPath.NotReached);
				if (effectivelyReached != output.effectivelyReachedDestination[agentIndex]) {
					output.effectivelyReachedDestination[agentIndex] = effectivelyReached;

					if (effectivelyReached == ReachedEndOfPath.Reached) {
						// Mark this agent as queued to prevent it from being added to the queue again.
						queued[agentIndex] = true;

						// Changing to the Reached flag may affect the calculations for other agents.
						// So we iterate over all agents that may be affected and enqueue them again.
						var count = inArrowCounts[agentIndex];
						for (int i = 0; i < count; i++) {
							var inArrow = inArrows[agentIndex*SimulatorBurst.MaxBlockingAgentCount + i];
							if (!queued[inArrow]) que.PushEnd(inArrow);
						}
					}
				}
			}
			MarkerFirstPass.End();


			int iteration = 0;
			while (que.Length > 0) {
				var agentIndex = que.PopStart();
				iteration++;
				// If we are already at the reached stage, the result can never change.
				if (output.effectivelyReachedDestination[agentIndex] == ReachedEndOfPath.Reached) continue;
				queued[agentIndex] = false;

				var ourSpeed = output.speed[agentIndex];
				var ourEndOfPath = agentData.endOfPath[agentIndex];
				// Ignore if destination is not set
				// TODO: Will this never trigger due to FloatMode.Fast?
				// Should be ok anyway, since the distance calculations below will filter it out anyway.
				if (!math.isfinite(ourEndOfPath.x)) continue;

				var ourPosition = agentData.position[agentIndex];
				var blockedAndSlow = tempData[agentIndex].blockedAndSlow;
				var distToEndSq = tempData[agentIndex].distToEndSq;
				var ourRadius = agentData.radius[agentIndex];
				var reachedEndOfPath = false;
				var flowFollowing = false;

				// Heuristic 4
				for (int i = 0; i < SimulatorBurst.MaxBlockingAgentCount; i++) {
					var blockingAgentIndex = output.blockedByAgents[agentIndex*SimulatorBurst.MaxBlockingAgentCount + i];
					if (blockingAgentIndex == -1) break;

					var otherEndOfPath = agentData.endOfPath[blockingAgentIndex];
					var otherRadius = agentData.radius[blockingAgentIndex];

					// Check if the other agent has a destination in roughly the same position as this agent.
					// If we are further from the destination we tolarate larger deviations.
					var endOfPathsOverlapping = math.lengthsq(otherEndOfPath - ourEndOfPath) <= distToEndSq*(0.5f*0.5f);
					var otherReached = output.effectivelyReachedDestination[blockingAgentIndex] == ReachedEndOfPath.Reached;

					if (otherReached && (endOfPathsOverlapping || math.lengthsq(ourEndOfPath - agentData.position[blockingAgentIndex]) < math.lengthsq(ourRadius+otherRadius))) {
						var otherSpeed = output.speed[blockingAgentIndex];
						flowFollowing |= math.min(ourSpeed, otherSpeed) < 0.01f;
						reachedEndOfPath |= blockedAndSlow;
					}
				}

				var effectivelyReached = reachedEndOfPath ? ReachedEndOfPath.Reached : (flowFollowing ? ReachedEndOfPath.ReachedSoon : ReachedEndOfPath.NotReached);
				// We do not check for all things that are checked in the first pass. So incorporate the previous information by taking the max.
				effectivelyReached = (ReachedEndOfPath)math.max((int)effectivelyReached, (int)output.effectivelyReachedDestination[agentIndex]);

				if (effectivelyReached != output.effectivelyReachedDestination[agentIndex]) {
					output.effectivelyReachedDestination[agentIndex] = effectivelyReached;

					if (effectivelyReached == ReachedEndOfPath.Reached) {
						// Mark this agent as queued to prevent it from being added to the queue again.
						queued[agentIndex] = true;

						// Changes to the Reached flag may affect the calculations for other agents.
						// So we iterate over all agents that may be affected and enqueue them again.
						var count = inArrowCounts[agentIndex];
						for (int i = 0; i < count; i++) {
							var inArrow = inArrows[agentIndex*SimulatorBurst.MaxBlockingAgentCount + i];
							if (!queued[inArrow]) que.PushEnd(inArrow);
						}
					}
				}
			}
		}
	}

	// Note: FloatMode should not be set to Fast because that causes inaccuracies which can lead to
	// agents failing to avoid walls sometimes.
	[BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Default)]
	public struct JobRVO<MovementPlaneWrapper> : Pathfinding.Jobs.IJobParallelForBatched where MovementPlaneWrapper : struct, IMovementPlaneWrapper {
		[ReadOnly]
		public SimulatorBurst.AgentData agentData;

		[ReadOnly]
		public SimulatorBurst.TemporaryAgentData temporaryAgentData;

		[ReadOnly]
		public NavmeshEdges.NavmeshBorderData navmeshEdgeData;

		[WriteOnly]
		public SimulatorBurst.AgentOutputData output;

		public float deltaTime;
		public float symmetryBreakingBias;
		public float priorityMultiplier;
		public bool useNavmeshAsObstacle;

		public bool allowBoundsChecks { get { return true; } }

		const int MaxObstacleCount = 50;

		public CommandBuilder draw;

		public void Execute (int startIndex, int batchSize) {
			ExecuteORCA(startIndex, batchSize);
		}

		struct SortByKey : IComparer<int> {
			public UnsafeSpan<float> keys;

			public int Compare (int x, int y) {
				return keys[x].CompareTo(keys[y]);
			}
		}

		/// <summary>
		/// Sorts the array in place using insertion sort.
		/// This is a stable sort.
		/// See: http://en.wikipedia.org/wiki/Insertion_sort
		///
		/// Used only because Unity.Collections.NativeSortExtension.Sort seems to have some kind of code generation bug when using Burst 1.8.2, causing it to throw exceptions.
		/// </summary>
		static void InsertionSort<T, U>(UnsafeSpan<T> data, U comparer) where T : unmanaged where U : IComparer<T> {
			for (int i = 1; i < data.Length; i++) {
				var value = data[i];
				int j = i - 1;
				while (j >= 0 && comparer.Compare(data[j], value) > 0) {
					data[j + 1] = data[j];
					j--;
				}
				data[j + 1] = value;
			}
		}

		private static readonly ProfilerMarker MarkerConvertObstacles1 = new ProfilerMarker("RVOConvertObstacles1");
		private static readonly ProfilerMarker MarkerConvertObstacles2 = new ProfilerMarker("RVOConvertObstacles2");

		/// <summary>
		/// Generates ORCA half-planes for all obstacles near the agent.
		/// For more details refer to the ORCA (Optimal Reciprocal Collision Avoidance) paper.
		///
		/// This function takes in several arrays which are just used for temporary data. This is to avoid the overhead of allocating the arrays once for every agent.
		/// </summary>
		void GenerateObstacleVOs (int agentIndex, NativeList<int> adjacentObstacleIdsScratch, NativeArray<int2> adjacentObstacleVerticesScratch, NativeArray<float> segmentDistancesScratch, NativeArray<int> sortedVerticesScratch, NativeArray<ORCALine> orcaLines, NativeArray<int> orcaLineToAgent, [NoAlias] ref int numLines, [NoAlias] in MovementPlaneWrapper movementPlane, float2 optimalVelocity) {
			if (!useNavmeshAsObstacle) return;

			var localPosition = movementPlane.ToPlane(agentData.position[agentIndex], out var agentElevation);
			var agentHeight = agentData.height[agentIndex];
			var agentRadius = agentData.radius[agentIndex];
			var obstacleRadius = agentRadius * 0.01f;
			var inverseObstacleTimeHorizon = math.rcp(agentData.obstacleTimeHorizon[agentIndex]);

			ExpectNotAliased(in agentData.collisionNormal, in agentData.position);

			var hierarchicalNodeIndex = agentData.hierarchicalNodeIndex[agentIndex];
			if (hierarchicalNodeIndex == -1) return;

			var size = (obstacleRadius + agentRadius + agentData.obstacleTimeHorizon[agentIndex] * agentData.maxSpeed[agentIndex]) * new float3(2, 0, 2);
			size.y = agentData.height[agentIndex] * 2f;
			var bounds = new Bounds(new Vector3(localPosition.x, agentElevation, localPosition.y), size);
			var boundingRadiusSq = math.lengthsq(bounds.extents);
			adjacentObstacleIdsScratch.Clear();

			var worldBounds = movementPlane.ToWorld(bounds);
			navmeshEdgeData.GetObstaclesInRange(hierarchicalNodeIndex, worldBounds, adjacentObstacleIdsScratch);

#if UNITY_EDITOR
			if (agentData.HasDebugFlag(agentIndex, AgentDebugFlags.Obstacles)) {
				draw.PushMatrix(movementPlane.matrix);
				draw.PushMatrix(new float4x4(
					new float4(1, 0, 0, 0),
					new float4(0, 0, -1, 0),
					new float4(0, 1, 0, 0),
					new float4(0, 0, 0, 1)
					));
				draw.WireBox(bounds, Color.blue);
				draw.PopMatrix();
				draw.PopMatrix();
			}
#endif

			// TODO: For correctness all obstacles should be added in nearest-to-farthest order.
			// This loop should be split up.
			for (int oi = 0; oi < adjacentObstacleIdsScratch.Length; oi++) {
				MarkerConvertObstacles1.Begin();
				var obstacleId = adjacentObstacleIdsScratch[oi];

				var obstacleAllocations = navmeshEdgeData.obstacleData.obstacles[obstacleId];
				var vertices = navmeshEdgeData.obstacleData.obstacleVertices.GetSpan(obstacleAllocations.verticesAllocation);
				var groups = navmeshEdgeData.obstacleData.obstacleVertexGroups.GetSpan(obstacleAllocations.groupsAllocation);
				int vertexOffset = 0;
				int candidateVertexCount = 0;
				for (int i = 0; i < groups.Length; i++) {
					var group = groups[i];
					// Check if the group does not overlap with our bounds at all
					if (!math.all((group.boundsMx >= worldBounds.min) & (group.boundsMn <= worldBounds.max))) {
						vertexOffset += group.vertexCount;
						continue;
					}


					var startVertex = vertexOffset;
					var endVertex = vertexOffset + group.vertexCount - 1;
					if (endVertex >= adjacentObstacleVerticesScratch.Length) {
						// Too many vertices. Skip remaining vertices.
						break;
					}

					for (int vi = startVertex; vi < startVertex + group.vertexCount; vi++) {
						// X coordinate is the index of the previous vertex, the y coordinate is the next vertex
						adjacentObstacleVerticesScratch[vi] = new int2(vi - 1, vi + 1);
					}
					// UnityEngine.Assertions.Assert.AreEqual(vertexCount, endVertex + 1);

					// Patch the start and end vertices to be correct.
					// In a chain the last vertex doesn't start a new segment so we just make it loop back on itself.
					// In a loop the last vertex connects to the first vertex.
					adjacentObstacleVerticesScratch[startVertex] = new int2(group.type == ObstacleType.Loop ? endVertex : startVertex, adjacentObstacleVerticesScratch[startVertex].y);
					adjacentObstacleVerticesScratch[endVertex] = new int2(adjacentObstacleVerticesScratch[endVertex].x, group.type == ObstacleType.Loop ? startVertex : endVertex);

					for (int vi = 0; vi < group.vertexCount; vi++) {
						var vertex = vertices[vi + vertexOffset];
						int next = adjacentObstacleVerticesScratch[vi + startVertex].y;
						var pos = movementPlane.ToPlane(vertex) - localPosition;
						var nextPos = movementPlane.ToPlane(vertices[next]) - localPosition;
						var dir = nextPos - pos;
						var closestT = ClosestPointOnSegment(pos, dir / math.lengthsq(dir), float2.zero, 0, 1);
						var dist = math.lengthsq(pos + dir*closestT);
						segmentDistancesScratch[vi + startVertex] = dist;

						if (dist <= boundingRadiusSq && candidateVertexCount < sortedVerticesScratch.Length) {
							sortedVerticesScratch[candidateVertexCount] = vi + startVertex;
							candidateVertexCount++;
						}
					}

					vertexOffset += group.vertexCount;
				}

				MarkerConvertObstacles1.End();

				MarkerConvertObstacles2.Begin();
				// Sort obstacle segments by distance from the agent
				InsertionSort(sortedVerticesScratch.AsUnsafeSpan().Slice(0, candidateVertexCount), new SortByKey {
					keys = segmentDistancesScratch.AsUnsafeSpan().Slice(0, vertexOffset)
				});

				for (int i = 0; i < candidateVertexCount; i++) {
					// In the unlikely event that we exceed the maximum number of obstacles, we just skip the remaining ones.
					if (numLines >= MaxObstacleCount) break;

					// Processing the obstacle defined by v1 and v2
					//
					// v0                v3
					//   \               /
					//    \             /
					//    v1 ========= v2
					//
					var v1Index = sortedVerticesScratch[i];

					// If the obstacle is too far away, we can skip it.
					// Since the obstacles are sorted by distance we can break here.
					if (segmentDistancesScratch[v1Index] > 0.25f*size.x*size.x) break;

					var v0Index = adjacentObstacleVerticesScratch[v1Index].x;
					var v2Index = adjacentObstacleVerticesScratch[v1Index].y;
					if (v2Index == v1Index) continue;
					var v3Index = adjacentObstacleVerticesScratch[v2Index].y;
					UnityEngine.Assertions.Assert.AreNotEqual(v1Index, v3Index);
					UnityEngine.Assertions.Assert.AreNotEqual(v0Index, v2Index);

					var v0 = vertices[v0Index];
					var v1 = vertices[v1Index];
					var v2 = vertices[v2Index];
					var v3 = vertices[v3Index];

					var v0Position = movementPlane.ToPlane(v0) - localPosition;
					var v1Position = movementPlane.ToPlane(v1, out var e1) - localPosition;
					var v2Position = movementPlane.ToPlane(v2, out var e2) - localPosition;
					var v3Position = movementPlane.ToPlane(v3) - localPosition;

					// Assume the obstacle has the same height as the agent, then check if they overlap along the elevation axis.
					if (math.max(e1, e2) + agentHeight < agentElevation || math.min(e1, e2) > agentElevation + agentHeight) {
						// The obstacle is not in the agent's elevation range. Ignore it.
						continue;
					}

					var length = math.length(v2Position - v1Position);
					if (length < 0.0001f) continue;
					var segmentDir = (v2Position - v1Position) * math.rcp(length);

					if (det(segmentDir, -v1Position) > obstacleRadius) {
						// Agent is significantly on the wrong side of the segment (on the "inside"). Ignore it.
						continue;
					}

					// Check if this velocity obstacle completely behind previously added ORCA lines.
					// If so, this obstacle is redundant and we can ignore it.
					// This is not just a performance optimization. Using the ORCA lines for closer
					// obstacles is better since obstacles further away can add ORCA lines that
					// restrict the velocity space unnecessarily. The ORCA line is more conservative than the VO.
					bool alreadyCovered = false;

					const float EPSILON = 0.0001f;
					for (var j = 0; j < numLines; j++) {
						var line = orcaLines[j];
						if (
							// Check if this velocity-obstacle is completely inside the previous ORCA line's infeasible half-plane region.
							det(inverseObstacleTimeHorizon * v1Position - line.point, line.direction) - inverseObstacleTimeHorizon * obstacleRadius >= -EPSILON &&
							det(inverseObstacleTimeHorizon * v2Position - line.point, line.direction) - inverseObstacleTimeHorizon * obstacleRadius >= -EPSILON
							) {
							alreadyCovered = true;
							break;
						}
					}
					if (alreadyCovered) {
						continue;
					}

					var obstacleOptimizationVelocity = float2.zero;
					var distanceAlongSegment = math.dot(obstacleOptimizationVelocity - v1Position, segmentDir);
					var closestPointOnSegment = v1Position + distanceAlongSegment * segmentDir;
					var distanceToLineSq = math.lengthsq(closestPointOnSegment - obstacleOptimizationVelocity);
					var distanceToSegmentSq = math.lengthsq((v1Position + math.clamp(distanceAlongSegment, 0, length) * segmentDir));

					var v1Convex = leftOrColinear(v1Position - v0Position, segmentDir);
					var v2Convex = leftOrColinear(segmentDir, v3Position - v2Position);

					if (distanceToSegmentSq < obstacleRadius*obstacleRadius) {
						if (distanceAlongSegment < 0.0f) {
							// Collision with left vertex, ignore if the vertex is not convex
							if (v1Convex) {
								orcaLineToAgent[numLines] = -1;
								orcaLines[numLines++] = new ORCALine {
									point = -v1Position * 0.1f,
									direction = math.normalizesafe(rot90(v1Position)),
								};
							}
						} else if (distanceAlongSegment > length) {
							// Collision with right vertex
							// Ignore if the vertex is not convex, or if it will be taken care of
							// by the neighbour obstacle segment.
							if (v2Convex && leftOrColinear(v2Position, v3Position - v2Position)) {
								orcaLineToAgent[numLines] = -1;
								orcaLines[numLines++] = new ORCALine {
									point = -v2Position * 0.1f,
									direction = math.normalizesafe(rot90(v2Position)),
								};
							}
						} else {
							// Collision with segment
							orcaLineToAgent[numLines] = -1;
							orcaLines[numLines++] = new ORCALine {
								point = -closestPointOnSegment * 0.1f,
								direction = -segmentDir,
							};
						}
						continue;
					}

					// Represents rays starting points on the VO circles, going in a tangent direction away from the agent.
					float2 leftLegDirection, rightLegDirection;

					if ((distanceAlongSegment < 0 || distanceAlongSegment > 1) && distanceToLineSq <= obstacleRadius*obstacleRadius) {
						// Obliquely viewed so that the circle around one of the vertices is all that is visible from p. p = obstacleOptimizationVelocity
						//     _____________________________  _ _ _ _ _ _ _ _ _ _ _ _
						//   _/     \_               _/     \_
						//  /         \             /         \
						//  |    v1   |             |    v2   |
						//  \_       _/             \_       _/          p
						//    \_____/_________________\_____/ _ _ _ _ _ _ _ _ _ _ _ _

						// Collapse segment to a single point, making sure that v0 and v3 are still the neighbouring vertices.
						if (distanceAlongSegment < 0) {
							// Collapse to v1
							// Ignore if not convex
							if (!v1Convex) continue;
							v3Position = v2Position;
							v2Position = v1Position;
							v2Convex = v1Convex;
						} else {
							// Collapse to v2
							if (!v2Convex) continue;
							v0Position = v1Position;
							v1Position = v2Position;
							v1Convex = v2Convex;
						}
						var vertexDistSq = math.lengthsq(v1Position);
						// Distance from p to the points where the legs (tangents) touch the circle around the vertex.
						float leg = math.sqrt(vertexDistSq - obstacleRadius*obstacleRadius);
						var posNormal = new float2(-v1Position.y, v1Position.x);
						// These become normalized
						leftLegDirection = (v1Position*leg + posNormal*obstacleRadius) / vertexDistSq;
						rightLegDirection = (v1Position*leg - posNormal*obstacleRadius) / vertexDistSq;
					} else {
						// This is the common case (several valid positions of p are shown). p = obstacleOptimizationVelocity
						//
						//        p
						//     _____________________________
						//   _/     \_               _/     \_
						//  /         \             /         \
						//  |    v1   |             |    v2   |
						//  \_       _/             \_       _/
						//    \_____/_________________\_____/
						//
						//                       p                   p

						if (v1Convex) {
							var vertexDistSq = math.lengthsq(v1Position);
							float leg = math.sqrt(vertexDistSq - obstacleRadius*obstacleRadius);
							var posNormal = new float2(-v1Position.y, v1Position.x);
							// This becomes normalized
							leftLegDirection = (v1Position*leg + posNormal*obstacleRadius) / vertexDistSq;
						} else {
							leftLegDirection = -segmentDir;
						}

						if (v2Convex) {
							var vertexDistSq = math.lengthsq(v2Position);
							float leg = math.sqrt(vertexDistSq - obstacleRadius*obstacleRadius);
							var posNormal = new float2(-v2Position.y, v2Position.x);
							rightLegDirection = (v2Position*leg - posNormal*obstacleRadius) / vertexDistSq;
						} else {
							rightLegDirection = segmentDir;
						}
					}

					// Legs should never point into the obstacle for legs added by convex vertices.
					// The neighbouring vertex will add a better obstacle for those cases.
					//
					// In that case we replace the legs with the neighbouring segments, and if the closest
					// point is on those segments we know we can ignore them because the
					// neighbour will handle it.
					//
					// It's important that we don't include the case when they are colinear,
					// because if v1=v0 (or v2=v3), which can happen at the end of a chain, the
					// determinant will always be zero and so they will seem colinear.
					//
					// Note: One might think that this should apply to all vertices, not just convex ones.
					// Consider this case where you might think a non-convex vertices otherwise would
					// cause 'ghost' obstacles:
					//   ___
					//  |  | A
					//  |  |
					//  |   \
					//  |____\ B
					//       <-X
					//
					// If X is an agent, moving to the left. It could get stuck against the segment A.
					// This is because the vertex between A and B is concave, and it will generate a leg
					// pointing downwards.
					//
					// However, this does not cause a problem in practice. Because if the horizontal segment at the bottom is added first (as it should be)
					// then A and B will be discarded since they will be completely behind the ORCA line added by the horizontal segment.
					bool isLeftLegForeign = false;
					bool isRightLegForeign = false;
					if (v1Convex && left(leftLegDirection, v0Position - v1Position)) {
						// Left leg points into obstacle
						leftLegDirection = v0Position - v1Position;
						isLeftLegForeign = true;
					}

					if (v2Convex && right(rightLegDirection, v3Position - v2Position)) {
						// Right leg points into obstacle
						rightLegDirection = v3Position - v2Position;
						isRightLegForeign = true;
					}


					// The velocity obstacle for this segment consists of a left leg, right leg,
					// a cutoff line, and two circular arcs where the legs and the cutoff line join together.
					// LeftLeg                                   RightLeg
					//    \      _____________________________      /
					//     \   _/     \_               _/     \_   /
					//      \ /         \             /         \ /
					//       \|    v1   |             |    v2   |/
					//        \_       _/             \_       _/
					//          \_____/_________________\_____/
					//                     Cutoff Line
					//
					// In case only one vertex makes up the obstacle then we instead have just a left leg, right leg, and a single circular arc.
					//
					//  LeftLeg           RightLeg
					//    \      _____      /
					//     \   _/     \_   /
					//      \ /         \ /
					//       \|         |/
					//        \_       _/
					//          \_____/
					//


					// We first check if the velocity will be projected on those circular segments.
					var leftCutoff = inverseObstacleTimeHorizon * v1Position;
					var rightCutoff = inverseObstacleTimeHorizon * v2Position;
					var cutoffDir = rightCutoff - leftCutoff;
					var cutoffLength = math.lengthsq(cutoffDir);

					// Projection on the cutoff line (between 0 and 1 if the projection is on the cutoff segment)
					var t = cutoffLength <= 0.00001f ? 0.5f : math.dot(optimalVelocity - leftCutoff, cutoffDir)/cutoffLength;
					// Negative if the closest point on the rays reprensenting the legs is before the ray starts
					var tLeft = math.dot(optimalVelocity - leftCutoff, leftLegDirection);
					var tRight = math.dot(optimalVelocity - rightCutoff, rightLegDirection);


					// Check if the projected velocity is on the circular arcs
					if ((t < 0.0f && tLeft < 0.0f) || (t > 1.0f && tRight < 0.0f) || (cutoffLength <= 0.00001f && tLeft < 0.0f && tRight < 0.0f)) {
						var arcCenter = t <= 0.5f ? leftCutoff : rightCutoff;

						var unitW = math.normalizesafe(optimalVelocity - arcCenter);
						orcaLineToAgent[numLines] = -1;
						orcaLines[numLines++] = new ORCALine {
							point = arcCenter + obstacleRadius * inverseObstacleTimeHorizon * unitW,
							direction = new float2(unitW.y, -unitW.x),
						};
						continue;
					}

					// If the closest point is not on the arcs, then we project it on the legs or the cutoff line and pick the closest one.
					// Note that all these distances should be reduced by obstacleRadius, but we only compare the values, so this doesn't matter.
					float distToCutoff = (t > 1.0f || t < 0.0f || cutoffLength < 0.0001f ? math.INFINITY : math.lengthsq(optimalVelocity - (leftCutoff + t * cutoffDir)));
					float distToLeftLeg = (tLeft < 0.0f ? math.INFINITY : math.lengthsq(optimalVelocity - (leftCutoff + tLeft * leftLegDirection)));
					float distToRightLeg = (tRight < 0.0f ? math.INFINITY : math.lengthsq(optimalVelocity - (rightCutoff + tRight * rightLegDirection)));
					var selected = 0;
					var mn = distToCutoff;
					if (distToLeftLeg < mn) {
						mn = distToLeftLeg;
						selected = 1;
					}
					if (distToRightLeg < mn) {
						mn = distToRightLeg;
						selected = 2;
					}

					if (selected == 0) {
						// Project on cutoff line
						orcaLineToAgent[numLines] = -1;
						orcaLines[numLines++] = new ORCALine {
							point = leftCutoff + obstacleRadius * inverseObstacleTimeHorizon * new float2(segmentDir.y, -segmentDir.x),
							direction = -segmentDir,
						};
					} else if (selected == 1) {
						if (!isLeftLegForeign) {
							orcaLineToAgent[numLines] = -1;
							orcaLines[numLines++] = new ORCALine {
								point = leftCutoff + obstacleRadius * inverseObstacleTimeHorizon * new float2(-leftLegDirection.y, leftLegDirection.x),
								direction = leftLegDirection,
							};
						}
					} else if (selected == 2) {
						if (!isRightLegForeign) {
							orcaLineToAgent[numLines] = -1;
							orcaLines[numLines++] = new ORCALine {
								point = rightCutoff + obstacleRadius * inverseObstacleTimeHorizon * new float2(rightLegDirection.y, -rightLegDirection.x),
								direction = -rightLegDirection,
							};
						}
					}
				}
				MarkerConvertObstacles2.End();
			}
		}

		public void ExecuteORCA (int startIndex, int batchSize) {
			int endIndex = startIndex + batchSize;

			NativeArray<ORCALine> orcaLines = new NativeArray<ORCALine>(SimulatorBurst.MaxNeighbourCount + MaxObstacleCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			NativeArray<ORCALine> scratchBuffer = new NativeArray<ORCALine>(SimulatorBurst.MaxNeighbourCount + MaxObstacleCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			NativeArray<float> segmentDistancesScratch = new NativeArray<float>(SimulatorBurst.MaxObstacleVertices, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			NativeArray<int> sortedVerticesScratch = new NativeArray<int>(SimulatorBurst.MaxObstacleVertices, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			NativeArray<int2> adjacentObstacleVertices = new NativeArray<int2>(4 * SimulatorBurst.MaxObstacleVertices, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			NativeArray<int> orcaLineToAgent = new NativeArray<int>(SimulatorBurst.MaxNeighbourCount + MaxObstacleCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			NativeList<int> adjacentObstacleIdsScratch = new NativeList<int>(16, Allocator.Temp);

			for (int agentIndex = startIndex; agentIndex < endIndex; agentIndex++) {
				if (!agentData.version[agentIndex].Valid) continue;

				if (agentData.manuallyControlled[agentIndex]) {
					output.speed[agentIndex] = agentData.desiredSpeed[agentIndex];
					output.targetPoint[agentIndex] = agentData.targetPoint[agentIndex];
					output.blockedByAgents[agentIndex*SimulatorBurst.MaxBlockingAgentCount] = -1;
					continue;
				}

				var position = agentData.position[agentIndex];

				if (agentData.locked[agentIndex]) {
					output.speed[agentIndex] = 0;
					output.targetPoint[agentIndex] = position;
					output.blockedByAgents[agentIndex*SimulatorBurst.MaxBlockingAgentCount] = -1;
					continue;
				}

				MovementPlaneWrapper movementPlane = default;
				movementPlane.Set(agentData.movementPlane[agentIndex]);

				// The RVO algorithm assumes we will continue to
				// move in roughly the same direction
				float2 optimalVelocity = movementPlane.ToPlane(temporaryAgentData.currentVelocity[agentIndex]);
				int numLines = 0;
				// TODO: Obstacles are typically behind agents, so it's better to add the agent orca lines first to improve culling.
				// However, the 3D optimization program requires obstacle lines to be added first. Not to mention that the culling
				// is not strictly accurate for fixed obstacle since they cannot be moved backwards by the 3D linear program.
				GenerateObstacleVOs(agentIndex, adjacentObstacleIdsScratch, adjacentObstacleVertices, segmentDistancesScratch, sortedVerticesScratch, orcaLines, orcaLineToAgent, ref numLines, in movementPlane, optimalVelocity);
				int numFixedLines = numLines;

				var neighbours = temporaryAgentData.neighbours.Slice(agentIndex*SimulatorBurst.MaxNeighbourCount, SimulatorBurst.MaxNeighbourCount);

				float agentTimeHorizon = agentData.agentTimeHorizon[agentIndex];
				float inverseAgentTimeHorizon = math.rcp(agentTimeHorizon);
				float priority = agentData.priority[agentIndex];

				var localPosition = movementPlane.ToPlane(position);
				var agentRadius = agentData.radius[agentIndex];
				var distSqToEndOfPath = math.all(math.isfinite(agentData.endOfPath[agentIndex])) ? math.lengthsq(agentData.endOfPath[agentIndex] - position) : float.PositiveInfinity;

				for (int neighbourIndex = 0; neighbourIndex < neighbours.Length; neighbourIndex++) {
					int otherIndex = neighbours[neighbourIndex];
					// Indicates that there are no more neighbours (see JobRVOCalculateNeighbours)
					if (otherIndex == -1) break;

					var otherPosition = agentData.position[otherIndex];
					var relativePosition = movementPlane.ToPlane(otherPosition - position);
					float combinedRadius = agentRadius + agentData.radius[otherIndex];

					var otherPriority = agentData.priority[otherIndex] * priorityMultiplier;

					// TODO: Remove branches to possibly vectorize
					float avoidanceStrength;
					if (agentData.locked[otherIndex] || agentData.manuallyControlled[otherIndex]) {
						avoidanceStrength = 1;
					} else if (otherPriority > 0.00001f || priority > 0.00001f) {
						avoidanceStrength = otherPriority / (priority + otherPriority);
					} else {
						// Both this agent's priority and the other agent's priority is zero or negative
						// Assume they have the same priority
						avoidanceStrength = 0.5f;
					}

					// We assume that the other agent will continue to move with roughly the same velocity if the priorities for the agents are similar.
					// If the other agent has a higher priority than this agent (avoidanceStrength > 0.5) then we will assume it will move more along its
					// desired velocity. This will have the effect of other agents trying to clear a path for where a high priority agent wants to go.
					// If this is not done then even high priority agents can get stuck when it is really crowded and they have had to slow down.
					float2 otherOptimalVelocity = movementPlane.ToPlane(math.lerp(temporaryAgentData.currentVelocity[otherIndex], temporaryAgentData.desiredVelocity[otherIndex], math.clamp(2*avoidanceStrength - 1, 0, 1)));

					if (agentData.flowFollowingStrength[otherIndex] > 0) {
						// When flow following strength is 1 the component of the other agent's velocity that is in the direction of this agent is removed.
						// That is, we pretend that the other agent does not move towards this agent at all.
						// This will make it impossible for the other agent to "push" this agent away.
						var strength = agentData.flowFollowingStrength[otherIndex] * agentData.flowFollowingStrength[agentIndex];
						var relativeDir = math.normalizesafe(relativePosition);
						otherOptimalVelocity -= relativeDir * (strength * math.min(0, math.dot(otherOptimalVelocity, relativeDir)));
					}

					var dist = math.length(relativePosition);

					// Figure out an approximate time to collision. We avoid using the current velocities of the agents because that leads to oscillations,
					// as the agents change their velocities, which results in a change to the time to collision, which makes them change their velocities again.
					var minimumDistanceToCollision = math.max(0, dist - combinedRadius);
					if (agentData.locked[otherIndex] && minimumDistanceToCollision*minimumDistanceToCollision > distSqToEndOfPath) {
						// The other agent is locked and we cannot collide with it until we have reached the end of our path.
						// That means it can be safely ignored.
						// This will reduce "shyness" around locked agents.
						// TODO: This should ideally be done for non-locked agents too, but with a better heuristic.
						continue;
					}

					var minimumTimeToCollision = minimumDistanceToCollision / math.max(combinedRadius, agentData.desiredSpeed[agentIndex] + agentData.desiredSpeed[otherIndex]);

					// Adjust the radius to make the avoidance smoother.
					// The agent will slowly start to take another agent into account instead of making a sharp turn.
					float normalizedTime = minimumTimeToCollision * inverseAgentTimeHorizon;
					// normalizedTime <= 0.5 => 0% effect
					// normalizedTime  = 1.0 => 100% effect
					var factor = math.clamp((normalizedTime - 0.5f)*2.0f, 0, 1);
					combinedRadius *= 1 - factor;

					// Adjust the time horizon to make the agent approach another agent less conservatively.
					// This makes the velocity curve closer to sqrt(1-t) instead of exp(-t) as it comes to a stop, which looks nicer.
					var tempInverseTimeHorizon = 1.0f/math.max(0.1f*agentTimeHorizon, agentTimeHorizon * math.clamp(math.sqrt(2f*minimumTimeToCollision), 0, 1));

					orcaLines[numLines] = new ORCALine(localPosition, relativePosition, optimalVelocity, otherOptimalVelocity, combinedRadius, 0.1f, tempInverseTimeHorizon);
					orcaLineToAgent[numLines] = otherIndex;
					numLines++;
#if UNITY_EDITOR
					if (agentData.HasDebugFlag(agentIndex, AgentDebugFlags.AgentVOs)) {
						draw.PushMatrix(math.mul(float4x4.TRS(position, quaternion.identity, 1), movementPlane.matrix));
						var voCenter = math.lerp(optimalVelocity, otherOptimalVelocity, 0.5f);
						DrawVO(draw, relativePosition * tempInverseTimeHorizon + otherOptimalVelocity, combinedRadius * tempInverseTimeHorizon, otherOptimalVelocity, Color.black);
						draw.PopMatrix();
					}
#endif
				}

				// Add an obstacle for the collision normal.
				// This is mostly deprecated, but kept for compatibility.
				var collisionNormal = math.normalizesafe(movementPlane.ToPlane(agentData.collisionNormal[agentIndex]));
				if (math.any(collisionNormal != 0)) {
					orcaLines[numLines] = new ORCALine {
						point = float2.zero,
						direction = new float2(collisionNormal.y, -collisionNormal.x),
					};
					orcaLineToAgent[numLines] = -1;
					numLines++;
				}

				var desiredVelocity = movementPlane.ToPlane(temporaryAgentData.desiredVelocity[agentIndex]);
				var desiredTargetPointInVelocitySpace = temporaryAgentData.desiredTargetPointInVelocitySpace[agentIndex];
				var originalDesiredVelocity = desiredVelocity;
				var symmetryBias = symmetryBreakingBias * (1 - agentData.flowFollowingStrength[agentIndex]);
				// Bias the desired velocity to avoid symmetry issues (esp. when two agents are heading straight towards one another).
				// Do not bias velocities if the agent is heading towards an obstacle (not an agent).
				bool insideAnyVO = BiasDesiredVelocity(orcaLines.AsUnsafeSpan().Slice(numFixedLines, numLines - numFixedLines), ref desiredVelocity, ref desiredTargetPointInVelocitySpace, symmetryBias);
				// If the velocity is outside all agent orca half-planes, do a more thorough check of all orca lines (including obstacles).
				insideAnyVO = insideAnyVO || DistanceInsideVOs(orcaLines.AsUnsafeSpan().Slice(0, numLines), desiredVelocity) > 0;


#if UNITY_EDITOR
				if (agentData.HasDebugFlag(agentIndex, AgentDebugFlags.ObstacleVOs)) {
					draw.PushColor(new Color(1, 1, 1, 0.2f));
					draw.PushMatrix(math.mul(float4x4.TRS(position, quaternion.identity, 1), movementPlane.matrix));
					for (int i = 0; i < numLines; i++) {
						orcaLines[i].DrawAsHalfPlane(draw, agentData.radius[agentIndex] * 5.0f, 1.0f, i >= numFixedLines ? Color.magenta : Color.Lerp(Color.magenta, Color.black, 0.5f));
					}
					draw.PopMatrix();
					draw.PopColor();
				}
#endif

				if (!insideAnyVO && math.all(math.abs(temporaryAgentData.collisionVelocityOffsets[agentIndex]) < 0.001f)) {
					// Desired velocity can be used directly since it was not inside any velocity obstacle.
					// No need to run optimizer because this will be the global minima.
					// This is also a special case in which we can set the
					// calculated target point to the desired target point
					// instead of calculating a point based on a calculated velocity
					// which is an important difference when the agent is very close
					// to the target point
					// TODO: Not actually guaranteed to be global minima if desiredTargetPointInVelocitySpace.magnitude < desiredSpeed
					// maybe do something different here?
#if UNITY_EDITOR
					if (agentData.HasDebugFlag(agentIndex, AgentDebugFlags.DesiredVelocity)) {
						draw.xy.Cross(movementPlane.ToWorld(localPosition + desiredVelocity), Color.magenta);
						draw.xy.Cross(movementPlane.ToWorld(localPosition + desiredTargetPointInVelocitySpace), Color.yellow);
					}
#endif

					output.targetPoint[agentIndex] = position + movementPlane.ToWorld(desiredTargetPointInVelocitySpace, 0);
					output.speed[agentIndex] = agentData.desiredSpeed[agentIndex];
					output.blockedByAgents[agentIndex*SimulatorBurst.MaxBlockingAgentCount] = -1;
					output.forwardClearance[agentIndex] = float.PositiveInfinity;
				} else {
					var maxSpeed = agentData.maxSpeed[agentIndex];
					var allowedVelocityDeviationAngles = agentData.allowedVelocityDeviationAngles[agentIndex];
					LinearProgram2Output lin;
					if (math.all(allowedVelocityDeviationAngles == 0)) {
						// Common case, the desired velocity is a point
						lin = LinearProgram2D(orcaLines, numLines, maxSpeed, desiredVelocity, false);
					} else {
						// The desired velocity is a segment, not a point

						// Rotate the desired velocity allowedVelocityDeviationAngles.x radians and allowedVelocityDeviationAngles.y radians respectively
						math.sincos(allowedVelocityDeviationAngles, out float2 s, out float2 c);
						var xs = desiredVelocity.x*c - desiredVelocity.y*s;
						var ys = desiredVelocity.x*s + desiredVelocity.y*c;
						var desiredVelocityLeft = new float2(xs.x, ys.x);
						var desiredVelocityRight = new float2(xs.y, ys.y);

						var desiredVelocityLeftDir = desiredVelocity - desiredVelocityLeft;

						// Normalize and store length
						var desiredVelocityLeftSegmentLength = math.length(desiredVelocityLeftDir);
						desiredVelocityLeftDir = math.select(float2.zero, desiredVelocityLeftDir * math.rcp(desiredVelocityLeftSegmentLength), desiredVelocityLeftSegmentLength > math.FLT_MIN_NORMAL);

						var desiredVelocityRightDir = desiredVelocity - desiredVelocityRight;
						var desiredVelocityRightSegmentLength = math.length(desiredVelocityRightDir);
						desiredVelocityRightDir = math.select(float2.zero, desiredVelocityRightDir * math.rcp(desiredVelocityRightSegmentLength), desiredVelocityRightSegmentLength > math.FLT_MIN_NORMAL);

						// var tOptimal = ClosestPointOnSegment(desiredVelocityLeft, desiredVelocityDir, desiredVelocity, 0, desiredVelocitySegmentLength);

						var lin1 = LinearProgram2DSegment(orcaLines, numLines, maxSpeed, desiredVelocityLeft, desiredVelocityLeftDir, 0, desiredVelocityLeftSegmentLength, 1.0f);
						var lin2 = LinearProgram2DSegment(orcaLines, numLines, maxSpeed, desiredVelocityRight, desiredVelocityRightDir, 0, desiredVelocityRightSegmentLength, 1.0f);

						if (lin1.firstFailedLineIndex < lin2.firstFailedLineIndex) {
							lin = lin1;
						} else if (lin2.firstFailedLineIndex < lin1.firstFailedLineIndex) {
							lin = lin2;
						} else {
							lin = math.lengthsq(lin1.velocity - desiredVelocity) < math.lengthsq(lin2.velocity - desiredVelocity) ? lin1 : lin2;
						}
					}

					float2 newVelocity;
					if (lin.firstFailedLineIndex < numLines) {
						newVelocity = lin.velocity;
						LinearProgram3D(orcaLines, numLines, numFixedLines, lin.firstFailedLineIndex, maxSpeed, ref newVelocity, scratchBuffer);
					} else {
						newVelocity = lin.velocity;
					}

#if UNITY_EDITOR
					if (agentData.HasDebugFlag(agentIndex, AgentDebugFlags.ChosenVelocity)) {
						draw.xy.Cross(position + movementPlane.ToWorld(newVelocity), Color.white);
						draw.Arrow(position + movementPlane.ToWorld(desiredVelocity), position + movementPlane.ToWorld(newVelocity), Color.magenta);
					}
#endif

					var blockedByAgentCount = 0;
					for (int i = 0; i < numLines && blockedByAgentCount < SimulatorBurst.MaxBlockingAgentCount; i++) {
						if (orcaLineToAgent[i] != -1 && det(orcaLines[i].direction, orcaLines[i].point - newVelocity) >= -0.001f) {
							// We are blocked by this line
							output.blockedByAgents[agentIndex*SimulatorBurst.MaxBlockingAgentCount + blockedByAgentCount] = orcaLineToAgent[i];
							blockedByAgentCount++;
						}
					}
					if (blockedByAgentCount < SimulatorBurst.MaxBlockingAgentCount) output.blockedByAgents[agentIndex*SimulatorBurst.MaxBlockingAgentCount + blockedByAgentCount] = -1;

					var collisionVelocityOffset = temporaryAgentData.collisionVelocityOffsets[agentIndex];
					if (math.any(collisionVelocityOffset != 0)) {
						// Make the agent move to avoid intersecting other agents (hard collisions)
						newVelocity += temporaryAgentData.collisionVelocityOffsets[agentIndex];

						// Adding the collision offset may have made the velocity invalid, causing it to intersect the wall-velocity-obstacles.
						// We run a second optimization on only the wall-velocity-obstacles to make sure the velocity is valid.
						newVelocity = LinearProgram2D(orcaLines, numFixedLines, maxSpeed, newVelocity, false).velocity;
					}

					output.targetPoint[agentIndex] = position + movementPlane.ToWorld(newVelocity, 0);
					output.speed[agentIndex] = math.min(math.length(newVelocity), maxSpeed);

					var targetDir = math.normalizesafe(movementPlane.ToPlane(agentData.targetPoint[agentIndex] - position));
					var forwardClearance = CalculateForwardClearance(neighbours, movementPlane, position, agentRadius, targetDir);
					output.forwardClearance[agentIndex] = forwardClearance;
					if (agentData.HasDebugFlag(agentIndex, AgentDebugFlags.ForwardClearance) && forwardClearance < float.PositiveInfinity) {
						draw.PushLineWidth(2);
						draw.Ray(position, movementPlane.ToWorld(targetDir) * forwardClearance, Color.red);
						draw.PopLineWidth();
					}
				}
			}
		}

		/// <summary>
		/// Find the distance we can move towards our target without colliding with anything.
		/// May become negative if we are currently colliding with something.
		/// </summary>
		float CalculateForwardClearance (NativeSlice<int> neighbours, MovementPlaneWrapper movementPlane, float3 position, float radius, float2 targetDir) {
			// TODO: Take obstacles into account.
			var smallestIntersectionDistance = float.PositiveInfinity;
			for (int i = 0; i < neighbours.Length; i++) {
				var other = neighbours[i];
				if (other == -1) break;
				var otherPosition = agentData.position[other];
				var combinedRadius = radius + agentData.radius[other];
				// Intersect the ray from our agent towards the destination and check the distance to the intersection with the other agent.
				var otherDir = movementPlane.ToPlane(otherPosition - position);
				// Squared cosine of the angle between otherDir and ourTargetDir
				var cosAlpha = math.dot(math.normalizesafe(otherDir), targetDir);

				// Check if the agent is behind us
				if (cosAlpha < 0) continue;

				var distToOtherSq = math.lengthsq(otherDir);
				var distToClosestPointAlongRay = math.sqrt(distToOtherSq) * cosAlpha;
				var discriminant = combinedRadius*combinedRadius - (distToOtherSq - distToClosestPointAlongRay*distToClosestPointAlongRay);

				// Check if we have any intersection at all
				if (discriminant < 0) continue;
				var distToIntersection = distToClosestPointAlongRay - math.sqrt(discriminant);
				smallestIntersectionDistance = math.min(smallestIntersectionDistance, distToIntersection);
			}
			return smallestIntersectionDistance;
		}

		/// <summary>True if vector2 is to the left of vector1 or if they are colinear.</summary>
		static bool leftOrColinear (float2 vector1, float2 vector2) {
			return det(vector1, vector2) >= 0;
		}

		/// <summary>True if vector2 is to the left of vector1.</summary>
		static bool left (float2 vector1, float2 vector2) {
			return det(vector1, vector2) > 0;
		}

		/// <summary>True if vector2 is to the right of vector1 or if they are colinear.</summary>
		static bool rightOrColinear (float2 vector1, float2 vector2) {
			return det(vector1, vector2) <= 0;
		}

		/// <summary>True if vector2 is to the right of vector1.</summary>
		static bool right (float2 vector1, float2 vector2) {
			return det(vector1, vector2) < 0;
		}

		/// <summary>
		/// Determinant of the 2x2 matrix defined by vector1 and vector2.
		/// Alternatively, the Z component of the cross product of vector1 and vector2.
		/// </summary>
		static float det (float2 vector1, float2 vector2) {
			return vector1.x * vector2.y - vector1.y * vector2.x;
		}

		static float2 rot90 (float2 v) {
			return new float2(-v.y, v.x);
		}

		/// <summary>
		/// A half-plane defined as the line splitting plane.
		///
		/// For ORCA purposes, the infeasible region of the half-plane is on the right side of the line.
		/// </summary>
		struct ORCALine {
			public float2 point;
			public float2 direction;

			public void DrawAsHalfPlane (CommandBuilder draw, float halfPlaneLength, float halfPlaneWidth, Color color) {
				var normal = new float2(direction.y, -direction.x);
				draw.xy.Line(point - direction*10, point + direction*10, color);

				var p = point + normal*halfPlaneWidth*0.5f;
				draw.SolidBox(new float3(p, 0), quaternion.RotateZ(math.atan2(direction.y, direction.x)), new float3(halfPlaneLength, halfPlaneWidth, 0.01f), new Color(0, 0, 0, 0.5f));
			}

			public ORCALine(float2 position, float2 relativePosition, float2 velocity, float2 otherVelocity, float combinedRadius, float timeStep, float invTimeHorizon) {
				var relativeVelocity = velocity - otherVelocity;
				float combinedRadiusSq = combinedRadius*combinedRadius;
				float distSq = math.lengthsq(relativePosition);

				if (distSq > combinedRadiusSq) {
					combinedRadius *= 1.001f;
					// No collision

					// A velocity obstacle is built which is shaped like a truncated cone (see ORCA paper).
					// The cone is truncated by an arc centered at relativePosition/timeHorizon
					// with radius combinedRadius/timeHorizon.
					// The cone extends in the direction of relativePosition.

					// Vector from truncation arc center to relative velocity
					var w = relativeVelocity - invTimeHorizon * relativePosition;
					var wLengthSq = math.lengthsq(w);

					float dot1 = math.dot(w, relativePosition);

					if (dot1 < 0.0f && dot1*dot1 > combinedRadiusSq * wLengthSq) {
						// Project on cut-off circle
						float wLength = math.sqrt(wLengthSq);
						var normalizedW = w / wLength;

						direction = new float2(normalizedW.y, -normalizedW.x);
						var u = (combinedRadius * invTimeHorizon - wLength) * normalizedW;
						point = velocity + 0.5f * u;
					} else {
						// Project on legs
						// Distance from the agent to the point where the "legs" start on the VO
						float legDistance = math.sqrt(distSq - combinedRadiusSq);

						if (det(relativePosition, w) > 0.0f) {
							// Project on left leg
							// Note: This vector is actually normalized
							direction = (relativePosition * legDistance + new float2(-relativePosition.y, relativePosition.x) * combinedRadius) / distSq;
						} else {
							// Project on right leg
							// Note: This vector is actually normalized
							direction = (-relativePosition * legDistance + new float2(-relativePosition.y, relativePosition.x) * combinedRadius) / distSq;
						}

						float dot2 = math.dot(relativeVelocity, direction);
						var u = dot2 * direction - relativeVelocity;
						point = velocity + 0.5f * u;
					}
				} else {
					float invTimeStep = math.rcp(timeStep);
					var dist = math.sqrt(distSq);
					var normalizedDir = math.select(0, relativePosition / dist, dist > math.FLT_MIN_NORMAL);
					var u = normalizedDir * (dist - combinedRadius - 0.001f) * 0.3f * invTimeStep;
					direction = math.normalizesafe(new float2(u.y, -u.x));
					point = math.lerp(velocity, otherVelocity, 0.5f) + u * 0.5f;


					// Original code, the above is a version which works better
					// Collision
					// Project on cut-off circle of timeStep
					//float invTimeStep = 1.0f / timeStep;
					// Vector from cutoff center to relative velocity
					//float2 w = relativeVelocity - invTimeStep * relativePosition;
					//float wLength = math.length(w);
					//float2 unitW = w / wLength;
					//direction = new float2(unitW.y, -unitW.x);
					//var u = (combinedRadius * invTimeStep - wLength) * unitW;
					//point = velocity + 0.5f * u;
				}
			}
		}

		/// <summary>
		/// Calculates how far inside the infeasible region of the ORCA half-planes the velocity is.
		/// Returns 0 if the velocity is in the feasible region of all half-planes.
		/// </summary>
		static float DistanceInsideVOs (UnsafeSpan<ORCALine> lines, float2 velocity) {
			float maxDistance = 0.0f;

			for (int i = 0; i < lines.Length; i++) {
				var distance = det(lines[i].direction, lines[i].point - velocity);
				maxDistance = math.max(maxDistance, distance);
			}

			return maxDistance;
		}

		/// <summary>
		/// Bias towards the right side of agents.
		/// Rotate desiredVelocity at most [value] number of radians. 1 radian ≈ 57°
		/// This breaks up symmetries.
		///
		/// The desired velocity will only be rotated if it is inside a velocity obstacle (VO).
		/// If it is inside one, it will not be rotated further than to the edge of it
		///
		/// The targetPointInVelocitySpace will be rotated by the same amount as the desired velocity
		///
		/// Returns: True if the desired velocity was inside any VO
		/// </summary>
		static bool BiasDesiredVelocity (UnsafeSpan<ORCALine> lines, ref float2 desiredVelocity, ref float2 targetPointInVelocitySpace, float maxBiasRadians) {
			float maxDistance = DistanceInsideVOs(lines, desiredVelocity);

			if (maxDistance == 0.0f) return false;

			var desiredVelocityMagn = math.length(desiredVelocity);

			// Avoid division by zero below
			if (desiredVelocityMagn >= 0.001f) {
				// Rotate the desired velocity clockwise (to the right) at most maxBiasRadians number of radians.
				// We clamp the angle so that we do not rotate more than to the edge of the VO.
				// Assuming maxBiasRadians is small, we can just move it instead and it will give approximately the same effect.
				// See https://en.wikipedia.org/wiki/Small-angle_approximation
				var angle = math.min(maxBiasRadians, maxDistance / desiredVelocityMagn);
				desiredVelocity += new float2(desiredVelocity.y, -desiredVelocity.x) * angle;
				targetPointInVelocitySpace += new float2(targetPointInVelocitySpace.y, -targetPointInVelocitySpace.x) * angle;
			}
			return true;
		}

		/// <summary>
		/// Clip a line to the feasible region of the half-plane given by the clipper.
		/// The clipped line is `line.point + line.direction*tLeft` to `line.point + line.direction*tRight`.
		///
		/// Returns false if the line is parallel to the clipper's border.
		/// </summary>
		static bool ClipLine (ORCALine line, ORCALine clipper, ref float tLeft, ref float tRight) {
			float denominator = det(line.direction, clipper.direction);
			float numerator = det(clipper.direction, line.point - clipper.point);

			if (math.abs(denominator) < 0.0001f) {
				// The two lines are almost parallel
				return false;
			}

			float t = numerator / denominator;

			if (denominator >= 0.0f) {
				// Line i bounds the line on the right
				tRight = math.min(tRight, t);
			} else {
				// Line i bounds the line on the left
				tLeft = math.max(tLeft, t);
			}
			return true;
		}

		static bool ClipBoundary (NativeArray<ORCALine> lines, int lineIndex, float radius, out float tLeft, out float tRight) {
			var line = lines[lineIndex];
			if (!VectorMath.LineCircleIntersectionFactors(line.point, line.direction, radius, out tLeft, out tRight)) {
				return false;
			}

			// Go through all previous lines/half-planes and clip the current line against them
			for (int i = 0; i < lineIndex; i++) {
				float denominator = det(line.direction, lines[i].direction);
				float numerator = det(lines[i].direction, line.point - lines[i].point);

				if (math.abs(denominator) < 0.0001f) {
					// The two lines are almost parallel
					if (numerator < 0.0f) {
						// This line is completely "behind" the other line. So we can ignore it.
						return false;
					} else continue;
				}

				float t = numerator / denominator;

				if (denominator >= 0.0f) {
					// Line i bounds the line on the right
					tRight = math.min(tRight, t);
				} else {
					// Line i bounds the line on the left
					tLeft = math.max(tLeft, t);
				}

				if (tLeft > tRight) {
					// The line is completely outside the previous half-planes
					return false;
				}
			}
			return true;
		}

		static bool LinearProgram1D (NativeArray<ORCALine> lines, int lineIndex, float radius, float2 optimalVelocity, bool directionOpt, ref float2 result) {
			if (!ClipBoundary(lines, lineIndex, radius, out float tLeft, out float tRight)) return false;
			var line = lines[lineIndex];

			if (directionOpt) {
				// Optimize direction
				if (math.dot(optimalVelocity, line.direction) > 0.0f) {
					// Take right extreme
					result = line.point + tRight * line.direction;
				} else {
					// Take left extreme
					result = line.point + tLeft * line.direction;
				}
			} else {
				// Optimize closest point
				float t = math.dot(line.direction, optimalVelocity - line.point);
				result = line.point + math.clamp(t, tLeft, tRight) * line.direction;
			}
			return true;
		}

		struct LinearProgram2Output {
			public float2 velocity;
			public int firstFailedLineIndex;
		}

		static LinearProgram2Output LinearProgram2D (NativeArray<ORCALine> lines, int numLines, float radius, float2 optimalVelocity, bool directionOpt) {
			float2 result;

			if (directionOpt) {
				// Optimize direction. Note that the optimization velocity is of unit length in this case
				result = optimalVelocity * radius;
			} else if (math.lengthsq(optimalVelocity) > radius*radius) {
				// Optimize closest point and outside circle
				result = math.normalize(optimalVelocity) * radius;
			} else {
				// Optimize closest point and inside circle
				result = optimalVelocity;
			}

			for (int i = 0; i < numLines; i++) {
				// Check if point is in the infeasible region of the half-plane
				if (det(lines[i].direction, lines[i].point - result) > 0.0f) {
					// Result does not satisfy constraint i. Compute new optimal result
					var tempResult = result;
					if (!LinearProgram1D(lines, i, radius, optimalVelocity, directionOpt, ref result)) {
						return new LinearProgram2Output {
								   velocity = tempResult,
								   firstFailedLineIndex = i,
						};
					}
				}
			}

			return new LinearProgram2Output {
					   velocity = result,
					   firstFailedLineIndex = numLines,
			};
		}

		static float ClosestPointOnSegment (float2 a, float2 dir, float2 p, float t0, float t1) {
			return math.clamp(math.dot(p - a, dir), t0, t1);
		}

		/// <summary>
		/// Closest point on segment a to segment b.
		/// The segments are given by infinite lines and bounded by t values. p = line.point + line.dir*t.
		///
		/// It is assumed that the two segments do not intersect.
		/// </summary>
		static float2 ClosestSegmentSegmentPointNonIntersecting (ORCALine a, ORCALine b, float ta1, float ta2, float tb1, float tb2) {
			// We know that the two segments do not intersect, so at least one of the closest points
			// must be one of the line segment endpoints.
			var ap0 = a.point + a.direction*ta1;
			var ap1 = a.point + a.direction*ta2;
			var bp0 = b.point + b.direction * tb1;
			var bp1 = b.point + b.direction * tb2;

			var t0 = ClosestPointOnSegment(a.point, a.direction, bp0, ta1, ta2);
			var t1 = ClosestPointOnSegment(a.point, a.direction, bp1, ta1, ta2);
			var t2 = ClosestPointOnSegment(b.point, b.direction, ap0, tb1, tb2);
			var t3 = ClosestPointOnSegment(b.point, b.direction, ap1, tb1, tb2);

			var c0 = a.point + a.direction * t0;
			var c1 = a.point + a.direction * t1;
			var c2 = b.point + b.direction * t2;
			var c3 = b.point + b.direction * t3;

			var d0 = math.lengthsq(c0 - bp0);
			var d1 = math.lengthsq(c1 - bp1);
			var d2 = math.lengthsq(c2 - ap0);
			var d3 = math.lengthsq(c3 - ap1);

			var result = c0;
			var d = d0;
			if (d1 < d) {
				result = c1;
				d = d1;
			}
			if (d2 < d) {
				result = ap0;
				d = d2;
			}
			if (d3 < d) {
				result = ap1;
				d = d3;
			}
			return result;
		}

		/// <summary>Like LinearProgram2D, but the optimal velocity space is a segment instead of a point, however the current result has collapsed to a point</summary>
		static LinearProgram2Output LinearProgram2DCollapsedSegment (NativeArray<ORCALine> lines, int numLines, int startLine, float radius, float2 currentResult, float2 optimalVelocityStart, float2 optimalVelocityDir, float optimalTLeft, float optimalTRight) {
			for (int i = startLine; i < numLines; i++) {
				// Check if point is in the infeasible region of the half-plane
				if (det(lines[i].direction, lines[i].point - currentResult) > 0.0f) {
					// Result does not satisfy constraint i. Compute new optimal result
					if (!ClipBoundary(lines, i, radius, out float tLeft2, out float tRight2)) {
						// We are partially not feasible, but no part of this constraint's boundary is in the feasible region.
						// This means that there is no feasible solution at all.
						return new LinearProgram2Output {
								   velocity = currentResult,
								   firstFailedLineIndex = i,
						};
					}

					// Optimize closest point
					currentResult = ClosestSegmentSegmentPointNonIntersecting(lines[i], new ORCALine {
						point = optimalVelocityStart,
						direction = optimalVelocityDir,
					}, tLeft2, tRight2, optimalTLeft, optimalTRight);
				}
			}

			return new LinearProgram2Output {
					   velocity = currentResult,
					   firstFailedLineIndex = numLines,
			};
		}

		/// <summary>Like LinearProgram2D, but the optimal velocity space is a segment instead of a point</summary>
		static LinearProgram2Output LinearProgram2DSegment (NativeArray<ORCALine> lines, int numLines, float radius, float2 optimalVelocityStart, float2 optimalVelocityDir, float optimalTLeft, float optimalTRight, float optimalT) {
			var hasIntersection = VectorMath.LineCircleIntersectionFactors(optimalVelocityStart, optimalVelocityDir, radius, out float resultTLeft, out float resultTRight);
			resultTLeft = math.max(resultTLeft, optimalTLeft);
			resultTRight = math.min(resultTRight, optimalTRight);
			hasIntersection &= resultTLeft <= resultTRight;

			if (!hasIntersection) {
				// In case the optimal velocity segment is not inside the max velocity circle, then collapse to a single optimal velocity which
				// is closest segment point to the circle
				var t = math.clamp(math.dot(-optimalVelocityStart, optimalVelocityDir), optimalTLeft, optimalTRight);
				var closestOnCircle = math.normalizesafe(optimalVelocityStart + optimalVelocityDir * t) * radius;

				// The best point is now a single point, not a segment.
				// So we can fall back to simpler code.
				return LinearProgram2DCollapsedSegment(lines, numLines, 0, radius, closestOnCircle, optimalVelocityStart, optimalVelocityDir, optimalTLeft, optimalTRight);
			}

			for (int i = 0; i < numLines; i++) {
				// Check if optimal line segment is at least partially in the infeasible region of the half-plane
				var line = lines[i];
				var leftInfeasible = det(line.direction, line.point - (optimalVelocityStart + optimalVelocityDir*resultTLeft)) > 0.0f;
				var rightInfeasible = det(line.direction, line.point - (optimalVelocityStart + optimalVelocityDir*resultTRight)) > 0.0f;
				if (leftInfeasible || rightInfeasible) {
					if (!ClipBoundary(lines, i, radius, out float tLeft, out float tRight)) {
						// We are partially not feasible, but no part of this constraint's boundary is in the feasible region.
						// This means that there is no feasible solution at all.
						return new LinearProgram2Output {
								   velocity = optimalVelocityStart + optimalVelocityDir * math.clamp(optimalT, resultTLeft, resultTRight),
								   firstFailedLineIndex = i,
						};
					}

					// Check if the optimal line segment is completely in the infeasible region
					if (leftInfeasible && rightInfeasible) {
						if (math.abs(det(line.direction, optimalVelocityDir)) < 0.001f) {
							// Lines are almost parallel.
							// Project the optimal velocity on the boundary
							var t1 = ClosestPointOnSegment(line.point, line.direction, optimalVelocityStart + optimalVelocityDir*resultTLeft, tLeft, tRight);
							var t2 = ClosestPointOnSegment(line.point, line.direction, optimalVelocityStart + optimalVelocityDir*resultTRight, tLeft, tRight);
							var t3 = ClosestPointOnSegment(line.point, line.direction, optimalVelocityStart + optimalVelocityDir*optimalT, tLeft, tRight);
							optimalVelocityStart = line.point;
							optimalVelocityDir = line.direction;
							resultTLeft = t1;
							resultTRight = t2;
							optimalT = t3;
						} else {
							// Find closest point on the constraint boundary segment to the optimal velocity segment
							var result = ClosestSegmentSegmentPointNonIntersecting(line, new ORCALine {
								point = optimalVelocityStart,
								direction = optimalVelocityDir,
							}, tLeft, tRight, optimalTLeft, optimalTRight);

							// The best point is now a single point, not a segment.
							// So we can fall back to simpler code.
							return LinearProgram2DCollapsedSegment(lines, numLines, i+1, radius, result, optimalVelocityStart, optimalVelocityDir, optimalTLeft, optimalTRight);
						}
					} else {
						// Clip optimal velocity segment to the constraint boundary.
						// If this returns false and the lines are almost parallel, then we don't do anything
						// because we already know they intersect. So the two lines must be almost identical.
						ClipLine(new ORCALine {
							point = optimalVelocityStart,
							direction = optimalVelocityDir,
						}, line, ref resultTLeft, ref resultTRight);
					}
				}
			}

			var resultT = math.clamp(optimalT, resultTLeft, resultTRight);

			return new LinearProgram2Output {
					   velocity = optimalVelocityStart + optimalVelocityDir * resultT,
					   firstFailedLineIndex = numLines,
			};
		}

		/// <summary>
		/// Finds the velocity with the smallest maximum penetration into the given half-planes.
		///
		/// Assumes there are no points in the feasible region of the given half-planes.
		///
		/// Runs a 3-dimensional linear program, but projected down to 2D.
		/// If there are no feasible regions outside all half-planes then we want to find the velocity
		/// for which the maximum penetration into infeasible regions is minimized.
		/// Conceptually we can solve this by taking our half-planes, and moving them outwards at a fixed speed
		/// until there is exactly 1 feasible point.
		/// We can formulate this in 3D space by thinking of the half-planes in 3D (velocity.x, velocity.y, penetration-depth) space, as sloped planes.
		/// Moving the planes outwards then corresponds to decreasing the z coordinate.
		/// In 3D space we want to find the point above all planes with the lowest z coordinate.
		/// We do this by going through each plane and testing if it is possible that this plane
		/// is the one with the maximum penetration.
		/// If so, we know that the point will lie on the portion of that plane bounded by the intersections
		/// with the other planes. We generate projected half-planes which represent the intersections with the
		/// other 3D planes, and then we run a new optimization to find the point which penetrates this
		/// half-plane the least.
		/// </summary>
		/// <param name="lines">The half-planes of all obstacles and agents.</param>
		/// <param name="numLines">The number of half-planes in lines.</param>
		/// <param name="numFixedLines">The number of half-planes in lines which are fixed (0..numFixedLines). These will be treated as static obstacles which should be avoided at all costs.</param>
		/// <param name="beginLine">The index of the first half-plane in lines for which the previous optimization failed (see \reflink{LinearProgram2Output.firstFailedLineIndex}).</param>
		/// <param name="radius">Maximum possible speed. This represents a circular velocity obstacle.</param>
		/// <param name="result">Input is best velocity as output by \reflink{LinearProgram2D}. Output is the new best velocity. The velocity with the smallest maximum penetration into the given half-planes.</param>
		/// <param name="scratchBuffer">A buffer of length at least numLines to use for scratch space.</param>
		static void LinearProgram3D (NativeArray<ORCALine> lines, int numLines, int numFixedLines, int beginLine, float radius, ref float2 result, NativeArray<ORCALine> scratchBuffer) {
			float distance = 0.0f;

			NativeArray<ORCALine> projectedLines = scratchBuffer;
			NativeArray<ORCALine>.Copy(lines, projectedLines, numFixedLines);

			for (int i = beginLine; i < numLines; i++) {
				// Check if #result is more than #distance units inside the infeasible region of the half-plane
				if (det(lines[i].direction, lines[i].point - result) > distance) {
					int numProjectedLines = numFixedLines;
					for (int j = numFixedLines; j < i; j++) {
						float determinant = det(lines[i].direction, lines[j].direction);
						if (math.abs(determinant) < 0.001f) {
							// Lines i and j are parallel
							if (math.dot(lines[i].direction, lines[j].direction) > 0.0f) {
								// Line i and j point in the same direction
								continue;
							} else {
								// Line i and j point in the opposite direction
								projectedLines[numProjectedLines] = new ORCALine {
									point = 0.5f * (lines[i].point + lines[j].point),
									direction = math.normalize(lines[j].direction - lines[i].direction),
								};
								numProjectedLines++;
							}
						} else {
							projectedLines[numProjectedLines] = new ORCALine {
								// The intersection between the two lines
								point = lines[i].point + (det(lines[j].direction, lines[i].point - lines[j].point) / determinant) * lines[i].direction,
								// The direction along which the intersection of the two 3D-planes intersect (projected onto the XY plane)
								direction = math.normalize(lines[j].direction - lines[i].direction),
							};
							numProjectedLines++;
						}
					}

					var lin = LinearProgram2D(projectedLines, numProjectedLines, radius, new float2(-lines[i].direction.y, lines[i].direction.x), true);
					if (lin.firstFailedLineIndex < numProjectedLines) {
						// This should in principle not happen.  The result is by definition
						// already in the feasible region of this linear program. If it fails,
						// it is due to small floating point error, and the current result is
						// kept.
					} else {
						result = lin.velocity;
					}

					distance = det(lines[i].direction, lines[i].point - result);
				}
			}
		}

		static void DrawVO (CommandBuilder draw, float2 circleCenter, float radius, float2 origin, Color color) {
#if UNITY_EDITOR
			draw.PushColor(color);
			float alpha = math.atan2((origin - circleCenter).y, (origin - circleCenter).x);
			float gamma = radius/math.length(origin-circleCenter);
			float delta = gamma <= 1.0f ? math.abs(math.acos(gamma)) : 0;

			draw.xy.Circle(circleCenter, radius, alpha-delta, alpha+delta);
			float2 p1 = new float2(math.cos(alpha-delta), math.sin(alpha-delta)) * radius;
			float2 p2 = new float2(math.cos(alpha+delta), math.sin(alpha+delta)) * radius;

			float2 p1t = -new float2(-p1.y, p1.x);
			float2 p2t = new float2(-p2.y, p2.x);
			p1 += circleCenter;
			p2 += circleCenter;

			draw.xy.Ray(p1, math.normalizesafe(p1t)*100);
			draw.xy.Ray(p2, math.normalizesafe(p2t)*100);
			draw.PopColor();
#endif
		}
	}
}
