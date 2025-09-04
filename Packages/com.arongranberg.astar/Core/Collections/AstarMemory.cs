using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Pathfinding.Pooling;

namespace Pathfinding.Util {
	/// <summary>Various utilities for handling arrays and memory</summary>
	public static class Memory {
		/// <summary>
		/// Returns a new array with at most length newLength.
		/// The array will contain a copy of all elements of arr up to but excluding the index newLength.
		/// </summary>
		public static T[] ShrinkArray<T>(T[] arr, int newLength) {
			newLength = Math.Min(newLength, arr.Length);
			var shrunkArr = new T[newLength];
			Array.Copy(arr, shrunkArr, newLength);
			return shrunkArr;
		}

		/// <summary>Swaps the variables a and b</summary>
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static void Swap<T>(ref T a, ref T b) {
			T tmp = a;

			a = b;
			b = tmp;
		}

		public static void Realloc<T>(ref NativeArray<T> arr, int newSize, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory) where T : struct {
			if (arr.IsCreated && arr.Length >= newSize) return;

			var newArr = new NativeArray<T>(newSize, allocator, options);
			if (arr.IsCreated) {
				// Copy over old data
				NativeArray<T>.Copy(arr, newArr, arr.Length);
				arr.Dispose();
			}
			arr = newArr;
		}

		public static void Realloc<T>(ref T[] arr, int newSize) {
			if (arr == null) {
				arr = new T[newSize];
			} else if (newSize > arr.Length) {
				var newArr = new T[newSize];
				arr.CopyTo(newArr, 0);
				arr = newArr;
			}
		}

		public static T[] UnsafeAppendBufferToArray<T>(UnsafeAppendBuffer src) where T : unmanaged {
			var elementCount = src.Length / UnsafeUtility.SizeOf<T>();
			var dst = new T[elementCount];

			unsafe {
				var gCHandle = System.Runtime.InteropServices.GCHandle.Alloc(dst, System.Runtime.InteropServices.GCHandleType.Pinned);
				System.IntPtr value = gCHandle.AddrOfPinnedObject();
				UnsafeUtility.MemCpy((byte*)(void*)value, src.Ptr, (long)elementCount * (long)UnsafeUtility.SizeOf<T>());
				gCHandle.Free();
			}
			return dst;
		}

		public static void Rotate3DArray<T>(T[] arr, int3 size, int dx, int dz) {
			int width = size.x;
			int height = size.y;
			int depth = size.z;
			dx = dx % width;
			dz = dz % depth;
			if (dx != 0) {
				if (dx < 0) dx = width + dx;
				var tmp = ArrayPool<T>.Claim(dx);
				for (int y = 0; y < height; y++) {
					var offset = y * width * depth;
					for (int z = 0; z < depth; z++) {
						Array.Copy(arr, offset + z * width + width - dx, tmp, 0, dx);
						Array.Copy(arr, offset + z * width, arr, offset + z * width + dx, width - dx);
						Array.Copy(tmp, 0, arr, offset + z * width, dx);
					}
				}
				ArrayPool<T>.Release(ref tmp);
			}

			if (dz != 0) {
				if (dz < 0) dz = depth + dz;
				var tmp = ArrayPool<T>.Claim(dz * width);
				for (int y = 0; y < height; y++) {
					var offset = y * width * depth;
					Array.Copy(arr, offset + (depth - dz) * width, tmp, 0, dz * width);
					Array.Copy(arr, offset, arr, offset + dz * width, (depth - dz) * width);
					Array.Copy(tmp, 0, arr, offset, dz * width);
				}
				ArrayPool<T>.Release(ref tmp);
			}
		}
	}
}
