using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

namespace Pathfinding.Examples.RTS {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rtswavespawner.html")]
	public class RTSWaveSpawner : MonoBehaviour {
		public Wave[] waves;

		public Transform target;
		public Text waveCounter;
		public int team = 2;

		[System.Serializable]
		public class Wave {
			public GameObject prefab;
			public Transform spawnPoint;
			public float delay;
			public int count;
			public int health;
		}

		// Use this for initialization
		IEnumerator Start () {
			float lastWave = 0;
			float multiplier = 1;

			for (int it = 0;; it++) {
				for (int i = 0; i < waves.Length; i++) {
					while (true) {
						float remaining = waves[i].delay - (Time.time - lastWave);
						if (remaining <= 0) break;
						waveCounter.text = Mathf.RoundToInt(remaining).ToString();
						yield return null;
					}
					waveCounter.text = "!";

					int from = i;
					while (i + 1 < waves.Length && waves[i+1].delay == 0) i++;

					var toSpawn = waves.Skip(from).Take(i - from + 1).ToArray();
					var names = toSpawn.Select(w => w.spawnPoint.gameObject.name).Distinct().OrderBy(s => s).ToArray();
					var message = "Incoming enemies from ";
					for (int j = 0; j < names.Length; j++) {
						if (j > 0) message += j == names.Length - 1 ? " and " : ", ";
						message += names[j];
					}
					Debug.Log(message);

					foreach (var wave in toSpawn) {
						for (int j = 0; j < wave.count; j++) {
							var rnd = Random.insideUnitSphere * 10;
							rnd.y = 0;
							var go = GameObject.Instantiate(wave.prefab, wave.spawnPoint.position + rnd, wave.spawnPoint.rotation) as GameObject;
							var unit = go.GetComponent<RTSUnit>();
							unit.team = team;
							unit.maxHealth = unit.health = wave.health * multiplier;
							unit.SetDestination(target.position, MovementMode.AttackMove);
						}
					}

					yield return new WaitForSeconds(10);
					lastWave = Time.time;
				}

				multiplier *= 2;
			}
		}

		// Update is called once per frame
		void Update () {
		}
	}
}
