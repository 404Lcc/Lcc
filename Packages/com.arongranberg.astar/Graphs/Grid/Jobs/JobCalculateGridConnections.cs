using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Pathfinding.Jobs;
using Pathfinding.Util;
using Pathfinding.Collections;

namespace Pathfinding.Graphs.Grid.Jobs {
	/// <summary>
	/// Calculates the grid connections for all nodes.
	///
	/// This is a IJobParallelForBatch job. Calculating the connections in multiple threads is faster,
	/// but due to hyperthreading (used on most intel processors) the individual threads will become slower.
	/// It is still worth it though.
	/// </summary>
	[BurstCompile(FloatMode = FloatMode.Fast, CompileSynchronously = true)]
	public struct JobCalculateGridConnections : IJobParallelForBatched {
		public float maxStepHeight;
		public float4x4 graphToWorld;
		public IntBounds bounds;
		public int3 arrayBounds;
		public NumNeighbours neighbours;
		public float characterHeight;
		public bool use2D;
		public bool cutCorners;
		public bool maxStepUsesSlope;
		public bool layeredDataLayout;

		[ReadOnly]
		public UnsafeSpan<bool> nodeWalkable;

		[ReadOnly]
		public UnsafeSpan<float4> nodeNormals;

		[ReadOnly]
		public UnsafeSpan<Vector3> nodePositions;

		/// <summary>All bitpacked node connections</summary>
		[WriteOnly]
		public UnsafeSpan<ulong> nodeConnections;

		public bool allowBoundsChecks => false;

		public static bool IsValidConnection (float y, float y2, float maxStepHeight) {
			return math.abs(y - y2) <= maxStepHeight;
		}

		public static bool IsValidConnection (float2 yRange, float2 yRange2, float maxStepHeight, float characterHeight) {
			if (!IsValidConnection(yRange.x, yRange2.x, maxStepHeight)) return false;

			// Find the overlap between the two spans to check if the character could pass through the vertical gap
			float bottom = math.max(yRange.x, yRange2.x);
			float top = math.min(yRange.y, yRange2.y);
			return top-bottom >= characterHeight;
		}

		static float ConnectionY (UnsafeSpan<float3> nodePositions, UnsafeSpan<float4> nodeNormals, NativeArray<float4> normalToHeightOffset, int nodeIndex, int dir, float4 up, bool reverse) {
			Unity.Burst.CompilerServices.Hint.Assume(nodeIndex >= 0 && nodeIndex < nodePositions.length);
			Unity.Burst.CompilerServices.Hint.Assume(nodeIndex >= 0 && nodeIndex < nodeNormals.length);
			Unity.Burst.CompilerServices.Hint.Assume(dir >= 0 && dir < normalToHeightOffset.Length);
			float4 pos = new float4(nodePositions[(uint)nodeIndex], 0);
			return math.dot(up, pos) + (reverse ? -1 : 1) * math.dot(nodeNormals[nodeIndex], normalToHeightOffset[dir]);
		}

		static float2 ConnectionYRange (UnsafeSpan<float3> nodePositions, UnsafeSpan<float4> nodeNormals, NativeArray<float4> normalToHeightOffset, int nodeIndex, int layerStride, int y, int maxY, int dir, float4 up, bool reverse) {
			var floor = ConnectionY(nodePositions, nodeNormals, normalToHeightOffset, nodeIndex, dir, up, reverse);

			float ceiling;
			var aboveNodeIndex = nodeIndex + layerStride;
			if ((uint)aboveNodeIndex < nodeNormals.length && math.any(nodeNormals[(uint)aboveNodeIndex])) {
				ceiling = ConnectionY(nodePositions, nodeNormals, normalToHeightOffset, aboveNodeIndex, dir, up, reverse);
			} else {
				ceiling = float.PositiveInfinity;
			}
			return new float2(floor, ceiling);
		}

		static NativeArray<float4> HeightOffsetProjections (float4x4 graphToWorldTranform, bool maxStepUsesSlope) {
			var normalToHeightOffset = new NativeArray<float4>(8, Allocator.Temp, NativeArrayOptions.ClearMemory);
			if (maxStepUsesSlope) {
				for (int dir = 0; dir < normalToHeightOffset.Length; dir++) {
					//
					//      |\
					//      | \
					//    H |  \       1  _ N = Normal
					//      |   \      _/ α |
					//      |  α \  _/      |
					// D<---|-----x ---------
					//        1/2  \
					//              \
					//               .
					//
					// Assume we have a node at x viewed from the side, with a surface normal N.
					// We want to find the height difference (H) between the node, and the point where it touches the adjacent node.
					//
					// This can be calculated as
					// H = tan(α) * 1/2
					//
					// We can approximate this for small angles as:
					// H = tan(α) * 1/2 ≈ sin(α) * 1/2 = N.x * 1/2
					//
					// This approximation is also desirable, because it doesn't allow extremely sloped nodes to connect to nodes arbitrarily far up or down.
					//
					// To calculate N.x, we need to take into account that the whole graph can be rotated, and it is also in 3D space.
					// Instead we calculate it as N.x = -N . D (where . is the dot product, and D = flatDir is the direction to the adjacent node along the ground plane).
					var flatDir = GridGraph.neighbourXOffsets[dir] * graphToWorldTranform.c0.xyz + GridGraph.neighbourZOffsets[dir] * graphToWorldTranform.c2.xyz;

					// Lastly, we create a linear transform that maps any node normal to H for a given direction
					// dot(normal, normalToHeightOffset[dir]) = H
					normalToHeightOffset[dir] = -new float4(flatDir, 0) * 0.5f;
				}
			}
			return normalToHeightOffset;
		}

		public void Execute (int start, int count) {
			if (nodePositions.Length != nodeNormals.Length) throw new System.Exception("nodePositions and nodeNormals must have the same length");
			if (nodePositions.Length != nodeWalkable.Length) throw new System.Exception("nodePositions and nodeWalkable must have the same length");
			if (nodePositions.Length != nodeConnections.Length) throw new System.Exception("nodePositions and nodeConnections must have the same length");
			if (layeredDataLayout) ExecuteLayered(start, count);
			else ExecuteFlat(start, count);
		}

		public void ExecuteFlat (int start, int count) {
			if (maxStepHeight <= 0 || use2D) maxStepHeight = float.PositiveInfinity;

			float4 up = graphToWorld.c1;

			NativeArray<int> neighbourOffsets = new NativeArray<int>(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < 8; i++) neighbourOffsets[i] = GridGraph.neighbourZOffsets[i] * arrayBounds.x + GridGraph.neighbourXOffsets[i];
			var nodePositions = this.nodePositions.Reinterpret<float3>();
			var normalToHeightOffset = HeightOffsetProjections(graphToWorld, maxStepUsesSlope);

			// The loop is parallelized over z coordinates
			start += bounds.min.z;
			for (int z = start; z < start + count; z++) {
				var initialConnections = 0xFF;

				// Disable connections to out-of-bounds nodes
				// See GridNode.HasConnectionInDirection
				if (z == 0) initialConnections &= ~((1 << 0) | (1 << 7) | (1 << 4));
				if (z == arrayBounds.z - 1) initialConnections &= ~((1 << 2) | (1 << 5) | (1 << 6));

				for (int x = bounds.min.x; x < bounds.max.x; x++) {
					int nodeIndex = z * arrayBounds.x + x;
					if (!nodeWalkable[nodeIndex]) {
						nodeConnections[nodeIndex] = 0;
						continue;
					}

					// Bitpacked connections
					// bit 0 is set if connection 0 is enabled
					// bit 1 is set if connection 1 is enabled etc.
					int conns = initialConnections;

					// Disable connections to out-of-bounds nodes
					if (x == 0) conns &= ~((1 << 3) | (1 << 6) | (1 << 7));
					if (x == arrayBounds.x - 1) conns &= ~((1 << 1) | (1 << 4) | (1 << 5));

					for (int dir = 0; dir < 8; dir++) {
						float y = ConnectionY(nodePositions, nodeNormals, normalToHeightOffset, nodeIndex, dir, up, false);
						int neighbourIndex = nodeIndex + neighbourOffsets[dir];
						if ((conns & (1 << dir)) != 0) {
							float y2 = ConnectionY(nodePositions, nodeNormals, normalToHeightOffset, neighbourIndex, dir, up, true);
							if (!nodeWalkable[neighbourIndex] || !IsValidConnection(y, y2, maxStepHeight)) {
								// Disable connection
								conns &= ~(1 << dir);
							}
						}
					}

					nodeConnections[nodeIndex] = (ulong)GridNode.FilterDiagonalConnections(conns, neighbours, cutCorners);
				}
			}
		}

		public void ExecuteLayered (int start, int count) {
			if (maxStepHeight <= 0 || use2D) maxStepHeight = float.PositiveInfinity;

			float4 up = graphToWorld.c1;

			NativeArray<int> neighbourOffsets = new NativeArray<int>(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < 8; i++) neighbourOffsets[i] = GridGraph.neighbourZOffsets[i] * arrayBounds.x + GridGraph.neighbourXOffsets[i];
			var nodePositions = this.nodePositions.Reinterpret<float3>();
			var normalToHeightOffset = HeightOffsetProjections(graphToWorld, maxStepUsesSlope);

			var layerStride = arrayBounds.z*arrayBounds.x;
			start += bounds.min.z;
			for (int y = bounds.min.y; y < bounds.max.y; y++) {
				// The loop is parallelized over z coordinates
				for (int z = start; z < start + count; z++) {
					for (int x = bounds.min.x; x < bounds.max.x; x++) {
						// Bitpacked connections
						ulong conns = 0;
						int nodeIndexXZ = z * arrayBounds.x + x;
						int nodeIndex = nodeIndexXZ + y * layerStride;

						if (nodeWalkable[nodeIndex]) {
							for (int dir = 0; dir < 8; dir++) {
								int nx = x + GridGraph.neighbourXOffsets[dir];
								int nz = z + GridGraph.neighbourZOffsets[dir];

								int conn = LevelGridNode.NoConnection;
								if (nx >= 0 && nz >= 0 && nx < arrayBounds.x && nz < arrayBounds.z) {
									float2 yRange = ConnectionYRange(nodePositions, nodeNormals, normalToHeightOffset, nodeIndex, layerStride, y, arrayBounds.y, dir, up, false);

									int neighbourStartIndex = nodeIndexXZ + neighbourOffsets[dir];
									for (int y2 = 0; y2 < arrayBounds.y; y2++) {
										var neighbourIndex = neighbourStartIndex + y2 * layerStride;
										if (!nodeWalkable[neighbourIndex]) continue;

										float2 yRange2 = ConnectionYRange(nodePositions, nodeNormals, normalToHeightOffset, neighbourIndex, layerStride, y2, arrayBounds.y, dir, up, true);
										if (IsValidConnection(yRange, yRange2, maxStepHeight, characterHeight)) {
											conn = y2;
											break;
										}
									}
								}

								conns |= (ulong)conn << LevelGridNode.ConnectionStride*dir;
							}
						} else {
							conns = LevelGridNode.AllConnectionsMask;
						}

						nodeConnections[nodeIndex] = conns;
					}
				}
			}
		}
	}
}
