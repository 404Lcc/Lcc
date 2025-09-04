#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding.ECS {
	/// <summary>Agent state related to gravity</summary>
	public struct GravityState : IComponentData, IEnableableComponent {
		/// <summary>
		/// Current vertical velocity of the agent.
		/// This is the velocity that the agent is moving with due to gravity.
		/// It is not necessarily the same as the Y component of the estimated velocity.
		/// </summary>
		public float verticalVelocity;
	}
}
#endif
