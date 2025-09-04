using System;
using UnityEditor;
using UnityEngine;

namespace Pathfinding {
	public class AstarUpdateWindow : EditorWindow {
		static GUIStyle largeStyle;
		static GUIStyle normalStyle;
		Version version;
		string summary;
		bool setReminder;

		public static AstarUpdateWindow Init (Version version, string summary) {
			// Get existing open window or if none, make a new one:
			AstarUpdateWindow window = EditorWindow.GetWindow<AstarUpdateWindow>(true, "", true);

			window.position = new Rect(Screen.currentResolution.width/2 - 300, Mathf.Max(5, Screen.currentResolution.height/3 - 150), 600, 400);
			window.version = version;
			window.summary = summary;
			window.titleContent = new GUIContent("New Version of the A* Pathfinding Project");
			return window;
		}

		public void OnDestroy () {
			if (version != null && !setReminder) {
				Debug.Log("Closed window, reminding again tomorrow");
				EditorPrefs.SetString("AstarRemindUpdateDate", DateTime.UtcNow.AddDays(1).ToString(System.Globalization.CultureInfo.InvariantCulture));
				EditorPrefs.SetString("AstarRemindUpdateVersion", version.ToString());
			}
		}

		void OnGUI () {
			if (largeStyle == null) {
				largeStyle = new GUIStyle(EditorStyles.largeLabel) {
					fontSize = 32,
					alignment = TextAnchor.UpperCenter,
					richText = true
				};

				normalStyle = new GUIStyle(EditorStyles.label) {
					wordWrap = true,
					richText = true
				};
			}

			if (version == null) {
				return;
			}

			GUILayout.Label("New Update Available!", largeStyle);
			GUILayout.Label("There is a new version of the <b>A* Pathfinding Project</b> available for download.\n" +
				"The new version is <b>" + version + "</b> you have <b>" + AstarPath.Version + "</b>\n\n"+
				"<i>Summary:</i>\n"+summary, normalStyle
				);

			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical();

			Color col = GUI.color;
			GUI.backgroundColor *= new Color(0.5f,  1f, 0.5f);
			if (GUILayout.Button("Take me to the download page!", GUILayout.Height(30), GUILayout.MaxWidth(300))) {
				Application.OpenURL(AstarUpdateChecker.GetURL("download"));
			}
			GUI.backgroundColor = col;


			if (GUILayout.Button("What's new? (full changelog)")) {
				Application.OpenURL(AstarUpdateChecker.GetURL("changelog"));
			}

			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Skip this version", GUILayout.MaxWidth(100))) {
				EditorPrefs.SetString("AstarSkipUpToVersion", version.ToString());
				setReminder = true;
				Close();
			}

			if (GUILayout.Button("Remind me later ( 1 week )", GUILayout.MaxWidth(200))) {
				EditorPrefs.SetString("AstarRemindUpdateDate", DateTime.UtcNow.AddDays(7).ToString(System.Globalization.CultureInfo.InvariantCulture));
				EditorPrefs.SetString("AstarRemindUpdateVersion", version.ToString());
				setReminder = true;
				Close();
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
	}
}
