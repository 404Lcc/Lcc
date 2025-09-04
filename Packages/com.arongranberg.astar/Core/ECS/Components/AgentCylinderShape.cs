#if MODULE_ENTITIES
using Unity.Entities;

namespace Pathfinding.ECS {
	using Pathfinding;
	using Pathfinding.ECS.RVO;

	/// <summary>An agent's shape represented as a cylinder</summary>
	[System.Serializable]
	public struct AgentCylinderShape : IComponentData {
		/// <summary>Radius of the agent in world units</summary>
		public float radius;

		/// <summary>Height of the agent in world units</summary>
		public float height;
	}
}
#endif
