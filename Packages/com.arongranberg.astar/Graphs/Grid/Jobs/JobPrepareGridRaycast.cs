using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Pathfinding.Collections;
using UnityEngine.Assertions;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Prepares a set of raycast commands for a grid graph.
	///
	/// Each ray will start at <see cref="raycastOffset"/> from the node's position. The end point of the raycast will be the start point + <see cref="raycastDirection"/>.
	///
	/// See: <see cref="GraphCollision"/>
	/// </summary>
	[BurstCompile]
	public struct JobPrepareGridRaycast : IJob {
		public Matrix4x4 graphToWorld;
		public IntBounds bounds;
		public Vector3 raycastOffset;
		public Vector3 raycastDirection;
		public LayerMask raycastMask;
		public PhysicsScene physicsScene;

		[WriteOnly]
		public NativeArray<RaycastCommand> raycastCommands;

		public void Execute () {
			float raycastLength = raycastDirection.magnitude;
			var size = bounds.size;

			// In particular Unity 2022.2 seems to assert that RaycastCommands use normalized directions
			var direction = raycastDirection.normalized;
			var commands = raycastCommands.AsUnsafeSpan();

			Assert.AreEqual(commands.Length, size.x * size.z);

#if UNITY_2022_2_OR_NEWER
			var queryParameters = new QueryParameters(raycastMask, false, QueryTriggerInteraction.Ignore, false);
			// This is about 30% faster than setting each command individually. MemCpy is fast.
			commands.Fill(new RaycastCommand(physicsScene, Vector3.zero, direction, queryParameters, raycastLength));
#else
			const int RaycastMaxHits = 1;
#endif

			for (int z = 0; z < size.z; z++) {
				int zw = z * size.x;
				for (int x = 0; x < size.x; x++) {
					int idx = zw + x;
					var pos = JobNodeGridLayout.NodePosition(graphToWorld, x + bounds.min.x, z + bounds.min.z);
#if UNITY_2022_2_OR_NEWER
					commands[idx].from = pos + raycastOffset;
#else
					commands[idx] = new RaycastCommand(pos + raycastOffset, direction, raycastLength, raycastMask, RaycastMaxHits);
#endif
				}
			}
		}
	}

	/// <summary>
	/// Prepares a set of thick raycast commands for a grid graph.
	///
	/// Each ray will start at <see cref="raycastOffset"/> from the node's position. The end point of the raycast will be the start point + <see cref="raycastDirection"/>.
	///
	/// See: <see cref="GraphCollision"/>
	/// </summary>
	[BurstCompile]
	public struct JobPrepareGridRaycastThick : IJob {
		public Matrix4x4 graphToWorld;
		public IntBounds bounds;
		public Vector3 raycastOffset;
		public Vector3 raycastDirection;
		public LayerMask raycastMask;
		public PhysicsScene physicsScene;
		public float radius;

		[WriteOnly]
		public NativeArray<SpherecastCommand> raycastCommands;

		public void Execute () {
			float raycastLength = raycastDirection.magnitude;
			var size = bounds.size;

			// In particular Unity 2022.2 seems to assert that RaycastCommands use normalized directions
			var direction = raycastDirection.normalized;
			var commands = raycastCommands.AsUnsafeSpan();

			Assert.AreEqual(commands.Length, size.x * size.z);

#if UNITY_2022_2_OR_NEWER
			var queryParameters = new QueryParameters(raycastMask, false, QueryTriggerInteraction.Ignore, false);
			// This is about 30% faster than setting each command individually. MemCpy is fast.
			commands.Fill(new SpherecastCommand(physicsScene, Vector3.zero, radius, direction, queryParameters, raycastLength));
#endif

			for (int z = 0; z < size.z; z++) {
				int zw = z * size.x;
				for (int x = 0; x < size.x; x++) {
					int idx = zw + x;
					var pos = JobNodeGridLayout.NodePosition(graphToWorld, x + bounds.min.x, z + bounds.min.z);
#if UNITY_2022_2_OR_NEWER
					commands[idx].origin = pos + raycastOffset;
#else
					commands[idx] = new SpherecastCommand(pos + raycastOffset, radius, direction, raycastLength, raycastMask);
#endif
				}
			}
		}
	}
}
