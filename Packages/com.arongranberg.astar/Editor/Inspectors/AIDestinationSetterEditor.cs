using Pathfinding;
using UnityEditor;

namespace Pathfinding {
	[CustomEditor(typeof(AIDestinationSetter), true)]
	[CanEditMultipleObjects]
	public class AIDestinationSetterEditor : EditorBase {
		protected override void Inspector () {
			PropertyField("target");
#if MODULE_ENTITIES
			if ((target as AIDestinationSetter).GetComponent<FollowerEntity>() != null) {
				PropertyField("useRotation");
			}
#endif
		}
	}
}
