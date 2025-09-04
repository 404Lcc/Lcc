using Pathfinding.Graphs.Grid.Rules;
using UnityEditor;
using UnityEngine;

namespace Pathfinding {
	/// <summary>Editor for the <see cref="RuleAnglePenalty"/> rule</summary>
	[CustomGridGraphRuleEditor(typeof(RuleAnglePenalty), "Penalty from Slope Angle")]
	public class RuleAnglePenaltyEditor : IGridGraphRuleEditor {
		public void OnInspectorGUI (GridGraph graph, GridGraphRule rule) {
			var target = rule as RuleAnglePenalty;

			if (target.curve == null || target.curve.length == 0) target.curve = AnimationCurve.Linear(0, 0, 90, 1);
			target.penaltyScale = EditorGUILayout.FloatField("Penalty Scale", target.penaltyScale);
			if (target.penaltyScale < 1) target.penaltyScale = 1;
			target.curve = EditorGUILayout.CurveField(target.curve, Color.red, new Rect(0, 0, 90, 1));

			EditorGUILayout.HelpBox("Nodes will get a penalty between 0 and " + target.penaltyScale.ToString("0") + " depending on the slope angle", MessageType.None);
		}

		public void OnSceneGUI (GridGraph graph, GridGraphRule rule) { }
	}
}
