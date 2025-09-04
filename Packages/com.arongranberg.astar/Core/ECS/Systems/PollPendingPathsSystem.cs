#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Burst;
using GCHandle = System.Runtime.InteropServices.GCHandle;
using Unity.Transforms;

namespace Pathfinding.ECS {
	/// <summary>
	/// Checks if paths have been calculated, and updates the agent's paths if they have.
	///
	/// This is essentially a replacement for <see cref="Path.callback"/> for ECS agents.
	///
	/// This system is a bit different in that it doesn't run in the normal update loop,
	/// but instead it will run when the <see cref="AstarPath.OnPathsCalculated"/> event fires.
	/// This is to avoid having to call a separate callback for every agent, since that
	/// would result in excessive overhead as it would have to synchronize with the ECS world
	/// on every such call.
	///
	/// See: <see cref="AstarPath.OnPathsCalculated"/>
	/// </summary>
	[BurstCompile]
	public partial struct PollPendingPathsSystem : ISystem {
		GCHandle onPathsCalculated;
		static bool anyPendingPaths;

		JobRepairPath.Scheduler jobRepairPathScheduler;
		EntityQuery entityQueryPrepare;

		public void OnCreate (ref SystemState state) {
			jobRepairPathScheduler = new JobRepairPath.Scheduler(ref state) {
				onlyApplyPendingPaths = true,
			};
			entityQueryPrepare = jobRepairPathScheduler.GetEntityQuery(Unity.Collections.Allocator.Temp).Build(ref state);

			var world = state.WorldUnmanaged;
			System.Action onPathsCalculated = () => {
				// Allow the system to run
				anyPendingPaths = true;
				try {
					// Update the system manually
					world.GetExistingUnmanagedSystem<PollPendingPathsSystem>().Update(world);
				} finally {
					anyPendingPaths = false;
				}
			};
			AstarPath.OnPathsCalculated += onPathsCalculated;
			// Store the callback in a GCHandle to get around limitations on unmanaged systems.
			this.onPathsCalculated = GCHandle.Alloc(onPathsCalculated);
		}

		public void OnDestroy (ref SystemState state) {
			AstarPath.OnPathsCalculated -= (System.Action)onPathsCalculated.Target;
			onPathsCalculated.Free();
			jobRepairPathScheduler.Dispose();
		}

		void OnUpdate (ref SystemState systemState) {
			// Only run the system when we have triggered it manually
			if (!anyPendingPaths) return;

			// During an off-mesh link traversal, we shouldn't calculate any paths, because it's somewhat undefined where they should start.
			// Paths are already cancelled when the off-mesh link traversal starts, but just in case it has been started by a user manually in some way, we also cancel them every frame.
			foreach (var state in SystemAPI.Query<ManagedState>().WithAll<AgentOffMeshLinkTraversal>()) state.CancelCurrentPathRequest();

			// The JobRepairPath may access graph data, so we need to lock it for reading.
			// Otherwise a graph update could start while the job was running, which could cause all kinds of problems.
			var readLock = AstarPath.active.LockGraphDataForReading();

			// Iterate over all agents and check if they have any pending paths, and if they have been calculated.
			// If they have, we update the agent's current path to the newly calculated one.
			//
			// We do this by running the JobRepairPath for all agents that have just had their path calculated.
			// This ensures that all properties like remainingDistance are up to date immediately after
			// a path recalculation.
			// This may seem wasteful, but during the next update, the regular JobRepairPath job
			// will most likely be able to early out, because we did most of the work here.
			systemState.Dependency = jobRepairPathScheduler.ScheduleParallel(ref systemState, entityQueryPrepare, systemState.Dependency);

			readLock.UnlockAfter(systemState.Dependency);
		}
	}
}
#endif
