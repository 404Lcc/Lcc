using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using Pathfinding.Util;
using Pathfinding.Collections;
using UnityEngine.Tilemaps;


namespace Pathfinding.Graphs.Navmesh {
	[BurstCompile]
	struct CircleGeometryUtilities {
		/// <summary>
		/// Cached values for CircleRadiusAdjustmentFactor.
		///
		/// We can calculate the area of a polygonized circle, and equate that with the area of a unit circle
		/// <code>
		/// x * cos(math.PI / steps) * sin(math.PI/steps) * steps = pi
		/// </code>
		/// Solving for the factor that makes them equal (x) gives the expression below.
		///
		/// Generated using the python code:
		/// <code>
		/// [math.sqrt(2 * math.pi / (i * math.sin(2 * math.pi / i))) for i in range(3, 23)]
		/// </code>
		///
		/// It would be nice to generate this using a static constructor, but that is not supported by Unity's burst compiler.
		/// </summary>
		static readonly float[] circleRadiusAdjustmentFactors = new float[] {
			1.56f, 1.25f, 1.15f, 1.1f, 1.07f, 1.05f, 1.04f, 1.03f, 1.03f, 1.02f, 1.02f, 1.02f, 1.01f, 1.01f, 1.01f, 1.01f, 1.01f, 1.01f, 1.01f, 1.01f,
		};

		/// <summary>The number of steps required to get a circle with a maximum error of maxError</summary>
		public static int CircleSteps (Matrix4x4 matrix, float radius, float maxError) {
			// Take the maximum scale factor among the 3 axes.
			// If the current matrix has a uniform scale then they are all the same.
			var maxScaleFactor = math.sqrt(math.max(math.max(math.lengthsq((Vector3)matrix.GetColumn(0)), math.lengthsq((Vector3)matrix.GetColumn(1))), math.lengthsq((Vector3)matrix.GetColumn(2))));
			var realWorldRadius = radius * maxScaleFactor;

			// This expression is the first taylor expansion term of the formula below.
			// It is almost identical to the formula below, but it avoids expensive trigonometric functions.
			// return math.max(3, (int)math.ceil(math.PI * math.sqrt(realWorldRadius / (2*maxError))));
			var cosAngle = 1 - maxError / realWorldRadius;
			int steps = math.max(3, (int)math.ceil(math.PI / math.acos(cosAngle)));
			return steps;
		}

		/// <summary>
		/// Radius factor to adjust for circle approximation errors.
		/// If a circle is approximated by fewer segments, it will be slightly smaller than the original circle.
		/// This factor is used to adjust the radius of the circle so that the resulting circle will have roughly the same area as the original circle.
		/// </summary>
#if MODULE_COLLECTIONS_2_0_0_OR_NEWER && UNITY_2022_2_OR_NEWER
		[GenerateTestsForBurstCompatibility]
#endif
		public static float CircleRadiusAdjustmentFactor (int steps) {
			var index = steps - 3;
			if (index < circleRadiusAdjustmentFactors.Length) {
				if (index < 0) throw new System.ArgumentOutOfRangeException("Steps must be at least 3");
				return circleRadiusAdjustmentFactors[index];
			} else {
				// Larger steps are approximately one
				return 1;
			}
		}
	}

	[BurstCompile]
	internal static class ColliderMeshBuilder2D {
		static int GetShapes (Collider2D coll, PhysicsShapeGroup2D group, HashSet<Rigidbody2D> handledRigidbodies) {
#if !UNITY_6000_0_OR_NEWER
			var rigid = coll.attachedRigidbody;
			if (rigid != null) {
				if (handledRigidbodies.Add(rigid)) {
					// Trying to get the shapes from a collider that is attached to a rigidbody will log annoying errors (this seems like a Unity bug tbh),
					// so we must call GetShapes on the rigidbody instead.
					// Not quite sure which version of Unity stopped logging these errors, but they don't seem to be present in Unity 6 anymore.
					return rigid.GetShapes(group);
				} else {
					return 0;
				}
			}
#endif

			if (coll is TilemapCollider2D tilemapColl) {
				// Ensure the tilemap is up to date
				tilemapColl.ProcessTilemapChanges();
			}
			return coll.GetShapes(group);
		}

		public static int GenerateMeshesFromColliders (Collider2D[] colliders, int numColliders, float maxError, out NativeArray<float3> outputVertices, out NativeArray<int> outputIndices, out NativeArray<ShapeMesh> outputShapeMeshes) {
			var group = new PhysicsShapeGroup2D();
			var shapeList = new NativeList<PhysicsShape2D>(numColliders, Allocator.Temp);
			var verticesList = new NativeList<Vector2>(numColliders*4, Allocator.Temp);
			var matricesList = new NativeList<Matrix4x4>(numColliders, Allocator.Temp);
			var colliderIndexList = new NativeList<int>(numColliders, Allocator.Temp);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
			var tempHandle = AtomicSafetyHandle.GetTempMemoryHandle();
#endif
#if UNITY_6000_0_OR_NEWER
			HashSet<Rigidbody2D> handledRigidbodies = null;
#else
			var handledRigidbodies = new HashSet<Rigidbody2D>();
#endif

			Profiler.BeginSample("GetShapes");

			// Get the low level physics shapes from all colliders
			var indexOffset = 0;
			for (int i = 0; i < numColliders; i++) {
				var coll = colliders[i];
				// Prevent errors from being logged when calling GetShapes on a collider that has no shapes
				if (coll == null || coll.shapeCount == 0) continue;

				var shapeCount = GetShapes(coll, group, handledRigidbodies);
				if (shapeCount == 0) continue;

				shapeList.Length += shapeCount;
				verticesList.Length += group.vertexCount;
				var subShapes = shapeList.AsArray().GetSubArray(shapeList.Length - shapeCount, shapeCount);
				var subVertices = verticesList.AsArray().GetSubArray(verticesList.Length - group.vertexCount, group.vertexCount);
				// Using AsArray and then GetSubArray will create an invalid safety handle due to unity limitations.
				// We work around this by setting the safety handle to a temporary handle.
#if ENABLE_UNITY_COLLECTIONS_CHECKS
				NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref subShapes, tempHandle);
				NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref subVertices, tempHandle);
#endif
				group.GetShapeData(subShapes, subVertices);
				for (int j = 0; j < shapeCount; j++) {
					var shape = subShapes[j];
					shape.vertexStartIndex += indexOffset;
					subShapes[j] = shape;
				}
				indexOffset += subVertices.Length;
				matricesList.AddReplicate(group.localToWorldMatrix, shapeCount);
				colliderIndexList.AddReplicate(i, shapeCount);
			}
			Profiler.EndSample();
			Assert.AreEqual(shapeList.Length, matricesList.Length);

			Profiler.BeginSample("GenerateMeshes");
			var vertexBuffer = new NativeList<float3>(Allocator.Temp);
			var indexBuffer = new NativeList<int3>(Allocator.Temp);
			var shapeSpan = shapeList.AsUnsafeSpan();
			var verticesSpan = verticesList.AsUnsafeSpan().Reinterpret<float2>();
			var matricesSpan = matricesList.AsUnsafeSpan();
			var indexSpan = colliderIndexList.AsUnsafeSpan();
			outputShapeMeshes = new NativeArray<ShapeMesh>(shapeList.Length, Allocator.Persistent);
			var outputShapeMeshesSpan = outputShapeMeshes.AsUnsafeSpan();
			int outputMeshCount;
			unsafe {
				outputMeshCount = GenerateMeshesFromShapes(
					ref shapeSpan,
					ref verticesSpan,
					ref matricesSpan,
					ref indexSpan,
					ref UnsafeUtility.AsRef<UnsafeList<float3> >(vertexBuffer.GetUnsafeList()),
					ref UnsafeUtility.AsRef<UnsafeList<int3> >(indexBuffer.GetUnsafeList()),
					ref outputShapeMeshesSpan,
					maxError
					);
			}

			Profiler.EndSample();
			Profiler.BeginSample("Copy");
			outputVertices = vertexBuffer.ToArray(Allocator.Persistent);
			outputIndices = new NativeArray<int>(indexBuffer.AsArray().Reinterpret<int>(12), Allocator.Persistent);
			Profiler.EndSample();
			return outputMeshCount;
		}

		public struct ShapeMesh {
			public Matrix4x4 matrix;
			public Bounds bounds;
			public int startIndex;
			public int endIndex;
			public int tag;
		}

		static void AddCapsuleMesh (float2 c1, float2 c2, ref Matrix4x4 shapeMatrix, float radius, float maxError, ref UnsafeList<float3> outputVertices, ref UnsafeList<int3> outputIndices, ref float3 mn, ref float3 mx) {
			var steps = math.max(4, CircleGeometryUtilities.CircleSteps(shapeMatrix, radius, maxError));
			// We are only generating a semicircle at a time, so reduce the number of steps
			steps = (steps / 2) + 1;
			radius = radius * CircleGeometryUtilities.CircleRadiusAdjustmentFactor(2*(steps-1));

			var center1 = new Vector3(c1.x, c1.y, 0);
			var center2 = new Vector3(c2.x, c2.y, 0);
			var axis = math.normalizesafe(c2 - c1);
			var crossAxis = new float2(-axis.y, axis.x);
			var dx = radius * new Vector3(crossAxis.x, crossAxis.y, 0);
			var dy = radius * new Vector3(axis.x, axis.y, 0);
			var angle = math.PI / (steps-1);

			var startVertex = outputVertices.Length;
			var startVertex2 = startVertex + steps;
			outputVertices.Length += steps*2;
			for (int j = 0; j < steps; j++) {
				math.sincos(angle * j, out var sin, out var cos);

				// Generate first semi-circle
				var p = center1 + cos * dx - sin * dy;
				mn = math.min(mn, p);
				mx = math.max(mx, p);
				outputVertices[startVertex + j] = p;

				// Generate second semi-circle
				p = center2 - cos * dx + sin * dy;
				mn = math.min(mn, p);
				mx = math.max(mx, p);
				outputVertices[startVertex2 + j] = p;
			}
			var startIndex = outputIndices.Length;
			var startIndex2 = startIndex + steps-2;
			outputIndices.Length += (steps-2)*2;
			for (int j = 1; j < steps - 1; j++) {
				// Triangle for first semi-circle
				outputIndices[startIndex + j - 1]  = new int3(startVertex, startVertex + j, startVertex + j + 1);
				// Triangle for second semi-circle
				outputIndices[startIndex2 + j - 1] = new int3(startVertex2, startVertex2 + j, startVertex2 + j + 1);
			}

			// Generate the connection between the two semi-circles
			outputIndices.Add(new int3(startVertex, startVertex + steps - 1, startVertex2));
			outputIndices.Add(new int3(startVertex, startVertex2, startVertex2 + steps - 1));
		}

		[BurstCompile]
		public static int GenerateMeshesFromShapes (
			ref UnsafeSpan<PhysicsShape2D> shapes,
			ref UnsafeSpan<float2> vertices,
			ref UnsafeSpan<Matrix4x4> shapeMatrices,
			ref UnsafeSpan<int> groupIndices,
			ref UnsafeList<float3> outputVertices,
			ref UnsafeList<int3> outputIndices,
			ref UnsafeSpan<ShapeMesh> outputShapeMeshes,
			float maxError
			) {
			var groupStartIndex = 0;
			var mn = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
			var mx = new float3(float.MinValue, float.MinValue, float.MinValue);
			int outputMeshIndex = 0;
			for (int i = 0; i < shapes.Length; i++) {
				var shape = shapes[i];
				var shapeVertices = vertices.Slice(shape.vertexStartIndex, shape.vertexCount);
				var shapeMatrix = shapeMatrices[i];
				switch (shape.shapeType) {
				case PhysicsShapeType2D.Circle: {
					var steps = CircleGeometryUtilities.CircleSteps(shapeMatrix, shape.radius, maxError);
					var radius = shape.radius * CircleGeometryUtilities.CircleRadiusAdjustmentFactor(steps);
					var center = new Vector3(shapeVertices[0].x, shapeVertices[0].y, 0);
					var d1 = new Vector3(radius, 0, 0);
					var d2 = new Vector3(0, radius, 0);
					var angle = 2 * math.PI / steps;
					var startVertex = outputVertices.Length;
					for (int j = 0; j < steps; j++) {
						math.sincos(angle * j, out var sin, out var cos);
						var p = center + cos * d1 + sin * d2;
						mn = math.min(mn, p);
						mx = math.max(mx, p);
						outputVertices.Add(p);
					}
					for (int j = 1; j < steps; j++) {
						outputIndices.Add(new int3(startVertex, startVertex + j, startVertex + (j + 1) % steps));
					}
					break;
				}
				case PhysicsShapeType2D.Capsule: {
					var c1 = shapeVertices[0];
					var c2 = shapeVertices[1];
					AddCapsuleMesh(c1, c2, ref shapeMatrix, shape.radius, maxError, ref outputVertices, ref outputIndices, ref mn, ref mx);
					break;
				}
				case PhysicsShapeType2D.Polygon: {
					var startVertex = outputVertices.Length;
					outputVertices.Resize(startVertex + shape.vertexCount, NativeArrayOptions.UninitializedMemory);
					for (int j = 0; j < shape.vertexCount; j++) {
						var p = new Vector3(shapeVertices[j].x, shapeVertices[j].y, 0);
						mn = math.min(mn, p);
						mx = math.max(mx, p);
						outputVertices[startVertex + j] = p;
					}
					outputIndices.SetCapacity(math.ceilpow2(outputIndices.Length + (shape.vertexCount - 2)));
					for (int j = 1; j < shape.vertexCount - 1; j++) {
						outputIndices.AddNoResize(new int3(startVertex, startVertex + j, startVertex + j + 1));
					}
					break;
				}
				case PhysicsShapeType2D.Edges: {
					if (shape.radius > maxError) {
						for (int j = 0; j < shape.vertexCount - 1; j++) {
							AddCapsuleMesh(shapeVertices[j], shapeVertices[j+1], ref shapeMatrix, shape.radius, maxError, ref outputVertices, ref outputIndices, ref mn, ref mx);
						}
					} else {
						var startVertex = outputVertices.Length;
						outputVertices.Resize(startVertex + shape.vertexCount, NativeArrayOptions.UninitializedMemory);
						for (int j = 0; j < shape.vertexCount; j++) {
							var p = new Vector3(shapeVertices[j].x, shapeVertices[j].y, 0);
							mn = math.min(mn, p);
							mx = math.max(mx, p);
							outputVertices[startVertex + j] = p;
						}
						outputIndices.SetCapacity(math.ceilpow2(outputIndices.Length + (shape.vertexCount - 1)));
						for (int j = 0; j < shape.vertexCount - 1; j++) {
							// An edge is represented by a degenerate triangle
							outputIndices.AddNoResize(new int3(startVertex + j, startVertex + j + 1, startVertex + j + 1));
						}
					}
					break;
				}
				default:
					throw new System.Exception("Unexpected PhysicsShapeType2D");
				}

				// Merge shapes which are in the same group into a single ShapeMesh struct.
				// This is done to reduce the per-shape overhead a bit.
				// Don't do it too much, though, since that can cause filtering to not work too well.
				// For example if a recast graph recalculates a single tile in a 2D scene, we don't want to include the whole collider for the
				// TilemapCollider2D in the scene when doing rasterization, only the shapes around the tile that is recalculated.
				// We will still process the whole TilemapCollider2D (no way around that), but we want to be able to exclude shapes shapes as quickly as possible
				// based on their bounding boxes.
				const int DesiredTrianglesPerGroup = 100;
				if (i == shapes.Length - 1 || groupIndices[i] != groupIndices[i+1] || outputIndices.Length - groupStartIndex > DesiredTrianglesPerGroup) {
					// Transform the bounding box to world space
					// This is not the tightest bounding box, but it is good enough
					var m = new ToWorldMatrix(new float3x3((float4x4)shapeMatrix));
					var bounds = new Bounds((mn + mx)*0.5f, mx - mn);
					bounds = m.ToWorld(bounds);
					bounds.center += (Vector3)shapeMatrix.GetColumn(3);

					outputShapeMeshes[outputMeshIndex++] = new ShapeMesh {
						bounds = bounds,
						matrix = shapeMatrix,
						startIndex = groupStartIndex * 3,
						endIndex = outputIndices.Length * 3,
						tag = groupIndices[i]
					};

					mn = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
					mx = new float3(float.MinValue, float.MinValue, float.MinValue);
					groupStartIndex = outputIndices.Length;
				}
			}

			return outputMeshIndex;
		}
	}
}
