#if MODULE_ENTITIES
using Unity.Entities;

namespace Pathfinding.Util {
	interface IRuntimeBaker {
		void OnCreatedEntity(World world, Entity entity);
	}
}
#endif
