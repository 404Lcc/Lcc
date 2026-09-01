using System;

namespace MackySoft.XPool.Collections {

    // Static TemporaryStack create methods
    public partial struct TemporaryStack<T> {

		/// <summary>
		/// Create an empty temporay stack using <see cref="ArrayPool{T}.Shared"/>.
		/// </summary>
		public static TemporaryStack<T> Create<T> () {
			return Create(ArrayPool<T>.Shared);
		}

		/// <summary>
		/// Create an empty temporay stack.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		public static TemporaryStack<T> Create<T> (ArrayPool<T> pool) {
			return new TemporaryStack<T>(pool,0);
		}

		/// <summary>
		/// Create an empty temporary stack with the specified initial capacity.
		/// </summary>
		public static TemporaryStack<T> Create<T> (int minimumCapacity) {
			return Create(minimumCapacity,ArrayPool<T>.Shared);
		}

		/// <summary>
		/// Create an empty temporary stack with the specified initial capacity.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		public static TemporaryStack<T> Create<T> (int minimumCapacity,ArrayPool<T> pool) {
			return new TemporaryStack<T>(pool,minimumCapacity);
		}

	}
}