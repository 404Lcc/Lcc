using Unity.Collections;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Pathfinding.Graphs.Navmesh.Jobs {
	/// <summary>
	/// Connects adjacent tiles together.
	///
	/// This only creates connections between tiles. Connections internal to a tile should be handled by <see cref="JobCalculateTriangleConnections"/>.
	///
	/// Use the <see cref="ScheduleBatch"/> method to connect a bunch of tiles efficiently using maximum parallelism.
	/// </summary>
	public struct JobConnectTiles : IJob {
		/// <summary>GCHandle referring to a NavmeshTile[] array of size tileRect.Width*tileRect.Height</summary>
		public System.Runtime.InteropServices.GCHandle tiles;
		public int coordinateSum;
		public int direction;
		public int zOffset;
		public int zStride;
		Vector2 tileWorldSize;
		IntRect tileRect;
		/// <summary>Maximum vertical distance between two tiles to create a connection between them</summary>
		public float maxTileConnectionEdgeDistance;

		static readonly Unity.Profiling.ProfilerMarker ConnectTilesMarker = new Unity.Profiling.ProfilerMarker("ConnectTiles");

		/// <summary>
		/// Schedule jobs to connect all the given tiles with each other while exploiting as much parallelism as possible.
		/// tilesHandle should be a GCHandle referring to a NavmeshTile[] array of size tileRect.Width*tileRect.Height.
		/// </summary>
		public static JobHandle ScheduleBatch (System.Runtime.InteropServices.GCHandle tilesHandle, JobHandle dependency, IntRect tileRect, Vector2 tileWorldSize, float maxTileConnectionEdgeDistance) {
			// First connect all tiles with an EVEN coordinate sum
			// This would be the white squares on a chess board.
			// Then connect all tiles with an ODD coordinate sum (which would be all black squares on a chess board).
			// This will prevent the different threads that do all
			// this in parallel from conflicting with each other.
			// The directions are also done separately
			// first they are connected along the X direction and then along the Z direction.
			// Looping over 0 and then 1

			int workers = Mathf.Max(1, JobsUtility.JobWorkerCount);
			var handles = new NativeArray<JobHandle>(workers, Allocator.Temp);
			for (int coordinateSum = 0; coordinateSum <= 1; coordinateSum++) {
				for (int direction = 0; direction <= 1; direction++) {
					for (int i = 0; i < workers; i++) {
						handles[i] = new JobConnectTiles {
							tiles = tilesHandle,
							tileRect = tileRect,
							tileWorldSize = tileWorldSize,
							coordinateSum = coordinateSum,
							direction = direction,
							maxTileConnectionEdgeDistance = maxTileConnectionEdgeDistance,
							zOffset = i,
							zStride = workers,
						}.Schedule(dependency);
					}
					dependency = JobHandle.CombineDependencies(handles);
				}
			}

			return dependency;
		}

		/// <summary>
		/// Schedule jobs to connect all the given tiles inside innerRect with tiles that are outside it, while exploiting as much parallelism as possible.
		/// tilesHandle should be a GCHandle referring to a NavmeshTile[] array of size tileRect.Width*tileRect.Height.
		/// </summary>
		public static JobHandle ScheduleRecalculateBorders (System.Runtime.InteropServices.GCHandle tilesHandle, JobHandle dependency, IntRect tileRect, IntRect innerRect, Vector2 tileWorldSize, float maxTileConnectionEdgeDistance) {
			var w = innerRect.Width;
			var h = innerRect.Height;

			// Note: conservative estimate of number of handles. There may be fewer in reality.
			var allDependencies = new NativeArray<JobHandle>(2*w + 2*math.max(0, h - 2), Allocator.Temp);
			int count = 0;
			for (int z = 0; z < h; z++) {
				for (int x = 0; x < w; x++) {
					// Check if the tile is on the border of the inner rect
					if (!(x == 0 || z == 0 || x == w - 1 || z == h - 1)) continue;

					var tileX = innerRect.xmin + x;
					var tileZ = innerRect.ymin + z;

					// For a corner tile, the jobs need to run sequentially
					var dep = dependency;
					for (int direction = 0; direction < 4; direction++) {
						var nx = tileX + (direction == 0 ? 1 : direction == 1 ? -1 : 0);
						var nz = tileZ + (direction == 2 ? 1 : direction == 3 ? -1 : 0);
						if (innerRect.Contains(nx, nz) || !tileRect.Contains(nx, nz)) {
							continue;
						}

						dep = new JobConnectTilesSingle {
							tiles = tilesHandle,
							tileIndex1 = tileX + tileZ * tileRect.Width,
							tileIndex2 = nx + nz * tileRect.Width,
							tileWorldSize = tileWorldSize,
							maxTileConnectionEdgeDistance = maxTileConnectionEdgeDistance,
						}.Schedule(dep);
					}

					allDependencies[count++] = dep;
				}
			}
			return JobHandle.CombineDependencies(allDependencies);
		}

		public void Execute () {
			var tiles = (NavmeshTile[])this.tiles.Target;

			var tileRectDepth = tileRect.Height;
			var tileRectWidth = tileRect.Width;
			for (int z = zOffset; z < tileRectDepth; z += zStride) {
				for (int x = 0; x < tileRectWidth; x++) {
					if ((x + z) % 2 == coordinateSum) {
						int tileIndex1 = x + z * tileRectWidth;
						int tileIndex2;
						if (direction == 0 && x < tileRectWidth - 1) {
							tileIndex2 = x + 1 + z * tileRectWidth;
						} else if (direction == 1 && z < tileRectDepth - 1) {
							tileIndex2 = x + (z + 1) * tileRectWidth;
						} else {
							continue;
						}

						ConnectTilesMarker.Begin();
						NavmeshBase.ConnectTiles(tiles[tileIndex1], tiles[tileIndex2], tileWorldSize.x, tileWorldSize.y, maxTileConnectionEdgeDistance);
						ConnectTilesMarker.End();
					}
				}
			}
		}
	}

	/// <summary>
	/// Connects two adjacent tiles together.
	///
	/// This only creates connections between tiles. Connections internal to a tile should be handled by <see cref="JobCalculateTriangleConnections"/>.
	/// </summary>
	struct JobConnectTilesSingle : IJob {
		/// <summary>GCHandle referring to a NavmeshTile[] array of size tileRect.Width*tileRect.Height</summary>
		public System.Runtime.InteropServices.GCHandle tiles;
		/// <summary>Index of the first tile in the <see cref="tiles"/> array</summary>
		public int tileIndex1;
		/// <summary>Index of the second tile in the <see cref="tiles"/> array</summary>
		public int tileIndex2;
		/// <summary>Size of a tile in world units</summary>
		public Vector2 tileWorldSize;
		/// <summary>Maximum vertical distance between two tiles to create a connection between them</summary>
		public float maxTileConnectionEdgeDistance;

		public void Execute () {
			var tiles = (NavmeshTile[])this.tiles.Target;

			NavmeshBase.ConnectTiles(tiles[tileIndex1], tiles[tileIndex2], tileWorldSize.x, tileWorldSize.y, maxTileConnectionEdgeDistance);
		}
	}
}
