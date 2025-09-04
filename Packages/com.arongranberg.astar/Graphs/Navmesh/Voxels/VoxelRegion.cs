using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Pathfinding.Util;

namespace Pathfinding.Graphs.Navmesh.Voxelization.Burst {
	[BurstCompile(CompileSynchronously = true)]
	public struct JobBuildRegions : IJob {
		public CompactVoxelField field;
		public NativeList<ushort> distanceField;
		public int borderSize;
		public int minRegionSize;
		public NativeQueue<Int3> srcQue;
		public NativeQueue<Int3> dstQue;
		public RecastGraph.RelevantGraphSurfaceMode relevantGraphSurfaceMode;
		public NativeArray<RelevantGraphSurfaceInfo> relevantGraphSurfaces;

		public float cellSize, cellHeight;
		public Matrix4x4 graphTransform;
		public Bounds graphSpaceBounds;

		void MarkRectWithRegion (int minx, int maxx, int minz, int maxz, ushort region, NativeArray<ushort> srcReg) {
			int md = maxz * field.width;

			for (int z = minz*field.width; z < md; z += field.width) {
				for (int x = minx; x < maxx; x++) {
					CompactVoxelCell c = field.cells[z+x];

					for (int i = c.index, ni = c.index+c.count; i < ni; i++) {
						if (field.areaTypes[i] != CompactVoxelField.UnwalkableArea) {
							srcReg[i] = region;
						}
					}
				}
			}
		}

		public static bool FloodRegion (int x, int z, int i, uint level, ushort r,
										CompactVoxelField field,
										NativeArray<ushort> distanceField,
										NativeArray<ushort> srcReg,
										NativeArray<ushort> srcDist,
										NativeArray<Int3> stack,
										NativeArray<int> flags,
										NativeArray<bool> closed) {
			int area = field.areaTypes[i];

			// Flood f mark region.
			int stackSize = 1;

			stack[0] = new Int3 {
				x = x,
				y = i,
				z = z,
			};

			srcReg[i] = r;
			srcDist[i] = 0;

			int lev = (int)(level >= 2 ? level-2 : 0);

			int count = 0;

			// Store these in local variables (for performance, avoids an extra indirection)
			var compactCells = field.cells;
			var compactSpans = field.spans;
			var areaTypes = field.areaTypes;

			while (stackSize > 0) {
				stackSize--;
				var c = stack[stackSize];
				//Similar to the Pop operation of an array, but Pop is not implemented in List<>
				int ci = c.y;
				int cx = c.x;
				int cz = c.z;

				CompactVoxelSpan cs = compactSpans[ci];

				// Check if any of the neighbours already have a valid region set.
				ushort ar = 0;

				// Loop through four neighbours
				// then check one neighbour of the neighbour
				// to get the diagonal neighbour
				for (int dir = 0; dir < 4; dir++) {
					// 8 connected
					if (cs.GetConnection(dir) != CompactVoxelField.NotConnected) {
						int ax = cx + VoxelUtilityBurst.DX[dir];
						int az = cz + VoxelUtilityBurst.DZ[dir]*field.width;

						int ai = (int)compactCells[ax+az].index + cs.GetConnection(dir);

						if (areaTypes[ai] != area)
							continue;

						ushort nr = srcReg[ai];

						if ((nr & VoxelUtilityBurst.BorderReg) == VoxelUtilityBurst.BorderReg) // Do not take borders into account.
							continue;

						if (nr != 0 && nr != r) {
							ar = nr;
							// Found a valid region, skip checking the rest
							break;
						}

						// Rotate dir 90 degrees
						int dir2 = (dir+1) & 0x3;
						var neighbour2 = compactSpans[ai].GetConnection(dir2);
						// Check the diagonal connection
						if (neighbour2 != CompactVoxelField.NotConnected) {
							int ax2 = ax + VoxelUtilityBurst.DX[dir2];
							int az2 = az + VoxelUtilityBurst.DZ[dir2]*field.width;

							int ai2 = compactCells[ax2+az2].index + neighbour2;

							if (areaTypes[ai2] != area)
								continue;

							ushort nr2 = srcReg[ai2];

							if ((nr2 & VoxelUtilityBurst.BorderReg) == VoxelUtilityBurst.BorderReg) // Do not take borders into account.
								continue;

							if (nr2 != 0 && nr2 != r) {
								ar = nr2;
								// Found a valid region, skip checking the rest
								break;
							}
						}
					}
				}

				if (ar != 0) {
					srcReg[ci] = 0;
					srcDist[ci] = 0xFFFF;
					continue;
				}
				count++;
				closed[ci] = true;


				// Expand neighbours.
				for (int dir = 0; dir < 4; ++dir) {
					if (cs.GetConnection(dir) == CompactVoxelField.NotConnected) continue;
					int ax = cx + VoxelUtilityBurst.DX[dir];
					int az = cz + VoxelUtilityBurst.DZ[dir]*field.width;
					int ai = compactCells[ax+az].index + cs.GetConnection(dir);

					if (areaTypes[ai] != area) continue;
					if (srcReg[ai] != 0) continue;

					if (distanceField[ai] >= lev && flags[ai] == 0) {
						srcReg[ai] = r;
						srcDist[ai] = 0;

						stack[stackSize] = new Int3 {
							x = ax,
							y = ai,
							z = az,
						};
						stackSize++;
					} else {
						flags[ai] = r;
						srcDist[ai] = 2;
					}
				}
			}


			return count > 0;
		}

		public void Execute () {
			srcQue.Clear();
			dstQue.Clear();

			int w = field.width;
			int d = field.depth;
			int wd = w*d;
			int spanCount = field.spans.Length;

			int expandIterations = 8;

			var srcReg = new NativeArray<ushort>(spanCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			var srcDist = new NativeArray<ushort>(spanCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			var closed = new NativeArray<bool>(spanCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			var spanFlags = new NativeArray<int>(spanCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
			var stack = new NativeArray<Int3>(spanCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

			// The array pool arrays may contain arbitrary data. We need to zero it out.
			for (int i = 0; i < spanCount; i++) {
				srcReg[i] = 0;
				srcDist[i] = 0xFFFF;
				closed[i] = false;
				spanFlags[i] = 0;
			}

			var spanDistances = distanceField;
			var areaTypes = field.areaTypes;
			var compactCells = field.cells;
			const ushort BorderReg = VoxelUtilityBurst.BorderReg;

			ushort regionId = 2;
			MarkRectWithRegion(0, borderSize, 0, d,    (ushort)(regionId | BorderReg), srcReg);    regionId++;
			MarkRectWithRegion(w-borderSize, w, 0, d,  (ushort)(regionId | BorderReg), srcReg);    regionId++;
			MarkRectWithRegion(0, w, 0, borderSize,    (ushort)(regionId | BorderReg), srcReg);    regionId++;
			MarkRectWithRegion(0, w, d-borderSize, d,  (ushort)(regionId | BorderReg), srcReg);    regionId++;

			// TODO: Can be optimized
			int maxDistance = 0;
			for (int i = 0; i < distanceField.Length; i++) {
				maxDistance = math.max(distanceField[i], maxDistance);
			}

			// A distance is 2 to an adjacent span and 1 for a diagonally adjacent one.
			NativeArray<int> sortedSpanCounts = new NativeArray<int>((maxDistance)/2 + 1, Allocator.Temp);
			for (int i = 0; i < field.spans.Length; i++) {
				// Do not take borders or unwalkable spans into account.
				if ((srcReg[i] & BorderReg) == BorderReg || areaTypes[i] == CompactVoxelField.UnwalkableArea)
					continue;

				sortedSpanCounts[distanceField[i]/2]++;
			}

			var distanceIndexOffsets = new NativeArray<int>(sortedSpanCounts.Length, Allocator.Temp);
			for (int i = 1; i < distanceIndexOffsets.Length; i++) {
				distanceIndexOffsets[i] = distanceIndexOffsets[i-1] + sortedSpanCounts[i-1];
			}
			var totalRelevantSpans = distanceIndexOffsets[distanceIndexOffsets.Length - 1] + sortedSpanCounts[sortedSpanCounts.Length - 1];

			var bucketSortedSpans = new NativeArray<Int3>(totalRelevantSpans, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

			// Bucket sort the spans based on distance
			for (int z = 0, pz = 0; z < wd; z += w, pz++) {
				for (int x = 0; x < field.width; x++) {
					CompactVoxelCell c = compactCells[z+x];

					for (int i = c.index, ni = c.index+c.count; i < ni; i++) {
						// Do not take borders or unwalkable spans into account.
						if ((srcReg[i] & BorderReg) == BorderReg || areaTypes[i] == CompactVoxelField.UnwalkableArea)
							continue;

						int distIndex = distanceField[i] / 2;
						bucketSortedSpans[distanceIndexOffsets[distIndex]++] = new Int3(x, i, z);
					}
				}
			}

#if ENABLE_UNITY_COLLECTIONS_CHECKS
			if (distanceIndexOffsets[distanceIndexOffsets.Length - 1] != totalRelevantSpans) throw new System.Exception("Unexpected span count");
#endif

			// Go through spans in reverse order (i.e largest distances first)
			for (int distIndex = sortedSpanCounts.Length - 1; distIndex >= 0; distIndex--) {
				var level = (uint)distIndex * 2;
				var spansAtLevel = sortedSpanCounts[distIndex];
				for (int i = 0; i < spansAtLevel; i++) {
					// Go through the spans stored in bucketSortedSpans for this distance index.
					// Note that distanceIndexOffsets[distIndex] will point to the element after the end of the group of spans.
					// There is no particular reason for this, the code just turned out to be a bit simpler to implemen that way.
					var spanInfo = bucketSortedSpans[distanceIndexOffsets[distIndex] - i - 1];
					int spanIndex = spanInfo.y;

					// This span is adjacent to a region, so we should start the BFS search from it
					if (spanFlags[spanIndex] != 0 && srcReg[spanIndex] == 0) {
						srcReg[spanIndex] = (ushort)spanFlags[spanIndex];
						srcQue.Enqueue(spanInfo);
						closed[spanIndex] = true;
					}
				}

				// Expand a few iterations out from every known node
				for (int expansionIteration = 0; expansionIteration < expandIterations && srcQue.Count > 0; expansionIteration++) {
					while (srcQue.Count > 0) {
						Int3 spanInfo = srcQue.Dequeue();
						var area = areaTypes[spanInfo.y];
						var span = field.spans[spanInfo.y];
						var region = srcReg[spanInfo.y];
						closed[spanInfo.y] = true;
						ushort nextDist = (ushort)(srcDist[spanInfo.y] + 2);

						// Go through the neighbours of the span
						for (int dir = 0; dir < 4; dir++) {
							var neighbour = span.GetConnection(dir);
							if (neighbour == CompactVoxelField.NotConnected) continue;

							int nx = spanInfo.x + VoxelUtilityBurst.DX[dir];
							int nz = spanInfo.z + VoxelUtilityBurst.DZ[dir]*field.width;

							int ni = compactCells[nx+nz].index + neighbour;

							if ((srcReg[ni] & BorderReg) == BorderReg) // Do not take borders into account.
								continue;

							// Do not combine different area types
							if (area == areaTypes[ni]) {
								if (nextDist < srcDist[ni]) {
									if (spanDistances[ni] < level) {
										srcDist[ni] = nextDist;
										spanFlags[ni] = region;
									} else if (!closed[ni]) {
										srcDist[ni] = nextDist;
										if (srcReg[ni] == 0) dstQue.Enqueue(new Int3(nx, ni, nz));
										srcReg[ni] = region;
									}
								}
							}
						}
					}
					Memory.Swap(ref srcQue, ref dstQue);
				}

				// Find the first span that has not been seen yet and start a new region that expands from there
				var distanceFieldArr = distanceField.AsArray();
				for (int i = 0; i < spansAtLevel; i++) {
					var info = bucketSortedSpans[distanceIndexOffsets[distIndex] - i - 1];
					if (srcReg[info.y] == 0) {
						if (!FloodRegion(info.x, info.z, info.y, level, regionId, field, distanceFieldArr, srcReg, srcDist, stack, spanFlags, closed)) {
							// The starting voxel was already adjacent to an existing region so we skip flooding it.
							// It will be visited in the next area expansion.
						} else {
							regionId++;
						}
					}
				}
			}

			var maxRegions = regionId;

			// Transform from voxel space to graph space.
			// then scale from voxel space (one unit equals one voxel)
			// Finally add min
			Matrix4x4 voxelMatrix = Matrix4x4.TRS(graphSpaceBounds.min, Quaternion.identity, Vector3.one) * Matrix4x4.Scale(new Vector3(cellSize, cellHeight, cellSize));

			// Transform from voxel space to world space
			// add half a voxel to fix rounding
			var voxel2worldMatrix = graphTransform * voxelMatrix * Matrix4x4.Translate(new Vector3(0.5f, 0, 0.5f));

			// Filter out small regions.
			FilterSmallRegions(field, srcReg, minRegionSize, maxRegions, this.relevantGraphSurfaces, this.relevantGraphSurfaceMode, voxel2worldMatrix);

			// Write the result out.
			for (int i = 0; i < spanCount; i++) {
				var span = field.spans[i];
				span.reg = srcReg[i];
				field.spans[i] = span;
			}

			// TODO:
			// field.maxRegions = maxRegions;

// #if ASTAR_DEBUGREPLAY
// 			DebugReplay.BeginGroup("Regions");
// 			for (int z = 0, pz = 0; z < wd; z += field.width, pz++) {
// 				for (int x = 0; x < field.width; x++) {
// 					CompactVoxelCell c = field.cells[x+z];
// 					for (int i = (int)c.index; i < c.index+c.count; i++) {
// 						CompactVoxelSpan s = field.spans[i];
// 						DebugReplay.DrawCube(CompactSpanToVector(x, pz, i), UnityEngine.Vector3.one*cellSize, AstarMath.IntToColor(s.reg, 1.0f));
// 					}
// 				}
// 			}

// 			DebugReplay.EndGroup();

// 			int maxDist = 0;
// 			for (int i = 0; i < srcDist.Length; i++) if (srcDist[i] != 0xFFFF) maxDist = Mathf.Max(maxDist, srcDist[i]);

// 			DebugReplay.BeginGroup("Distances");
// 			for (int z = 0, pz = 0; z < wd; z += field.width, pz++) {
// 				for (int x = 0; x < field.width; x++) {
// 					CompactVoxelCell c = field.cells[x+z];
// 					for (int i = (int)c.index; i < c.index+c.count; i++) {
// 						CompactVoxelSpan s = field.spans[i];
// 						float f = (float)srcDist[i]/maxDist;
// 						DebugReplay.DrawCube(CompactSpanToVector(x, z/field.width, i), Vector3.one*cellSize, new Color(f, f, f));
// 					}
// 				}
// 			}

// 			DebugReplay.EndGroup();
// #endif
		}

		/// <summary>
		/// Find method in the UnionFind data structure.
		/// See: https://en.wikipedia.org/wiki/Disjoint-set_data_structure
		/// </summary>
		static int union_find_find (NativeArray<int> arr, int x) {
			if (arr[x] < 0) return x;
			return arr[x] = union_find_find(arr, arr[x]);
		}

		/// <summary>
		/// Join method in the UnionFind data structure.
		/// See: https://en.wikipedia.org/wiki/Disjoint-set_data_structure
		/// </summary>
		static void union_find_union (NativeArray<int> arr, int a, int b) {
			a = union_find_find(arr, a);
			b = union_find_find(arr, b);
			if (a == b) return;
			if (arr[a] > arr[b]) {
				int tmp = a;
				a = b;
				b = tmp;
			}
			arr[a] += arr[b];
			arr[b] = a;
		}

		public struct RelevantGraphSurfaceInfo {
			public float3 position;
			public float range;
		}

		/// <summary>Filters out or merges small regions.</summary>
		public static void FilterSmallRegions (CompactVoxelField field, NativeArray<ushort> reg, int minRegionSize, int maxRegions, NativeArray<RelevantGraphSurfaceInfo> relevantGraphSurfaces, RecastGraph.RelevantGraphSurfaceMode relevantGraphSurfaceMode, float4x4 voxel2worldMatrix) {
			// RelevantGraphSurface c = RelevantGraphSurface.Root;
			// Need to use ReferenceEquals because it might be called from another thread
			bool anySurfaces = relevantGraphSurfaces.Length != 0 && (relevantGraphSurfaceMode != RecastGraph.RelevantGraphSurfaceMode.DoNotRequire);

			// Nothing to do here
			if (!anySurfaces && minRegionSize <= 0) {
				return;
			}

			var counter = new NativeArray<int>(maxRegions, Allocator.Temp);
			var bits = new NativeArray<ushort>(maxRegions, Allocator.Temp, NativeArrayOptions.ClearMemory);
			for (int i = 0; i < counter.Length; i++) counter[i] = -1;

			int nReg = counter.Length;

			int wd = field.width*field.depth;

			const int RelevantSurfaceSet = 1 << 1;
			const int BorderBit = 1 << 0;

			// Mark RelevantGraphSurfaces

			const ushort BorderReg = VoxelUtilityBurst.BorderReg;
			// If they can also be adjacent to tile borders, this will also include the BorderBit
			int RelevantSurfaceCheck = RelevantSurfaceSet | ((relevantGraphSurfaceMode == RecastGraph.RelevantGraphSurfaceMode.OnlyForCompletelyInsideTile) ? BorderBit : 0x0);
			// int RelevantSurfaceCheck = 0;

			if (anySurfaces) {
				var world2voxelMatrix = math.inverse(voxel2worldMatrix);
				for (int j = 0; j < relevantGraphSurfaces.Length; j++) {
					var relevantGraphSurface = relevantGraphSurfaces[j];
					var positionInVoxelSpace = math.transform(world2voxelMatrix, relevantGraphSurface.position);
					int3 cellIndex = (int3)math.round(positionInVoxelSpace);

					// Check for out of bounds
					if (cellIndex.x >= 0 && cellIndex.z >= 0 && cellIndex.x < field.width && cellIndex.z < field.depth) {
						var yScaleFactor = math.length(voxel2worldMatrix.c1.xyz);
						int rad = (int)(relevantGraphSurface.range / yScaleFactor);

						CompactVoxelCell cell = field.cells[cellIndex.x+cellIndex.z*field.width];
						for (int i = cell.index; i < cell.index+cell.count; i++) {
							CompactVoxelSpan s = field.spans[i];
							if (System.Math.Abs(s.y - cellIndex.y) <= rad && reg[i] != 0) {
								bits[union_find_find(counter, reg[i] & ~BorderReg)] |= RelevantSurfaceSet;
							}
						}
					}
				}
			}

			for (int z = 0; z < wd; z += field.width) {
				for (int x = 0; x < field.width; x++) {
					CompactVoxelCell cell = field.cells[x+z];

					for (int i = cell.index; i < cell.index+cell.count; i++) {
						CompactVoxelSpan s = field.spans[i];

						int r = reg[i];

						// Check if this is an unwalkable span
						if ((r & ~BorderReg) == 0) continue;

						if (r >= nReg) { //Probably border
							bits[union_find_find(counter, r & ~BorderReg)] |= BorderBit;
							continue;
						}

						int root = union_find_find(counter, r);
						// Count this span
						counter[root]--;

						// Iterate through all neighbours of the span.
						for (int dir = 0; dir < 4; dir++) {
							if (s.GetConnection(dir) == CompactVoxelField.NotConnected) { continue; }

							int nx = x + VoxelUtilityBurst.DX[dir];
							int nz = z + VoxelUtilityBurst.DZ[dir] * field.width;

							int ni = field.cells[nx+nz].index + s.GetConnection(dir);

							int r2 = reg[ni];

							// Check if the other span belongs to a different region and is walkable
							if (r != r2 && (r2 & ~BorderReg) != 0) {
								if ((r2 & BorderReg) != 0) {
									// If it's a border region we just mark the current region as being adjacent to a border
									bits[root] |= BorderBit;
								} else {
									// Join the adjacent region with this region.
									union_find_union(counter, root, r2);
								}
								//counter[r] = minRegionSize;
							}
						}
						//counter[r]++;
					}
				}
			}

			// Propagate bits to the region group representative using the union find structure
			for (int i = 0; i < counter.Length; i++) bits[union_find_find(counter, i)] |= bits[i];

			for (int i = 0; i < counter.Length; i++) {
				int ctr = union_find_find(counter, i);

				// Check if the region is adjacent to border.
				// Mark it as being just large enough to always be included in the graph.
				if ((bits[ctr] & BorderBit) != 0) counter[ctr] = -minRegionSize-2;

				// Not in any relevant surface
				// or it is adjacent to a border (see RelevantSurfaceCheck)
				if (anySurfaces && (bits[ctr] & RelevantSurfaceCheck) == 0) counter[ctr] = -1;
			}

			for (int i = 0; i < reg.Length; i++) {
				int r = reg[i];
				// Ignore border regions
				if (r >= nReg) {
					continue;
				}

				// If the region group is too small then make the span unwalkable
				if (counter[union_find_find(counter, r)] >= -minRegionSize-1) {
					reg[i] = 0;
				}
			}
		}
	}

	static class VoxelUtilityBurst {
		/// <summary>All bits in the region which will be interpreted as a tag.</summary>
		public const int TagRegMask = TagReg - 1;

		/// <summary>
		/// If a cell region has this bit set then
		/// The remaining region bits (see <see cref="TagRegMask)"/> will be used for the node's tag.
		/// </summary>
		public const int TagReg = 1 << 14;

		/// <summary>
		/// If heightfield region ID has the following bit set, the region is on border area
		/// and excluded from many calculations.
		/// </summary>
		public const ushort BorderReg = 1 << 15;

		/// <summary>
		/// If contour region ID has the following bit set, the vertex will be later
		/// removed in order to match the segments and vertices at tile boundaries.
		/// </summary>
		public const int RC_BORDER_VERTEX = 1 << 16;

		public const int RC_AREA_BORDER = 1 << 17;

		public const int VERTEX_BUCKET_COUNT = 1<<12;

		/// <summary>Tessellate wall edges</summary>
		public const int RC_CONTOUR_TESS_WALL_EDGES = 1 << 0;

		/// <summary>Tessellate edges between areas</summary>
		public const int RC_CONTOUR_TESS_AREA_EDGES = 1 << 1;

		/// <summary>Tessellate edges at the border of the tile</summary>
		public const int RC_CONTOUR_TESS_TILE_EDGES = 1 << 2;

		/// <summary>Mask used with contours to extract region id.</summary>
		public const int ContourRegMask = 0xffff;

		public static readonly int[] DX = new int[] { -1, 0, 1, 0 };
		public static readonly int[] DZ = new int[] { 0, 1, 0, -1 };

		public static void CalculateDistanceField (CompactVoxelField field, NativeArray<ushort> output) {
			int wd = field.width*field.depth;

			// Mark boundary cells
			for (int z = 0; z < wd; z += field.width) {
				for (int x = 0; x < field.width; x++) {
					CompactVoxelCell c = field.cells[x+z];

					for (int i = c.index, ci = c.index+c.count; i < ci; i++) {
						CompactVoxelSpan s = field.spans[i];

						int numConnections = 0;
						for (int d = 0; d < 4; d++) {
							if (s.GetConnection(d) != CompactVoxelField.NotConnected) {
								//This function (CalculateDistanceField) is used for both ErodeWalkableArea and by itself.
								//The C++ recast source uses different code for those two cases, but I have found it works with one function
								//the field.areaTypes[ni] will actually only be one of two cases when used from ErodeWalkableArea
								//so it will have the same effect as
								// if (area != UnwalkableArea) {
								//This line is the one where the differ most

								numConnections++;
							} else {
								break;
							}
						}

						// TODO: Check initialization
						output[i] = numConnections == 4 ? ushort.MaxValue : (ushort)0;
					}
				}
			}

			// Grassfire transform
			// Pass 1

			for (int z = 0; z < wd; z += field.width) {
				for (int x = 0; x < field.width; x++) {
					int cellIndex = x + z;
					CompactVoxelCell c = field.cells[cellIndex];

					for (int i = c.index, ci = c.index+c.count; i < ci; i++) {
						CompactVoxelSpan s = field.spans[i];
						var dist = (int)output[i];

						if (s.GetConnection(0) != CompactVoxelField.NotConnected) {
							// (-1,0)
							int neighbourCell = field.GetNeighbourIndex(cellIndex, 0);

							int ni = field.cells[neighbourCell].index+s.GetConnection(0);

							dist = math.min(dist, (int)output[ni]+2);

							CompactVoxelSpan ns = field.spans[ni];

							if (ns.GetConnection(3) != CompactVoxelField.NotConnected) {
								// (-1,0) + (0,-1) = (-1,-1)
								int neighbourCell2 = field.GetNeighbourIndex(neighbourCell, 3);

								int nni = (int)(field.cells[neighbourCell2].index+ns.GetConnection(3));

								dist = math.min(dist, (int)output[nni]+3);
							}
						}

						if (s.GetConnection(3) != CompactVoxelField.NotConnected) {
							// (0,-1)
							int neighbourCell = field.GetNeighbourIndex(cellIndex, 3);

							int ni = (int)(field.cells[neighbourCell].index+s.GetConnection(3));

							dist = math.min(dist, (int)output[ni]+2);

							CompactVoxelSpan ns = field.spans[ni];

							if (ns.GetConnection(2) != CompactVoxelField.NotConnected) {
								// (0,-1) + (1,0) = (1,-1)
								int neighbourCell2 = field.GetNeighbourIndex(neighbourCell, 2);

								int nni = (int)(field.cells[neighbourCell2].index+ns.GetConnection(2));

								dist = math.min(dist, (int)output[nni]+3);
							}
						}

						output[i] = (ushort)dist;
					}
				}
			}

			// Pass 2

			for (int z = wd-field.width; z >= 0; z -= field.width) {
				for (int x = field.width-1; x >= 0; x--) {
					int cellIndex = x + z;
					CompactVoxelCell c = field.cells[cellIndex];

					for (int i = (int)c.index, ci = (int)(c.index+c.count); i < ci; i++) {
						CompactVoxelSpan s = field.spans[i];
						var dist = (int)output[i];

						if (s.GetConnection(2) != CompactVoxelField.NotConnected) {
							// (-1,0)
							int neighbourCell = field.GetNeighbourIndex(cellIndex, 2);

							int ni = (int)(field.cells[neighbourCell].index+s.GetConnection(2));

							dist = math.min(dist, (int)output[ni]+2);

							CompactVoxelSpan ns = field.spans[ni];

							if (ns.GetConnection(1) != CompactVoxelField.NotConnected) {
								// (-1,0) + (0,-1) = (-1,-1)
								int neighbourCell2 = field.GetNeighbourIndex(neighbourCell, 1);

								int nni = (int)(field.cells[neighbourCell2].index+ns.GetConnection(1));

								dist = math.min(dist, (int)output[nni]+3);
							}
						}

						if (s.GetConnection(1) != CompactVoxelField.NotConnected) {
							// (0,-1)
							int neighbourCell = field.GetNeighbourIndex(cellIndex, 1);

							int ni = (int)(field.cells[neighbourCell].index+s.GetConnection(1));

							dist = math.min(dist, (int)output[ni]+2);

							CompactVoxelSpan ns = field.spans[ni];

							if (ns.GetConnection(0) != CompactVoxelField.NotConnected) {
								// (0,-1) + (1,0) = (1,-1)
								int neighbourCell2 = field.GetNeighbourIndex(neighbourCell, 0);

								int nni = (int)(field.cells[neighbourCell2].index+ns.GetConnection(0));

								dist = math.min(dist, (int)output[nni]+3);
							}
						}

						output[i] = (ushort)dist;
					}
				}
			}

// #if ASTAR_DEBUGREPLAY && FALSE
// 			DebugReplay.BeginGroup("Distance Field");
// 			for (int z = wd-field.width; z >= 0; z -= field.width) {
// 				for (int x = field.width-1; x >= 0; x--) {
// 					CompactVoxelCell c = field.cells[x+z];

// 					for (int i = (int)c.index, ci = (int)(c.index+c.count); i < ci; i++) {
// 						DebugReplay.DrawCube(CompactSpanToVector(x, z/field.width, i), Vector3.one*cellSize, new Color((float)output[i]/maxDist, (float)output[i]/maxDist, (float)output[i]/maxDist));
// 					}
// 				}
// 			}
// 			DebugReplay.EndGroup();
// #endif
		}

		public static void BoxBlur (CompactVoxelField field, NativeArray<ushort> src, NativeArray<ushort> dst) {
			ushort thr = 20;

			int wd = field.width*field.depth;

			for (int z = wd-field.width; z >= 0; z -= field.width) {
				for (int x = field.width-1; x >= 0; x--) {
					int cellIndex = x + z;
					CompactVoxelCell c = field.cells[cellIndex];

					for (int i = (int)c.index, ci = (int)(c.index+c.count); i < ci; i++) {
						CompactVoxelSpan s = field.spans[i];

						ushort cd = src[i];

						if (cd < thr) {
							dst[i] = cd;
							continue;
						}

						int total = (int)cd;

						for (int d = 0; d < 4; d++) {
							if (s.GetConnection(d) != CompactVoxelField.NotConnected) {
								var neighbourIndex = field.GetNeighbourIndex(cellIndex, d);
								int ni = (int)(field.cells[neighbourIndex].index+s.GetConnection(d));

								total += (int)src[ni];

								CompactVoxelSpan ns = field.spans[ni];

								int d2 = (d+1) & 0x3;

								if (ns.GetConnection(d2) != CompactVoxelField.NotConnected) {
									var neighbourIndex2 = field.GetNeighbourIndex(neighbourIndex, d2);

									int nni = (int)(field.cells[neighbourIndex2].index+ns.GetConnection(d2));
									total += (int)src[nni];
								} else {
									total += cd;
								}
							} else {
								total += cd*2;
							}
						}
						dst[i] = (ushort)((total+5)/9F);
					}
				}
			}
		}
	}
}
