using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Pathfinding {
	[CustomEditor(typeof(ProceduralGraphMover))]
	[CanEditMultipleObjects]
	public class ProceduralGridMoverEditor : EditorBase {
		GUIContent[] graphLabels = new GUIContent[32];

		protected override void Inspector () {
			// Make sure the AstarPath object is initialized and the graphs are loaded, this is required to be able to show graph names in the mask popup
			AstarPath.FindAstarPath();

			for (int i = 0; i < graphLabels.Length; i++) {
				if (AstarPath.active == null || AstarPath.active.data.graphs == null || i >= AstarPath.active.data.graphs.Length || AstarPath.active.data.graphs[i] == null) {
					graphLabels[i] = new GUIContent("Graph " + i + (i == 31 ? "+" : ""));
				} else {
					graphLabels[i] = new GUIContent(AstarPath.active.data.graphs[i].name + " (graph " + i + ")");
				}
			}

			Popup("graphIndex", graphLabels, "Graph");
			PropertyField("target");

			// Only show the update distance field if the graph is a grid graph, or if we are not sure which graph type it is
			var graphIndexProp = FindProperty("graphIndex");
			bool showField = true;
			if (!graphIndexProp.hasMultipleDifferentValues && AstarPath.active != null) {
				var graphIndex = graphIndexProp.intValue;
				if (graphIndex >= 0 && graphIndex < AstarPath.active.data.graphs.Length) {
					var graph = AstarPath.active.data.graphs[graphIndex];
					if (graph is GridGraph) {
						// NOOP
					} else if (graph is RecastGraph) {
						showField = false;
					} else {
						EditorGUILayout.HelpBox("The selected graph is not a grid, layered grid or recast graph", MessageType.Warning);
					}
				}
			}
			if (showField) {
				PropertyField("updateDistance");
			}
		}
	}
}
