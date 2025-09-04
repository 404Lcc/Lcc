using UnityEngine;
using UnityEditor;

namespace Pathfinding {
	[CustomGraphEditor(typeof(NavMeshGraph), "Navmesh Graph")]
	public class NavMeshGraphEditor : GraphEditor {
		public override void OnInspectorGUI (NavGraph target) {
			var graph = target as NavMeshGraph;

			graph.sourceMesh = ObjectField("Source Mesh", graph.sourceMesh, typeof(Mesh), false, true) as Mesh;

			graph.offset = EditorGUILayout.Vector3Field("Offset", graph.offset);

			graph.rotation = EditorGUILayout.Vector3Field("Rotation", graph.rotation);

			graph.scale = EditorGUILayout.FloatField(new GUIContent("Scale", "Scale of the mesh"), graph.scale);
			graph.scale = Mathf.Abs(graph.scale) < 0.01F ? (graph.scale >= 0 ? 0.01F : -0.01F) : graph.scale;

			#pragma warning disable 618
			if (graph.nearestSearchOnlyXZ) {
				graph.nearestSearchOnlyXZ = EditorGUILayout.Toggle(new GUIContent("Nearest node queries in XZ space",
					"Recomended for single-layered environments.\nFaster but can be inacurate esp. in multilayered contexts."), graph.nearestSearchOnlyXZ);

				EditorGUILayout.HelpBox("The global toggle for node queries in XZ space has been deprecated. Use the NNConstraint settings instead.", MessageType.Warning);
			}
			#pragma warning restore 618

			graph.recalculateNormals = EditorGUILayout.Toggle(new GUIContent("Recalculate Normals", "Disable for spherical graphs or other complicated surfaces that allow the agents to e.g walk on walls or ceilings. See docs for more info."), graph.recalculateNormals);
			graph.enableNavmeshCutting = EditorGUILayout.Toggle(new GUIContent("Affected By Navmesh Cuts", "Makes this graph affected by NavmeshCut and NavmeshAdd components. See the documentation for more info."), graph.enableNavmeshCutting);
			if (graph.enableNavmeshCutting) {
				EditorGUI.indentLevel++;
				EditorGUI.BeginChangeCheck();
				var newValue = EditorGUILayout.FloatField(new GUIContent("Agent Radius", "Navmesh cuts can optionally be expanded by the agent radius"), graph.navmeshCuttingCharacterRadius);
				if (EditorGUI.EndChangeCheck()) {
					graph.navmeshCuttingCharacterRadius = Mathf.Max(0, newValue);
					graph.navmeshUpdateData.ReloadAllTiles();
				}
				EditorGUI.indentLevel--;
			}

			GUILayout.BeginHorizontal();
			GUILayout.Space(18);
			graph.showMeshSurface = GUILayout.Toggle(graph.showMeshSurface, new GUIContent("Show surface", "Toggles gizmos for drawing the surface of the mesh"), EditorStyles.miniButtonLeft);
			graph.showMeshOutline = GUILayout.Toggle(graph.showMeshOutline, new GUIContent("Show outline", "Toggles gizmos for drawing an outline of the nodes"), EditorStyles.miniButtonMid);
			graph.showNodeConnections = GUILayout.Toggle(graph.showNodeConnections, new GUIContent("Show connections", "Toggles gizmos for drawing node connections"), EditorStyles.miniButtonRight);
			GUILayout.EndHorizontal();
		}
	}
}
