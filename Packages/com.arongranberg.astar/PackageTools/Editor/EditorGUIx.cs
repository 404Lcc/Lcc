using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace Pathfinding {
	/// <summary>Simple GUI utility functions</summary>
	public static class GUIUtilityx {
		static Stack<Color> colors = new Stack<Color>();

		public static void PushTint (Color tint) {
			colors.Push(GUI.color);
			GUI.color *= tint;
		}

		public static void PopTint () {
			GUI.color = colors.Pop();
		}

		public static Rect SliceRow (ref Rect rect, float height) {
			var r = new Rect(rect.x, rect.y, rect.width, height);
			rect.yMin += height + EditorGUIUtility.standardVerticalSpacing;
			return r;
		}

		public static Rect SliceColumn (ref Rect rect, float width, float spacing = 0) {
			var r = new Rect(rect.x, rect.y, width, rect.height);
			rect.xMin += width + spacing;
			return r;
		}
	}

	/// <summary>
	/// Editor helper for hiding and showing a group of GUI elements.
	/// Call order in OnInspectorGUI should be:
	/// - Begin
	/// - Header/HeaderLabel (optional)
	/// - BeginFade
	/// - [your gui elements] (if BeginFade returns true)
	/// - End
	/// </summary>
	public class FadeArea {
		Rect lastRect;
		float value;
		float lastUpdate;
		GUIStyle labelStyle;
		GUIStyle areaStyle;
		bool visible;
		Editor editor;

		/// <summary>
		/// Is this area open.
		/// This is not the same as if any contents are visible, use <see cref="BeginFade"/> for that.
		/// </summary>
		public bool open;

		/// <summary>Animate dropdowns when they open and close</summary>
		public static bool fancyEffects;
		const float animationSpeed = 100f;

		public FadeArea (bool open, Editor editor, GUIStyle areaStyle, GUIStyle labelStyle = null) {
			this.areaStyle = areaStyle;
			this.labelStyle = labelStyle;
			this.editor = editor;
			visible = this.open = open;
			value = open ? 1 : 0;
		}

		void Tick () {
			if (Event.current.type == EventType.Repaint) {
				float deltaTime = Time.realtimeSinceStartup-lastUpdate;

				// Right at the start of a transition the deltaTime will
				// not be reliable, so use a very small value instead
				// until the next repaint
				if (value == 0f || value == 1f) deltaTime = 0.001f;
				deltaTime = Mathf.Clamp(deltaTime, 0.00001F, 0.1F);

				// Larger regions fade slightly slower
				deltaTime /= Mathf.Sqrt(Mathf.Max(lastRect.height, 100));

				lastUpdate = Time.realtimeSinceStartup;


				float targetValue = open ? 1F : 0F;
				if (!Mathf.Approximately(targetValue, value)) {
					value += deltaTime*animationSpeed*Mathf.Sign(targetValue-value);
					value = Mathf.Clamp01(value);
					editor.Repaint();

					if (!fancyEffects) {
						value = targetValue;
					}
				} else {
					value = targetValue;
				}
			}
		}

		public void Begin () {
			if (areaStyle != null) {
				lastRect = EditorGUILayout.BeginVertical(areaStyle);
			} else {
				lastRect = EditorGUILayout.BeginVertical();
			}
		}

		public void HeaderLabel (string label) {
			GUILayout.Label(label, labelStyle);
		}

		public void Header (string label) {
			Header(label, ref open);
		}

		public void Header (string label, ref bool open) {
			if (GUILayout.Button(label, labelStyle)) {
				open = !open;
				editor.Repaint();
			}
			this.open = open;
		}

		/// <summary>Hermite spline interpolation</summary>
		static float Hermite (float start, float end, float value) {
			return Mathf.Lerp(start, end, value * value * (3.0f - 2.0f * value));
		}

		public bool BeginFade () {
			var hermite = Hermite(0, 1, value);

			visible = EditorGUILayout.BeginFadeGroup(hermite);
			GUIUtilityx.PushTint(new Color(1, 1, 1, hermite));
			Tick();

			// Another vertical group is necessary to work around
			// a kink of the BeginFadeGroup implementation which
			// causes the padding to change when value!=0 && value!=1
			EditorGUILayout.BeginVertical();

			return visible;
		}

		public void End () {
			EditorGUILayout.EndVertical();

			if (visible) {
				// Some space that cannot be placed in the GUIStyle unfortunately
				GUILayout.Space(4);
			}

			EditorGUILayout.EndFadeGroup();
			EditorGUILayout.EndVertical();
			GUIUtilityx.PopTint();
		}
	}
	/// <summary>Handles fading effects and also some custom GUI functions such as LayerMaskField</summary>
	public static class EditorGUILayoutx {
		static Dictionary<int, string[]> layerNames = new Dictionary<int, string[]>();
		static long lastUpdateTick;
		static List<string> dummyList = new List<string>();

		/// <summary>Displays a LayerMask field.</summary>
		/// <param name="label">Label to display</param>
		/// <param name="selected">Current LayerMask</param>
		public static LayerMask LayerMaskField (string label, LayerMask selected) {
			if (Event.current.type == EventType.Layout && System.DateTime.UtcNow.Ticks - lastUpdateTick > 10000000L) {
				layerNames.Clear();
				lastUpdateTick = System.DateTime.UtcNow.Ticks;
			}

			string[] currentLayerNames;
			if (!layerNames.TryGetValue(selected.value, out currentLayerNames)) {
				var layers = dummyList;
				layers.Clear();

				int emptyLayers = 0;
				for (int i = 0; i < 32; i++) {
					string layerName = LayerMask.LayerToName(i);

					if (layerName != "") {
						for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer "+(i-emptyLayers));
						layers.Add(layerName);
					} else {
						emptyLayers++;
						if (((selected.value >> i) & 1) != 0 && selected.value != -1) {
							for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer "+(i+1-emptyLayers));
						}
					}
				}

				currentLayerNames = layerNames[selected.value] = layers.ToArray();
			}

			selected.value = EditorGUILayout.MaskField(label, selected.value, currentLayerNames);
			return selected;
		}
	}
}
