using UnityEditor;
using UnityEngine;
using System.Linq;
using Pathfinding.Graphs.Grid.Rules;

namespace Pathfinding {
	/// <summary>Editor for the <see cref="RulePerLayerModifications"/> rule</summary>
	[CustomGridGraphRuleEditor(typeof(RulePerLayerModifications), "Per Layer Modifications")]
	public class RulePerLayerModificationsEditor : IGridGraphRuleEditor {
		public void OnInspectorGUI (GridGraph graph, GridGraphRule rule) {
			var target = rule as RulePerLayerModifications;

			for (int i = 0; i < target.layerRules.Length; i++) {
				GUILayout.BeginHorizontal();
				target.layerRules[i].layer = EditorGUILayout.LayerField((int)target.layerRules[i].layer);
				target.layerRules[i].action = (RulePerLayerModifications.RuleAction)EditorGUILayout.EnumPopup(target.layerRules[i].action);
				if (target.layerRules[i].action == RulePerLayerModifications.RuleAction.SetTag) {
					target.layerRules[i].tag = Pathfinding.Util.EditorGUILayoutHelper.TagField(target.layerRules[i].tag, AstarPathEditor.EditTags);
				} else {
					EditorGUILayout.LabelField("");
				}
				if (GUILayout.Button("", AstarPathEditor.astarSkin.FindStyle("SimpleDeleteButton"))) {
					var ls = target.layerRules.ToList();
					ls.RemoveAt(i);
					target.layerRules = ls.ToArray();
				}
				GUILayout.Space(5);
				GUILayout.EndHorizontal();
			}

			GUILayout.BeginHorizontal();
			GUILayout.Space(32);
			if (GUILayout.Button("Add per layer rule", GUILayout.MaxWidth(160))) {
				var ls = target.layerRules.ToList();
				ls.Add(new RulePerLayerModifications.PerLayerRule());
				target.layerRules = ls.ToArray();
			}
			GUILayout.EndHorizontal();

			if (graph.collision.use2D) {
				EditorGUILayout.HelpBox("This rule does not work with 2D physics, since it requires height testing information", MessageType.Error);
			} else if (!graph.collision.heightCheck) {
				EditorGUILayout.HelpBox("This rule requires height testing to be enabled on the grid graph", MessageType.Error);
			}
		}

		public void OnSceneGUI (GridGraph graph, GridGraphRule rule) { }
	}
}
