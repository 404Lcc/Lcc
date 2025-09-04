using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace Pathfinding.Examples {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rtstiltinmovementdirection.html")]
	public class RTSTiltInMovementDirection : MonoBehaviour {
		public Transform target;
		public float amount;
		public float speed;
		public AudioSource motorSound;
		public float soundGain = 1f;
		public float soundPitchGain = 1f;
		public float soundIdleVolume = 0.5f;
		public float soundAdjustmentSpeed = 2;

		[Range(0, 1)]
		public float accelerationFraction = 0.5f;
		IAstarAI ai;
		Vector3 lastVelocity;

		Vector3 smoothAcceleration;

		// Use this for initialization
		void Awake () {
			ai = GetComponent<IAstarAI>();
			if (motorSound != null) {
				motorSound.time = Random.value * motorSound.clip.length;
			}
		}

		// Update is called once per frame
		void LateUpdate () {
			var acc = Vector3.Lerp(ai.velocity, (ai.velocity - lastVelocity)/Time.deltaTime, accelerationFraction);

			lastVelocity = ai.velocity;

			smoothAcceleration = Vector3.Lerp(smoothAcceleration, acc, Time.deltaTime * 10);

			var dir = Vector3.up + smoothAcceleration * amount;
			Debug.DrawRay(target.position, dir, Color.blue);
			var targetRot = Quaternion.LookRotation(dir, -target.forward) * Quaternion.Euler(90, 0, 0);
			target.rotation = Quaternion.Slerp(target.rotation, targetRot, Time.deltaTime * speed);
			if (motorSound != null) {
				motorSound.volume = Mathf.Lerp(motorSound.volume, Mathf.Log(smoothAcceleration.magnitude+1)*soundGain + soundIdleVolume, soundAdjustmentSpeed * Time.deltaTime);
				motorSound.pitch = Mathf.Lerp(motorSound.pitch, Mathf.Log(smoothAcceleration.magnitude+1)*soundPitchGain + 1f, soundAdjustmentSpeed * Time.deltaTime);
			}
		}
	}
}
