#if MODULE_ENTITIES
using Unity.Entities;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.Util;
	using Unity.Mathematics;

	/// <summary>Holds an agent's movement plane</summary>
	[System.Serializable]
	public struct AgentMovementPlane : IComponentData {
		/// <summary>
		/// The movement plane for the agent.
		///
		/// The movement plane determines what the "up" direction of the agent is.
		/// For most typical 3D games, this will be aligned with the Y axis, but there are
		/// games in which the agent needs to navigate on walls, or on spherical worlds.
		/// For those games this movement plane will track the plane in which the agent is currently moving.
		///
		/// See: spherical (view in online documentation for working links)
		/// </summary>
		public NativeMovementPlane value;

		/// <summary>Create a movement plane aligned with the XZ plane of the specified rotation</summary>
		public AgentMovementPlane (quaternion rotation) {
			value = new NativeMovementPlane(rotation);
		}

		public AgentMovementPlane (NativeMovementPlane plane) {
			value = plane;
		}
	}
}
#endif
