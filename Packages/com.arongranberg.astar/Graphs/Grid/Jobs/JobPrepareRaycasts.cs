using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Pathfinding.Collections;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Prepares a set of raycast commands for a grid graph.
	///
	/// This is very similar to <see cref="JobPrepareGridRaycast"/> but it uses an array of origin points instead of a grid pattern.
	///
	/// See: <see cref="GraphCollision"/>
	/// </summary>
	[BurstCompile]
	public struct JobPrepareRaycasts : IJob {
		public Vector3 direction;
		public Vector3 originOffset;
		public float distance;
		public LayerMask mask;
		public PhysicsScene physicsScene;

		[ReadOnly]
		public NativeArray<Vector3> origins;

		[WriteOnly]
		public NativeArray<RaycastCommand> raycastCommands;

		public void Execute () {
			// In particular Unity 2022.2 seems to assert that RaycastCommands use normalized directions
			var direction = this.direction.normalized;
			var commands = raycastCommands.AsUnsafeSpan();

#if UNITY_2022_2_OR_NEWER
			var queryParameters = new QueryParameters(mask, false, QueryTriggerInteraction.Ignore, false);
			var defaultCommand = new RaycastCommand(physicsScene, Vector3.zero, direction, queryParameters, distance);
			// This is about 30% faster than setting each command individually. MemCpy is fast.
			commands.Fill(defaultCommand);
#endif

			for (int i = 0; i < raycastCommands.Length; i++) {
#if UNITY_2022_2_OR_NEWER
				commands[i].from = origins[i] + originOffset;
#else
				raycastCommands[i] = new RaycastCommand(origins[i] + originOffset, direction, distance, mask, 1);
#endif
			}
		}
	}
}
