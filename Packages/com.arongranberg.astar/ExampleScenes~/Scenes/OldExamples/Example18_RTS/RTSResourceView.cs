using UnityEngine;
using UnityEngine.UI;

namespace Pathfinding.Examples.RTS {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rtsresourceview.html")]
	public class RTSResourceView : VersionedMonoBehaviour {
		public float adjustmentSpeed = 10;

		[System.Serializable]
		public class Item {
			public RTSUnit.Type resource;
			public string name;
			public Text label;
			float smoothedValue;

			public void Tick (RTSPlayerResources resources, float adjustmentSpeed) {
				float val = resources.GetResource(resource);
				var diff = Mathf.Abs(val - smoothedValue);
				var dv = Mathf.Min(diff, Mathf.Max(diff * adjustmentSpeed * Time.deltaTime, 10f * adjustmentSpeed * Time.deltaTime));

				smoothedValue += dv * Mathf.Sign(val - smoothedValue);
				label.text = name + ": " + Mathf.Round(smoothedValue).ToString("0");
			}
		}

		public int team;
		public Item[] items;

		void Update () {
			var resources = RTSManager.instance.GetPlayer(team).resources;

			for (int i = 0; i < items.Length; i++) {
				items[i].Tick(resources, adjustmentSpeed);
			}
		}
	}
}
