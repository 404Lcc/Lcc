using Unity.Collections;
using Unity.Profiling;

namespace Pathfinding.Graphs.Navmesh {
	using Pathfinding.Util;
	using Pathfinding.Collections;

	/// <summary>
	/// A single tile in a recast or navmesh graph.
	///
	/// A tile is a single rectangular (but usually square) part of the graph.
	/// Tiles can be updated individually, which is great for large worlds where updating the whole graph would take a long time.
	/// </summary>
	public class NavmeshTile : INavmeshHolder {
		/// <summary>
		/// All vertices in the tile.
		/// The vertices are in graph space.
		///
		/// This represents an allocation using the Persistent allocator.
		/// </summary>
		public UnsafeSpan<Int3> vertsInGraphSpace;
		/// <summary>
		/// All vertices in the tile.
		/// The vertices are in world space.
		///
		/// This represents an allocation using the Persistent allocator.
		/// </summary>
		public UnsafeSpan<Int3> verts;
		/// <summary>
		/// All triangle indices in the tile.
		/// One triangle is 3 indices.
		/// The triangles are in the same order as the <see cref="nodes"/>.
		///
		/// This represents an allocation using the Persistent allocator.
		/// </summary>
		public UnsafeSpan<int> tris;

		/// <summary>
		/// True if this tile may have been cut by <see cref="NavmeshCut"/>s, or had pieces added by <see cref="NavmeshAdd"/> components.
		///
		/// If true, the <see cref="preCutVertsInTileSpace"/>, <see cref="preCutTris"/> and <see cref="preCutTags"/> fields will be valid.
		/// </summary>
		public bool isCut;
		public UnsafeSpan<Int3> preCutVertsInTileSpace;
		public UnsafeSpan<int> preCutTris;
		public UnsafeSpan<uint> preCutTags;


		/// <summary>Tile X Coordinate</summary>
		public int x;

		/// <summary>Tile Z Coordinate</summary>
		public int z;

		/// <summary>
		/// Width, in tile coordinates.
		/// Warning: Widths other than 1 are not supported. This is mainly here for possible future features.
		/// </summary>
		public int w;

		/// <summary>
		/// Depth, in tile coordinates.
		/// Warning: Depths other than 1 are not supported. This is mainly here for possible future features.
		/// </summary>
		public int d;

		/// <summary>All nodes in the tile</summary>
		public TriangleMeshNode[] nodes;

		/// <summary>Bounding Box Tree for node lookups</summary>
		public BBTree bbTree;

		/// <summary>Temporary flag used for batching</summary>
		public bool flag;

		/// <summary>The graph which contains this tile</summary>
		public NavmeshBase graph;

		#region INavmeshHolder implementation

		public void GetTileCoordinates (int tileIndex, out int x, out int z) {
			x = this.x;
			z = this.z;
		}

		public int GetVertexArrayIndex (int index) {
			return index & NavmeshBase.VertexIndexMask;
		}

		/// <summary>Get a specific vertex in the tile</summary>
		public Int3 GetVertex (int index) {
			int idx = index & NavmeshBase.VertexIndexMask;

			return verts[idx];
		}

		[IgnoredByDeepProfiler]
		public Int3 GetVertexInGraphSpace (int index) {
			return vertsInGraphSpace[index & NavmeshBase.VertexIndexMask];
		}

		/// <summary>Transforms coordinates from graph space to world space</summary>
		public GraphTransform transform { get { return graph.transform; } }

		#endregion

		public void GetNodes (System.Action<GraphNode> action) {
			if (nodes == null) return;
			for (int i = 0; i < nodes.Length; i++) action(nodes[i]);
		}

		public void Dispose () {
			unsafe {
				bbTree.Dispose();
				vertsInGraphSpace.Free(Allocator.Persistent);
				verts.Free(Allocator.Persistent);
				tris.Free(Allocator.Persistent);
				preCutTags.Free(Allocator.Persistent);
				preCutVertsInTileSpace.Free(Allocator.Persistent);
				preCutTris.Free(Allocator.Persistent);
				// Ensure Dispose is idempotent
				vertsInGraphSpace = default;
				verts = default;
				tris = default;
				preCutTags = default;
				preCutVertsInTileSpace = default;
				preCutTris = default;
				isCut = false;
			}
		}
	}
}
