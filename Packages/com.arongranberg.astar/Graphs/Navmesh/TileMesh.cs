using Pathfinding.Collections;
using Unity.Collections;

namespace Pathfinding.Graphs.Navmesh {
	/// <summary>
	/// A tile in a navmesh graph.
	///
	/// This is an intermediate representation used when building the navmesh, and also in some cases for serializing the navmesh to a portable format.
	///
	/// See: <see cref="NavmeshTile"/> for the representation used for pathfinding.
	/// </summary>
	public struct TileMesh {
		public int[] triangles;
		public Int3[] verticesInTileSpace;
		/// <summary>One tag per triangle</summary>
		public uint[] tags;

		/// <summary>Unsafe version of <see cref="TileMesh"/></summary>
		public struct TileMeshUnsafe {
			/// <summary>Three indices per triangle</summary>
			public UnsafeSpan<int> triangles;
			/// <summary>One vertex per triangle</summary>
			public UnsafeSpan<Int3> verticesInTileSpace;
			/// <summary>One tag per triangle</summary>
			public UnsafeSpan<uint> tags;

			/// <summary>
			/// Frees the underlaying memory.
			/// This struct should not be used after this method has been called.
			///
			/// Warning: Only call if you know that the memory is owned by this struct, as it is entirely possible for it to just represent views into other memory.
			/// </summary>
			public void Dispose (Allocator allocator) {
				triangles.Free(allocator);
				verticesInTileSpace.Free(allocator);
				tags.Free(allocator);
			}

			public TileMesh ToManaged () {
				return new TileMesh {
						   triangles = triangles.ToArray(),
						   verticesInTileSpace = verticesInTileSpace.ToArray(),
						   tags = tags.ToArray(),
				};
			}
		}
	}
}
