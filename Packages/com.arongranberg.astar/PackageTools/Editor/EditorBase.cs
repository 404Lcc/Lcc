using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinding {
	/// <summary>Helper for creating editors</summary>
	[CustomEditor(typeof(VersionedMonoBehaviour), true)]
	[CanEditMultipleObjects]
	public class EditorBase : Editor {
		static System.Collections.Generic.Dictionary<string, string> cachedTooltips;
		static System.Collections.Generic.Dictionary<string, string> cachedURLs;
		Dictionary<string, SerializedProperty> props = new Dictionary<string, SerializedProperty>();

		static GUIContent content = new GUIContent();
		static GUIContent showInDocContent = new GUIContent("Show in online documentation", "");
		static GUILayoutOption[] noOptions = new GUILayoutOption[0];
		public static System.Func<string> getDocumentationURL;

		protected HashSet<string> remainingUnhandledProperties;


		static void LoadMeta () {
			if (cachedTooltips == null) {
				var filePath = EditorResourceHelper.editorAssets + "/tooltips.tsv";

				try {
					var lines = System.IO.File.ReadAllLines(filePath).Select(l => l.Split('\t', 3)).Where(l => l.Length == 3).ToArray();
					cachedURLs = lines.ToDictionary(l => l[0], l => l[1]);
					cachedTooltips = lines.ToDictionary(l => l[0], l => l[2].Replace("\\n", "\n"));
				} catch (System.Exception e) {
					Debug.LogWarning("Could not load tooltips from " + filePath + "\n" + e);
					cachedURLs = new System.Collections.Generic.Dictionary<string, string>();
					cachedTooltips = new System.Collections.Generic.Dictionary<string, string>();
				}
			}
		}


		static string LookupPath (System.Type type, string path, Dictionary<string, string> lookupData) {
			// Common case for backing fields of properties
			if (path.EndsWith("Backing")) {
				var basePath = LookupPath(type, path.Substring(0, path.Length - "Backing".Length), lookupData);
				if (basePath != null) return basePath;
			}

			// Find the correct type if the path was not an immediate member of #type
			while (true) {
				var index = path.IndexOf('.');
				if (index == -1) break;
				var fieldName = path.Substring(0, index);
				var remaining = path.Substring(index + 1);
				var field = type.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
				if (field != null) {
					type = field.FieldType;
					path = remaining;
				} else {
					// Could not find the correct field
					return null;
				}
			}

			// Find a documentation entry for the field, fall back to parent classes if necessary
			while (type != null) {
				if (lookupData.TryGetValue(type.FullName + "." + path, out var value)) {
					return value;
				}
				type = type.BaseType;
			}
			return null;
		}

		string FindTooltip (string path) {
			LoadMeta();
			return LookupPath(target.GetType(), path, cachedTooltips);
		}

		protected virtual void OnEnable () {
			foreach (var target in targets) if (target != null) (target as IVersionedMonoBehaviourInternal).UpgradeFromUnityThread();
			EditorApplication.contextualPropertyMenu += OnContextMenu;
		}

		protected virtual void OnDisable () {
			EditorApplication.contextualPropertyMenu -= OnContextMenu;
		}

		void OnContextMenu (GenericMenu menu, SerializedProperty property) {
			if (property.serializedObject != this.serializedObject) return;

			LoadMeta();
			var url = LookupPath(target.GetType(), property.propertyPath, cachedURLs);

			if (url != null && getDocumentationURL != null) {
				menu.AddItem(showInDocContent, false, () => Application.OpenURL(getDocumentationURL() + url));
			}
		}

		public sealed override void OnInspectorGUI () {
			EditorGUI.indentLevel = 0;
			serializedObject.Update();
			try {
				Inspector();
				InspectorForRemainingAttributes(false, true);
			} catch (System.Exception e) {
				// This exception type should never be caught. See https://docs.unity3d.com/ScriptReference/ExitGUIException.html
				if (e is ExitGUIException) throw e;
				Debug.LogException(e, target);
			}
			serializedObject.ApplyModifiedProperties();
			if (targets.Length == 1 && (target as MonoBehaviour).enabled) {
				var attr = target.GetType().GetCustomAttributes(typeof(UniqueComponentAttribute), true);
				for (int i = 0; i < attr.Length; i++) {
					string tag = (attr[i] as UniqueComponentAttribute).tag;
					foreach (var other in (target as MonoBehaviour).GetComponents<MonoBehaviour>()) {
						// Note: other can be null if some scripts are missing references
						if (other == null || !other.enabled || other == target) continue;
						if (other.GetType().GetCustomAttributes(typeof(UniqueComponentAttribute), true).Where(c => (c as UniqueComponentAttribute).tag == tag).Any()) {
							EditorGUILayout.HelpBox("This component and " + other.GetType().Name + " cannot be used at the same time", MessageType.Warning);
						}
					}
				}
			}
		}


		protected virtual void Inspector () {
			InspectorForRemainingAttributes(true, false);
		}

		/// <summary>Draws an inspector for all fields that are likely not handled by the editor script itself</summary>
		protected virtual void InspectorForRemainingAttributes (bool showHandled, bool showUnhandled) {
			if (remainingUnhandledProperties == null) {
				remainingUnhandledProperties = new HashSet<string>();

				var tp = serializedObject.targetObject.GetType();
				var handledAssemblies = new List<System.Reflection.Assembly>();

				// Find all types for which we have a [CustomEditor(type)] attribute.
				// Unity hides this field, so we have to use reflection to get it.
				var customEditorAttrs = this.GetType().GetCustomAttributes(typeof(CustomEditor), true).Cast<CustomEditor>().ToArray();
				foreach (var attr in customEditorAttrs) {
					var inspectedTypeField = attr.GetType().GetField("m_InspectedType", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					var inspectedType = inspectedTypeField.GetValue(attr) as System.Type;
					if (!handledAssemblies.Contains(inspectedType.Assembly)) {
						handledAssemblies.Add(inspectedType.Assembly);
					}
				}
				bool enterChildren = true;
				for (var prop = serializedObject.GetIterator(); prop.NextVisible(enterChildren); enterChildren = false) {
					var name = prop.propertyPath;
					var field = tp.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					if (field == null) {
						// Can happen for some built-in Unity fields. They are not important
						continue;
					} else {
						var declaringType = field.DeclaringType;
						var foundOtherAssembly = false;
						var foundThisAssembly = false;
						while (declaringType != null) {
							if (handledAssemblies.Contains(declaringType.Assembly)) {
								foundThisAssembly = true;
								break;
							} else {
								foundOtherAssembly = true;
							}
							declaringType = declaringType.BaseType;
						}
						if (foundOtherAssembly && foundThisAssembly) {
							// This is a field in a class in a different assembly, which inherits from a class in one of the handled assemblies.
							// That probably means the editor script doesn't explicitly know about that field and we should show it anyway.
							remainingUnhandledProperties.Add(prop.propertyPath);
						}
					}
				}
			}

			// Basically the same as DrawDefaultInspector, but with tooltips
			bool enterChildren2 = true;

			for (var prop = serializedObject.GetIterator(); prop.NextVisible(enterChildren2); enterChildren2 = false) {
				var handled = !remainingUnhandledProperties.Contains(prop.propertyPath);
				if ((showHandled && handled) || (showUnhandled && !handled)) {
					PropertyField(prop.propertyPath);
				}
			}
		}

		protected SerializedProperty FindProperty (string name) {
			if (!props.TryGetValue(name, out SerializedProperty res)) res = props[name] = serializedObject.FindProperty(name);
			if (res == null) throw new System.ArgumentException(name);
			return res;
		}

		protected void Section (string label) {
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
		}

		protected bool SectionEnableable (string label, string enabledProperty) {
			EditorGUILayout.Separator();
			var v = EditorGUILayout.ToggleLeft(label, FindProperty(enabledProperty).boolValue, EditorStyles.boldLabel);
			FindProperty(enabledProperty).boolValue = v;
			return v;
		}

		/// <summary>Bounds field using center/size instead of center/extent</summary>
		protected void BoundsField (string propertyPath) {
			PropertyField(propertyPath + ".m_Center", "Center");
			var extentsProp = FindProperty(propertyPath + ".m_Extent");
			var r = EditorGUILayout.GetControlRect();
			var label = EditorGUI.BeginProperty(r, new GUIContent("Size"), extentsProp);
			extentsProp.vector3Value = 0.5f * EditorGUI.Vector3Field(r, label, extentsProp.vector3Value * 2.0f);
			EditorGUI.EndProperty();
		}

		protected void FloatField (string propertyPath, string label = null, string tooltip = null, float min = float.NegativeInfinity, float max = float.PositiveInfinity) {
			PropertyField(propertyPath, label, tooltip);
			Clamp(propertyPath, min, max);
		}

		protected void FloatField (SerializedProperty prop, string label = null, string tooltip = null, float min = float.NegativeInfinity, float max = float.PositiveInfinity) {
			PropertyField(prop, label, tooltip);
			Clamp(prop, min, max);
		}

		protected bool PropertyField (string propertyPath, string label = null, string tooltip = null) {
			return PropertyField(FindProperty(propertyPath), label, tooltip, propertyPath);
		}

		protected bool PropertyField (SerializedProperty prop, string label = null, string tooltip = null) {
			return PropertyField(prop, label, tooltip, prop.propertyPath);
		}

		bool PropertyField (SerializedProperty prop, string label, string tooltip, string propertyPath) {
			content.text = label ?? prop.displayName;
			content.tooltip = tooltip ?? FindTooltip(propertyPath);
			EditorGUILayout.PropertyField(prop, content, true, noOptions);
			return prop.propertyType == SerializedPropertyType.Boolean ? !prop.hasMultipleDifferentValues && prop.boolValue : true;
		}

		protected void Popup (string propertyPath, GUIContent[] options, string label = null) {
			var prop = FindProperty(propertyPath);

			content.text = label ?? prop.displayName;
			content.tooltip = FindTooltip(propertyPath);
			EditorGUI.BeginChangeCheck();
			var r = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.popup);
			r = EditorGUI.PrefixLabel(r, EditorGUI.BeginProperty(r, content, prop));
			var tmpIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			int indexValue;
			if (prop.propertyType == SerializedPropertyType.Enum) {
				indexValue = prop.enumValueIndex;
			} else if (prop.propertyType == SerializedPropertyType.Integer) {
				indexValue = prop.intValue;
			} else if (prop.propertyType == SerializedPropertyType.Boolean) {
				indexValue = prop.boolValue ? 1 : 0;
			} else {
				throw new System.ArgumentException("Property is not an enum, integer or boolean");
			}
			indexValue = EditorGUI.Popup(r, indexValue, options);
			EditorGUI.indentLevel = tmpIndent;
			if (EditorGUI.EndChangeCheck()) {
				if (prop.propertyType == SerializedPropertyType.Enum) {
					prop.enumValueIndex = indexValue;
				} else if (prop.propertyType == SerializedPropertyType.Integer) {
					prop.intValue = indexValue;
				} else if (prop.propertyType == SerializedPropertyType.Boolean) {
					prop.boolValue = indexValue != 0;
				} else {
					throw new System.ArgumentException("Property is not an enum, integer or boolean");
				}
			}
			EditorGUI.EndProperty();
		}

		protected void IntSlider (string propertyPath, int left, int right) {
			var prop = FindProperty(propertyPath);

			content.text = prop.displayName;
			content.tooltip = FindTooltip(propertyPath);
			EditorGUILayout.IntSlider(prop, left, right, content, noOptions);
		}

		protected void Slider (string propertyPath, float left, float right) {
			var prop = FindProperty(propertyPath);

			content.text = prop.displayName;
			content.tooltip = FindTooltip(propertyPath);
			EditorGUILayout.Slider(prop, left, right, content, noOptions);
		}

		protected bool ByteAsToggle (string propertyPath, string label) {
			var prop = FindProperty(propertyPath);

			content.text = label;
			content.tooltip = FindTooltip(propertyPath);
			EditorGUI.BeginChangeCheck();
			var r = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, EditorStyles.popup);
			r = EditorGUI.PrefixLabel(r, EditorGUI.BeginProperty(r, content, prop));
			var tmpIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			prop.intValue = EditorGUI.Toggle(r, prop.intValue != 0) ? 1 : 0;
			EditorGUI.indentLevel = tmpIndent;
			EditorGUI.EndProperty();
			return prop.intValue != 0;
		}

		protected void Clamp (SerializedProperty prop, float min, float max = float.PositiveInfinity) {
			if (!prop.hasMultipleDifferentValues) prop.floatValue = Mathf.Clamp(prop.floatValue, min, max);
		}

		protected void Clamp (string name, float min, float max = float.PositiveInfinity) {
			Clamp(FindProperty(name), min, max);
		}

		protected void ClampInt (string name, int min, int max = int.MaxValue) {
			var prop = FindProperty(name);

			if (!prop.hasMultipleDifferentValues) prop.intValue = Mathf.Clamp(prop.intValue, min, max);
		}
	}
}
