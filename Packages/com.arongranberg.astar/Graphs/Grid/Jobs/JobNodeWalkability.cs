using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>Calculates for each grid node if it should be walkable or not</summary>
	[BurstCompile(FloatMode = FloatMode.Fast)]
	public struct JobNodeWalkability : IJob {
		/// <summary>
		/// If true, use the normal of the raycast hit to check if the ground is flat enough to stand on.
		///
		/// Any nodes with a steeper slope than <see cref="maxSlope"/> will be made unwalkable.
		/// </summary>
		public bool useRaycastNormal;
		/// <summary>Max slope in degrees</summary>
		public float maxSlope;
		/// <summary>Normalized up direction of the graph</summary>
		public Vector3 up;
		/// <summary>If true, nodes will be made unwalkable if no ground was found under them</summary>
		public bool unwalkableWhenNoGround;
		/// <summary>For layered grid graphs, if there's a node above another node closer than this distance, the lower node will be made unwalkable</summary>
		public float characterHeight;
		/// <summary>Number of nodes in each layer</summary>
		public int layerStride;

		[ReadOnly]
		public NativeArray<float3> nodePositions;

		public NativeArray<float4> nodeNormals;

		[WriteOnly]
		public NativeArray<bool> nodeWalkable;

		public void Execute () {
			// Cosinus of the max slope
			float cosMaxSlopeAngle = math.cos(math.radians(maxSlope));
			float4 upNative = new float4(up.x, up.y, up.z, 0);
			float3 upNative3 = upNative.xyz;

			for (int i = 0; i < nodeNormals.Length; i++) {
				// walkable will be set to false if no ground was found (unless that setting has been disabled)
				// The normal will only be non-zero if something was hit.
				bool didHit = math.any(nodeNormals[i]);
				var walkable = didHit;
				if (!didHit && !unwalkableWhenNoGround && i < layerStride) {
					walkable = true;
					// If there was no hit, but we still want to make the node walkable, then we set the normal to the up direction
					nodeNormals[i] = upNative;
				}

				// Check if the node is on a slope steeper than permitted
				if (walkable && useRaycastNormal && didHit) {
					// Take the dot product to find out the cosine of the angle it has (faster than Vector3.Angle)
					float angle = math.dot(nodeNormals[i], upNative);

					// Check if the ground is flat enough to stand on
					if (angle < cosMaxSlopeAngle) {
						walkable = false;
					}
				}

				// Check if there is a node above this one (layered grid graph only)
				if (walkable && i + layerStride < nodeNormals.Length && math.any(nodeNormals[i + layerStride])) {
					walkable = math.dot(upNative3, nodePositions[i + layerStride] - nodePositions[i]) >= characterHeight;
				}

				nodeWalkable[i] = walkable;
			}
		}
	}
}
