using Pathfinding.Graphs.Grid.Rules;
using UnityEditor;
using UnityEngine;

namespace Pathfinding {
	/// <summary>Editor for the <see cref="RuleElevationPenalty"/> rule</summary>
	[CustomGridGraphRuleEditor(typeof(RuleElevationPenalty), "Penalty from Elevation")]
	public class RuleElevationPenaltyEditor : IGridGraphRuleEditor {
		float lastChangedTime = -10000;

		public void OnInspectorGUI (GridGraph graph, GridGraphRule rule) {
			var target = rule as RuleElevationPenalty;

			if (target.curve == null || target.curve.length == 0) target.curve = AnimationCurve.Linear(0, 0, 1, 1);
			target.penaltyScale = EditorGUILayout.FloatField("Penalty Scale", target.penaltyScale);
			target.penaltyScale = Mathf.Max(target.penaltyScale, 1.0f);

			EditorGUILayout.LabelField("Elevation Range", "");
			EditorGUI.BeginChangeCheck();
			EditorGUI.indentLevel++;
			target.elevationRange.x = EditorGUILayout.FloatField("Min", target.elevationRange.x);
			target.elevationRange.y = EditorGUILayout.FloatField("Max", target.elevationRange.y);
			target.elevationRange.x = Mathf.Max(target.elevationRange.x, 0);
			target.elevationRange.y = Mathf.Max(target.elevationRange.y, target.elevationRange.x + 1.0f);
			EditorGUI.indentLevel--;
			if (EditorGUI.EndChangeCheck()) lastChangedTime = Time.realtimeSinceStartup;

			target.curve = EditorGUILayout.CurveField(target.curve, Color.red, new Rect(0, 0, 1, 1));

			EditorGUILayout.HelpBox("Nodes will get a penalty between 0 and " + target.penaltyScale.ToString("0") + " depending on their elevation above the grid graph plane", MessageType.None);
		}

		protected static readonly Color GizmoColorMax = new Color(222.0f/255, 113.0f/255, 33.0f/255, 0.5f);
		protected static readonly Color GizmoColorMin = new Color(33.0f/255, 104.0f/255, 222.0f/255, 0.5f);

		public void OnSceneGUI (GridGraph graph, GridGraphRule rule) {
			var target = rule as RuleElevationPenalty;

			// Draw some helpful gizmos in the scene view for a few seconds whenever the settings change
			const float FullAlphaTime = 2.0f;
			const float FadeoutTime = 0.5f;
			float alpha = Mathf.SmoothStep(1, 0, (Time.realtimeSinceStartup - lastChangedTime - FullAlphaTime)/FadeoutTime);

			if (alpha <= 0) return;

			var currentTransform = graph.transform * Matrix4x4.Scale(new Vector3(graph.width, 1, graph.depth));
			Handles.matrix = currentTransform.matrix;
			var zTest = Handles.zTest;
			Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
			Handles.color = GizmoColorMin * new Color(1.0f, 1.0f, 1.0f, alpha);
			Handles.DrawAAConvexPolygon(new Vector3[] { new Vector3(0, target.elevationRange.x, 0), new Vector3(1, target.elevationRange.x, 0), new Vector3(1, target.elevationRange.x, 1), new Vector3(0, target.elevationRange.x, 1) });
			Handles.color = GizmoColorMax * new Color(1.0f, 1.0f, 1.0f, alpha);
			Handles.DrawAAConvexPolygon(new Vector3[] { new Vector3(0, target.elevationRange.y, 0), new Vector3(1, target.elevationRange.y, 0), new Vector3(1, target.elevationRange.y, 1), new Vector3(0, target.elevationRange.y, 1) });
			Handles.zTest = zTest;
			Handles.matrix = Matrix4x4.identity;

			// Repaint the scene view until the alpha goes to zero
			SceneView.RepaintAll();
		}
	}
}
