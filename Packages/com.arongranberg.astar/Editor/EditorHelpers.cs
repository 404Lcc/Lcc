using UnityEngine;
using UnityEditor;

namespace Pathfinding.Util {
	/// <summary>Some editor gui helper methods</summary>
	public static class EditorGUILayoutHelper {
		/// <summary>
		/// Tag names and an additional 'Edit Tags...' entry.
		/// Used for SingleTagField
		/// </summary>
		static GUIContent[] tagNamesAndEditTagsButton;
		static int[] tagValues = new [] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, -1 };

		/// <summary>
		/// Last time tagNamesAndEditTagsButton was updated.
		/// Uses EditorApplication.timeSinceStartup
		/// </summary>
		static double timeLastUpdatedTagNames;

		static void FindTagNames () {
			// Make sure the AstarPath object is initialized, this is required to be able to show tag names in the popup
			AstarPath.FindAstarPath();

			// Make sure the tagNamesAndEditTagsButton is relatively up to date
			if (tagNamesAndEditTagsButton == null || EditorApplication.timeSinceStartup - timeLastUpdatedTagNames > 1) {
				timeLastUpdatedTagNames = EditorApplication.timeSinceStartup;
				tagNamesAndEditTagsButton = new GUIContent[GraphNode.MaxTagIndex + 2];
				if (AstarPath.active != null) {
					var tagNames = AstarPath.active.GetTagNames();
					for (int i = 0; i <= GraphNode.MaxTagIndex; i++) {
						if (AstarPath.active == null) tagNamesAndEditTagsButton[i] = new GUIContent("Tag " + i + (i == GraphNode.MaxTagIndex ? "+" : ""));
						else {
							var tagName = tagNames[i];
							if (tagName != i.ToString()) {
								tagNamesAndEditTagsButton[i] = new GUIContent(tagName + " (tag " + i + ")");
							} else {
								tagNamesAndEditTagsButton[i] = new GUIContent("Tag " + i);
							}
						}
					}
				} else {
					for (int i = 0; i <= GraphNode.MaxTagIndex; i++) {
						tagNamesAndEditTagsButton[i] = new GUIContent("Tag " + i + (i == GraphNode.MaxTagIndex ? "+" : ""));
					}
				}
				tagNamesAndEditTagsButton[tagNamesAndEditTagsButton.Length-1] = new GUIContent("Edit Tags...");
			}
		}

		public static int TagField (int value, System.Action editCallback) {
			FindTagNames();
			value = Mathf.Clamp(value, 0, GraphNode.MaxTagIndex);

			var newValue = EditorGUILayout.IntPopup(value, tagNamesAndEditTagsButton, tagValues);

			// Last element corresponds to the 'Edit Tags...' entry. Open the tag editor
			if (newValue == -1) {
				editCallback();
			} else {
				value = newValue;
			}

			return value;
		}

		public static int TagField (Rect rect, GUIContent label, int value, System.Action editCallback) {
			FindTagNames();
			// Tags are between 0 and GraphNode.MaxTagIndex
			value = Mathf.Clamp(value, 0, GraphNode.MaxTagIndex);

			var newValue = EditorGUI.IntPopup(rect, label, value, tagNamesAndEditTagsButton, tagValues);

			// Last element corresponds to the 'Edit Tags...' entry. Open the tag editor
			if (newValue == -1) {
				editCallback();
			} else {
				value = newValue;
			}

			return value;
		}

		public static int TagField (GUIContent label, int value, System.Action editCallback) {
			return TagField(GUILayoutUtility.GetRect(label, EditorStyles.popup), label, value, editCallback);
		}

		public static void TagField (Rect position, GUIContent label, SerializedProperty property, System.Action editCallback) {
			FindTagNames();
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
			property = property.FindPropertyRelative("value");
			var newValue = EditorGUI.IntPopup(position, label, (int)property.intValue, tagNamesAndEditTagsButton, tagValues);

			if (EditorGUI.EndChangeCheck() || property.intValue < 0 || property.intValue > GraphNode.MaxTagIndex) {
				if (newValue == -1) {
					editCallback();
				} else {
					property.intValue = Mathf.Clamp(newValue, 0, GraphNode.MaxTagIndex);
				}
			}
			EditorGUI.showMixedValue = false;
		}
	}
}
