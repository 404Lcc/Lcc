using UnityEngine;
using UnityEditor;

namespace Pathfinding {
	[CustomGraphEditor(typeof(PointGraph), "Point Graph")]
	public class PointGraphEditor : GraphEditor {
		static readonly GUIContent[] nearestNodeDistanceModeLabels = {
			new GUIContent("Node"),
			new GUIContent("Connection (slower)"),
		};

		public override void OnInspectorGUI (NavGraph target) {
			var graph = target as PointGraph;

			graph.root = ObjectField(new GUIContent("Root", "All children of this object will be used as nodes, if it is not set, a tag search will be used instead (see below)"), graph.root, typeof(Transform), true, false) as Transform;

			graph.recursive = EditorGUILayout.Toggle(new GUIContent("Recursive", "Should children of the children in the root GameObject be searched"), graph.recursive);
			graph.searchTag = EditorGUILayout.TagField(new GUIContent("Tag", "If root is not set, all objects with this tag will be used as nodes"), graph.searchTag);

			if (graph.root != null) {
				EditorGUILayout.HelpBox("All children "+(graph.recursive ? "and sub-children " : "") +"of 'root' will be used as nodes\nSet root to null to use a tag search instead", MessageType.None);
			} else {
				EditorGUILayout.HelpBox("All object with the tag '"+graph.searchTag+"' will be used as nodes"+(graph.searchTag == "Untagged" ? "\nNote: the tag 'Untagged' cannot be used" : ""), MessageType.None);
			}

			graph.maxDistance = EditorGUILayout.FloatField(new GUIContent("Max Distance", "The max distance in world space for a connection to be valid. A zero counts as infinity"), graph.maxDistance);

			graph.limits = EditorGUILayout.Vector3Field("Max Distance (axis aligned)", graph.limits);

			graph.raycast = EditorGUILayout.Toggle(new GUIContent("Raycast", "Use raycasting to check if connections are valid between each pair of nodes"), graph.raycast);

			if (graph.raycast) {
				EditorGUI.indentLevel++;

				graph.use2DPhysics = EditorGUILayout.Toggle(new GUIContent("Use 2D Physics", "If enabled, all raycasts will use the Unity 2D Physics API instead of the 3D one."), graph.use2DPhysics);
				graph.thickRaycast = EditorGUILayout.Toggle(new GUIContent("Thick Raycast", "A thick raycast checks along a thick line with radius instead of just along a line"), graph.thickRaycast);

				if (graph.thickRaycast) {
					EditorGUI.indentLevel++;
					graph.thickRaycastRadius = EditorGUILayout.FloatField(new GUIContent("Raycast Radius", "The radius in world units for the thick raycast"), graph.thickRaycastRadius);
					EditorGUI.indentLevel--;
				}

				graph.mask = EditorGUILayoutx.LayerMaskField("Mask", graph.mask);
				EditorGUI.indentLevel--;
			}

			graph.optimizeForSparseGraph = EditorGUILayout.Toggle(new GUIContent("Optimize For Sparse Graph", "Check online documentation for more information."), graph.optimizeForSparseGraph);
			graph.nearestNodeDistanceMode = (PointGraph.NodeDistanceMode)EditorGUILayout.Popup(new GUIContent("Nearest Node Queries Find Closest"), (int)graph.nearestNodeDistanceMode, nearestNodeDistanceModeLabels);

			if (graph.nearestNodeDistanceMode == PointGraph.NodeDistanceMode.Connection && !graph.optimizeForSparseGraph) {
				EditorGUILayout.HelpBox("Connection mode can only be used if Optimize For Sparse Graph is enabled", MessageType.Error);
			}
		}
	}
}
