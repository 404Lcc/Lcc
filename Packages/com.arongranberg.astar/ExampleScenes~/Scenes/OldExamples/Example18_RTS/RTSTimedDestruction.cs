using UnityEngine;
using System.Collections;

namespace Pathfinding.Examples.RTS {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rtstimeddestruction.html")]
	public class RTSTimedDestruction : MonoBehaviour {
		public float time = 1f;

		// Use this for initialization
		IEnumerator Start () {
			yield return new WaitForSeconds(time);
			GameObject.Destroy(gameObject);
		}
	}
}
