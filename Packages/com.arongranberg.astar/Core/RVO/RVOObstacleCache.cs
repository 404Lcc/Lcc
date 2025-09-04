namespace Pathfinding.RVO {
	using Pathfinding;
	using UnityEngine;
	using Pathfinding.Util;
	using Unity.Mathematics;
	using Unity.Collections;
	using Pathfinding.Collections;
	using System.Collections.Generic;
	using Unity.Burst;
	using Unity.Profiling;
	using Pathfinding.Sync;
#if MODULE_COLLECTIONS_2_1_0_OR_NEWER
	using NativeHashMapIntInt = Unity.Collections.NativeHashMap<int, int>;
#else
	using NativeHashMapIntInt = Unity.Collections.NativeParallelHashMap<int, int>;
#endif

	[BurstCompile]
	public static class RVOObstacleCache {
		public struct ObstacleSegment {
			public float3 vertex1;
			public float3 vertex2;
			public int vertex1LinkId;
			public int vertex2LinkId;
		}

		static ulong HashKey (GraphNode sourceNode, int traversableTags, SimpleMovementPlane movementPlane) {
			var hash = (ulong)sourceNode.NodeIndex;
			hash = hash * 786433 ^ (ulong)traversableTags;
			// The rotation is not particularly important for the obstacle. It's only used
			// to simplify the output a bit. So we allow similar rotations to share the same hash.
			const float RotationQuantization = 4;
			hash = hash * 786433 ^ (ulong)(movementPlane.rotation.x*RotationQuantization);
			hash = hash * 786433 ^ (ulong)(movementPlane.rotation.y*RotationQuantization);
			hash = hash * 786433 ^ (ulong)(movementPlane.rotation.z*RotationQuantization);
			hash = hash * 786433 ^ (ulong)(movementPlane.rotation.w*RotationQuantization);
			return hash;
		}

		/// <summary>
		/// Collects an unordered list of contour segments based on the given nodes.
		///
		/// Note: All nodes must be from the same graph.
		/// </summary>
		public static void CollectContours (List<GraphNode> nodes, NativeList<ObstacleSegment> obstacles) {
			if (nodes.Count == 0) return;
			if (nodes[0] is TriangleMeshNode) {
				for (int i = 0; i < nodes.Count; i++) {
					var tnode = nodes[i] as TriangleMeshNode;
					var used = 0;
					if (tnode.connections != null) {
						for (int j = 0; j < tnode.connections.Length; j++) {
							var conn = tnode.connections[j];
							if (conn.isEdgeShared) {
								used |= 1 << conn.shapeEdge;
							}
						}
					}

					tnode.GetVertices(out var v0, out var v1, out var v2);
					for (int edgeIndex = 0; edgeIndex < 3; edgeIndex++) {
						if ((used & (1 << edgeIndex)) == 0) {
							// This edge is not shared, therefore it's a contour edge
							Int3 leftVertex, rightVertex;
							switch (edgeIndex) {
							case 0:
								leftVertex = v0;
								rightVertex = v1;
								break;
							case 1:
								leftVertex = v1;
								rightVertex = v2;
								break;
							case 2:
							default:
								leftVertex = v2;
								rightVertex = v0;
								break;
							}
							var leftVertexHash = leftVertex.GetHashCode();
							var rightVertexHash = rightVertex.GetHashCode();

							obstacles.Add(new ObstacleSegment {
								vertex1 = (Vector3)leftVertex,
								vertex2 = (Vector3)rightVertex,
								vertex1LinkId = leftVertexHash,
								vertex2LinkId = rightVertexHash,
							});
						}
					}
				}
			} else if (nodes[0] is GridNodeBase) {
				GridGraph graph;
				if (nodes[0] is LevelGridNode) graph = LevelGridNode.GetGridGraph(nodes[0].GraphIndex);
				else
					graph = GridNode.GetGridGraph(nodes[0].GraphIndex);
				unsafe {
					// Offsets from the center of the node to the corners of the node, in world space
					// Index dir is the offset to the left corner of the edge in direction dir
					// See GridNodeBase.GetNeighbourAlongDirection for the direction indices
					Vector3* offsets = stackalloc Vector3[4];
					for (int dir = 0; dir < 4; dir++) {
						var dl = (dir + 1) % 4;
						offsets[dir] = graph.transform.TransformVector(0.5f * new Vector3(GridGraph.neighbourXOffsets[dir] + GridGraph.neighbourXOffsets[dl], 0, GridGraph.neighbourZOffsets[dir] + GridGraph.neighbourZOffsets[dl]));
					}

					for (int i = 0; i < nodes.Count; i++) {
						var gnode = nodes[i] as GridNodeBase;
						if (gnode.HasConnectionsToAllAxisAlignedNeighbours) continue;

						for (int dir = 0; dir < 4; dir++) {
							if (!gnode.HasConnectionInDirection(dir)) {
								// ┌─────────┬─────────┐
								// │         │         │
								// │   nl1   │   nl2   │     ^
								// │         │         │     |
								// ├────────vL─────────┤     dl
								// │         │#########│
								// │   node  │#########│     dir->
								// │         │#########│
								// ├────────vR─────────┤     dr
								// │         │         │      |
								// │   nr1   │   nr2   │      v
								// │         │         │
								// └─────────┴─────────┘
								var dl = (dir + 1) % 4;
								var dr = (dir - 1 + 4) % 4;
								var nl1 = gnode.GetNeighbourAlongDirection(dl);
								var nr1 = gnode.GetNeighbourAlongDirection(dr);

								// All this hashing code looks slow, but really it's not compared to all the memory accesses to get the node data
								uint leftVertexHash;
								if (nl1 != null) {
									var nl2 = nl1.GetNeighbourAlongDirection(dir);
									if (nl2 != null) {
										// Outer corner. Uniquely determined by the 3 nodes that touch the corner.
										var a = gnode.NodeIndex;
										var b = nl1.NodeIndex;
										var c = nl2.NodeIndex;
										// Sort the values so that a <= b <= c
										if (a > b) Memory.Swap(ref a, ref b);
										if (b > c) Memory.Swap(ref b, ref c);
										if (a > b) Memory.Swap(ref a, ref b);
										leftVertexHash = math.hash(new uint3(a, b, c));
									} else {
										// Straight wall. Uniquely determined by the 2 nodes that touch the corner and the direction to the wall.
										var a = gnode.NodeIndex;
										var b = nl1.NodeIndex;
										// Sort the values so that a <= b
										if (a > b) Memory.Swap(ref a, ref b);
										leftVertexHash = math.hash(new uint3(a, b, (uint)dir));
									}
								} else {
									// Inner corner. Uniquely determined by the single node that touches the corner and the direction to it.
									var diagonalToCorner = dir + 4;
									leftVertexHash = math.hash(new uint2(gnode.NodeIndex, (uint)diagonalToCorner));
								}

								uint rightVertexHash;
								if (nr1 != null) {
									var nr2 = nr1.GetNeighbourAlongDirection(dir);
									if (nr2 != null) {
										// Outer corner. Uniquely determined by the 3 nodes that touch the corner.
										var a = gnode.NodeIndex;
										var b = nr1.NodeIndex;
										var c = nr2.NodeIndex;
										// Sort the values so that a <= b <= c
										if (a > b) Memory.Swap(ref a, ref b);
										if (b > c) Memory.Swap(ref b, ref c);
										if (a > b) Memory.Swap(ref a, ref b);
										rightVertexHash = math.hash(new uint3(a, b, c));
									} else {
										// Straight wall. Uniquely determined by the 2 nodes that touch the corner and the direction to the wall.
										var a = gnode.NodeIndex;
										var b = nr1.NodeIndex;
										// Sort the values so that a <= b
										if (a > b) Memory.Swap(ref a, ref b);
										rightVertexHash = math.hash(new uint3(a, b, (uint)dir));
									}
								} else {
									// Inner corner. Uniquely determined by the single node that touches the corner and the direction to it.
									// Note: It's not a typo that we use `dr+4` here and `dir+4` above. They are different directions.
									var diagonalToCorner = dr + 4;
									rightVertexHash = math.hash(new uint2(gnode.NodeIndex, (uint)diagonalToCorner));
								}

								var nodePos = (Vector3)gnode.position;
								obstacles.Add(new ObstacleSegment {
									vertex1 = nodePos + offsets[dir], // Left corner. Yes, it should be dir, not dl, as the offsets array already points to the left corners of each segment.
									vertex2 = nodePos + offsets[dr], // Right corner
									vertex1LinkId = (int)leftVertexHash,
									vertex2LinkId = (int)rightVertexHash,
								});
							}
						}
					}
				}
			}
		}

		private static readonly ProfilerMarker MarkerAllocate = new ProfilerMarker("Allocate");

		/// <summary>Trace contours generated by CollectContours.</summary>
		/// <param name="obstaclesSpan">Obstacle segments, typically the borders of the navmesh. In no particular order.
		///                  Each edge must be oriented the same way (e.g. all clockwise, or all counter-clockwise around the obstacles).</param>
		/// <param name="movementPlane">The movement plane used for simplification. The up direction will be treated as less important for purposes of simplification.</param>
		/// <param name="obstacleId">The ID of the obstacle to write into the outputObstacles array.</param>
		/// <param name="outputObstacles">Array to write the obstacle to.</param>
		/// <param name="verticesAllocator">Allocator to use for the vertices of the obstacle.</param>
		/// <param name="obstaclesAllocator">Allocator to use for the obstacle metadata.</param>
		/// <param name="spinLock">Lock to use when allocating from the allocators.</param>
		/// <param name="simplifyObstacles">If true, the obstacle will be simplified. This means that colinear vertices (when projected onto the movement plane) will be removed.</param>
		[BurstCompile]
		internal static unsafe void TraceContours (ref UnsafeSpan<ObstacleSegment> obstaclesSpan, ref NativeMovementPlane movementPlane, int obstacleId, UnmanagedObstacle* outputObstacles, ref SlabAllocator<float3> verticesAllocator, ref SlabAllocator<ObstacleVertexGroup> obstaclesAllocator, ref SpinLock spinLock, bool simplifyObstacles) {
			var obstacles = obstaclesSpan;
			if (obstacles.Length == 0) {
				outputObstacles[obstacleId] = new UnmanagedObstacle {
					verticesAllocation = SlabAllocator<float3>.ZeroLengthArray,
					groupsAllocation = SlabAllocator<ObstacleVertexGroup>.ZeroLengthArray,
				};
				return;
			}

			MarkerAllocate.Begin();
			var traceLookup = new NativeHashMapIntInt(obstacles.Length, Unity.Collections.Allocator.Temp);
			// For each edge: Will be 0 if the segment should be ignored or if it has been visited, 1 if it has not been visited and has an ingoing edge, and 2 if it has not been visited and has no ingoing edge.
			var priority = new NativeArray<byte>(obstacles.Length, Unity.Collections.Allocator.Temp, Unity.Collections.NativeArrayOptions.UninitializedMemory);
			MarkerAllocate.End();
			for (int i = 0; i < obstacles.Length; i++) {
				// var obstacle = obstacles[i];
				// Add the edge to the lookup. But if it already exists, ignore it.
				// That it already exists is very much a special case that can happen if there is
				// overlapping geometry (typically added by a NavmeshAdd component).
				// In that case the "outer edge" that we want to trace is kinda undefined, but we will
				// do our best with it.
				if (traceLookup.TryAdd(obstacles[i].vertex1LinkId, i)) {
					priority[i] = 2;
				} else {
					priority[i] = 0;
				}
			}
			for (int i = 0; i < obstacles.Length; i++) {
				if (traceLookup.TryGetValue(obstacles[i].vertex2LinkId, out var other) && priority[other] > 0) {
					// The other segment has an ingoing edge. This means it cannot be the start of a contour.
					// Reduce the priority so that we only consider it when looking for cycles.
					priority[other] = 1;
				}
			}

			var outputMetadata = new NativeList<ObstacleVertexGroup>(16, Allocator.Temp);
			var outputVertices = new NativeList<float3>(16, Allocator.Temp);
			// We now have a hashmap of directed edges (vertex1 -> vertex2) such that these edges are directed the same (cw or ccw), and "outer edges".
			// We can now follow these directed edges to trace out the contours of the mesh.
			var toPlaneMatrix = movementPlane.AsWorldToPlaneMatrix();
			for (int allowLoops = 0; allowLoops <= 1; allowLoops++) {
				var minPriority = allowLoops == 1 ? 1 : 2;
				for (int i = 0; i < obstacles.Length; i++) {
					if (priority[i] >= minPriority) {
						int startVertices = outputVertices.Length;
						outputVertices.Add(obstacles[i].vertex1);

						var lastAdded = obstacles[i].vertex1;
						var candidateVertex = obstacles[i].vertex2;
						var index = i;
						var obstacleType = ObstacleType.Chain;
						var boundsMn = lastAdded;
						var boundsMx = lastAdded;
						while (true) {
							if (priority[index] == 0) {
								// This should not happen for a regular navmesh.
								// But it can happen if there are degenerate edges or overlapping triangles.
								// In that case we will just stop here
								break;
							}
							priority[index] = 0;

							float3 nextVertex;
							if (traceLookup.TryGetValue(obstacles[index].vertex2LinkId, out int nextIndex)) {
								nextVertex = 0.5f * (obstacles[index].vertex2 + obstacles[nextIndex].vertex1);
							} else {
								nextVertex = obstacles[index].vertex2;
								nextIndex = -1;
							}

							// Try to simplify and see if we even need to add the vertex C.
							var p1 = lastAdded;
							var p2 = nextVertex;
							var p3 = candidateVertex;
							var d1 = toPlaneMatrix.ToPlane(p2 - p1);
							var d2 = toPlaneMatrix.ToPlane(p3 - p1);
							var det = VectorMath.Determinant(d1, d2);
							if (math.abs(det) < 0.01f && simplifyObstacles) {
								// We don't need to add candidateVertex. It's just a straight line (p1,p2,p3 are colinear).
							} else {
								outputVertices.Add(candidateVertex);
								boundsMn = math.min(boundsMn, candidateVertex);
								boundsMx = math.max(boundsMx, candidateVertex);
								lastAdded = p3;
							}

							if (nextIndex == i) {
								// Loop
								outputVertices[startVertices] = nextVertex;
								obstacleType = ObstacleType.Loop;
								break;
							} else if (nextIndex == -1) {
								// End of chain
								outputVertices.Add(nextVertex);
								boundsMn = math.min(boundsMn, nextVertex);
								boundsMx = math.max(boundsMx, nextVertex);
								break;
							}

							index = nextIndex;
							candidateVertex = nextVertex;
						}

						outputMetadata.Add(new ObstacleVertexGroup {
							type = obstacleType,
							vertexCount = outputVertices.Length - startVertices,
							boundsMn = boundsMn,
							boundsMx = boundsMx,
						});
					}
				}
			}

			int obstacleSet, vertexSet;
			if (outputMetadata.Length > 0) {
				spinLock.Lock();
				obstacleSet = obstaclesAllocator.Allocate(outputMetadata);
				vertexSet = verticesAllocator.Allocate(outputVertices);
				spinLock.Unlock();
			} else {
				obstacleSet = SlabAllocator<ObstacleVertexGroup>.ZeroLengthArray;
				vertexSet = SlabAllocator<float3>.ZeroLengthArray;
			}
			outputObstacles[obstacleId] = new UnmanagedObstacle {
				verticesAllocation = vertexSet,
				groupsAllocation = obstacleSet,
			};
		}
	}
}
