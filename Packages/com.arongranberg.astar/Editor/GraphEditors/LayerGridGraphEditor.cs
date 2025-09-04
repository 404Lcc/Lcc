using UnityEngine;
using UnityEditor;
using Pathfinding.Graphs.Grid;

namespace Pathfinding {
	[CustomGraphEditor(typeof(LayerGridGraph), "Layered Grid Graph")]
	public class LayerGridGraphEditor : GridGraphEditor {
		protected override void DrawMiddleSection (GridGraph graph) {
			var layerGridGraph = graph as LayerGridGraph;

			DrawNeighbours(graph);

			layerGridGraph.characterHeight = EditorGUILayout.DelayedFloatField("Character Height", layerGridGraph.characterHeight);
			DrawMaxClimb(graph);

			DrawMaxSlope(graph);
			DrawErosion(graph);
		}

		protected override void DrawMaxClimb (GridGraph graph) {
			var layerGridGraph = graph as LayerGridGraph;

			base.DrawMaxClimb(graph);
			layerGridGraph.maxStepHeight = Mathf.Clamp(layerGridGraph.maxStepHeight, 0, layerGridGraph.characterHeight);

			if (layerGridGraph.maxStepHeight >= layerGridGraph.characterHeight) {
				EditorGUILayout.HelpBox("Max step height needs to be smaller or equal to character height", MessageType.Info);
			}
		}

		protected override void DrawUse2DPhysics (GraphCollision collision) {
			// 2D physics does not make sense for a layered grid graph
			collision.use2D = false;
		}
	}
}
