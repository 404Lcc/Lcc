using UnityEditor;
using UnityEngine;

namespace Pathfinding {
	[CustomPropertyDrawer(typeof(PathfindingTag))]
	public class PathfindingTagDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			Pathfinding.Util.EditorGUILayoutHelper.TagField(position, label, property, AstarPathEditor.EditTags);
		}
	}
}
