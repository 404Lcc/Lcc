using UnityEditor;
using UnityEngine;
using Pathfinding;

namespace Pathfinding.Examples {
	[CustomEditor(typeof(LocalSpaceRichAI), true)]
	[CanEditMultipleObjects]
	public class LocalSpaceRichAIEditor : BaseAIEditor {
		protected override void Inspector () {
			Section("Local Movement");
			PropertyField("graph");
			base.Inspector();
		}
	}
}
