#if MODULE_ENTITIES
using Unity.Entities;

namespace Pathfinding.ECS {
	/// <summary>Enabled if the agnet is ready to start traversing an off-mesh link</summary>
	[System.Serializable]
	public struct ReadyToTraverseOffMeshLink : IComponentData, IEnableableComponent {
	}
}
#endif
