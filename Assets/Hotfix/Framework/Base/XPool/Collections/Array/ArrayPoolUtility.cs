using System;
using MackySoft.XPool.Collections.Internal;

namespace MackySoft.XPool.Collections {
	public static class ArrayPoolUtility {

		public const int kMinArraySize = 8;

		public static void EnsureCapacity<T> (ref T[] array,int newSize,ArrayPool<T> pool) {
			if (array.Length < newSize) {
				int minimumSize = (array.Length != 0) ? array.Length * 2 : kMinArraySize;
				T[] newArray = pool.Rent((newSize < minimumSize) ? minimumSize : (newSize * 2));
				Array.Copy(array,0,newArray,0,array.Length);

				pool.Return(array,!RuntimeHelpers.IsWellKnownNoReferenceContainsType<T>());

				array = newArray;
			}
		}

		// Circular Buffer: https://en.wikipedia.org/wiki/Circular_buffer
		public static bool EnsureCapacityCircular<T> (ref T[] array,int count,int newSize,ref int first,ref int last,ArrayPool<T> pool) {
			if (array.Length < newSize) {
				int minimumSize = (array.Length != 0) ? array.Length * 2 : kMinArraySize;
				T[] newArray = pool.Rent((newSize < minimumSize) ? minimumSize : (newSize * 2));
				// 只搬迁 count 个有效元素；newSize 是目标容量，用作拷贝长度会多拷脏数据且可能越界
				if (count > 0) {
					if (first < last) {
						Array.Copy(array,first,newArray,0,count);
					}
					else {
						Array.Copy(array,first,newArray,0,array.Length - first);
						Array.Copy(array,0,newArray,array.Length - first,last);
					}
				}

				pool.Return(array,!RuntimeHelpers.IsWellKnownNoReferenceContainsType<T>());

				array = newArray;
				first = 0;
				last = (count == array.Length) ? 0 : count;
				return true;
			}
			return false;
		}

	}
}