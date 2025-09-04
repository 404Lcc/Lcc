#if UNITY_2022_2_OR_NEWER
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Fills the output with true or false depending on if the collider hit was a hit.
	///
	/// result[i] = false if hits[i] is a valid hit, otherwise true.
	/// </summary>
	[BurstCompile]
	public struct JobColliderHitsToBooleans : IJob {
		[ReadOnly]
		public NativeArray<ColliderHit> hits;

		[WriteOnly]
		public NativeArray<bool> result;

		public void Execute () {
			for (int i = 0; i < hits.Length; i++) {
				result[i] = hits[i].instanceID == 0;
			}
		}
	}
}
#endif
