using UnityEngine;
using Pathfinding.Util;
using UnityEngine.Tilemaps;

namespace Pathfinding.Graphs.Navmesh {
	/// <summary>
	/// Represents the position and size of a tile grid for a recast/navmesh graph.
	///
	/// This separates out the physical layout of tiles from all the other recast graph settings.
	/// </summary>
	public struct TileLayout {
		/// <summary>How many tiles there are in the grid</summary>
		public Vector2Int tileCount;

		/// <summary>Transforms coordinates from graph space to world space</summary>
		public GraphTransform transform;

		/// <summary>Size of a tile in voxels along the X and Z axes</summary>
		public Vector2Int tileSizeInVoxels;

		/// <summary>
		/// Size in graph space of the whole grid.
		///
		/// If the original bounding box was not an exact multiple of the tile size, this will be less than the total width of all tiles.
		/// </summary>
		public Vector3 graphSpaceSize;

		/// <summary>\copydocref{RecastGraph.cellSize}</summary>
		public float cellSize;

		/// <summary>
		/// Voxel y coordinates will be stored as ushorts which have 65536 values.
		/// Leave a margin to make sure things do not overflow
		/// </summary>
		public float CellHeight => Mathf.Max(graphSpaceSize.y / 64000, 0.001f);

		public Vector2 TileWorldSize => new Vector2(TileWorldSizeX, TileWorldSizeZ);

		/// <summary>Size of a tile in world units, along the graph's X axis</summary>
		public float TileWorldSizeX => tileSizeInVoxels.x * cellSize;

		/// <summary>Size of a tile in world units, along the graph's Z axis</summary>
		public float TileWorldSizeZ => tileSizeInVoxels.y * cellSize;

		/// <summary>Returns an XZ bounds object with the bounds of a group of tiles in graph space</summary>
		public Bounds GetTileBoundsInGraphSpace (int x, int z, int width = 1, int depth = 1) {
			var bounds = new Bounds();

			bounds.SetMinMax(new Vector3(x*TileWorldSizeX, 0, z*TileWorldSizeZ),
				new Vector3((x+width)*TileWorldSizeX, graphSpaceSize.y, (z+depth)*TileWorldSizeZ)
				);

			return bounds;
		}

		/// <summary>
		/// Returns a rect containing the indices of all tiles touching the specified bounds.
		/// If a margin is passed, the bounding box in graph space is expanded by that amount in every direction.
		/// </summary>
		public IntRect GetTouchingTiles (Bounds bounds, float margin = 0) {
			bounds = transform.InverseTransform(bounds);

			// Calculate world bounds of all affected tiles
			return new IntRect(Mathf.FloorToInt((bounds.min.x - margin) / TileWorldSizeX), Mathf.FloorToInt((bounds.min.z - margin) / TileWorldSizeZ), Mathf.FloorToInt((bounds.max.x + margin) / TileWorldSizeX), Mathf.FloorToInt((bounds.max.z + margin) / TileWorldSizeZ));
		}

		/// <summary>Returns a rect containing the indices of all tiles touching the specified bounds.</summary>
		/// <param name="rect">Graph space rectangle (in graph space all tiles are on the XZ plane regardless of graph rotation and other transformations, the first tile has a corner at the origin)</param>
		public IntRect GetTouchingTilesInGraphSpace (Rect rect) {
			// Calculate world bounds of all affected tiles
			var r = new IntRect(Mathf.FloorToInt(rect.xMin / TileWorldSizeX), Mathf.FloorToInt(rect.yMin / TileWorldSizeZ), Mathf.FloorToInt(rect.xMax / TileWorldSizeX), Mathf.FloorToInt(rect.yMax / TileWorldSizeZ));

			// Clamp to bounds
			r = IntRect.Intersection(r, new IntRect(0, 0, tileCount.x-1, tileCount.y-1));
			return r;
		}

		public TileLayout(RecastGraph graph) : this(new Bounds(graph.forcedBoundsCenter, graph.forcedBoundsSize), Quaternion.Euler(graph.rotation), graph.cellSize, graph.editorTileSize, graph.useTiles) {
		}

		public TileLayout(NavMeshGraph graph) : this(new Bounds(graph.transform.Transform(graph.forcedBoundsSize*0.5f), graph.forcedBoundsSize), Quaternion.Euler(graph.rotation), 0.001f, 0, false) {
		}

		public TileLayout(Bounds bounds, Quaternion rotation, float cellSize, int tileSizeInVoxels, bool useTiles) {
			this.transform = RecastGraph.CalculateTransform(bounds, rotation);
			this.cellSize = cellSize;

			// Voxel grid size
			var size = bounds.size;
			graphSpaceSize = size;
			int totalVoxelWidth = (int)(size.x/cellSize + 0.5f);
			int totalVoxelDepth = (int)(size.z/cellSize + 0.5f);

			if (!useTiles) {
				this.tileSizeInVoxels = new Vector2Int(totalVoxelWidth, totalVoxelDepth);
				tileCount = new Vector2Int(1, 1);
			} else {
				this.tileSizeInVoxels = new Vector2Int(tileSizeInVoxels, tileSizeInVoxels);
				// Number of tiles
				tileCount = new Vector2Int(
					Mathf.Max(0, (totalVoxelWidth + this.tileSizeInVoxels.x-1) / this.tileSizeInVoxels.x),
					Mathf.Max(0, (totalVoxelDepth + this.tileSizeInVoxels.y-1) / this.tileSizeInVoxels.y)
					);
			}

			if (tileCount.x*tileCount.y > NavmeshBase.TileIndexMask + 1) {
				throw new System.Exception("Too many tiles ("+(tileCount.x*tileCount.y)+") maximum is "+(NavmeshBase.TileIndexMask + 1)+
					"\nTry disabling ASTAR_RECAST_LARGER_TILES under the 'Optimizations' tab in the A* inspector.");
			}
		}
	}
}
