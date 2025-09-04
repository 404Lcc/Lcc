using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinding {
	[CustomEditor(typeof(Seeker))]
	[CanEditMultipleObjects]
	public class SeekerEditor : EditorBase {
		static bool tagPenaltiesOpen;
		static List<Seeker> scripts = new List<Seeker>();

		GUIContent[] exactnessLabels = new [] { new GUIContent("Node Center (Snap To Node)"), new GUIContent("Original"), new GUIContent("Interpolate (deprecated)"), new GUIContent("Closest On Node Surface"), new GUIContent("Node Connection") };

		protected override void Inspector () {
			base.Inspector();

			scripts.Clear();
			foreach (var script in targets) scripts.Add(script as Seeker);

			Undo.RecordObjects(targets, "Modify settings on Seeker");

			var startEndModifierProp = FindProperty("startEndModifier");
			startEndModifierProp.isExpanded = EditorGUILayout.Foldout(startEndModifierProp.isExpanded, startEndModifierProp.displayName);
			if (startEndModifierProp.isExpanded) {
				EditorGUI.indentLevel++;
				Popup("startEndModifier.exactStartPoint", exactnessLabels, "Start Point Snapping");
				Popup("startEndModifier.exactEndPoint", exactnessLabels, "End Point Snapping");
				PropertyField("startEndModifier.addPoints", "Add Points");

				if (FindProperty("startEndModifier.exactStartPoint").enumValueIndex == (int)StartEndModifier.Exactness.Original || FindProperty("startEndModifier.exactEndPoint").enumValueIndex == (int)StartEndModifier.Exactness.Original) {
					if (PropertyField("startEndModifier.useRaycasting", "Physics Raycasting")) {
						EditorGUI.indentLevel++;
						PropertyField("startEndModifier.mask", "Layer Mask");
						EditorGUI.indentLevel--;
						EditorGUILayout.HelpBox("Using raycasting to snap the start/end points has largely been superseded by the 'ClosestOnNode' snapping option. It is both faster and usually closer to what you want to achieve.", MessageType.Info);
					}

					if (PropertyField("startEndModifier.useGraphRaycasting", "Graph Raycasting")) {
						EditorGUILayout.HelpBox("Using raycasting to snap the start/end points has largely been superseded by the 'ClosestOnNode' snapping option. It is both faster and usually closer to what you want to achieve.", MessageType.Info);
					}
				}

				EditorGUI.indentLevel--;
			}

			PropertyField("graphMask", "Traversable Graphs");

			tagPenaltiesOpen = EditorGUILayout.Foldout(tagPenaltiesOpen, new GUIContent("Tags", "Settings for each tag"));
			if (tagPenaltiesOpen) {
				var traversableTags = scripts.Select(s => s.traversableTags).ToArray();
				EditorGUI.indentLevel++;
				TagsEditor(FindProperty("tagPenalties"), traversableTags);
				for (int i = 0; i < scripts.Count; i++) {
					scripts[i].traversableTags = traversableTags[i];
				}
				EditorGUI.indentLevel--;
			}

			if (scripts.Count > 0 && scripts[0].traversalProvider != null) {
				EditorGUILayout.HelpBox("A custom traversal provider has been set", MessageType.None);
			}

			// Make sure we don't leak any memory
			scripts.Clear();
		}

		public static void TagsEditor (SerializedProperty tagPenaltiesProp, int[] traversableTags) {
			string[] tagNames = AstarPath.FindTagNames();
			if (tagNames.Length != 32) {
				tagNames = new string[32];
				for (int i = 0; i < tagNames.Length; i++) tagNames[i] = "" + i;
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Tag", EditorStyles.boldLabel, GUILayout.MaxWidth(120));
			for (int i = 0; i < tagNames.Length; i++) {
				EditorGUILayout.LabelField(tagNames[i], GUILayout.MaxWidth(120));
			}

			if (GUILayout.Button("Edit names", EditorStyles.miniButton)) {
				AstarPathEditor.EditTags();
			}
			EditorGUILayout.EndVertical();

			// Prevent indent from affecting the other columns
			var originalIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

#if !ASTAR_NoTagPenalty
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Penalty", EditorStyles.boldLabel, GUILayout.MaxWidth(100));
			var prop = tagPenaltiesProp;
			if (prop.arraySize != 32) prop.arraySize = 32;
			for (int i = 0; i < tagNames.Length; i++) {
				var element = prop.GetArrayElementAtIndex(i);
				EditorGUILayout.PropertyField(element, GUIContent.none, false, GUILayout.MinWidth(100));
				// Penalties should not be negative
				if (!element.hasMultipleDifferentValues && element.intValue < 0) element.intValue = 0;
			}
			if (GUILayout.Button("Reset all", EditorStyles.miniButton)) {
				for (int i = 0; i < tagNames.Length; i++) {
					prop.GetArrayElementAtIndex(i).intValue = 0;
				}
			}
			EditorGUILayout.EndVertical();
#endif

			EditorGUILayout.BeginVertical(GUILayout.MaxWidth(100));
			EditorGUILayout.LabelField("Traversable", EditorStyles.boldLabel, GUILayout.MaxWidth(100));
			for (int i = 0; i < tagNames.Length; i++) {
				var anyFalse = false;
				var anyTrue = false;
				for (int j = 0; j < traversableTags.Length; j++) {
					var prevTraversable = ((traversableTags[j] >> i) & 0x1) != 0;
					anyTrue |= prevTraversable;
					anyFalse |= !prevTraversable;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUI.showMixedValue = anyTrue & anyFalse;
				var newTraversable = EditorGUILayout.Toggle(anyTrue);
				EditorGUI.showMixedValue = false;
				if (EditorGUI.EndChangeCheck()) {
					for (int j = 0; j < traversableTags.Length; j++) {
						traversableTags[j] = (traversableTags[j] & ~(1 << i)) | ((newTraversable ? 1 : 0) << i);
					}
				}
			}

			if (GUILayout.Button("Set all/none", EditorStyles.miniButton)) {
				for (int j = traversableTags.Length - 1; j >= 0; j--) {
					traversableTags[j] = (traversableTags[0] & 0x1) == 0 ? -1 : 0;
				}
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel = originalIndent;
		}
	}
}
