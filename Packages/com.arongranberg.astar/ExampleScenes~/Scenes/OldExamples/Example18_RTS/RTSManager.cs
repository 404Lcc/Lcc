using UnityEngine;

namespace Pathfinding.Examples.RTS {
	[HelpURL("https://arongranberg.com/astar/documentation/stable/rtsmanager.html")]
	public class RTSManager : VersionedMonoBehaviour {
		public static RTSManager instance;

		public RTSUnitManager units;

		[UnityEngine.Serialization.FormerlySerializedAs("audio")]
		public RTSAudio audioManager;

		RTSPlayer[] players;

		protected override void Awake () {
			if (instance != null) throw new System.Exception("Multiple RTSManager instances in the scene. You should only have one.");
			instance = this;

			units = new RTSUnitManager();
			units.Awake();

			players = new RTSPlayer[3];
			for (int i = 0; i < players.Length; i++) {
				players[i] = new RTSPlayer();
				players[i].index = i;
			}
		}

		void OnDestroy () {
			units.OnDestroy();
			instance = null;
		}

		public int PlayerCount => players.Length;

		public RTSPlayer GetPlayer (int team) {
			return players[team];
		}
	}
}
