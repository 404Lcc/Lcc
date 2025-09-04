using Pathfinding.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace Pathfinding.Graphs.Navmesh.Voxelization.Burst {
	struct CellMinMax {
		public int objectID;
		public int min;
		public int max;
	}

	public struct LinkedVoxelField : IArenaDisposable {
		public const uint MaxHeight = 65536;
		public const int MaxHeightInt = 65536;
		/// <summary>
		/// Constant for default LinkedVoxelSpan top and bottom values.
		/// It is important with the U since ~0 != ~0U
		/// This can be used to check if a LinkedVoxelSpan is valid and not just the default span
		/// </summary>
		public const uint InvalidSpanValue = ~0U;

		/// <summary>Initial estimate on the average number of spans (layers) in the voxel representation. Should be greater or equal to 1</summary>
		public const float AvgSpanLayerCountEstimate = 8;

		/// <summary>The width of the field along the x-axis. [Limit: >= 0] [Units: voxels]</summary>
		public int width;

		/// <summary>The depth of the field along the z-axis. [Limit: >= 0] [Units: voxels]</summary>
		public int depth;
		/// <summary>The maximum height coordinate. [Limit: >= 0, <= MaxHeight] [Units: voxels]</summary>
		public int height;
		public bool flatten;

		public NativeList<LinkedVoxelSpan> linkedSpans;
		private NativeList<int> removedStack;
		private NativeList<CellMinMax> linkedCellMinMax;

		public LinkedVoxelField (int width, int depth, int height) {
			this.width = width;
			this.depth = depth;
			this.height = height;
			this.flatten = true;
			linkedSpans = new NativeList<LinkedVoxelSpan>(0, Allocator.Persistent);
			removedStack = new NativeList<int>(128, Allocator.Persistent);
			linkedCellMinMax = new NativeList<CellMinMax>(0, Allocator.Persistent);
		}

		void IArenaDisposable.DisposeWith (DisposeArena arena) {
			arena.Add(linkedSpans);
			arena.Add(removedStack);
			arena.Add(linkedCellMinMax);
		}

		public void ResetLinkedVoxelSpans () {
			int len = width * depth;

			LinkedVoxelSpan df = new LinkedVoxelSpan(InvalidSpanValue, InvalidSpanValue, -1, -1);

			linkedSpans.ResizeUninitialized(len);
			linkedCellMinMax.Resize(len, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < len; i++) {
				linkedSpans[i] = df;
				linkedCellMinMax[i] = new CellMinMax {
					objectID = -1,
					min = 0,
					max = 0,
				};
			}
			removedStack.Clear();
		}

		void PushToSpanRemovedStack (int index) {
			removedStack.Add(index);
		}

		public int GetSpanCount () {
			int count = 0;

			int wd = width*depth;

			for (int x = 0; x < wd; x++) {
				for (int s = x; s != -1 && linkedSpans[s].bottom != InvalidSpanValue; s = linkedSpans[s].next) {
					count += linkedSpans[s].area != 0 ? 1 : 0;
				}
			}
			return count;
		}

		public void ResolveSolid (int index, int objectID, int voxelWalkableClimb) {
			var minmax = linkedCellMinMax[index];

			if (minmax.objectID != objectID) return;

			if (minmax.min < minmax.max - 1) {
				// Add a span for the solid part of the object.
				//
				// This span ends at max-1 (where max is the top of the original object).
				// This is to avoid issues when merging spans with different areas.
				// Assume we had 3 spans like:
				// y=0..5 walkable span from another object, area=2
				// y=9..10 walkable span, area=3
				// and min=0, max=10 for the current object.
				// If we added a span for the whole solid range (0..10), then it will first get merged with the 0..5 span, receiving its area (assuming walkable climb was high enough),
				// and then get merged with the 9..10 span, replacing its area. This would make the final area be 2, instead of 3 like it should be.
				// If we instead add a solid span for the range 0..9, then the tie breaking will ensure that the final area is 3.
				// Spans are always at least 1 voxel tall, so the solid span will always get merged with the original span.
				AddLinkedSpan(index, minmax.min, minmax.max-1, CompactVoxelField.UnwalkableArea, voxelWalkableClimb, objectID);
			}
		}

		public void SetWalkableBackground () {
			int wd = width*depth;

			for (int i = 0; i < wd; i++) {
				linkedSpans[i] = new LinkedVoxelSpan(0, 1, 1);
			}
		}

		public void AddFlattenedSpan (int index, int area) {
			if (linkedSpans[index].bottom == InvalidSpanValue) {
				linkedSpans[index] = new LinkedVoxelSpan(0, 1, area);
			} else {
				// The prioritized area is (in order):
				// - the unwalkable area (area=0)
				// - the higher valued area
				linkedSpans[index] = new LinkedVoxelSpan(0, 1, linkedSpans[index].area == 0 || area == 0 ? 0 : math.max(linkedSpans[index].area, area));
			}
		}

		public void AddLinkedSpan (int index, int bottom, int top, int area, int voxelWalkableClimb, int objectID) {
			var minmax = linkedCellMinMax[index];

			if (minmax.objectID != objectID) {
				linkedCellMinMax[index] = new CellMinMax {
					objectID = objectID,
					min = bottom,
					max = top,
				};
			} else {
				minmax.min = math.min(minmax.min, bottom);
				minmax.max = math.max(minmax.max, top);
				linkedCellMinMax[index] = minmax;
			}

			// Clamp to bounding box. If the span was outside the bbox, then bottom will become greater than top.
			top = math.min(top, height);
			bottom = math.max(bottom, 0);

			// Skip span if below or above the bounding box or if the span is zero voxels tall
			if (bottom >= top) return;

			var utop = (uint)top;
			var ubottom = (uint)bottom;

			// linkedSpans[index] is the span with the lowest y-coordinate at the position x,z such that index=x+z*width
			// i.e linkedSpans is a 2D array laid out in a 1D array (for performance and simplicity)

			// Check if there is a root span, otherwise we can just add a new (valid) span and exit
			if (linkedSpans[index].bottom == InvalidSpanValue) {
				linkedSpans[index] = new LinkedVoxelSpan(ubottom, utop, area);
				return;
			}

			int prev = -1;

			// Original index, the first span we visited
			int oindex = index;

			while (index != -1) {
				var current = linkedSpans[index];
				if (current.bottom > utop) {
					// If the current span's bottom higher up than the span we want to insert's top, then they do not intersect
					// and we should just insert a new span here
					break;
				} else if (current.top < ubottom) {
					// The current span and the span we want to insert do not intersect
					// so just skip to the next span (it might intersect)
					prev = index;
					index = current.next;
				} else {
					// Intersection! Merge the spans

					// If two spans have almost the same upper y coordinate then
					// we don't just pick the area from the topmost span.
					// Instead we pick the maximum of the two areas.
					// This ensures that unwalkable spans that end up at the same y coordinate
					// as a walkable span (very common for vertical surfaces that meet a walkable surface at a ledge)
					// do not end up making the surface unwalkable.
					// This is also important for larger distances when there are very small obstacles on the ground.
					// For example if a small rock happened to have a surface that was greater than the max slope angle,
					// then its surface would be unwalkable. Without this check, even if the rock was tiny, it would
					// create a hole in the navmesh.

					// voxelWalkableClimb is flagMergeDistance, when a walkable flag is favored before an unwalkable one
					// So if a walkable span intersects an unwalkable span, the walkable span can be up to voxelWalkableClimb
					// below the unwalkable span and the merged span will still be walkable.
					// If both spans are walkable we use the area from the topmost span.
					if (math.abs((int)utop - (int)current.top) < voxelWalkableClimb && (area == CompactVoxelField.UnwalkableArea || current.area == CompactVoxelField.UnwalkableArea)) {
						// linkedSpans[index] is the lowest span, but we might use that span's area anyway if it is walkable
						area = math.max(area, current.area);
					} else {
						// Pick the area from the topmost span
						if (utop < current.top) area = current.area;
					}

					// Find the new bottom and top for the merged span
					ubottom = math.min(current.bottom, ubottom);
					utop = math.max(current.top, utop);

					// Find the next span in the linked list
					int next = current.next;
					if (prev != -1) {
						// There is a previous span
						// Remove this span from the linked list
						// TODO: Kinda slow. Check what asm is generated.
						var p = linkedSpans[prev];
						p.next = next;
						linkedSpans[prev] = p;

						// Add this span index to a list for recycling
						PushToSpanRemovedStack(index);

						// Move to the next span in the list
						index = next;
					} else if (next != -1) {
						// This was the root span and there is a span left in the linked list
						// Remove this span from the linked list by assigning the next span as the root span
						linkedSpans[oindex] = linkedSpans[next];

						// Recycle the old span index
						PushToSpanRemovedStack(next);

						// Move to the next span in the list
						// NOP since we just removed the current span, the next span
						// we want to visit will have the same index as we are on now (i.e oindex)
					} else {
						// This was the root span and there are no other spans in the linked list
						// Just replace the root span with the merged span and exit
						linkedSpans[oindex] = new LinkedVoxelSpan(ubottom, utop, area);
						return;
					}
				}
			}

			// We now have a merged span that needs to be inserted
			// and connected with the existing spans

			// The new merged span will be inserted right after 'prev' (if it exists, otherwise before index)

			// Take a node from the recycling stack if possible
			// Otherwise create a new node (well, just a new index really)
			int nextIndex;
			if (removedStack.Length > 0) {
				// Pop
				nextIndex = removedStack[removedStack.Length - 1];
				removedStack.RemoveAtSwapBack(removedStack.Length - 1);
			} else {
				nextIndex = linkedSpans.Length;
				linkedSpans.Resize(linkedSpans.Length + 1, NativeArrayOptions.UninitializedMemory);
			}

			if (prev != -1) {
				linkedSpans[nextIndex] = new LinkedVoxelSpan(ubottom, utop, area, linkedSpans[prev].next);
				// TODO: Check asm
				var p = linkedSpans[prev];
				p.next = nextIndex;
				linkedSpans[prev] = p;
			} else {
				linkedSpans[nextIndex] = linkedSpans[oindex];
				linkedSpans[oindex] = new LinkedVoxelSpan(ubottom, utop, area, nextIndex);
			}
		}
	}

	public struct LinkedVoxelSpan {
		public uint bottom;
		public uint top;

		public int next;

		/*Area
		 * 0 is an unwalkable span (triangle face down)
		 * 1 is a walkable span (triangle face up)
		 */
		public int area;

		public LinkedVoxelSpan (uint bottom, uint top, int area) {
			this.bottom = bottom; this.top = top; this.area = area; this.next = -1;
		}

		public LinkedVoxelSpan (uint bottom, uint top, int area, int next) {
			this.bottom = bottom; this.top = top; this.area = area; this.next = next;
		}
	}
}
