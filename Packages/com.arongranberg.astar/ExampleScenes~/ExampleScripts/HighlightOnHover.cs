using UnityEngine;

namespace Pathfinding.Examples {
	/// <summary>Activates a GameObject when the cursor is over this object.</summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/highlightonhover.html")]
	public class HighlightOnHover : VersionedMonoBehaviour {
		public GameObject highlight;

		void Start () {
			highlight.SetActive(false);
		}

		public void OnMouseEnter () {
			highlight.SetActive(true);
		}

		public void OnMouseExit () {
			highlight.SetActive(false);
		}
	}
}
