using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Pathfinding.Examples;
using System.Collections.Generic;
using UnityEngine.AI;

namespace Pathfinding.Examples {
	[CustomEditor(typeof(Interactable))]
	[CanEditMultipleObjects]
	public class InteractableEditor : EditorBase {
		ReorderableList actions;

		static Rect SliceRow (ref Rect rect, float height) {
			return GUIUtilityx.SliceRow(ref rect, height);
		}

		protected override void OnEnable () {
			base.OnEnable();
			actions = new ReorderableList(serializedObject, serializedObject.FindProperty("actions"), true, true, true, true);
			actions.drawElementCallback = (Rect rect, int index, bool active, bool isFocused) => {
				var item = actions.serializedProperty.GetArrayElementAtIndex(index);
				var ob = item.managedReferenceValue as Interactable.InteractableAction;
				if (ob == null) {
					EditorGUI.LabelField(rect, "Null");
					return;
				}
				var tp = ob.GetType();

				var lineHeight = EditorGUIUtility.singleLineHeight;
				if (tp == typeof(Interactable.AnimatorSetBoolAction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Set Animator Property", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("animator"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("propertyName"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("value"));
				} else if (tp == typeof(Interactable.AnimatorPlay)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Play Animator State", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("animator"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("stateName"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("normalizedTime"));
				} else if (tp == typeof(Interactable.ActivateParticleSystem)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Activate Particle System", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("particleSystem"));
				} else if (tp == typeof(Interactable.DelayAction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Delay", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("delay"));
				} else if (tp == typeof(Interactable.SetObjectActiveAction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Set Object Active", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("target"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("active"));
				} else if (tp == typeof(Interactable.TeleportAgentAction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Teleport Agent", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("destination"));
				} else if (tp == typeof(Interactable.TeleportAgentOnLinkAction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Teleport Agent on Off-Mesh Link", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("destination"));
				} else if (tp == typeof(Interactable.SetTransformAction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Set Transform", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("transform"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("source"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("setPosition"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("setRotation"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("setScale"));
				} else if (tp == typeof(Interactable.MoveToAction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Move To", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("destination"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("useRotation"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("waitUntilReached"));
				} else if (tp == typeof(Interactable.InstantiatePrefab)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Instantiate Prefab", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("prefab"));
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("position"));
				} else if (tp == typeof(Interactable.CallFunction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Call Function", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("function"));
				} else if (tp == typeof(Interactable.InteractAction)) {
					EditorGUI.LabelField(SliceRow(ref rect, lineHeight), "Interact", EditorStyles.boldLabel);
					EditorGUI.PropertyField(SliceRow(ref rect, lineHeight), item.FindPropertyRelative("interactable"));
				}
			};
			actions.elementHeightCallback = (int index) => {
				var actions = (target as Interactable).actions;
				var tp = index < actions.Count ? actions[index]?.GetType() : null;
				var h = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
				if (tp == null) return h;
				else if (tp == typeof(Interactable.AnimatorSetBoolAction)) return 4*h;
				else if (tp == typeof(Interactable.AnimatorPlay)) return 4*h;
				else if (tp == typeof(Interactable.ActivateParticleSystem)) return 2*h;
				else if (tp == typeof(Interactable.DelayAction)) return 2*h;
				else if (tp == typeof(Interactable.SetObjectActiveAction)) return 3*h;
				else if (tp == typeof(Interactable.TeleportAgentAction)) return 2*h;
				else if (tp == typeof(Interactable.TeleportAgentOnLinkAction)) return 2*h;
				else if (tp == typeof(Interactable.SetTransformAction)) return 6*h;
				else if (tp == typeof(Interactable.MoveToAction)) return 4*h;
				else if (tp == typeof(Interactable.InstantiatePrefab)) return 3*h;
				else if (tp == typeof(Interactable.InteractAction)) return 2*h;
				else if (tp == typeof(Interactable.CallFunction)) {
					return (3.5f + Mathf.Max(1, (actions[index] as Interactable.CallFunction).function.GetPersistentEventCount())*2.5f) * h;
				} else throw new System.Exception("Unexpected type " + tp);
			};
			actions.drawHeaderCallback = (Rect rect) => {
				EditorGUI.LabelField(rect, "Actions");
			};
			actions.onAddDropdownCallback = (rect, _) => {
				GenericMenu menu = new GenericMenu();
				var interactable = target as Interactable;
				menu.AddItem(new GUIContent("AnimatorSetBool"), false, () => interactable.actions.Add(new Interactable.AnimatorSetBoolAction()));
				menu.AddItem(new GUIContent("AnimatorPlay"), false, () => interactable.actions.Add(new Interactable.AnimatorPlay()));
				menu.AddItem(new GUIContent("ActivateParticleSystem"), false, () => interactable.actions.Add(new Interactable.ActivateParticleSystem()));
				menu.AddItem(new GUIContent("Delay"), false, () => interactable.actions.Add(new Interactable.DelayAction()));
				menu.AddItem(new GUIContent("SetObjectActive"), false, () => interactable.actions.Add(new Interactable.SetObjectActiveAction()));
				menu.AddItem(new GUIContent("TeleportAgent"), false, () => interactable.actions.Add(new Interactable.TeleportAgentAction()));
				if (interactable.TryGetComponent<NodeLink2>(out var _)) {
					menu.AddItem(new GUIContent("Teleport Agent on Off-Mesh Link"), false, () => interactable.actions.Add(new Interactable.TeleportAgentOnLinkAction()));
				}
				menu.AddItem(new GUIContent("SetTransform"), false, () => interactable.actions.Add(new Interactable.SetTransformAction()));
				menu.AddItem(new GUIContent("MoveTo"), false, () => interactable.actions.Add(new Interactable.MoveToAction()));
				menu.AddItem(new GUIContent("InstantiatePrefab"), false, () => interactable.actions.Add(new Interactable.InstantiatePrefab()));
				menu.AddItem(new GUIContent("CallFunction"), false, () => interactable.actions.Add(new Interactable.CallFunction()));
				menu.AddItem(new GUIContent("Interact with other interactable"), false, () => interactable.actions.Add(new Interactable.InteractAction()));
				menu.DropDown(rect);
			};
		}

		protected override void Inspector () {
			var script = target as Interactable;

			script.actions = script.actions ?? new List<Interactable.InteractableAction>();
			actions.DoLayoutList();
		}
	}
}
