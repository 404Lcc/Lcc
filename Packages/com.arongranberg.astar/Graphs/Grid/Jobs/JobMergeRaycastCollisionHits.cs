using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Calculates if either of the two input hits actually hit something.
	///
	/// result[i] = true if hit1[i] or hit2[i] is a valid hit.
	///
	/// A valid hit will always have a non-zero normal.
	/// </summary>
	[BurstCompile]
	public struct JobMergeRaycastCollisionHits : IJob {
		[ReadOnly]
		public NativeArray<RaycastHit> hit1;

		[ReadOnly]
		public NativeArray<RaycastHit> hit2;

		[WriteOnly]
		public NativeArray<bool> result;

		public void Execute () {
			for (int i = 0; i < hit1.Length; i++) {
				result[i] = hit1[i].normal == Vector3.zero && hit2[i].normal == Vector3.zero;
			}
		}
	}
}
