#pragma warning disable CS0282
#if MODULE_ENTITIES
using Unity.Entities;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

namespace Pathfinding.ECS {
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using Pathfinding;
	using Pathfinding.Util;
	using Pathfinding.Collections;
	using Unity.Transforms;
	using UnityEngine.Profiling;

	[UpdateBefore(typeof(RepairPathSystem))]
	[UpdateInGroup(typeof(AIMovementSystemGroup))]
	[RequireMatchingQueriesForUpdate]
	[BurstCompile]
	public partial struct MovementPlaneFromGraphSystem : ISystem {
		public EntityQuery entityQueryGraph;
		public EntityQuery entityQueryNormal;
		// Store the queue in a GCHandle to avoid restrictions on ISystem
		GCHandle graphNodeQueue;

		public void OnCreate (ref SystemState state) {
			entityQueryGraph = state.GetEntityQuery(ComponentType.ReadOnly<MovementState>(), ComponentType.ReadWrite<AgentMovementPlane>(), ComponentType.ReadOnly<AgentMovementPlaneSource>());
			entityQueryGraph.SetSharedComponentFilter(new AgentMovementPlaneSource { value = MovementPlaneSource.Graph });
			entityQueryNormal = state.GetEntityQuery(
				ComponentType.ReadWrite<ManagedState>(),
				ComponentType.ReadOnly<LocalTransform>(),
				ComponentType.ReadWrite<AgentMovementPlane>(),
				ComponentType.ReadOnly<AgentCylinderShape>(),
				ComponentType.ReadOnly<AgentMovementPlaneSource>()
				);
			entityQueryNormal.AddSharedComponentFilter(new AgentMovementPlaneSource { value = MovementPlaneSource.NavmeshNormal });

			graphNodeQueue = GCHandle.Alloc(new List<GraphNode>(32));
		}

		public void OnDestroy (ref SystemState state) {
			graphNodeQueue.Free();
		}

		public void OnUpdate (ref SystemState systemState) {
			var graphs = AstarPath.active?.data.graphs;
			if (graphs == null) return;

			var movementPlanes = CollectionHelper.CreateNativeArray<AgentMovementPlane>(graphs.Length, systemState.WorldUpdateAllocator, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < graphs.Length; i++) {
				movementPlanes[i] = new AgentMovementPlane(MovementPlaneFromGraph(graphs[i]));
			}

			if (!entityQueryNormal.IsEmpty) {
				Profiler.BeginSample("MovementPlaneSource.NavmeshNormal");
				systemState.CompleteDependency();
				var vertices = new NativeList<Int3>(16, Allocator.Temp);
				new JobMovementPlaneFromNavmeshNormal {
					dt = AIMovementSystemGroup.TimeScaledRateManager.CheapStepDeltaTime,
					vertices = vertices,
					que = (List<GraphNode>)graphNodeQueue.Target,
				}.Run(entityQueryNormal);
				Profiler.EndSample();
			}

			systemState.Dependency = new JobMovementPlaneFromGraph {
				movementPlanes = movementPlanes,
			}.Schedule(entityQueryGraph, systemState.Dependency);
		}

		/// <summary>
		/// Natural movement plane for a graph traversing a given graph.
		///
		/// This is the movement plane used for <see cref="MovementPlaneSource"/>.Graph.
		///
		/// See: <see cref="FollowerEntity.movementPlaneSource"/>
		/// </summary>
		public static NativeMovementPlane MovementPlaneFromGraph (NavGraph graph) {
			if (graph is NavmeshBase navmesh) {
				return new NativeMovementPlane(navmesh.transform.rotation);
			} else if (graph is GridGraph grid) {
				return new NativeMovementPlane(grid.transform.rotation);
			} else {
				return new NativeMovementPlane(quaternion.identity);
			}
		}

		partial struct JobMovementPlaneFromNavmeshNormal : IJobEntity {
			public float dt;
			public NativeList<Int3> vertices;
			public List<GraphNode> que;

			public void Execute (ManagedState managedState, in LocalTransform localTransform, ref AgentMovementPlane agentMovementPlane, in AgentCylinderShape shape) {
				var node = managedState.pathTracer.startNode as TriangleMeshNode;
				if (node != null) {
					// TODO: Expose this parameter?
					const float InverseSmoothness = 20f;
					var radius = math.max(0.01f, shape.radius);
					SampleSmoothNavmeshNormal(node, que, vertices, localTransform.Position, radius, ref agentMovementPlane, dt * InverseSmoothness);
				}
			}
		}

		[BurstCompile]
		partial struct JobMovementPlaneFromGraph : IJobEntity {
			[ReadOnly]
			public NativeArray<AgentMovementPlane> movementPlanes;

			public void Execute (in MovementState movementState, ref AgentMovementPlane movementPlane) {
				if (movementState.graphIndex < (uint)movementPlanes.Length) {
					movementPlane = movementPlanes[(int)movementState.graphIndex];
				} else {
					// This can happen if the agent has no path, or if the path is stale.
					// Potentially also if a graph has been removed.
				}
			}
		}

		public static void SampleSmoothNavmeshNormal (TriangleMeshNode node, List<GraphNode> scratchList, NativeList<Int3> scratchBuffer, float3 position, float agentRadius, ref AgentMovementPlane agentMovementPlane, float alpha) {
			var vertices = scratchBuffer;
			var que = scratchList;
			vertices.Clear();
			que.Clear();
			int queStart = 0;
			node.TemporaryFlag1 = true;
			que.Add(node);
			var i0 = node.v0;
			var i1 = node.v1;
			var i2 = node.v2;

			while (queStart < que.Count) {
				var current = que[queStart++] as TriangleMeshNode;
				if (current == null) continue;
				var anyVertex = current.v0 == i0 | current.v1 == i0 | current.v2 == i0 | current.v0 == i1 | current.v1 == i1 | current.v2 == i1 | current.v0 == i2 | current.v1 == i2 | current.v2 == i2;
				if (anyVertex) {
					current.GetVertices(out var v0, out var v1, out var v2);
					vertices.Add(v0);
					vertices.Add(v1);
					vertices.Add(v2);
					current.GetConnections((GraphNode con, ref List<GraphNode> que) => {
						if (!con.TemporaryFlag1) {
							con.TemporaryFlag1 = true;
							que.Add(con);
						}
					}, ref que);
				}
			}

			// Reset temporary flags
			for (int i = 0; i < que.Count; i++) {
				que[i].TemporaryFlag1 = false;
			}

			var verticesSpan = vertices.AsUnsafeSpan();
			SampleSmoothTriangleNormal(ref position, ref verticesSpan, ref agentMovementPlane, agentRadius, alpha);
		}

		static float Square (float x) {
			return x * x;
		}

		/// <summary>Sine of the angle ABC</summary>
		static float SinAngle (float3 a, float3 b, float3 c) {
			return math.sqrt(1 - Square(math.dot(math.normalizesafe(a - b), math.normalizesafe(c - b))));
		}

		[BurstCompile(FloatMode = FloatMode.Fast)]
		static void SampleSmoothTriangleNormal (ref float3 position, ref UnsafeSpan<Int3> _triangleVertices, ref AgentMovementPlane agentMovementPlane, float agentRadius, float alpha) {
			var triangleVertices = _triangleVertices.Reinterpret<int3>();
			if (triangleVertices.Length < 3) throw new System.ArgumentException("triangleVertices.Length < 3");
			unsafe {
				// First 3 vertices represent the triangle we start on
				var sourceVertices = triangleVertices.ptr;
				var normals = stackalloc float3[3];
				var weights = stackalloc float[3];
				normals[0] = normals[1] = normals[2] = float3.zero;
				weights[0] = weights[1] = weights[2] = 0;
				var currentNormal = agentMovementPlane.value.up;

				for (uint i = 0; i < triangleVertices.length; i += 3) {
					var p0 = triangleVertices[i + 0];
					var p1 = triangleVertices[i + 1];
					var p2 = triangleVertices[i + 2];
					var f0 = (float3)p0 * Int3.PrecisionFactor;
					var f1 = (float3)p1 * Int3.PrecisionFactor;
					var f2 = (float3)p2 * Int3.PrecisionFactor;

					var triangleNormal = math.normalizesafe(math.cross(f1 - f0, f2 - f0));

					const float COS_SMOOTH_ANGLE_LIMIT = 0.86f;
					float weight = 1;
					var cosAngle = math.dot(triangleNormal, currentNormal);
					if (cosAngle < COS_SMOOTH_ANGLE_LIMIT) {
						// Hard angle. Lower the weight of this triangle to avoid starting to rotate too early.
						Polygon.ClosestPointOnTriangleByRef(in f0, in f1, in f2, in position, out var closest);
						var distance = math.lengthsq(closest - position) / Square(1.5f * agentRadius);
						var distanceWeight = math.max(0.1f, 1 - distance);
						var angleWeight = (COS_SMOOTH_ANGLE_LIMIT - math.max(0, cosAngle)) / COS_SMOOTH_ANGLE_LIMIT;
						weight = math.lerp(1, distanceWeight, angleWeight);
					}

					for (int j = 0; j < 3; j++) {
						if (math.all(p0 == sourceVertices[j])) {
							// When calculating smooth normals, we ideally want to weigh the contributions from
							// differnt triangles by the angle of the triangle at the vertex.
							// We use the sine of that angle instead, which is a decent approximation.
							var w = weight * SinAngle(p2, p0, p1);
							weights[j] += w;
							normals[j] += w * triangleNormal;
						}
					}

					for (int j = 0; j < 3; j++) {
						if (math.all(p1 == sourceVertices[j])) {
							var w = weight * SinAngle(p0, p1, p2);
							weights[j] += w;
							normals[j] += w * triangleNormal;
						}
					}

					for (int j = 0; j < 3; j++) {
						if (math.all(p2 == sourceVertices[j])) {
							var w = weight * SinAngle(p1, p2, p0);
							weights[j] += w;
							normals[j] += w * triangleNormal;
						}
					}
				}

				for (int j = 0; j < 3; j++) {
					if (weights[j] > 0) normals[j] /= weights[j];
				}

				var v0 = (float3)sourceVertices[0] * Int3.PrecisionFactor;
				var v1 = (float3)sourceVertices[1] * Int3.PrecisionFactor;
				var v2 = (float3)sourceVertices[2] * Int3.PrecisionFactor;
				var barycentric = Polygon.ClosestPointOnTriangleBarycentric(v0, v1, v2, position);
				var targetNormal = math.normalizesafe(normals[0] * barycentric.x + normals[1] * barycentric.y + normals[2] * barycentric.z);

				var nextNormal = math.lerp(currentNormal, targetNormal, math.clamp(alpha, 0, 1));
				agentMovementPlane.value = agentMovementPlane.value.MatchUpDirection(nextNormal);
			}
		}
	}
}
#endif
