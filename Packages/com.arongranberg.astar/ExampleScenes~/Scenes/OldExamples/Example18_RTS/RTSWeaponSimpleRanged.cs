using UnityEngine;
using System.Collections;

namespace Pathfinding.Examples.RTS {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rtsweaponsimpleranged.html")]
	public class RTSWeaponSimpleRanged : RTSWeapon {
		public GameObject sourceEffect;
		public GameObject targetEffect;

		public Transform sourceEffectRoot;

		public float rotationSpeed;
		public Transform rotationRootY;

		public float damage;
		public AudioClip[] sfx;
		public float volume = 1f;

		public override bool Aim (RTSUnit target) {
			bool rotationDone = true;

			if (rotationRootY != null) {
				var dir = target.transform.position - rotationRootY.position;
				dir.y = 0;

				var targetRot = Quaternion.LookRotation(dir);
				rotationRootY.rotation = Quaternion.RotateTowards(rotationRootY.rotation, targetRot, rotationSpeed * Time.deltaTime);

				rotationDone = Quaternion.Angle(targetRot, rotationRootY.rotation) < rotationSpeed * 0.1f;
			}
			return base.Aim(target) && rotationDone;
		}

		public override void Attack (RTSUnit target) {
			base.Attack(target);
			if (sfx.Length > 0) AudioSource.PlayClipAtPoint(sfx[Random.Range(0, sfx.Length)], transform.position, volume);
			if (sourceEffect != null) GameObject.Instantiate(sourceEffect, sourceEffectRoot != null ? sourceEffectRoot.position : transform.position, sourceEffectRoot != null ? sourceEffectRoot.rotation : Quaternion.LookRotation(target.transform.position - transform.position, Vector3.up));
			if (targetEffect != null) GameObject.Instantiate(targetEffect, target.transform.position, Quaternion.LookRotation(transform.position - target.transform.position, Vector3.up));

			target.ApplyDamage(damage);
		}
	}
}
