using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

namespace Pathfinding.RVO {
	/// <summary>
	/// Controls if the agent slows down to a stop if the area around the destination is crowded.
	/// The main idea for this script is to
	/// - Reduce the local avoidance priority for agents that have reached their destination once.
	/// - Make agents stop if there is a high density of units around its destination.
	///
	/// 'High density' is defined as:
	/// Take the circle with the center at the AI's destination and a radius such that the AI's current position
	/// is touching its border. Let 'A' be the area of that circle. Further let 'a' be the total area of all
	/// individual agents inside that circle.
	/// The agent should stop if a > A*0.6 or something like that. I.e if the agents inside the circle cover
	/// over 60% of the surface of the circle. The 60% figure can be modified (see <see cref="densityThreshold)"/>.
	///
	/// This script was inspired by how Starcraft 2 does its local avoidance.
	///
	/// See: <see cref="Pathfinding.AIBase.rvoDensityBehavior"/>
	/// </summary>
	[System.Serializable]
	public struct RVODestinationCrowdedBehavior {
		/// <summary>Enables or disables this module</summary>
		public bool enabled;

		/// <summary>
		/// The threshold for when to stop.
		/// See the class description for more info.
		/// </summary>
		[Range(0, 1)]
		public float densityThreshold;

		/// <summary>
		/// If true, the agent will start to move to the destination again if it determines that it is now less crowded.
		/// If false and the destination becomes less crowded (or if the agent is pushed away from the destination in some way), then the agent will still stay put.
		/// </summary>
		public bool returnAfterBeingPushedAway;

		public float progressAverage;
		bool wasEnabled;
		float timer1;
		float shouldStopDelayTimer;
		bool lastShouldStopResult;
		Vector3 lastShouldStopDestination;
		Vector3 reachedDestinationPoint;
		public bool lastJobDensityResult;

		/// <summary>See https://en.wikipedia.org/wiki/Circle_packing</summary>
		const float MaximumCirclePackingDensity = 0.9069f;

		[BurstCompile(CompileSynchronously = false, FloatMode = FloatMode.Fast)]
		public struct JobDensityCheck : Pathfinding.Jobs.IJobParallelForBatched {
			[ReadOnly]
			RVOQuadtreeBurst quadtree;
			[ReadOnly]
			public NativeArray<QueryData> data;
			[ReadOnly]
			public NativeArray<float3> agentPosition;
			[ReadOnly]
			NativeArray<float3> agentTargetPoint;
			[ReadOnly]
			NativeArray<float> agentRadius;
			[ReadOnly]
			NativeArray<float> agentDesiredSpeed;
			[ReadOnly]
			NativeArray<float3> agentOutputTargetPoint;
			[ReadOnly]
			NativeArray<float> agentOutputSpeed;
			[WriteOnly]
			public NativeArray<bool> outThresholdResult;
			public NativeArray<float> progressAverage;

			public float deltaTime;

			public bool allowBoundsChecks => false;

			public struct QueryData {
				public float3 agentDestination;
				public int agentIndex;
				public float densityThreshold;
			}

			public JobDensityCheck(int size, float deltaTime, SimulatorBurst simulator) {
				agentPosition = simulator.simulationData.position;
				agentTargetPoint = simulator.simulationData.targetPoint;
				agentRadius = simulator.simulationData.radius;
				agentDesiredSpeed = simulator.simulationData.desiredSpeed;
				agentOutputTargetPoint = simulator.outputData.targetPoint;
				agentOutputSpeed = simulator.outputData.speed;
				quadtree = simulator.quadtree;
				data = new NativeArray<QueryData>(size, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				outThresholdResult = new NativeArray<bool>(size, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				progressAverage = new NativeArray<float>(size, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				this.deltaTime = deltaTime;
			}

			public void Dispose () {
				data.Dispose();
				outThresholdResult.Dispose();
				progressAverage.Dispose();
			}

			public void Set (int index, int rvoAgentIndex, float3 destination, float densityThreshold, float progressAverage) {
				data[index] = new QueryData {
					agentDestination = destination,
					densityThreshold = densityThreshold,
					agentIndex = rvoAgentIndex,
				};
				this.progressAverage[index] = progressAverage;
			}

			void Pathfinding.Jobs.IJobParallelForBatched.Execute (int start, int count) {
				for (int i = start; i < start + count; i++) {
					Execute(i);
				}
			}

			float AgentDensityInCircle (float3 position, float radius) {
				return quadtree.QueryArea(position, radius) / (radius * radius * math.PI);
			}

			void Execute (int i) {
				var query = data[i];

				var position = agentPosition[query.agentIndex];
				var radius = agentRadius[query.agentIndex];

				var desiredDirection = math.normalizesafe(agentTargetPoint[query.agentIndex] - position);
				float delta;

				if (agentDesiredSpeed[query.agentIndex] > 0.01f) {
					// How quickly the agent can move
					var speedTowardsTarget = math.dot(desiredDirection, math.normalizesafe(agentOutputTargetPoint[query.agentIndex] - position) * agentOutputSpeed[query.agentIndex]);
					// Make it relative to how quickly it wants to move
					// So 0.0 means it is stuck
					// 1.0 means it is moving as quickly as it wants
					// Cap the desired speed by the agent's radius. This avoids making agents that want to move very quickly
					// but are slowed down to a still reasonable speed get a very low progressAverage.
					delta = speedTowardsTarget / math.max(0.001f, math.min(agentDesiredSpeed[query.agentIndex], agentRadius[query.agentIndex]));
					// Clamp between -1 and 1
					delta = math.clamp(delta, -1.0f, 1.0f);
				} else {
					// If the agent doesn't want to move anywhere then it has 100% progress
					delta = 1.0f;
				}

				// Exponentially decaying average of the deltas (in seconds^-1)
				const float FilterConvergenceSpeed = 2.0f;
				progressAverage[i] = math.lerp(progressAverage[i], delta, FilterConvergenceSpeed * deltaTime);

				// If no destination has been set, then always stop
				if (math.any(math.isinf(query.agentDestination))) {
					outThresholdResult[i] = true;
					return;
				}

				var checkRadius = math.length(query.agentDestination - position);
				var checkRadius2 = radius*5;

				if (checkRadius > checkRadius2) {
					// TODO: Change center to be slightly biased towards agent position
					// If the agent is far away from the destination then do a faster check around the destination first.
					if (AgentDensityInCircle(query.agentDestination, checkRadius2) < MaximumCirclePackingDensity*query.densityThreshold) {
						outThresholdResult[i] = false;
						return;
					}
				}

				outThresholdResult[i] = AgentDensityInCircle(query.agentDestination, checkRadius) > MaximumCirclePackingDensity*query.densityThreshold;
			}
		}

		public void ReadJobResult (ref JobDensityCheck jobResult, int index) {
			bool shouldStop = jobResult.outThresholdResult[index];

			progressAverage = jobResult.progressAverage[index];

			lastJobDensityResult = shouldStop;
			shouldStopDelayTimer = Mathf.Lerp(shouldStopDelayTimer, shouldStop ? 1 : 0, Time.deltaTime);
			shouldStop = shouldStop && shouldStopDelayTimer > 0.1f;
			lastShouldStopResult = shouldStop;
			lastShouldStopDestination = jobResult.data[index].agentDestination;
		}

		public RVODestinationCrowdedBehavior (bool enabled, float densityFraction, bool returnAfterBeingPushedAway) {
			this.enabled = wasEnabled = enabled;
			this.densityThreshold = densityFraction;
			this.returnAfterBeingPushedAway = returnAfterBeingPushedAway;
			this.lastJobDensityResult = false;
			this.progressAverage = 0;
			this.wasStopped = false;
			this.lastShouldStopDestination = new Vector3(float.NaN, float.NaN, float.NaN);
			this.reachedDestinationPoint = new Vector3(float.NaN, float.NaN, float.NaN);
			timer1 = 0;
			shouldStopDelayTimer = 0;
			reachedDestination = false;
			lastShouldStopResult = false;
		}

		/// <summary>
		/// Marks the destination as no longer being reached.
		///
		/// If the agent had stopped because the destination was crowded, this will make it immediately try again
		/// to move forwards	if it can. If the destination is still crowded it will soon stop again.
		///
		/// This is useful to call when a user gave an agent an explicit order to ensure it doesn't
		/// just stay in the same location without even trying to move forwards.
		/// </summary>
		public void ClearDestinationReached () {
			wasStopped = false;
			progressAverage = 1.0f;
			reachedDestination = false;
		}

		public void OnDestinationChanged (Vector3 newDestination, bool reachedDestination) {
			timer1 = float.PositiveInfinity;
			// TODO: Check previous ShouldStop result. Check how much the circles overlap.
			// With significant overlap we may want to keep reachedCurrentDestination = true
			this.reachedDestination = reachedDestination; // (ideal: || ShouldStop(ai, rvo))
		}

		/// <summary>
		/// True if the agent has reached its destination.
		/// If the agents destination changes this may return false until the next frame.
		/// Note that changing the destination every frame may cause this value to never return true.
		///
		/// True will be returned if the agent has stopped due to being close enough to the destination.
		/// This may be quite some distance away if there are many other agents around the destination.
		///
		/// See: <see cref="Pathfinding.IAstarAI.destination"/>
		/// </summary>
		public bool reachedDestination { get; private set; }

		bool wasStopped;

		const float DefaultPriority = 1.0f;
		const float StoppedPriority = 0.1f;
		const float MoveBackPriority = 0.5f;

		public void Update (bool rvoControllerEnabled, bool reachedDestination, ref bool isStopped, ref float rvoPriorityMultiplier, ref float rvoFlowFollowingStrength, Vector3 agentPosition) {
			if (!(enabled && rvoControllerEnabled)) {
				if (wasEnabled) {
					wasEnabled = false;
					// Reset to default values
					rvoPriorityMultiplier = DefaultPriority;
					rvoFlowFollowingStrength = 0;
					timer1 = float.PositiveInfinity;
					progressAverage = 1.0f;
				}
				return;
			}
			wasEnabled = true;

			if (reachedDestination) {
				var validRange = (agentPosition - this.reachedDestinationPoint).sqrMagnitude;
				if ((lastShouldStopDestination - this.reachedDestinationPoint).sqrMagnitude > validRange) {
					// The reachedDestination bool is no longer valid.
					// The destination has moved significantly from the last point where we detected that it was crowded.
					// It may end up being set to true immediately afterwards though if
					// the parameter reachedDestination (not this.reachedDestination) is true.
					this.reachedDestination = false;
				}
			}

			if (reachedDestination || lastShouldStopResult) {
				// We have reached the destination the destination is crowded enough that we should stop here anyway
				timer1 = 0f;
				this.reachedDestination = true;
				this.reachedDestinationPoint = this.lastShouldStopDestination;
				rvoPriorityMultiplier = Mathf.Lerp(rvoPriorityMultiplier, StoppedPriority, Time.deltaTime * 2);
				rvoFlowFollowingStrength = Mathf.Lerp(rvoFlowFollowingStrength, 1.0f, Time.deltaTime * 4);
				wasStopped |= math.abs(progressAverage) < 0.1f;
				isStopped |= wasStopped; // false && rvoPriorityMultiplier > 0.9f;
			} else if (isStopped) {
				// We have not reached the destination, but a separate script is telling is to stop
				timer1 = 0f;
				this.reachedDestination = false;
				rvoPriorityMultiplier = Mathf.Lerp(rvoPriorityMultiplier, StoppedPriority, Time.deltaTime * 2);
				rvoFlowFollowingStrength = Mathf.Lerp(rvoFlowFollowingStrength, 1.0f, Time.deltaTime * 4);
				wasStopped |= math.abs(progressAverage) < 0.1f;
			} else {
				// Check if we had reached the current destination previously (but it is not reached any longer)
				// TODO: Rename variable, confusing
				if (this.reachedDestination) {
					timer1 += Time.deltaTime;
					if (timer1 > 3 && returnAfterBeingPushedAway) {
						// Make the agent try to move back to the destination
						// Use a slightly higher priority than agents that are just standing still, but lower than regular agents
						rvoPriorityMultiplier = Mathf.Lerp(rvoPriorityMultiplier, MoveBackPriority, Time.deltaTime * 2);
						rvoFlowFollowingStrength = 0;
						isStopped = false;
						wasStopped = false;
					} else {
						rvoPriorityMultiplier = Mathf.Lerp(rvoPriorityMultiplier, StoppedPriority, Time.deltaTime * 2);
						rvoFlowFollowingStrength = Mathf.Lerp(rvoFlowFollowingStrength, 1.0f, Time.deltaTime * 4);
						wasStopped |= math.abs(progressAverage) < 0.1f;
						isStopped = wasStopped;
						//isStopped = false && rvoPriorityMultiplier > 0.9f;
					}
				} else {
					// This is the common case: the agent is just on its way to the destination
					rvoPriorityMultiplier = Mathf.Lerp(rvoPriorityMultiplier, DefaultPriority, Time.deltaTime * 4);
					rvoFlowFollowingStrength = 0f;
					isStopped = false;
					wasStopped = false;
				}
			}
		}
	}
}
