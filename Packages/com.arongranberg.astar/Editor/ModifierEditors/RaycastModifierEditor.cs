using UnityEditor;

namespace Pathfinding {
	[CustomEditor(typeof(RaycastModifier))]
	[CanEditMultipleObjects]
	public class RaycastModifierEditor : EditorBase {
		protected override void Inspector () {
			PropertyField("quality");

			if (PropertyField("useRaycasting", "Use Physics Raycasting")) {
				EditorGUI.indentLevel++;

				PropertyField("use2DPhysics");
				if (PropertyField("thickRaycast")) {
					EditorGUI.indentLevel++;
					FloatField("thickRaycastRadius", min: 0f);
					EditorGUI.indentLevel--;
				}

				PropertyField("raycastOffset");
				PropertyField("mask", "Layer Mask");
				EditorGUI.indentLevel--;
			}

			PropertyField("useGraphRaycasting");
			if (!FindProperty("useGraphRaycasting").boolValue && !FindProperty("useRaycasting").boolValue) {
				EditorGUILayout.HelpBox("You should use either raycasting, graph raycasting or both, otherwise this modifier will not do anything", MessageType.Warning);
			}

			if (FindProperty("useGraphRaycasting").boolValue && !FindProperty("useRaycasting").boolValue) {
				AstarPath.FindAstarPath();
				if (AstarPath.active != null && AstarPath.active.data.gridGraph != null) {
					EditorGUILayout.HelpBox("For grid graphs, when using only graph raycasting the funnel modifier has superceded this modifier. Try using that instead. It's faster and more accurate.", MessageType.Warning);
				}
			}
		}
	}
}
