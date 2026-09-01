using System;

namespace MackySoft.XPool.Collections {

	// Static TemporaryQueue create methods
	public partial struct TemporaryQueue<T> {

		/// <summary>
		/// Create an empty temporay queue using <see cref="ArrayPool{T}.Shared"/>.
		/// </summary>
		public static TemporaryQueue<T> Create<T> () {
			return Create(ArrayPool<T>.Shared);
		}

		/// <summary>
		/// Create an empty temporay queue.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		public static TemporaryQueue<T> Create<T> (ArrayPool<T> pool) {
			return new TemporaryQueue<T>(pool,0);
		}

		/// <summary>
		/// Create an empty temporary queue with the specified initial capacity.
		/// </summary>
		public static TemporaryQueue<T> Create<T> (int minimumCapacity) {
			return Create(minimumCapacity,ArrayPool<T>.Shared);
		}

		/// <summary>
		/// Create an empty temporary queue with the specified initial capacity.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		public static TemporaryQueue<T> Create<T> (int minimumCapacity,ArrayPool<T> pool) {
			return new TemporaryQueue<T>(pool,minimumCapacity);
		}

	}
}