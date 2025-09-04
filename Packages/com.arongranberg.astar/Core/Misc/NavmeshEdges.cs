using Pathfinding.Sync;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;

namespace Pathfinding {
	using Pathfinding.Util;
	using Pathfinding.RVO;
	using Pathfinding.Jobs;
	using Pathfinding.Drawing;
	using Pathfinding.Collections;

	[BurstCompile]
	public class NavmeshEdges {
		public RVO.SimulatorBurst.ObstacleData obstacleData;
		NativeReference<SpinLock> allocationLock;
		const int JobRecalculateObstaclesBatchCount = 32;
		RWLock rwLock = new RWLock();
		public HierarchicalGraph hierarchicalGraph;
		int gizmoVersion = 0;

		public void Dispose () {
			// Waits for any jobs to finish
			rwLock.WriteSync().Unlock();
			obstacleData.Dispose();
			// Note: The IsCreated check is necessary to not throw an exception in old versions of the collections package.
			if (allocationLock.IsCreated) allocationLock.Dispose();
		}

		void Init () {
			obstacleData.Init(Allocator.Persistent);
			if (!allocationLock.IsCreated) allocationLock = new NativeReference<SpinLock>(Allocator.Persistent);
		}

		public JobHandle RecalculateObstacles (NativeList<int> dirtyHierarchicalNodes, NativeReference<int> numHierarchicalNodes, JobHandle dependency) {
			Init();

			unsafe {
				// Resize the obstacle data arrays if necessary.
				// We need to do this in a separate single-threaded job before we branch out to multiple threads.
				var writeLock = rwLock.Write();
				var lastJob = new JobResizeObstacles {
					numHierarchicalNodes = numHierarchicalNodes,
					obstacles = obstacleData.obstacles,
				}.Schedule(JobHandle.CombineDependencies(dependency, writeLock.dependency));
				lastJob = new JobCalculateObstacles {
					hGraphGC = hierarchicalGraph.gcHandle,
					obstacleVertices = obstacleData.obstacleVertices,
					obstacleVertexGroups = obstacleData.obstacleVertexGroups,
					obstacles = obstacleData.obstacles.AsDeferredJobArray(),
					bounds = hierarchicalGraph.bounds.AsDeferredJobArray(),
					dirtyHierarchicalNodes = dirtyHierarchicalNodes,
					allocationLock = allocationLock,
				}.ScheduleBatch(JobRecalculateObstaclesBatchCount, 1, lastJob);
				writeLock.UnlockAfter(lastJob);
				gizmoVersion++;
				return lastJob;
			}
		}

		public void OnDrawGizmos (DrawingData gizmos, RedrawScope redrawScope) {
			if (!obstacleData.obstacleVertices.IsCreated) return;

			var hasher = new NodeHasher(AstarPath.active);
			hasher.Add(12314127); // Some random constant to avoid hash collisions with other systems
			hasher.Add(gizmoVersion);

			if (!gizmos.Draw(hasher, redrawScope)) {
				var readLock = rwLock.ReadSync();
				try {
					using (var builder = gizmos.GetBuilder(hasher, redrawScope)) {
						for (int i = 1; i < obstacleData.obstacles.Length; i++) {
							var ob = obstacleData.obstacles[i];
							var vertices = obstacleData.obstacleVertices.GetSpan(ob.verticesAllocation);
							var groups = obstacleData.obstacleVertexGroups.GetSpan(ob.groupsAllocation);
							var vertexOffset = 0;
							for (int g = 0; g < groups.Length; g++) {
								var group = groups[g];
								builder.PushLineWidth(2f);
								for (int j = 0; j < group.vertexCount - 1; j++) {
									builder.ArrowRelativeSizeHead(vertices[vertexOffset + j], vertices[vertexOffset + j + 1], new float3(0, 1, 0), 0.05f, Color.black);
								}
								if (group.type == RVO.ObstacleType.Loop) {
									builder.Arrow(vertices[vertexOffset + group.vertexCount - 1], vertices[vertexOffset], new float3(0, 1, 0), 0.05f, Color.black);
								}
								builder.PopLineWidth();
								vertexOffset += group.vertexCount;
								builder.WireBox(0.5f*(group.boundsMn + group.boundsMx), group.boundsMx - group.boundsMn, Color.white);
							}
						}
					}
				} finally {
					readLock.Unlock();
				}
			}
		}

		/// <summary>
		/// Obstacle data for navmesh edges.
		///
		/// Can be queried in burst jobs.
		/// </summary>
		public NavmeshBorderData GetNavmeshEdgeData (out RWLock.CombinedReadLockAsync readLock) {
			Init();
			var readLock1 = rwLock.Read();
			var hierarchicalNodeData = hierarchicalGraph.GetHierarhicalNodeData(out var readLock2);
			readLock = new RWLock.CombinedReadLockAsync(readLock1, readLock2);
			return new NavmeshBorderData {
					   hierarhicalNodeData = hierarchicalNodeData,
					   obstacleData = obstacleData,
			};
		}

		[BurstCompile]
		struct JobResizeObstacles : IJob {
			public NativeList<UnmanagedObstacle> obstacles;
			public NativeReference<int> numHierarchicalNodes;

			public void Execute () {
				var prevLength = obstacles.Length;
				var newLength = numHierarchicalNodes.Value;
				obstacles.Resize(newLength, NativeArrayOptions.UninitializedMemory);
				for (int i = prevLength; i < obstacles.Length; i++) obstacles[i] = new RVO.UnmanagedObstacle { verticesAllocation = SlabAllocator<float3>.ZeroLengthArray, groupsAllocation = SlabAllocator<ObstacleVertexGroup>.ZeroLengthArray };
				// First hierarchical node is always invalid
				if (obstacles.Length > 0) obstacles[0] = new RVO.UnmanagedObstacle { verticesAllocation = SlabAllocator<float3>.InvalidAllocation, groupsAllocation = SlabAllocator<ObstacleVertexGroup>.InvalidAllocation };
			}
		}

		struct JobCalculateObstacles : IJobParallelForBatch {
			public System.Runtime.InteropServices.GCHandle hGraphGC;
			public SlabAllocator<float3> obstacleVertices;
			public SlabAllocator<ObstacleVertexGroup> obstacleVertexGroups;
			[NativeDisableParallelForRestriction]
			public NativeArray<UnmanagedObstacle> obstacles;
			[NativeDisableParallelForRestriction]
			public NativeArray<Bounds> bounds;
			[ReadOnly]
			public NativeList<int> dirtyHierarchicalNodes;
			[NativeDisableParallelForRestriction]
			public NativeReference<SpinLock> allocationLock;

			public void Execute (int startIndex, int count) {
				var hGraph = hGraphGC.Target as HierarchicalGraph;
				var stepMultiplier = (dirtyHierarchicalNodes.Length + JobRecalculateObstaclesBatchCount - 1) / JobRecalculateObstaclesBatchCount;
				startIndex *= stepMultiplier;
				count *= stepMultiplier;
				var finalIndex = math.min(startIndex + count, dirtyHierarchicalNodes.Length);
				var edges = new NativeList<RVO.RVOObstacleCache.ObstacleSegment>(Allocator.Temp);
				for (int i = startIndex; i < finalIndex; i++) {
					edges.Clear();
					var hNode = dirtyHierarchicalNodes[i];
					UnityEngine.Assertions.Assert.IsTrue(hNode > 0 && hNode < obstacles.Length);
					// These tasks are independent, but they benefit a lot from running at the same time
					// due to cache locality (they use mostly the same data).
					CalculateBoundingBox(hGraph, hNode);
					CalculateObstacles(hGraph, hNode, obstacleVertexGroups, obstacleVertices, obstacles, edges);
				}
			}

			private static readonly ProfilerMarker MarkerBBox = new ProfilerMarker("HierarchicalBBox");
			private static readonly ProfilerMarker MarkerObstacles = new ProfilerMarker("CalculateObstacles");
			private static readonly ProfilerMarker MarkerCollect = new ProfilerMarker("Collect");
			private static readonly ProfilerMarker MarkerTrace = new ProfilerMarker("Trace");

			void CalculateBoundingBox (HierarchicalGraph hGraph, int hierarchicalNode) {
				var nodes = hGraph.children[hierarchicalNode];
				MarkerBBox.Begin();
				var b = new Bounds();
				// We know that all nodes in an hierarchical node only belongs to a single graph,
				// so we can branch on the type of the first node, and use optimized code for each node type.
				if (nodes.Count == 0) {
					// NOOP
				} else if (nodes[0] is TriangleMeshNode) {
					var mn = new Int3(int.MaxValue, int.MaxValue, int.MaxValue);
					var mx = new Int3(int.MinValue, int.MinValue, int.MinValue);
					for (int i = 0; i < nodes.Count; i++) {
						var node = nodes[i] as TriangleMeshNode;
						node.GetVertices(out var v0, out var v1, out var v2);
						mn = Int3.Min(Int3.Min(Int3.Min(mn, v0), v1), v2);
						mx = Int3.Max(Int3.Max(Int3.Max(mx, v0), v1), v2);
					}
					b.SetMinMax((Vector3)mn, (Vector3)mx);
				} else {
					var mn = new Int3(int.MaxValue, int.MaxValue, int.MaxValue);
					var mx = new Int3(int.MinValue, int.MinValue, int.MinValue);
					for (int i = 0; i < nodes.Count; i++) {
						var node = nodes[i];
						mn = Int3.Min(mn, node.position);
						mx = Int3.Max(mx, node.position);
					}
					if (nodes[0] is GridNodeBase) {
						float nodeSize;
						if (nodes[0] is LevelGridNode) nodeSize = LevelGridNode.GetGridGraph(nodes[0].GraphIndex).nodeSize;
						else
							nodeSize = GridNode.GetGridGraph(nodes[0].GraphIndex).nodeSize;
						// Grid nodes have a surface. We don't know how it is oriented, so we pad conservatively in all directions.
						// The surface can extend at most nodeSize*sqrt(2)/2 in any direction.
						const float SQRT2_DIV_2 = 0.70710678f;
						var padding = nodeSize*SQRT2_DIV_2*Vector3.one;
						b.SetMinMax((Vector3)mn - padding, (Vector3)mx + padding);
					} else {
						// Point node, or other custom node type
						b.SetMinMax((Vector3)mn, (Vector3)mx);
					}
				}
				bounds[hierarchicalNode] = b;
				MarkerBBox.End();
			}

			void CalculateObstacles (HierarchicalGraph hGraph, int hierarchicalNode, SlabAllocator<ObstacleVertexGroup> obstacleVertexGroups, SlabAllocator<float3> obstacleVertices, NativeArray<UnmanagedObstacle> obstacles, NativeList<RVO.RVOObstacleCache.ObstacleSegment> edgesScratch) {
				MarkerObstacles.Begin();
				MarkerCollect.Begin();
				RVO.RVOObstacleCache.CollectContours(hGraph.children[hierarchicalNode], edgesScratch);
				MarkerCollect.End();
				var prev = obstacles[hierarchicalNode];
				unsafe {
					ref var allocationLockRef = ref UnsafeUtility.AsRef<SpinLock>(allocationLock.GetUnsafePtr());
					if (prev.groupsAllocation != SlabAllocator<ObstacleVertexGroup>.ZeroLengthArray) {
						unsafe {
							allocationLockRef.Lock();
							obstacleVertices.Free(prev.verticesAllocation);
							obstacleVertexGroups.Free(prev.groupsAllocation);
							allocationLockRef.Unlock();
						}
					}
					unsafe {
						// Find the graph's natural movement plane.
						// This is used to simplify almost colinear segments into a single segment.
						var children = hGraph.children[hierarchicalNode];
						NativeMovementPlane movementPlane;
						bool simplifyObstacles = true;
						if (children.Count > 0) {
							if (children[0] is GridNodeBase) {
								movementPlane = new NativeMovementPlane((children[0].Graph as GridGraph).transform.rotation);
							} else if (children[0] is TriangleMeshNode) {
								var graph = children[0].Graph as NavmeshBase;
								movementPlane = new NativeMovementPlane(graph.transform.rotation);
								// If normal recalculation is disabled, the graph may have very a strange shape, like a spherical world.
								// In that case we should not simplify the obstacles, as there is no well defined movement plane.
								simplifyObstacles = graph.RecalculateNormals;
							} else {
								movementPlane = new NativeMovementPlane(quaternion.identity);
								simplifyObstacles = false;
							}
						} else {
							movementPlane = default;
						}
						MarkerTrace.Begin();
						var edgesSpan = edgesScratch.AsUnsafeSpan();
						RVO.RVOObstacleCache.TraceContours(
							ref edgesSpan,
							ref movementPlane,
							hierarchicalNode,
							(UnmanagedObstacle*)obstacles.GetUnsafePtr(),
							ref obstacleVertices,
							ref obstacleVertexGroups,
							ref allocationLockRef,
							simplifyObstacles
							);
						MarkerTrace.End();
					}
				}
				MarkerObstacles.End();
			}
		}

		/// <summary>
		/// Burst-accessible data about borders in the navmesh.
		///
		/// Can be queried from burst, and from multiple threads in parallel.
		/// </summary>
		// TODO: Change to a quadtree/kdtree/aabb tree that stored edges as { index: uint10, prev: uint10, next: uint10 }, with a natural max of 1024 vertices per obstacle (hierarchical node). This is fine because hnodes have at most 256 nodes, which cannot create more than 1024 edges.
		public struct NavmeshBorderData {
			public HierarchicalGraph.HierarhicalNodeData hierarhicalNodeData;
			public RVO.SimulatorBurst.ObstacleData obstacleData;

			/// <summary>
			/// An empty set of edges.
			///
			/// Must be disposed using <see cref="DisposeEmpty"/>.
			/// </summary>
			public static NavmeshBorderData CreateEmpty (Allocator allocator) {
				return new NavmeshBorderData {
						   hierarhicalNodeData = new HierarchicalGraph.HierarhicalNodeData {
							   connectionAllocator = default,
							   connectionAllocations = new NativeList<int>(0, allocator),
							   bounds = new NativeList<Bounds>(0, allocator),
						   },
						   obstacleData = new RVO.SimulatorBurst.ObstacleData {
							   obstacleVertexGroups = default,
							   obstacleVertices = default,
							   obstacles = new NativeList<UnmanagedObstacle>(0, allocator),
						   }
				};
			}

			public void DisposeEmpty (JobHandle dependsOn) {
				if (hierarhicalNodeData.connectionAllocator.IsCreated) throw new System.InvalidOperationException("NavmeshEdgeData was not empty");
				hierarhicalNodeData.connectionAllocations.Dispose(dependsOn);
				hierarhicalNodeData.bounds.Dispose(dependsOn);
				obstacleData.obstacles.Dispose(dependsOn);
			}

			static void GetHierarchicalNodesInRangeRec (int hierarchicalNode, Bounds bounds, SlabAllocator<int> connectionAllocator, [NoAlias] NativeList<int> connectionAllocations, NativeList<Bounds> nodeBounds, [NoAlias] NativeList<int> indices) {
				indices.Add(hierarchicalNode);
				var conns = connectionAllocator.GetSpan(connectionAllocations[hierarchicalNode]);
				for (int i = 0; i < conns.Length; i++) {
					var neighbour = conns[i];
					if (nodeBounds[neighbour].Intersects(bounds) && !indices.Contains(neighbour)) {
						GetHierarchicalNodesInRangeRec(neighbour, bounds, connectionAllocator, connectionAllocations, nodeBounds, indices);
					}
				}
			}

			static unsafe void ConvertObstaclesToEdges (ref RVO.SimulatorBurst.ObstacleData obstacleData, NativeList<int> obstacleIndices, Bounds localBounds, NativeList<float2> edgeBuffer, NativeMovementPlane movementPlane) {
				var globalBounds = movementPlane.ToWorld(localBounds);
				var worldToMovementPlane = movementPlane.AsWorldToPlaneMatrix();
				var globalMn = (float3)globalBounds.min;
				var globalMx = (float3)globalBounds.max;
				var localMn = (float3)localBounds.min;
				var localMx = (float3)localBounds.max;
				int vertexCount = 0;
				for (int obstacleIndex = 0; obstacleIndex < obstacleIndices.Length; obstacleIndex++) {
					var obstacle = obstacleData.obstacles[obstacleIndices[obstacleIndex]];
					vertexCount += obstacleData.obstacleVertices.GetSpan(obstacle.verticesAllocation).Length;
				}
				edgeBuffer.ResizeUninitialized(vertexCount*3);
				int edgeVertexOffset = 0;
				for (int obstacleIndex = 0; obstacleIndex < obstacleIndices.Length; obstacleIndex++) {
					var obstacle = obstacleData.obstacles[obstacleIndices[obstacleIndex]];
					if (obstacle.verticesAllocation != SlabAllocator<float3>.ZeroLengthArray) {
						var vertices = obstacleData.obstacleVertices.GetSpan(obstacle.verticesAllocation);
						var groups = obstacleData.obstacleVertexGroups.GetSpan(obstacle.groupsAllocation);
						int offset = 0;
						for (int i = 0; i < groups.Length; i++) {
							var group = groups[i];
							if (!math.all((group.boundsMx >= globalMn) & (group.boundsMn <= globalMx))) {
								offset += group.vertexCount;
								continue;
							}

							var loop = group.type == RVO.ObstacleType.Loop;
							for (int a = offset + (loop ? group.vertexCount - 1 : 0), b = offset + (loop ? 0 : 1); b < offset + group.vertexCount; a = b, b++) {
								var p1 = vertices[a];
								var p2 = vertices[b];
								var mn = math.min(p1, p2);
								var mx = math.max(p1, p2);
								// Check for intersection with the global bounds (coarse check)
								if (math.all((mx >= globalMn) & (mn <= globalMx))) {
									var p1local = worldToMovementPlane.ToXZPlane(p1);
									var p2local = worldToMovementPlane.ToXZPlane(p2);
									mn = math.min(p1local, p2local);
									mx = math.max(p1local, p2local);
									// Check for intersection with the local bounds (more accurate)
									if (math.all((mx >= localMn) & (mn <= localMx))) {
										edgeBuffer[edgeVertexOffset++] = p1local.xz;
										edgeBuffer[edgeVertexOffset++] = p2local.xz;
									}
								}
							}
							offset += group.vertexCount;
						}
					}
				}
				UnityEngine.Assertions.Assert.IsTrue(edgeVertexOffset <= edgeBuffer.Length);
				edgeBuffer.Length = edgeVertexOffset;
			}

			public void GetObstaclesInRange (int hierarchicalNode, Bounds bounds, NativeList<int> obstacleIndexBuffer) {
				if (!obstacleData.obstacleVertices.IsCreated) return;
				GetHierarchicalNodesInRangeRec(hierarchicalNode, bounds, hierarhicalNodeData.connectionAllocator, hierarhicalNodeData.connectionAllocations, hierarhicalNodeData.bounds, obstacleIndexBuffer);
			}

			public void GetEdgesInRange (int hierarchicalNode, Bounds localBounds, NativeList<float2> edgeBuffer, NativeList<int> scratchBuffer, NativeMovementPlane movementPlane) {
				if (!obstacleData.obstacleVertices.IsCreated) return;

				GetObstaclesInRange(hierarchicalNode, movementPlane.ToWorld(localBounds), scratchBuffer);
				ConvertObstaclesToEdges(ref obstacleData, scratchBuffer, localBounds, edgeBuffer, movementPlane);
			}
		}
	}
}
