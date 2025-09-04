using UnityEngine;

namespace Pathfinding.Examples {
	/// <summary>
	/// Smooth Camera Following.
	/// \author http://wiki.unity3d.com/index.php/SmoothFollow2
	/// </summary>
	[HelpURL("https://arongranberg.com/astar/documentation/stable/smoothcamerafollow.html")]
	public class SmoothCameraFollow : VersionedMonoBehaviour {
		public Transform target;
		public float distance = 3.0f;
		public float height = 3.0f;
		public float damping = 5.0f;
		public bool enableRotation = true;
		public bool smoothRotation = true;
		public float rotationDamping = 10.0f;
		public bool staticOffset = false;

		void LateUpdate () {
			Vector3 wantedPosition;

			if (staticOffset) {
				wantedPosition = target.position + new Vector3(0, height, distance);
			} else {
				wantedPosition = target.TransformPoint(0, height, -distance);
			}
			transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * damping);

			if (enableRotation) {
				if (smoothRotation) {
					Quaternion wantedRotation = Quaternion.LookRotation(target.position - transform.position, target.up);
					transform.rotation = Quaternion.Slerp(transform.rotation, wantedRotation, Time.deltaTime * rotationDamping);
				} else transform.LookAt(target, target.up);
			}
		}
	}
}
