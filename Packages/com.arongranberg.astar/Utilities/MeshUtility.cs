using UnityEngine;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Pathfinding.Collections;
using Unity.Mathematics;

namespace Pathfinding.Util {
#if MODULE_COLLECTIONS_2_1_0_OR_NEWER
	using NativeHashMapInt3Int = Unity.Collections.NativeHashMap<Int3, int>;
#else
	using NativeHashMapInt3Int = Unity.Collections.NativeParallelHashMap<Int3, int>;
#endif

	/// <summary>Helper class for working with meshes efficiently</summary>
	[BurstCompile]
	static class MeshUtility {
		public static void GetMeshData (Mesh.MeshDataArray meshData, int meshIndex, out NativeArray<Vector3> vertices, out NativeArray<int> indices) {
			var rawMeshData = meshData[meshIndex];
			vertices = new NativeArray<Vector3>(rawMeshData.vertexCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			rawMeshData.GetVertices(vertices);
			int totalIndices = 0;
			for (int subMeshIndex = 0; subMeshIndex < rawMeshData.subMeshCount; subMeshIndex++) {
				totalIndices += rawMeshData.GetSubMesh(subMeshIndex).indexCount;
			}
			indices = new NativeArray<int>(totalIndices, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			int offset = 0;
			for (int subMeshIndex = 0; subMeshIndex < rawMeshData.subMeshCount; subMeshIndex++) {
				var submesh = rawMeshData.GetSubMesh(subMeshIndex);
				rawMeshData.GetIndices(indices.GetSubArray(offset, submesh.indexCount), subMeshIndex);
				offset += submesh.indexCount;
			}
		}

		/// <summary>
		/// Flips triangles such that they are all clockwise in graph space.
		///
		/// The triangles may not be clockwise in world space since the graphs can be rotated.
		///
		/// The triangles array will be modified in-place.
		/// </summary>
		[BurstCompile]
		public static void MakeTrianglesClockwise (ref UnsafeSpan<Int3> vertices, ref UnsafeSpan<int> triangles) {
			for (int i = 0; i < triangles.Length; i += 3) {
				// Make sure the triangle is clockwise in graph space (it may not be in world space since the graphs can be rotated)
				// Note that we also modify the original triangle array because if the graph is cached then we will re-initialize the nodes from that array and assume all triangles are clockwise.
				if (!VectorMath.IsClockwiseXZ(vertices[triangles[i+0]], vertices[triangles[i+1]], vertices[triangles[i+2]])) {
					var tmp = triangles[i+0];
					triangles[i+0] = triangles[i+2];
					triangles[i+2] = tmp;
				}
			}
		}

		/// <summary>
		/// Removes duplicate vertices from the array and updates the triangle array.
		///
		/// Uses a sweep line algorithm. For mergeRadiusSq=0, this is slower than a hash map based approach (by a factor of 3-4 even, primarily due to the sort),
		/// but this code doesn't tend to be a bottleneck so it's not a big deal.
		/// A hash based approach cannot easily support a mergeRadiusSq > 0.
		///
		/// A hash based approach was removed in the commit after cc57efb0c.
		/// </summary>
		[BurstCompile]
		public struct JobMergeNearbyVertices : IJob {
			public NativeList<Int3> vertices;
			public NativeList<int> triangles;
			public int mergeRadiusSq;

			struct CoordinateSorter : System.Collections.Generic.IComparer<int> {
				public UnsafeSpan<int3> vertices;

				public int Compare (int a, int b) {
					Unity.Burst.CompilerServices.Hint.Assume((uint)a < vertices.length);
					Unity.Burst.CompilerServices.Hint.Assume((uint)b < vertices.length);
					return vertices[a].x.CompareTo(vertices[b].x);
				}
			}

			public void Execute () {
				var indicesArr = new NativeArray<int>(vertices.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
				for (int i = 0; i < indicesArr.Length; i++) indicesArr[i] = i;
				indicesArr.Sort(new CoordinateSorter {
					vertices = vertices.AsUnsafeSpan().Reinterpret<int3>()
				});
				var indices = indicesArr.AsUnsafeSpan();

				var compressedPointers = new NativeArray<int>(vertices.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
				var verticesSpan = vertices.AsUnsafeSpan().Reinterpret<int3>();
				var trianglesSpan = triangles.AsUnsafeSpan();
				var compressedVerticesArr = new NativeArray<int3>(vertices.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
				var compressedVertices = compressedVerticesArr.AsUnsafeSpan();

				int vertexCount = 0;
				var mergeRadiusCeil = (int)math.ceil(math.sqrt(mergeRadiusSq));
				uint rangeEndIndex = 1;

				// Use a sweep line algorithm to merge nearby vertices
				for (uint i = 0; i < indices.length; i++) {
					if (indices[i] == -1) continue;
					var v = verticesSpan[indices[i]];
					compressedPointers[indices[i]] = vertexCount;

					while (rangeEndIndex < indices.length && verticesSpan[indices[rangeEndIndex]].x <= v.x + mergeRadiusCeil) rangeEndIndex++;

					var mean = v;
					int count = 1;
					for (uint j = i + 1; j < rangeEndIndex; j++) {
						if (indices[j] == -1) continue;

						var v2 = verticesSpan[indices[j]];
						if (math.lengthsq(v2 - v) <= mergeRadiusSq) {
							mean += v2;
							count++;
							compressedPointers[indices[j]] = vertexCount;
							indices[j] = -1;
						}
					}
					compressedVertices[vertexCount] = mean / count;
					vertexCount++;
				}

				vertices.Length = vertexCount;
				compressedVertices.Slice(0, vertexCount).CopyTo(vertices.AsUnsafeSpan().Reinterpret<int3>());
				for (uint i = 0; i < trianglesSpan.length; i++) {
					trianglesSpan[i] = compressedPointers[trianglesSpan[i]];
				}
			}
		}

		[BurstCompile]
		public struct JobRemoveDegenerateTriangles : IJob {
			public NativeList<Int3> vertices;
			public NativeList<int> triangles;
			public NativeList<int> tags;
			public bool verbose;

			public static int3 cross(int3 lhs, int3 rhs) => (lhs * rhs.yzx - lhs.yzx * rhs).yzx;

			public void Execute () {
				int numDegenerate = 0;
				var verticesSpan = vertices.AsUnsafeSpan().Reinterpret<int3>();
				var trianglesSpan = triangles.AsUnsafeSpan().Reinterpret<int3>(4);
				var tagsSpan = tags.AsUnsafeSpan();

				uint triCount = 0;
				for (uint ti = 0; ti < trianglesSpan.length; ti++) {
					var tri = trianglesSpan[ti];

					// In some cases, users feed a navmesh graph a mesh with degenerate triangles.
					// These are triangles with a zero area.
					// We must remove these as they can otherwise cause issues for the JobCalculateTriangleConnections job, and they are generally just bad to include a navmesh.
					// Note: This cross product calculation can result in overflows if the triangle is large, but since we check for equality with zero it should not be a problem in practice.
					if (math.all(cross(verticesSpan[tri.y] - verticesSpan[tri.x], verticesSpan[tri.z] - verticesSpan[tri.x]) == 0)) {
						// Degenerate triangle
						numDegenerate++;
						continue;
					}
					trianglesSpan[triCount] = tri;
					tagsSpan[triCount] = tagsSpan[ti];
					triCount++;
				}

				triangles.Length = (int)triCount * 3;
				tags.Length = (int)triCount;
				if (verbose && numDegenerate > 0) {
					Debug.LogWarning($"Input mesh contained {numDegenerate} degenerate triangles. These have been removed.\nA degenerate triangle is a triangle with zero area. It resembles a line or a point.");
				}
			}
		}
	}
}
