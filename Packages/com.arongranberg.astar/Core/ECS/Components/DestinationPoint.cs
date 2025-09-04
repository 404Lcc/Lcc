#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;

namespace Pathfinding.ECS {
	/// <summary>Holds an agent's destination point</summary>
	public struct DestinationPoint : IComponentData {
		/// <summary>
		/// The destination point that the agent is moving towards.
		///
		/// This is the point that the agent is trying to reach, but it may not always be possible to reach it.
		///
		/// See: <see cref="AIDestinationSetter"/>
		/// See: <see cref="IAstarAI.destination"/>
		/// </summary>
		public float3 destination;

		/// <summary>
		/// The direction the agent should face when it reaches the destination.
		///
		/// If zero, the agent will not try to face any particular direction when reaching the destination.
		/// </summary>
		public float3 facingDirection;
	}
}
#endif
