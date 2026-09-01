namespace MackySoft.XPool {

	/// <summary>
	/// Interface provides that basic features of pool.
	/// </summary>
	public interface IPool {

		/// <summary>
		/// Capacity to store instances in the pool.
		/// </summary>
		int Capacity { get; }

		/// <summary>
		/// Quantity of instances stored in the pool.
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Keeps the specified quantity and releases the pooled instances.
		/// </summary>
		/// <param name="keep"> Quantity that keep pooled instances. </param>
		void ReleaseInstances (int keep);
	}

	/// <summary>
	/// Interface provides that basic features of pool.
	/// </summary>
	/// <typeparam name="T"> Type of instance to pool. </typeparam>
	public interface IPool<T> : IPool {

		/// <summary>
		/// Return the pooled instance. If pool is empty, create new instance and returns it.
		/// </summary>
		T Rent ();

		/// <summary>
		/// Return instance to the pool.
		/// </summary>
		void Return (T instance);
	}
}