using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding {
	[ExecuteInEditMode]
	/// <summary>
	/// Helper class to keep track of references to GameObjects.
	/// Does nothing more than to hold a GUID value.
	/// </summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/unityreferencehelper.html")]
	public class UnityReferenceHelper : MonoBehaviour {
		[HideInInspector]
		[SerializeField]
		private string guid;

		public string GetGUID() => guid;

		public void Awake () {
			Reset();
		}

		public void Reset () {
			if (string.IsNullOrEmpty(guid)) {
				guid = Pathfinding.Util.Guid.NewGuid().ToString();
				Debug.Log("Created new GUID - " + guid, this);
			} else if (gameObject.scene.name != null) {
				// Create a new GUID if there are duplicates in the scene.
				// Don't do this if this is a prefab (scene.name == null)
				foreach (UnityReferenceHelper urh in UnityCompatibility.FindObjectsByTypeUnsorted<UnityReferenceHelper>()) {
					if (urh != this && guid == urh.guid) {
						guid = Pathfinding.Util.Guid.NewGuid().ToString();
						Debug.Log("Created new GUID - " + guid, this);
						return;
					}
				}
			}
		}
	}
}
