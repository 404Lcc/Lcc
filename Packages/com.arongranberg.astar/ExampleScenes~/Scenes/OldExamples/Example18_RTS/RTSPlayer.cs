using System.Collections.Generic;

namespace Pathfinding.Examples.RTS {
	public class RTSPlayerResources {
		Dictionary<RTSUnit.Type, int> resources = new Dictionary<RTSUnit.Type, int>();

		public int GetResource (RTSUnit.Type type) {
			int value;

			if (!resources.TryGetValue(type, out value)) {
				return 0;
			}
			return value;
		}

		public void AddResource (RTSUnit.Type type, int amount) {
			resources[type] = GetResource(type) + amount;
		}
	}

	public class RTSPlayer {
		public readonly RTSPlayerResources resources = new RTSPlayerResources();
		public int index;

		public bool IsHostile (RTSPlayer other) {
			return other != this && other.index != 0;
		}
	}
}
