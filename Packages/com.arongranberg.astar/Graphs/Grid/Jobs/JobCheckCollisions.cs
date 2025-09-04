using UnityEngine;
using Unity.Collections;
using Pathfinding.Jobs;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Checks if nodes are obstructed by obstacles or not.
	///
	/// See: <see cref="GraphCollision"/>
	/// </summary>
	struct JobCheckCollisions : IJobTimeSliced {
		[ReadOnly]
		public NativeArray<Vector3> nodePositions;
		public NativeArray<bool> collisionResult;
		public GraphCollision collision;
		int startIndex;

		public void Execute () {
			Execute(TimeSlice.Infinite);
		}

		public bool Execute (TimeSlice timeSlice) {
			for (int i = startIndex; i < nodePositions.Length; i++) {
				collisionResult[i] = collisionResult[i] && collision.Check(nodePositions[i]);
				if ((i & 127) == 0 && timeSlice.expired) {
					startIndex = i + 1;
					return false;
				}
			}
			return true;
		}
	}
}
