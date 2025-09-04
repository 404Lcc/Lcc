using UnityEditor;

namespace Pathfinding {
	[CustomEditor(typeof(FunnelModifier))]
	[CanEditMultipleObjects]
	public class FunnelModifierEditor : EditorBase {
		protected override void Inspector () {
			Section("Settings for navmeshes");
			PropertyField("quality");
			PropertyField("splitAtEveryPortal");

			Section("Settings for grids");
			PropertyField("accountForGridPenalties");
		}
	}
}
