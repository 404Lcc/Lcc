using UnityEngine;
using UnityEditor;

namespace Pathfinding {
	[CustomEditor(typeof(NavmeshCut))]
	[CanEditMultipleObjects]
	public class NavmeshCutEditor : EditorBase {
		GUIContent[] MeshTypeOptions = new [] {
			new GUIContent("Rectangle (legacy)"),
			new GUIContent("Circle (legacy)"),
			new GUIContent("Custom Mesh"),
			new GUIContent("Box"),
			new GUIContent("Sphere"),
			new GUIContent("Capsule"),
		};

		protected override void Inspector () {
			// Make sure graphs are deserialized.
			// The gizmos on the navmesh cut uses the graph information to visualize the character radius
			AstarPath.FindAstarPath();

			EditorGUI.BeginChangeCheck();
			var type = FindProperty("type");
			var circleResolution = FindProperty("circleResolution");
			Popup("type", MeshTypeOptions, label: "Shape");
			EditorGUI.indentLevel++;

			if (!type.hasMultipleDifferentValues) {
				switch ((NavmeshCut.MeshType)type.intValue) {
				case NavmeshCut.MeshType.Circle:
				case NavmeshCut.MeshType.Capsule:
					FloatField("circleRadius", "Radius", min: 0.01f);
					PropertyField("circleResolution", "Resolution");
					FloatField("height", min: 0f);

					if (circleResolution.intValue >= 20) {
						EditorGUILayout.HelpBox("Be careful with large resolutions. It is often better with a relatively low resolution since it generates cleaner navmeshes with fewer nodes.", MessageType.Warning);
					}
					break;
				case NavmeshCut.MeshType.Sphere:
					FloatField("circleRadius", "Radius", min: 0.01f);
					PropertyField("circleResolution", "Resolution");

					if (circleResolution.intValue >= 20) {
						EditorGUILayout.HelpBox("Be careful with large resolutions. It is often better with a relatively low resolution since it generates cleaner navmeshes with fewer nodes.", MessageType.Warning);
					}
					break;
				case NavmeshCut.MeshType.Rectangle:
					PropertyField("rectangleSize");
					FloatField("height", min: 0f);
					break;
				case NavmeshCut.MeshType.Box:
					PropertyField("rectangleSize.x", "Width");
					PropertyField("height", "Height");
					PropertyField("rectangleSize.y", "Depth");
					break;
				case NavmeshCut.MeshType.CustomMesh:
					PropertyField("mesh");
					PropertyField("meshScale");
					FloatField("height", min: 0f);
					EditorGUILayout.HelpBox("This mesh should be a planar surface. Take a look at the documentation for an example.", MessageType.Info);
					break;
				}
			}

			PropertyField("center");
			EditorGUI.indentLevel--;

			EditorGUILayout.Separator();
			PropertyField("updateDistance");
			if (PropertyField("useRotationAndScale")) {
				EditorGUI.indentLevel++;
				FloatField("updateRotationDistance", min: 0f, max: 180f);
				EditorGUI.indentLevel--;
			}

			PropertyField("isDual");
			PropertyField("cutsAddedGeom", "Cuts Added Geometry");
			PropertyField("radiusExpansionMode", "Radius Expansion");

			EditorGUI.BeginChangeCheck();
			PropertyField("graphMask", "Affected Graphs");
			bool changedMask = EditorGUI.EndChangeCheck();

			serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck()) {
				foreach (NavmeshCut tg in targets) {
					tg.ForceUpdate();
					// If the mask is changed we disable and then enable the component
					// to make sure it is removed from the right graphs and then added back
					if (changedMask && tg.enabled) {
						tg.enabled = false;
						tg.enabled = true;
					}
				}
			}

#if !UNITY_2022_3_OR_NEWER
			EditorGUILayout.HelpBox("The NavmeshCut component requires Unity 2022.3 or newer to work, due to Unity bugs in earlier versions. Please update Unity to 2022.3.21 or later, if you want to use navmesh cutting.", MessageType.Error);
#elif !MODULE_COLLECTIONS_2_2_0_OR_NEWER
			EditorGUILayout.HelpBox("The NavmeshCut component requires the com.unity.collections package version 2.2.0 or newer. Please install it using the Package Manager.", MessageType.Error);
#endif
		}
	}
}
