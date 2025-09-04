#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Collections;
using Unity.Core;
using Unity.Jobs;

namespace Pathfinding.ECS {
	[UpdateAfter(typeof(TransformSystemGroup))]
	public partial class AIMovementSystemGroup : ComponentSystemGroup {
		/// <summary>Rate manager which runs a system group multiple times if the delta time is higher than desired, but always executes the group at least once per frame</summary>
		public class TimeScaledRateManager : IRateManager, System.IDisposable {
			int numUpdatesThisFrame;
			int updateIndex;
			float stepDt;
			float maximumDt = 1.0f / 30.0f;
			float ownProcessingTimePerIteration = 0;
			NativeList<TimeData> cheapTimeDataQueue;
			NativeList<TimeData> timeDataQueue;
			double lastFullSimulation;
			double lastCheapSimulation;
			static bool cheapSimulationOnly;
			static bool isLastSubstep, isFirstSubstep;
			static bool inGroup;
			static TimeData cheapTimeData;

			/// <summary>
			/// True if it was determined that zero substeps should be simulated.
			/// In this case all systems will get an opportunity to run a single update,
			/// but they should avoid systems that don't have to run every single frame.
			/// </summary>
			public static bool CheapSimulationOnly {
				get {
					if (!inGroup) throw new System.InvalidOperationException("Cannot call this method outside of a simulation group using TimeScaledRateManager");
					return cheapSimulationOnly;
				}
			}

			public static float CheapStepDeltaTime {
				get {
					if (!inGroup) throw new System.InvalidOperationException("Cannot call this method outside of a simulation group using TimeScaledRateManager");
					return cheapTimeData.DeltaTime;
				}
			}

			/// <summary>True when this is the last substep of the current simulation</summary>
			public static bool IsLastSubstep {
				get {
					if (!inGroup) throw new System.InvalidOperationException("Cannot call this method outside of a simulation group using TimeScaledRateManager");
					return isLastSubstep;
				}
			}

			/// <summary>True when this is the first substep of the current simulation</summary>
			public static bool IsFirstSubstep {
				get {
					if (!inGroup) throw new System.InvalidOperationException("Cannot call this method outside of a simulation group using TimeScaledRateManager");
					return isFirstSubstep;
				}
			}

			public int NumUpdatesThisFrame => numUpdatesThisFrame;

			public TimeScaledRateManager () {
				cheapTimeDataQueue = new NativeList<TimeData>(Allocator.Persistent);
				timeDataQueue = new NativeList<TimeData>(Allocator.Persistent);
			}

			public void Dispose () {
				cheapTimeDataQueue.Dispose();
				timeDataQueue.Dispose();
			}

			public void OnSimulationStepsFinished (float totalSimulationProcessingTime) {
				if (cheapSimulationOnly) return;

				ownProcessingTimePerIteration = totalSimulationProcessingTime / numUpdatesThisFrame;
			}

			public bool ShouldGroupUpdate (ComponentSystemGroup group) {
				// if this is true, means we're being called a second or later time in a loop.
				if (inGroup) {
					group.World.PopTime();
					updateIndex++;
					if (updateIndex >= numUpdatesThisFrame) {
						inGroup = false;
						return false;
					}
				} else {
					cheapTimeDataQueue.Clear();
					timeDataQueue.Clear();

					if (inGroup) throw new System.InvalidOperationException("Cannot nest simulation groups using TimeScaledRateManager");
					var fullDt = (float)(group.World.Time.ElapsedTime - lastFullSimulation);

					// It has been observed that the time move backwards.
					// Not quite sure when it happens, but we need to guard against it.
					if (fullDt < 0) fullDt = 0;

					// If the delta time is large enough we may want to perform multiple simulation sub-steps per frame.
					// This is done to improve simulation stability. In particular at high time scales, but it also
					// helps at low fps, or if the game has a sudden long stutter.
					// We raise the value to a power slightly smaller than 1 to make the number of sub-steps increase
					// more slowly as the delta time increases. This is important to avoid the edge case when
					// the time it takes to run the simulation is longer than maximumDt. Otherwise the number of
					// simulation sub-steps would increase without bound. However, the simulation quality
					// may decrease a bit as the number of sub-steps increases.
					//
					// If the time it takes to run a single iteration grows too large,
					// the number of simulation steps will also be reduced. In the limit where the simulation
					// takes up essentially the whole frame's cpu time, then we only do 1 simulation step per frame.
					numUpdatesThisFrame = Mathf.FloorToInt(Mathf.Pow(fullDt / (maximumDt + ownProcessingTimePerIteration), 0.8f));
					var currentTime = group.World.Time.ElapsedTime;
					cheapSimulationOnly = numUpdatesThisFrame == 0;
					if (cheapSimulationOnly) {
						timeDataQueue.Add(new TimeData(
							lastFullSimulation,
							0.0f
							));
						cheapTimeDataQueue.Add(new TimeData(
							currentTime,
							(float)(currentTime - lastCheapSimulation)
							));
						lastCheapSimulation = currentTime;
					} else {
						stepDt = fullDt / numUpdatesThisFrame;
						// Push the time for each sub-step
						for (int i = 0; i < numUpdatesThisFrame; i++) {
							var stepTime = lastFullSimulation + (i+1) * stepDt;
							timeDataQueue.Add(new TimeData(
								stepTime,
								stepDt
								));
							cheapTimeDataQueue.Add(new TimeData(
								stepTime,
								(float)(stepTime - lastCheapSimulation)
								));
							lastCheapSimulation = stepTime;
						}
						lastFullSimulation = currentTime;
					}
					numUpdatesThisFrame = Mathf.Max(1, numUpdatesThisFrame);
					inGroup = true;
					updateIndex = 0;
				}

				group.World.PushTime(timeDataQueue[updateIndex]);
				cheapTimeData = cheapTimeDataQueue[updateIndex];
				isLastSubstep = updateIndex + 1 >= numUpdatesThisFrame;
				isFirstSubstep = updateIndex == 0;

				return true;
			}

			public float Timestep {
				get => maximumDt;
				set => maximumDt = value;
			}
		}

		protected override void OnUpdate () {
			// Various jobs (e.g. the JobRepairPath) in this system group may use graph data,
			// and they also need the graph data to be consistent during the whole update.
			// For example the MovementState.hierarchicalNodeIndex field needs to be valid
			// during the whole group update, as it may be used by the RVOSystem and FollowerControlSystem.
			// Locking the graph data as read-only here means that no graph updates will be performed
			// while these jobs are running.
			var readLock = AstarPath.active != null? AstarPath.active.LockGraphDataForReading() : default;

			// And here I thought the entities package reaching 1.0 would mean that they wouldn't just rename
			// properties without any compatibility code... but nope...
#if MODULE_ENTITIES_1_0_8_OR_NEWER
			var systems = this.GetUnmanagedSystems();
			for (int i = 0; i < systems.Length; i++) {
				ref var state = ref this.World.Unmanaged.ResolveSystemStateRef(systems[i]);
				state.Dependency = JobHandle.CombineDependencies(state.Dependency, readLock.dependency);
			}
#else
			var systems = this.Systems;
			for (int i = 0; i < systems.Count; i++) {
				ref var state = ref this.World.Unmanaged.ResolveSystemStateRef(systems[i].SystemHandle);
				state.Dependency = JobHandle.CombineDependencies(state.Dependency, readLock.dependency);
			}
#endif

			var t1 = System.Diagnostics.Stopwatch.GetTimestamp();
			base.OnUpdate();
			var t2 = System.Diagnostics.Stopwatch.GetTimestamp();
			var rateManager = RateManager as TimeScaledRateManager;
			var timePerSimulationStep = (t2 - t1) / (rateManager.NumUpdatesThisFrame * (double)System.Diagnostics.Stopwatch.Frequency);
			rateManager.OnSimulationStepsFinished((float)((t2 - t1) / (double)System.Diagnostics.Stopwatch.Frequency));

			JobHandle readDependency = default;
#if MODULE_ENTITIES_1_0_8_OR_NEWER
			for (int i = 0; i < systems.Length; i++) {
				ref var state = ref this.World.Unmanaged.ResolveSystemStateRef(systems[i]);
				readDependency = JobHandle.CombineDependencies(readDependency, state.Dependency);
			}
			systems.Dispose();
#else
			for (int i = 0; i < systems.Count; i++) {
				ref var state = ref this.World.Unmanaged.ResolveSystemStateRef(systems[i].SystemHandle);
				readDependency = JobHandle.CombineDependencies(readDependency, state.Dependency);
			}
#endif
			readLock.UnlockAfter(readDependency);
		}

		protected override void OnDestroy () {
			base.OnDestroy();
			(this.RateManager as TimeScaledRateManager).Dispose();
		}

		protected override void OnCreate () {
			base.OnCreate();
			this.RateManager = new TimeScaledRateManager();
		}
	}
}
#endif
