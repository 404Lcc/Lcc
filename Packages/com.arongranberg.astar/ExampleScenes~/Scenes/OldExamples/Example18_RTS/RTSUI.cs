using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Pathfinding;
using System.Linq;

namespace Pathfinding.Examples.RTS {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rtsui.html")]
	public class RTSUI : MonoBehaviour {
		public static RTSUI active;
		public RectTransform selectionBox;
		public GameObject menuRoot;
		public GameObject menuItemPrefab;
		public State state;
		public Button clickFallback;
		public GameObject buildingPreview;
		public LayerMask groundMask;

		public AudioClip click;
		public AudioClip notEnoughResources;

		public RTSWorldSpaceUI worldSpaceUI;

		RTSUnitBuilder.BuildingItem buildingInfo;
		int ignoreFrame = -1;

		Menu activeMenu;

		public enum State {
			Normal,
			PlacingBuilding,
			Dragging
		}

		[System.Serializable]
		public class MenuItem {
			public Sprite icon;
			public string label;
			public string description;
		}

		// Use this for initialization
		void Awake () {
			active = this;

			/*foreach (var btn in buildingMenuRoot.GetComponentsInChildren<RTSBuildingButton>()) {
			    btn.gameObject.GetComponent<Button>().onClick.AddListener(() => {
			        StartBuildingPlacement(btn);
			    });
			}*/
		}

		public class Menu {
			GameObject root;
			GameObject itemPrefab;

			public Menu (GameObject root, GameObject itemPrefab) {
				this.root = root;
				this.itemPrefab = itemPrefab;
			}

			public void Hide () {
				if (root != null) {
					for (int i = 0; i < root.transform.childCount; i++) {
						GameObject.Destroy(root.transform.GetChild(i).gameObject);
					}
					root = null;
				}
			}

			public void AddItem (MenuItem item, System.Action callback) {
				var go = GameObject.Instantiate(itemPrefab) as GameObject;

				go.transform.SetParent(root.transform);
				go.GetComponent<Button>().onClick.AddListener(() => callback());
				go.transform.Find("Image/Icon").GetComponent<Image>().sprite = item.icon;
				go.transform.Find("Label").GetComponent<Text>().text = item.label;
				go.transform.Find("Description").GetComponent<Text>().text = item.description;
			}
		}

		public Menu ShowMenu () {
			if (activeMenu != null) {
				activeMenu.Hide();
			}

			activeMenu = new Menu(menuRoot, menuItemPrefab);
			return activeMenu;
		}

		Vector2 dragStart;
		bool hasSelected = false;

		// Update is called once per frame
		void Update () {
			HandleSelection();
			HandleMovement();
			HandleBuildingPlacement();
		}

		void HandleBuildingPlacement () {
			var overUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

			if (state == State.PlacingBuilding) {
				if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.Escape)) {
					RTSManager.instance.audioManager.Play(notEnoughResources);
					AbortBuildingPlacement();
				}

				buildingPreview.transform.position = RaycastScreenPoint(Input.mousePosition, groundMask).point;

				// Ignore the first frame as that will also be a key down event
				if (!overUI && Input.GetKeyDown(KeyCode.Mouse0) && Time.frameCount != ignoreFrame) {
					if (RTSBuildingManager.IsValidBuildingPlacement(buildingPreview)) {
						if (RTSManager.instance.GetPlayer(1).resources.GetResource(RTSUnit.Type.ResourceCrystal) < buildingInfo.cost) {
							RTSManager.instance.audioManager.Play(notEnoughResources);
							Debug.LogError("Not enouch resources");
						} else {
							RTSManager.instance.audioManager.Play(click);
							RTSManager.instance.GetPlayer(1).resources.AddResource(RTSUnit.Type.ResourceCrystal, -buildingInfo.cost);
							GameObject.Instantiate(buildingInfo.prefab, buildingPreview.transform.position, buildingPreview.transform.rotation);
						}
						GameObject.Destroy(buildingPreview);
						state = State.Normal;
					} else {
						// Building placement failed
					}
				}
			}
		}

		public void StartBuildingPlacement (RTSUnitBuilder.BuildingItem buildingInfo) {
			if (state == State.PlacingBuilding) {
				GameObject.Destroy(buildingPreview);
				state = State.Normal;
			}

			if (RTSManager.instance.GetPlayer(1).resources.GetResource(RTSUnit.Type.ResourceCrystal) < buildingInfo.cost) {
				Debug.LogError("Not enouch resources");
				RTSManager.instance.audioManager.Play(notEnoughResources);
				return;
			}

			this.buildingInfo = buildingInfo;
			buildingPreview = GameObject.Instantiate(buildingInfo.prefab) as GameObject;
			ignoreFrame = Time.frameCount;
			state = State.PlacingBuilding;

			// Disable all navmesh cuts and colliders on the preview
			foreach (var cut in buildingPreview.GetComponentsInChildren<NavmeshCut>()) {
				cut.enabled = false;
			}
			foreach (var coll in buildingPreview.GetComponentsInChildren<Collider>()) {
				coll.enabled = false;
			}
		}

		void AbortBuildingPlacement () {
			if (buildingPreview != null) {
				GameObject.Destroy(buildingPreview);
				state = State.Normal;
			}
		}

		void HandleMovement () {
			if (state == State.Normal && Input.GetKeyDown(KeyCode.Mouse1)) {
				RTSManager.instance.audioManager.Play(click);
				RTSManager.instance.units.MoveGroupTo(RTSManager.instance.units.selectedUnits, RaycastScreenPoint(Input.mousePosition, groundMask).point, true, Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.A) ? MovementMode.AttackMove : MovementMode.Move);
			}
		}

		void HandleSelection () {
			var overUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

			if (state != State.Normal && state != State.Dragging) return;

			if (!overUI && Input.GetKeyDown(KeyCode.Mouse0)) {
				dragStart = Input.mousePosition;
				state = State.Dragging;
				hasSelected = false;
			}

			if (state == State.Dragging) {
				Vector2 dragEnd = Input.mousePosition;
				var mn = Vector2.Min(dragStart, dragEnd);
				var mx = Vector2.Max(dragStart, dragEnd);
				var rect = Rect.MinMaxRect(mn.x, mn.y, mx.x, mx.y);

				bool isSelecting = (dragEnd - dragStart).magnitude > 5;
				hasSelected |= isSelecting;
				selectionBox.gameObject.SetActive(isSelecting);

				if (isSelecting) {
					selectionBox.offsetMin = Vector2.Min(dragStart, dragEnd);
					selectionBox.offsetMax = Vector2.Max(dragStart, dragEnd);
				}

				if (!Input.GetKey(KeyCode.Mouse0)) {
					state = State.Normal;
					selectionBox.gameObject.SetActive(false);

					if (isSelecting) {
						RTSManager.instance.units.SetSelection(unit => {
							var screenPos = RTSManager.instance.units.cam.WorldToScreenPoint(unit.transform.position);
							return rect.Contains(screenPos);
						});
					} else {
						// Clicking
						var hit = RaycastScreenPoint(dragEnd, -1);
						var p = hit.point;
						float clickFuzzDistance = 0.1f;
						float minDist = 0;
						RTSUnit minUnit = hit.collider != null? hit.collider.gameObject.GetComponentInParent<RTSUnit>() : null;
						if (minUnit == null) {
							foreach (var unit in RTSManager.instance.units.units) {
								var dir = unit.transform.position - p;
								dir.y = 0;
								var dist = dir.magnitude - unit.radius - clickFuzzDistance;
								if (dist < minDist) {
									minDist = dist;
									minUnit = unit;
								}
							}
						}

						RTSManager.instance.units.SetSelection(u => u == minUnit);
					}
				} else if (hasSelected) {
					foreach (var unit in RTSManager.instance.units.units) {
						var screenPos = RTSManager.instance.units.cam.WorldToScreenPoint(unit.transform.position);
						unit.selectionIndicatorEnabled = rect.Contains(screenPos);
					}
				}
			}
		}

		RaycastHit RaycastScreenPoint (Vector2 mousePosition, LayerMask mask) {
			var ray = RTSManager.instance.units.cam.ScreenPointToRay(mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, float.PositiveInfinity, mask)) {
				Debug.DrawRay(hit.point, Vector3.up, Color.red, 2);
				return hit;
			} else {
				throw new System.Exception("Mouse raycast did not hit anything");
			}
		}
	}
}
