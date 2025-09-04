#if UNITY_2022_2_OR_NEWER
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Pathfinding.Collections;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Prepares a set of capsule commands for collision checking in a grid graph.
	///
	/// See: <see cref="GraphCollision"/>
	/// </summary>
	[BurstCompile]
	public struct JobPrepareCapsuleCommands : IJob {
		public Vector3 direction;
		public Vector3 originOffset;
		public float radius;
		public LayerMask mask;
		public PhysicsScene physicsScene;

		[ReadOnly]
		public NativeArray<Vector3> origins;

		[WriteOnly]
		public NativeArray<OverlapCapsuleCommand> commands;

		public void Execute () {
			var commandSpan = commands.AsUnsafeSpan();
			// It turns out it is faster to set all commands to the same value using MemCpyReplicate and then patch point0 and point1,
			// rather than setting each command individually (about 30% faster even).
			// MemCpy is very fast.
			var queryParameters = new QueryParameters(mask, false, QueryTriggerInteraction.Ignore, false);
			commandSpan.Fill(new OverlapCapsuleCommand(physicsScene, Vector3.zero, Vector3.zero, radius, queryParameters));

			for (int i = 0; i < commandSpan.Length; i++) {
				var origin = origins[i] + originOffset;
				commandSpan[i].point0 = origin;
				commandSpan[i].point1 = origin + direction;
			}
		}
	}
}
#endif
