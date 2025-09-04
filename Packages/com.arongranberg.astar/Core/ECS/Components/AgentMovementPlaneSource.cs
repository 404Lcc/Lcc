#if MODULE_ENTITIES
using Unity.Entities;

namespace Pathfinding.ECS {
	/// <summary>
	/// The movement plane source for an agent.
	///
	/// See: <see cref="MovementPlaneSource"/>
	/// </summary>
	[System.Serializable]
	public struct AgentMovementPlaneSource : ISharedComponentData {
		public MovementPlaneSource value;
	}
}
#endif
