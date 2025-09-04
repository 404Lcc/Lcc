using System.Diagnostics;
using UnityEditor;

namespace Pathfinding {
	[FilePath("ProjectSettings/com.arongranberg.astar/settings.asset", FilePathAttribute.Location.ProjectFolder)]
	internal class PathfindingEditorSettings : ScriptableSingleton<PathfindingEditorSettings> {
		public bool hasShownWelcomeScreen = false;

		public void Save () {
			Save(true);
		}
	}
}
