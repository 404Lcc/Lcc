using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Pathfinding.Examples.RTS {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rtsworldspaceui.html")]
	public class RTSWorldSpaceUI : VersionedMonoBehaviour {
		public Camera worldCamera;
		List<Item> items = new List<Item>();


		public class Item {
			public RectTransform transform;
			public Transform tracking;
			public Vector3 worldOffset;
			public Vector2 screenOffset;

			public Item (Transform tracking) {
				this.tracking = tracking;
			}

			public virtual void SetUIRoot (GameObject root) {
				transform = root.GetComponent<RectTransform>();
			}

			public virtual bool valid {
				get {
					return tracking != null;
				}
			}

			public virtual void Update (Camera cam) {
				var p = cam.WorldToScreenPoint(tracking.position + worldOffset) + (Vector3)screenOffset;

				p.z = 0;
				transform.anchoredPosition = p;
			}
		}

		public void Add (Item item, GameObject prefab) {
			var go = GameObject.Instantiate(prefab);

			go.transform.SetParent(transform, false);
			item.SetUIRoot(go);
			items.Add(item);
		}

		void LateUpdate () {
			for (int i = items.Count - 1; i >= 0; i--) {
				if (items[i].valid) {
					items[i].Update(worldCamera);
				} else {
					GameObject.Destroy(items[i].transform.gameObject);
					items.RemoveAt(i);
				}
			}
		}
	}
}
