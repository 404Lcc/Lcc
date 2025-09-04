using Pathfinding.Graphs.Navmesh.Voxelization.Burst;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>Convert recast region IDs to the tags that should be applied to the nodes</summary>
	[BurstCompile]
	public struct JobConvertAreasToTags : IJob {
		public NativeList<int> areas;

		public void Execute () {
			unsafe {
				for (int i = 0; i < areas.Length; i++) {
					var area = areas[i];
					// The user supplied IDs start at 1 because 0 is reserved for NotWalkable
					areas[i] = (area & VoxelUtilityBurst.TagReg) != 0 ? (area & VoxelUtilityBurst.TagRegMask) - 1 : 0;
				}
			}
		}
	}
}
