#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding.ECS {
	/// <summary>
	/// Policy for how often to recalculate an agent's path.
	///
	/// See: <see cref="FollowerEntity.autoRepath"/>
	///
	/// This is the unmanaged equivalent of <see cref="Pathfinding.AutoRepathPolicy"/>.
	/// </summary>
	[System.Serializable]
	public struct AutoRepathPolicy : IComponentData {
		/// <summary>
		/// How sensitive the agent should be to changes in its destination for Mode.Dynamic.
		/// A higher value means the destination has to move less for the path to be recalculated.
		///
		/// See: <see cref="AutoRepathPolicy.Mode"/>
		/// </summary>
		public const float Sensitivity = 10.0f;

		/// <summary>
		/// Policy to use when recalculating paths.
		///
		/// See: <see cref="Pathfinding.AutoRepathPolicy.Mode"/> for more details.
		/// </summary>
		public Pathfinding.AutoRepathPolicy.Mode mode;
		byte pathFailures;

		/// <summary>Number of seconds between each automatic path recalculation for Mode.EveryNSeconds, and the maximum interval for Mode.Dynamic</summary>
		public float period;

		float3 lastDestination;
		float lastRepathTime;

		public static AutoRepathPolicy Default => new AutoRepathPolicy {
			mode = Pathfinding.AutoRepathPolicy.Mode.Dynamic,
			period = 2,
			lastDestination = float.PositiveInfinity,
			lastRepathTime = float.NegativeInfinity
		};

		public AutoRepathPolicy (Pathfinding.AutoRepathPolicy policy) {
			mode = policy.mode;
			period = policy.mode == Pathfinding.AutoRepathPolicy.Mode.Dynamic ? policy.maximumPeriod : policy.period;
			lastDestination = float.PositiveInfinity;
			lastRepathTime = float.NegativeInfinity;
			pathFailures = 0;
		}

		/// <summary>
		/// True if the path should be recalculated according to the policy
		///
		/// The above parameters are relevant only if <see cref="mode"/> is <see cref="Mode.Dynamic"/>.
		/// </summary>
		/// <param name="position">The current position of the agent.</param>
		/// <param name="radius">The radius of the agent. You may pass 0.0 if the agent doesn't have a radius.</param>
		/// <param name="destination">The goal of the agent right now</param>
		/// <param name="time">The current time in seconds</param>
		/// <param name="isPathStale">You may pass true if the agent knows that the current path is outdated for some reason (for example if some nodes in it have been destroyed).</param>
		public bool ShouldRecalculatePath (float3 position, float radius, float3 destination, float time, bool isPathStale) {
			if (mode == Pathfinding.AutoRepathPolicy.Mode.Never || !float.IsFinite(destination.x)) return false;

			float timeSinceLast = time - lastRepathTime;

			var tmpPeriod = period;
			if (isPathStale) {
				// If the path is stale, we recalculate the path more often.
				// But if the path just continues to fail, then we back off exponentially up to the maximum period.
				// 0 failures  => 0
				// 1 failure   => period/4
				// 2 failures  => period/2
				// 3+ failures => period
				if (pathFailures == 0) return true;
				tmpPeriod = period * math.min(1, 0.125f * (1 << pathFailures));
			}

			if (mode == Pathfinding.AutoRepathPolicy.Mode.EveryNSeconds) {
				return timeSinceLast >= tmpPeriod;
			} else {
				// cost = change in destination / max(distance to destination, radius)
				float squaredCost = math.lengthsq(destination - lastDestination) / math.max(math.lengthsq(position - lastDestination), radius*radius);
				float fraction = squaredCost * (Sensitivity*Sensitivity);
				if (float.IsNaN(fraction)) {
					// The agent's radius is zero, and the destination is precisely at the agent's position, which is also the destination of the last calculated path
					// This is a special case. It happens sometimes for the AILerp component when it reaches its
					// destination, as the AILerp component has no radius.
					// In this case we just use the maximum period.
					fraction = 0;
				}

				return timeSinceLast >= tmpPeriod*(1 - math.sqrt(fraction));
			}
		}

		public void OnPathCalculated (bool hadError) {
			if (hadError) {
				pathFailures = (byte)math.min(255, pathFailures++);
			} else {
				pathFailures = 0;
			}
		}

		public void Reset () {
			lastDestination = float.PositiveInfinity;
			lastRepathTime = float.NegativeInfinity;
		}

		/// <summary>Must be called when a path request has been scheduled</summary>
		public void OnScheduledPathRecalculation (float3 destination, float time) {
			lastRepathTime = time;
			lastDestination = destination;
			// Randomize the repath time slightly so that all agents don't request a path at the same time
			// in the future. This is useful when there are a lot of agents instantiated at exactly the same time.
			const float JITTER_AMOUNT = 0.3f;
			lastRepathTime -= (UnityEngine.Random.value - 0.5f) * JITTER_AMOUNT * period;
		}
	}
}
#endif
