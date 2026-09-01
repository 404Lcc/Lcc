using System;
using MackySoft.XPool.Internal;

namespace MackySoft.XPool {
	public static partial class PoolExtensions {

		/// <summary>
		/// Release the all pooled instances.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		public static void Clear (this IPool pool) {
			if (pool == null) {
				throw Error.ArgumentNullException(nameof(pool));
			}
			pool.ReleaseInstances(0);
		}

		/// <summary>
		/// Return the instance to the pool and set reference to null.
		/// </summary>
		/// <exception cref="ArgumentNullException"></exception>
		public static void Return<T> (this IPool<T> pool,ref T instance) {
			if (pool == null) {
				throw Error.ArgumentNullException(nameof(pool));
			}
			pool.Return(instance);
			instance = default;
		}

		/// <summary>
		/// Temporary rent an instance from pool. By using the using statement, you can safely return instance.
		/// <code>
		/// using (myPool.RentTemporary(out var instance)) {
		///		// Use instance...
		/// }
		/// </code>
		/// </summary>
		public static RentInstance<T> RentTemporary<T> (this IPool<T> pool,out T instance) {
			if (pool == null) {
				throw Error.ArgumentNullException(nameof(pool));
			}
			instance = pool.Rent();
			return new RentInstance<T>(pool,instance);
		}
	}
}