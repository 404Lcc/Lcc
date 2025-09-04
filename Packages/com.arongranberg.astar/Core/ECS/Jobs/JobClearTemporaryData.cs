#if MODULE_ENTITIES
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Pathfinding.ECS {
	[BurstCompile]
	public partial struct JobClearTemporaryData : IJobEntity {
		public void Execute (ref MovementState movementState, ref MovementControl movementControl) {
			// The hierarchicalNodeIndex is only valid within the AIMovementSystemGroup.
			// We clear the value at the end of that group to prevent it from being accidentally used later.
			movementState.hierarchicalNodeIndex = -1;
			movementControl.hierarchicalNodeIndex = -1;
		}
	}
}
#endif
