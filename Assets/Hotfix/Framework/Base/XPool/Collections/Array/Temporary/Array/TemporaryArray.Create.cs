using System;
using System.Collections.Generic;
using MackySoft.XPool.Internal;

namespace MackySoft.XPool.Collections {

	// Static TemporaryArray create methods
	public partial struct TemporaryArray<T> {

		/// <summary>
		/// Create a temporary array of the specified length.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static TemporaryArray<T> Create (int length) {
			return Create(length,ArrayPool<T>.Shared);
		}

		/// <summary>
		/// Create a temporary array of the specified length.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public static TemporaryArray<T> Create (int length,ArrayPool<T> pool) {
			return new TemporaryArray<T>(pool,length);
		}

		/// <summary>
		/// Create a temporary array from the elements of <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		public static TemporaryArray<T> From (IEnumerable<T> source) {
			return From(source,ArrayPool<T>.Shared);
		}

		/// <summary>
		/// Create a temporary array from the elements of <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		public static TemporaryArray<T> From (IEnumerable<T> source,ArrayPool<T> pool) {
			if (source == null) {
				throw Error.ArgumentNullException(nameof(source));
			}
			if (pool == null) {
				throw Error.ArgumentNullException(nameof(pool));
			}

			T[] array = source.ToArrayFromPool(pool,out int count);
			return new TemporaryArray<T>(pool,array,count);
		}

		/// <summary>
		/// Create a temporary array from the elements of <see cref="TemporaryArray{T}"/>.
		/// </summary>
		public static TemporaryArray<T> From (TemporaryArray<T> source) {
			return From(source,ArrayPool<T>.Shared);
		}

		/// <summary>
		/// Create a temporary array from the elements of <see cref="TemporaryArray{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static TemporaryArray<T> From (TemporaryArray<T> source,ArrayPool<T> pool) {
			if (pool == null) {
				throw Error.ArgumentNullException(nameof(pool));
			}
			if (source.Array == null) {
				throw Error.TemporaryIsDisposed(nameof(source));
			}

			var result = Create(source.Length,pool);
			for (int i = 0;source.Length > i;i++) {
				result[i] = source[i];
			}
			return result;
		}

	}

	public static class TemporaryArrayExtensions {

		/// <summary>
		/// Create a temporary array from the elements of <see cref="IEnumerable{T}"/>.
		/// </summary>
		public static TemporaryArray<T> ToTemporaryArray<T> (this IEnumerable<T> source) {
			return ToTemporaryArray(source,ArrayPool<T>.Shared);
		}

		/// <summary>
		/// Create a temporary array from the elements of <see cref="IEnumerable{T}"/>.
		/// </summary>
		public static TemporaryArray<T> ToTemporaryArray<T> (this IEnumerable<T> source,ArrayPool<T> pool) {
			return TemporaryArray<T>.From(source,pool);
		}

	}
}