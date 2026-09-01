using System;
using System.Collections.Generic;
using MackySoft.XPool.Internal;

namespace MackySoft.XPool.Collections {

	// Static TemporaryList create methods
	public partial struct TemporaryList<T> {

		/// <summary>
		/// Create an empty temporay list using <see cref="ArrayPool{T}.Shared"/>.
		/// </summary>
		public static TemporaryList<T> Create () {
			return Create(ArrayPool<T>.Shared);
		}

		/// <summary>
		/// Create an empty temporay list.
		/// </summary>
		public static TemporaryList<T> Create (ArrayPool<T> pool) {
			return new TemporaryList<T>(pool,0);
		}

		/// <summary>
		/// Create an empty temporary list with the specified initial capacity.
		/// </summary>
		public static TemporaryList<T> Create (int minimumCapacity) {
			return Create(minimumCapacity,ArrayPool<T>.Shared);
		}

		/// <summary>
		/// Create an empty temporary list with the specified initial capacity.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		public static TemporaryList<T> Create (int minimumCapacity,ArrayPool<T> pool) {
			return new TemporaryList<T>(pool,minimumCapacity);
		}

		/// <summary>
		/// Create a temporary list from the elements of <see cref="IEnumerable{T}"/> using <see cref="ArrayPool{T}.Shared"/>.
		/// </summary>
		public static TemporaryList<T> From (IEnumerable<T> source) {
			return From(source,ArrayPool<T>.Shared);
		}

		/// <summary>
		/// Create a temporary list from the elements of <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static TemporaryList<T> From (IEnumerable<T> source,ArrayPool<T> pool) {
			if (source == null) {
				throw Error.ArgumentNullException(nameof(source));
			}
			if (pool == null) {
				throw Error.ArgumentNullException(nameof(pool));
			}

			T[] array = source.ToArrayFromPool(pool,out int count);
			return new TemporaryList<T>(pool,array,count);
		}

	}

	public static class TemporaryListExtensions {

		/// <summary>
		/// Create a temporary list from the elements of <see cref="IEnumerable{T}"/>.
		/// </summary>
		public static TemporaryList<T> ToTemporaryList<T> (this IEnumerable<T> source) {
			return ToTemporaryList(source,ArrayPool<T>.Shared);
		}

		/// <summary>
		/// Create a temporary list from the elements of <see cref="IEnumerable{T}"/> using <see cref="ArrayPool{T}.Shared"/>.
		/// </summary>
		public static TemporaryList<T> ToTemporaryList<T> (this IEnumerable<T> source,ArrayPool<T> pool) {
			return TemporaryList<T>.From(source,pool);
		}

	}
}