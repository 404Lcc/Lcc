using UnityEngine;

namespace Pathfinding.Examples {
	/// <summary>Small sample script for placing obstacles</summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/objectplacer.html")]
	public class ObjectPlacer : MonoBehaviour {
		/// <summary>
		/// GameObject to place.
		/// When using a Grid Graph you need to make sure the object's layer is included in the collision mask in the GridGraph settings.
		/// </summary>
		public GameObject go;

		/// <summary>Flush Graph Updates directly after placing. Slower, but updates are applied immidiately</summary>
		public bool direct = false;

		/// <summary>Issue a graph update object after placement</summary>
		public bool issueGUOs = true;

		/// <summary>Align created objects to the surface normal where it is created</summary>
		public bool alignToSurface = false;

		/// <summary>Global offset of the placed object relative to the mouse cursor</summary>
		public Vector3 offset;

		/// <summary>Randomize rotation of the placed object</summary>
		public bool randomizeRotation = false;

		float lastPlacedTime;

		/// <summary>Update is called once per frame</summary>
		void Update () {
			// Check if P is being pressed.
			// Don't place objects if ctrl is pressed to avoid conflicts with the pause shortcut (ctrl+shift+P)
			if (!Input.GetKey(KeyCode.LeftControl) && (Input.GetKeyDown("p") || (Input.GetKey("p") && Time.time - lastPlacedTime > 0.3f))) {
				PlaceObject();
			}

			if (Input.GetKeyDown("r")) {
				RemoveObject();
			}
		}

		public void PlaceObject () {
			lastPlacedTime = Time.time;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			// Figure out where the ground is
			if (Physics.Raycast(ray, out var hit, Mathf.Infinity, ~0)) {
				Vector3 p = hit.point + offset;
				var rot = Quaternion.identity;
				if (alignToSurface) rot = Quaternion.LookRotation(hit.normal, Vector3.right) * Quaternion.Euler(90, 0, 0);
				if (randomizeRotation) rot = Random.rotation;
				GameObject obj = GameObject.Instantiate(go, p, rot) as GameObject;

				if (issueGUOs) {
					Bounds b = obj.GetComponent<Collider>().bounds;
					GraphUpdateObject guo = new GraphUpdateObject(b);
					AstarPath.active.UpdateGraphs(guo);
					if (direct) {
						AstarPath.active.FlushGraphUpdates();
					}
				}
			}
		}

		public void RemoveObject () {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			// Check what object is under the mouse cursor
			if (Physics.Raycast(ray, out var hit, Mathf.Infinity)) {
				// Ignore ground and triggers
				if (hit.collider.isTrigger || hit.transform.gameObject.name == "Ground") return;

				Bounds b = hit.collider.bounds;
				Destroy(hit.collider);
				Destroy(hit.collider.gameObject);

				if (issueGUOs) {
					GraphUpdateObject guo = new GraphUpdateObject(b);
					AstarPath.active.UpdateGraphs(guo);
					if (direct) {
						AstarPath.active.FlushGraphUpdates();
					}
				}
			}
		}
	}
}
