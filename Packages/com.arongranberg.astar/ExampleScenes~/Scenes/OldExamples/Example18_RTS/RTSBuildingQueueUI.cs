using UnityEngine;
using UnityEngine.UI;

namespace Pathfinding.Examples.RTS {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rtsbuildingqueueui.html")]
	public class RTSBuildingQueueUI : VersionedMonoBehaviour {
		RTSBuildingBarracks building;
		public GameObject prefab;
		public Vector3 worldOffset;
		public Vector2 screenOffset;
		UIItem item;

		class UIItem : RTSWorldSpaceUI.Item {
			QueItem[] queItems;
			RTSBuildingQueueUI parent;

			public UIItem (Transform tracking, RTSBuildingQueueUI parent) : base(tracking) {
				this.parent = parent;
			}

			struct QueItem {
				public GameObject root;
				public UnityEngine.UI.Image icon;
				public UnityEngine.UI.Image progress;

				public QueItem (Transform root) {
					this.root = root.gameObject;
					icon = root.Find("Mask/Image").GetComponent<Image>();
					var p = root.Find("QueProgress");
					progress = p != null? p.GetComponent<Image>() : null;
				}
			}

			public override void SetUIRoot (UnityEngine.GameObject root) {
				base.SetUIRoot(root);
				queItems = new QueItem[4];
				queItems[0] = new QueItem(root.transform.Find("Que0"));
				queItems[1] = new QueItem(root.transform.Find("Que/Que1"));
				queItems[2] = new QueItem(root.transform.Find("Que/Que2"));
				queItems[3] = new QueItem(root.transform.Find("Que/Que3"));
			}

			public override void Update (UnityEngine.Camera cam) {
				base.Update(cam);
				for (int i = 0; i < queItems.Length; i++) {
					if (i >= parent.building.queue.Count) {
						queItems[i].root.SetActive(false);
					} else {
						queItems[i].root.SetActive(true);
						if (i == 0) {
							queItems[i].progress.fillAmount = parent.building.queueProgressFraction;
						}
						queItems[i].icon.sprite = null; //parent.building.queue[i].prefab.GetComponent;
					}
				}
			}
		}

		protected override void Awake () {
			base.Awake();
			building = GetComponent<RTSBuildingBarracks>();
		}

		void Start () {
			item = new UIItem(transform, this);
			RTSUI.active.worldSpaceUI.Add(item, prefab);
		}

#if UNITY_EDITOR
		void Update () {
			item.worldOffset = worldOffset;
			item.screenOffset = screenOffset;
		}
#endif
	}
}
