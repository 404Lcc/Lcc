using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Pathfinding.Jobs;
using UnityEngine.Assertions;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Copies 3D arrays with grid data from one shape to another.
	///
	/// Only the data for the nodes that exist in both buffers will be copied.
	///
	/// This essentially is several <see cref="JobCopyRectangle"/> jobs in one (to avoid scheduling overhead).
	/// See that job for more documentation.
	/// </summary>
	[BurstCompile]
	public struct JobCopyBuffers : IJob {
		[ReadOnly]
		[DisableUninitializedReadCheck]
		public GridGraphNodeData input;

		[WriteOnly]
		public GridGraphNodeData output;
		public IntBounds bounds;

		public bool copyPenaltyAndTags;

		public void Execute () {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			if (!input.bounds.Contains(bounds)) throw new System.ArgumentException("Bounds are outside the source buffer");
			if (!output.bounds.Contains(bounds)) throw new System.ArgumentException("Bounds are outside the destination buffer");
#endif
			var inputSlice = new Slice3D(input.bounds, bounds);
			var outputSlice = new Slice3D(output.bounds, bounds);
			// Note: Having a single job that copies all of the buffers avoids a lot of scheduling overhead.
			// We do miss out on parallelization, however for this job it is not that significant.
			JobCopyRectangle<Vector3>.Copy(input.positions, output.positions, inputSlice, outputSlice);
			JobCopyRectangle<float4>.Copy(input.normals, output.normals, inputSlice, outputSlice);
			JobCopyRectangle<ulong>.Copy(input.connections, output.connections, inputSlice, outputSlice);
			if (copyPenaltyAndTags) {
				JobCopyRectangle<uint>.Copy(input.penalties, output.penalties, inputSlice, outputSlice);
				JobCopyRectangle<int>.Copy(input.tags, output.tags, inputSlice, outputSlice);
			}
			JobCopyRectangle<bool>.Copy(input.walkable, output.walkable, inputSlice, outputSlice);
			JobCopyRectangle<bool>.Copy(input.walkableWithErosion, output.walkableWithErosion, inputSlice, outputSlice);
		}
	}
}
