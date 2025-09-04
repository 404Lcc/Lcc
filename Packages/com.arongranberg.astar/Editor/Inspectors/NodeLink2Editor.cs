using Pathfinding;
using UnityEditor;
using UnityEngine;

namespace Pathfinding {
	[CustomEditor(typeof(NodeLink2), true)]
	[CanEditMultipleObjects]
	public class NodeLink2Editor : EditorBase {
		GUIContent HandlerContent = new GUIContent("Handler", "The object that handles movement when traversing the link");

		protected override void Inspector () {
			base.Inspector();

			var target = this.target as NodeLink2;
			if (target.onTraverseOffMeshLink != null) {
				var name = target.onTraverseOffMeshLink.name;
				if (name == null || name == "") name = target.onTraverseOffMeshLink.GetType().Name;
				else name += " → " + target.onTraverseOffMeshLink.GetType().Name;
				if (target.onTraverseOffMeshLink is UnityEngine.Component) {
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.ObjectField(HandlerContent, target.onTraverseOffMeshLink as UnityEngine.Object, typeof(UnityEngine.Component), true);
					EditorGUI.EndDisabledGroup();
				} else {
					EditorGUILayout.LabelField(HandlerContent, name);
				}
			}
		}
	}
}
