using UnityEngine;
namespace Pathfinding {
	using Pathfinding.Util;

	/// <summary>Helper for <see cref="Pathfinding.Examples.LocalSpaceRichAI"/></summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/localspacegraph.html")]
	public class LocalSpaceGraph : VersionedMonoBehaviour {
		Matrix4x4 originalMatrix;
		MutableGraphTransform graphTransform = new MutableGraphTransform(Matrix4x4.identity);
		public GraphTransform transformation { get { return graphTransform; } }

		void Start () {
			originalMatrix = transform.worldToLocalMatrix;
			transform.hasChanged = true;
			Refresh();
		}

		public void Refresh () {
			// Avoid updating the GraphTransform if the object has not moved
			if (transform.hasChanged) {
				graphTransform.SetMatrix(transform.localToWorldMatrix * originalMatrix);
				transform.hasChanged = false;
			}
		}
	}
}
