using UnityEditor;
using Pathfinding.RVO;

namespace Pathfinding {
	[CustomEditor(typeof(RVONavmesh))]
	public class RVONavmeshEditor : EditorBase {
		protected override void Inspector () {
			EditorGUILayout.HelpBox("This component is deprecated. The RVOSimulator now has an option to take the navmesh into account automatically.", MessageType.Warning);
		}
	}
}
