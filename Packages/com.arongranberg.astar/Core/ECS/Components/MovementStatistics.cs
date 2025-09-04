#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding.ECS {
	public struct MovementStatistics : IComponentData {
		/// <summary>
		/// The estimated velocity that the agent is moving with.
		/// This includes all form of movement, including local avoidance and gravity.
		/// </summary>
		public float3 estimatedVelocity;

		/// <summary>The position of the agent at the end of the last movement simulation step</summary>
		public float3 lastPosition;
	}
}
#endif
