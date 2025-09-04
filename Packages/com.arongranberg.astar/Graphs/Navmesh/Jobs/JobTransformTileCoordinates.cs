using Pathfinding.Util;
using Pathfinding.Collections;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>
	/// Transforms vertices from voxel coordinates to tile coordinates.
	///
	/// This essentially constitutes multiplying the vertices by the <see cref="matrix"/>.
	///
	/// Note: The input space is in raw voxel coordinates, the output space is in tile coordinates stored in millimeters (as is typical for the Int3 struct. See <see cref="Int3.Precision"/>).
	/// </summary>
	[BurstCompile(FloatMode = FloatMode.Fast)]
	public struct JobTransformTileCoordinates : IJob {
		public unsafe UnsafeSpan<Int3> vertices;
		public Matrix4x4 matrix;

		public void Execute () {
			unsafe {
				for (uint i = 0; i < vertices.length; i++) {
					// Transform from voxel indices to a proper Int3 coordinate, then convert it to a Vector3 float coordinate
					var p = vertices[i];
					vertices[i] = (Int3)matrix.MultiplyPoint3x4(new Vector3(p.x, p.y, p.z));
				}
			}
		}
	}
}
