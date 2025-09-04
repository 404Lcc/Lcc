using UnityEngine;

namespace Pathfinding.Examples {
	[ExecuteInEditMode]
	[HelpURL("https://arongranberg.com/astar/documentation/stable/documentationbutton.html")]
	public class DocumentationButton : MonoBehaviour {
		public string page;

		const string UrlBase = "https://arongranberg.com/astar/docs/";

		GUIContent buttonContent = new GUIContent("Example Scene Documentation");

		void Awake () {
			useGUILayout = false;
		}

#if UNITY_EDITOR
		void OnGUI () {
			if (GUI.Button(new Rect(Screen.width - 250, Screen.height - 60, 240, 50), buttonContent)) {
				Application.OpenURL(UrlBase + page + ".html");
			}
		}
#endif
	}
}
