using Pathfinding.Util;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Pathfinding.Collections;
using Pathfinding.Sync;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>
	/// Builds tiles from raw mesh vertices and indices.
	///
	/// This job takes the following steps:
	/// - Transform all vertices using the <see cref="meshToGraph"/> matrix.
	/// - Remove duplicate vertices
	/// - If <see cref="recalculateNormals"/> is enabled: ensure all triangles are laid out in the clockwise direction.
	/// </summary>
	[BurstCompile(FloatMode = FloatMode.Default)]
	public struct JobBuildTileMeshFromVertices : IJob {
		public NativeArray<Vector3> vertices;
		public NativeArray<int> indices;
		public Matrix4x4 meshToGraph;
		public NativeArray<TileMesh.TileMeshUnsafe> outputBuffers;
		public bool recalculateNormals;


		[BurstCompile(FloatMode = FloatMode.Fast)]
		public struct JobTransformTileCoordinates : IJob {
			public NativeArray<Vector3> vertices;
			public NativeArray<Int3> outputVertices;
			public Matrix4x4 matrix;

			public void Execute () {
				if (vertices.Length != outputVertices.Length) throw new System.ArgumentException("Input and output arrays must have the same length");
				for (int i = 0; i < vertices.Length; i++) {
					outputVertices[i] = (Int3)matrix.MultiplyPoint3x4(vertices[i]);
				}
			}
		}

		public static Promise<TileBuilder.TileBuilderOutput> Schedule (NativeArray<Vector3> vertices, NativeArray<int> indices, Matrix4x4 meshToGraph, bool recalculateNormals) {
			if (vertices.Length > NavmeshBase.VertexIndexMask) throw new System.ArgumentException("Too many vertices in the navmesh graph. Provided " + vertices.Length + ", but the maximum number of vertices per tile is " + NavmeshBase.VertexIndexMask + ". You can raise this limit by enabling ASTAR_RECAST_LARGER_TILES in the A* Inspector Optimizations tab");

			var outputBuffers = new NativeArray<TileMesh.TileMeshUnsafe>(1, Allocator.Persistent);

			var job = new JobBuildTileMeshFromVertices {
				vertices = vertices,
				indices = indices,
				meshToGraph = meshToGraph,
				outputBuffers = outputBuffers,
				recalculateNormals = recalculateNormals,
			}.Schedule();
			return new Promise<TileBuilder.TileBuilderOutput>(job, new TileBuilder.TileBuilderOutput {
				// TODO: Tile world size is wrong
				tileMeshes = new TileMeshesUnsafe(outputBuffers, new IntRect(0, 0, 0, 0), new Vector2(100000, 100000)),
			});
		}

		public void Execute () {
			var int3vertices = new NativeList<Int3>(vertices.Length, Allocator.Temp);
			int3vertices.Length = vertices.Length;
			var tags = new NativeList<int>(indices.Length / 3, Allocator.Temp);
			tags.Length = indices.Length / 3;
			var triangles = new NativeList<int>(indices.Length, Allocator.Temp);
			triangles.AddRange(indices);

			new JobTransformTileCoordinates {
				vertices = vertices,
				outputVertices = int3vertices.AsArray(),
				matrix = meshToGraph,
			}.Execute();

			unsafe {
				UnityEngine.Assertions.Assert.IsTrue(this.outputBuffers.Length == 1);
				var tile = (TileMesh.TileMeshUnsafe*) this.outputBuffers.GetUnsafePtr();
				new MeshUtility.JobMergeNearbyVertices {
					vertices = int3vertices,
					triangles = triangles,
					mergeRadiusSq = 0,
				}.Execute();
				new MeshUtility.JobRemoveDegenerateTriangles {
					vertices = int3vertices,
					triangles = triangles,
					tags = tags,
				}.Execute();

				// Convert the buffers to spans that own their memory.
				// The spans may be smaller than the underlaying allocation,
				// but the whole allocation will be freed using the span's Free method.
				tile->verticesInTileSpace = int3vertices.AsUnsafeSpan<Int3>().Clone(Allocator.Persistent);
				tile->triangles = triangles.AsUnsafeSpan<int>().Clone(Allocator.Persistent);
				tile->tags = tags.AsUnsafeSpan().Reinterpret<uint>().Clone(Allocator.Persistent);

				if (recalculateNormals) {
					MeshUtility.MakeTrianglesClockwise(ref tile->verticesInTileSpace, ref tile->triangles);
				}
			}

			int3vertices.Dispose();
		}
	}
}
