using System;

namespace MackySoft.XPool {

	/// <summary>
	/// <para> This is a token that indicates that rented an instance from the pool. </para>
	/// <para> You can return an instance to the pool by calling <see cref="Dispose"/>. </para>
	/// <para> See: <see cref="PoolExtensions.RentTemporary{T}(IPool{T}, out T)"/> </para>
	/// </summary>
	public struct RentInstance<T> : IDisposable {

		readonly T m_Instance;
		readonly IPool<T> m_Pool;

		internal RentInstance (IPool<T> pool,T instance) {
			m_Pool = pool;
			m_Instance = instance;
		}

		/// <summary>
		/// Return instance to the pool.
		/// </summary>
		public void Dispose () => m_Pool.Return(m_Instance);

	}
}