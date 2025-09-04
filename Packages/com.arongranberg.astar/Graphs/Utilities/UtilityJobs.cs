namespace Pathfinding.Jobs {
	using UnityEngine;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;
	using UnityEngine.Assertions;
	using Pathfinding.Graphs.Grid;
	using Pathfinding.Collections;

	/// <summary>
	/// Slice of a 3D array.
	///
	/// This is a helper struct used in many jobs to make them work on a part of the data.
	///
	/// The outer array has the size <see cref="outerSize"/>.x * <see cref="outerSize"/>.y * <see cref="outerSize"/>.z, laid out as if the coordinates were sorted by the tuple (Y,Z,X).
	/// The inner array has the size <see cref="slice.size"/>.x * <see cref="slice.size"/>.y * <see cref="slice.size"/>.z, also laid out as if the coordinates were sorted by the tuple (Y,Z,X).
	/// </summary>
	public readonly struct Slice3D {
		public readonly int3 outerSize;
		public readonly IntBounds slice;

		public Slice3D (IntBounds outer, IntBounds slice) : this(outer.size, slice.Offset(-outer.min)) {}


		public Slice3D (int3 outerSize, IntBounds slice) {
			this.outerSize = outerSize;
			this.slice = slice;
			Assert.IsTrue(slice.min.x >= 0 && slice.min.y >= 0 && slice.min.z >= 0);
			Assert.IsTrue(slice.max.x <= outerSize.x && slice.max.y <= outerSize.y && slice.max.z <= outerSize.z);
			Assert.IsTrue(slice.size.x > 0 && slice.size.y > 0 && slice.size.z > 0);
		}

		public void AssertMatchesOuter<T>(UnsafeSpan<T> values) where T : unmanaged {
			Assert.AreEqual(outerSize.x * outerSize.y * outerSize.z, values.Length);
		}

		public void AssertMatchesOuter<T>(NativeArray<T> values) where T : struct {
			Assert.AreEqual(outerSize.x * outerSize.y * outerSize.z, values.Length);
		}

		public void AssertMatchesInner<T>(NativeArray<T> values) where T : struct {
			Assert.AreEqual(slice.size.x * slice.size.y * slice.size.z, values.Length);
		}

		public void AssertSameSize (Slice3D other) {
			Assert.AreEqual(slice.size, other.slice.size);
		}

		public int InnerCoordinateToOuterIndex (int x, int y, int z) {
			var(dx, dy, dz) = outerStrides;
			return (x + slice.min.x) * dx + (y + slice.min.y) * dy + (z + slice.min.z) * dz;
		}

		public int length => slice.size.x * slice.size.y * slice.size.z;

		public (int, int, int)outerStrides => (1, outerSize.x * outerSize.z, outerSize.x);
		public (int, int, int)innerStrides => (1, slice.size.x * slice.size.z, slice.size.x);
		public int outerStartIndex {
			get {
				var(dx, dy, dz) = outerStrides;
				return slice.min.x * dx + slice.min.y * dy + slice.min.z * dz;
			}
		}

		/// <summary>True if the slice covers the whole outer array</summary>
		public bool coversEverything => math.all(slice.size == outerSize);
	}

	/// <summary>Helpers for scheduling simple NativeArray jobs</summary>
	static class NativeArrayExtensions {
		/// <summary>this[i] = value</summary>
		public static JobMemSet<T> MemSet<T>(this NativeArray<T> self, T value) where T : unmanaged {
			return new JobMemSet<T> {
					   data = self,
					   value = value,
			};
		}

		/// <summary>this[i] &= other[i]</summary>
		public static JobAND BitwiseAndWith (this NativeArray<bool> self, NativeArray<bool> other) {
			return new JobAND {
					   result = self,
					   data = other,
			};
		}

		/// <summary>to[i] = from[i]</summary>
		public static JobCopy<T> CopyToJob<T>(this NativeArray<T> from, NativeArray<T> to) where T : struct {
			return new JobCopy<T> {
					   from = from,
					   to = to,
			};
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static SliceActionJob<T> WithSlice<T>(this T action, Slice3D slice) where T : struct, GridIterationUtilities.ISliceAction {
			return new SliceActionJob<T> {
					   action = action,
					   slice = slice,
			};
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static IndexActionJob<T> WithLength<T>(this T action, int length) where T : struct, GridIterationUtilities.ISliceAction {
			return new IndexActionJob<T> {
					   action = action,
					   length = length,
			};
		}

		public static JobRotate3DArray<T> Rotate3D<T>(this NativeArray<T> arr, int3 size, int dx, int dz) where T : unmanaged {
			return new JobRotate3DArray<T> {
					   arr = arr,
					   size = size,
					   dx = dx,
					   dz = dz,
			};
		}
	}

	/// <summary>
	/// Treats input as a 3-dimensional array and copies it into the output at the specified position.
	///
	/// The <see cref="input"/> is a 3D array, and <see cref="inputSlice"/> refers to a rectangular slice of this array.
	/// The <see cref="output"/> is defined similarly.
	///
	/// The two slices must naturally have the same shape.
	/// </summary>
	[BurstCompile]
	public struct JobCopyRectangle<T> : IJob where T : struct {
		[ReadOnly]
		[DisableUninitializedReadCheck] // TODO: Fix so that job doesn't run instead
		public NativeArray<T> input;

		[WriteOnly]
		public NativeArray<T> output;

		public Slice3D inputSlice;
		public Slice3D outputSlice;

		public void Execute () {
			Copy(input, output, inputSlice, outputSlice);
		}

		/// <summary>
		/// Treats input as a 3-dimensional array and copies it into the output at the specified position.
		///
		/// The input is a 3D array, and inputSlice refers to a rectangular slice of this array.
		/// The output is defined similarly.
		///
		/// The two slices must naturally have the same shape.
		/// </summary>
		public static void Copy (NativeArray<T> input, NativeArray<T> output, Slice3D inputSlice, Slice3D outputSlice) {
			inputSlice.AssertMatchesOuter(input);
			outputSlice.AssertMatchesOuter(output);
			inputSlice.AssertSameSize(outputSlice);

			if (inputSlice.coversEverything && outputSlice.coversEverything) {
				// One contiguous chunk
				// TODO: Check can be made better by only checking if it is a contiguous chunk instead of covering the whole arrays
				input.CopyTo(output);
			} else {
				// Copy row-by-row
				for (int y = 0; y < outputSlice.slice.size.y; y++) {
					for (int z = 0; z < outputSlice.slice.size.z; z++) {
						var rowOffsetInput = inputSlice.InnerCoordinateToOuterIndex(0, y, z);
						var rowOffsetOutput = outputSlice.InnerCoordinateToOuterIndex(0, y, z);
						// Using a raw MemCpy call is a bit faster, but that requires unsafe code
						// Using a for loop is *a lot* slower (except for very small arrays, in which case it is about the same or very slightly faster).
						NativeArray<T>.Copy(input, rowOffsetInput, output, rowOffsetOutput, outputSlice.slice.size.x);
					}
				}
			}
		}
	}

	/// <summary>result[i] = value</summary>
	[BurstCompile]
	public struct JobMemSet<T> : IJob where T : unmanaged {
		[WriteOnly]
		public NativeArray<T> data;

		public T value;

		public void Execute() => data.AsUnsafeSpan().Fill(value);
	}

	/// <summary>to[i] = from[i]</summary>
	[BurstCompile]
	public struct JobCopy<T> : IJob where T : struct {
		[ReadOnly]
		public NativeArray<T> from;

		[WriteOnly]
		public NativeArray<T> to;

		public void Execute () {
			from.CopyTo(to);
		}
	}

	[BurstCompile]
	public struct IndexActionJob<T> : IJob where T : struct, GridIterationUtilities.ISliceAction {
		public T action;
		public int length;

		public void Execute () {
			for (int i = 0; i < length; i++) action.Execute((uint)i, (uint)i);
		}
	}

	[BurstCompile]
	public struct SliceActionJob<T> : IJob where T : struct, GridIterationUtilities.ISliceAction {
		public T action;
		public Slice3D slice;

		public void Execute () {
			GridIterationUtilities.ForEachCellIn3DSlice(slice, ref action);
		}
	}

	/// <summary>result[i] &= data[i]</summary>
	public struct JobAND : GridIterationUtilities.ISliceAction {
		public NativeArray<bool> result;

		[ReadOnly]
		public NativeArray<bool> data;

		public void Execute (uint outerIdx, uint innerIdx) {
			result[(int)outerIdx] &= data[(int)outerIdx];
		}
	}

	[BurstCompile]
	public struct JobMaxHitCount : IJob {
		[ReadOnly]
		public NativeArray<RaycastHit> hits;
		public int maxHits;
		public int layerStride;
		[WriteOnly]
		public NativeArray<int> maxHitCount;
		public void Execute () {
			int maxHit = 0;

			for (; maxHit < maxHits; maxHit++) {
				int offset = maxHit * layerStride;
				bool any = false;
				for (int i = offset; i < offset + layerStride; i++) {
					if (math.any(hits[i].normal)) {
						any = true;
						break;
					}
				}

				if (!any) break;
			}

			maxHitCount[0] = math.max(1, maxHit);
		}
	}

	/// <summary>
	/// Clamps spherecast hit points to the ray, based on the origin and direction.
	/// Spherecasts will return a hit on the sphere, not the ray.
	/// </summary>
	[BurstCompile(FloatMode = FloatMode.Fast)]
	public struct JobClampHitToRay : IJob {
		[ReadOnly]
		public NativeArray<SpherecastCommand> commands;
		public NativeArray<RaycastHit> hits;

		public void Execute () {
			Assert.AreEqual(hits.Length, commands.Length);
			for (int i = 0; i < hits.Length; i++) {
				var hit = hits[i];
				hit.point = (Vector3)VectorMath.ClosestPointOnLine((float3)commands[i].origin, (float3)commands[i].origin+(float3)commands[i].direction, (float3)hit.point);
				hits[i] = hit;
			}
		}
	}

	/// <summary>
	/// Copies hit points and normals.
	/// points[i] = hits[i].point (if anything was hit), normals[i] = hits[i].normal.normalized.
	/// </summary>
	[BurstCompile(FloatMode = FloatMode.Fast)]
	public struct JobCopyHits : IJob, GridIterationUtilities.ISliceAction {
		[ReadOnly]
		public NativeArray<RaycastHit> hits;

		[WriteOnly]
		public NativeArray<Vector3> points;

		[WriteOnly]
		public NativeArray<float4> normals;
		public Slice3D slice;

		public void Execute () {
			// The number of hits may be larger than the number of points. The remaining hits are not actually hits.
			Assert.IsTrue(hits.Length >= slice.length);
			slice.AssertMatchesOuter(points);
			slice.AssertMatchesOuter(normals);
			GridIterationUtilities.ForEachCellIn3DSlice(slice, ref this);
		}

		public void Execute (uint outerIdx, uint innerIdx) {
			Unity.Burst.CompilerServices.Aliasing.ExpectNotAliased(points, normals);
			var normal = hits[(int)innerIdx].normal;
			var normalV4 = new float4(normal.x, normal.y, normal.z, 0);
			normals[(int)outerIdx] = math.normalizesafe(normalV4);

			// Check if anything was hit. The normal will be zero otherwise
			// If nothing was hit then the existing data in the points array is reused
			if (math.lengthsq(normalV4) > math.FLT_MIN_NORMAL) {
				points[(int)outerIdx] = hits[(int)innerIdx].point;
			}
		}
	}

	[BurstCompile]
	public struct JobRotate3DArray<T>: IJob where T : unmanaged {
		public NativeArray<T> arr;
		public int3 size;
		public int dx, dz;

		public void Execute () {
			int width = size.x;
			int height = size.y;
			int depth = size.z;
			var span = arr.AsUnsafeSpan();
			dx = dx % width;
			dz = dz % depth;
			if (dx != 0) {
				if (dx < 0) dx = width + dx;
				var tmp = new NativeArray<T>(dx, Allocator.Temp);
				var tmpSpan = tmp.AsUnsafeSpan();
				for (int y = 0; y < height; y++) {
					var offset = y * width * depth;
					for (int z = 0; z < depth; z++) {
						span.Slice(offset + z * width + width - dx, dx).CopyTo(tmpSpan);
						span.Move(offset + z * width, offset + z * width + dx, width - dx);
						tmpSpan.CopyTo(span.Slice(offset + z * width, dx));
					}
				}
			}

			if (dz != 0) {
				if (dz < 0) dz = depth + dz;
				var tmp = new NativeArray<T>(dz * width, Allocator.Temp);
				var tmpSpan = tmp.AsUnsafeSpan();
				for (int y = 0; y < height; y++) {
					var offset = y * width * depth;
					span.Slice(offset + (depth - dz) * width, dz * width).CopyTo(tmpSpan);
					span.Move(offset, offset + dz * width, (depth - dz) * width);
					tmpSpan.CopyTo(span.Slice(offset, dz * width));
				}
			}
		}
	}
}
