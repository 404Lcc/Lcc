using System;
using MackySoft.XPool.Internal;
using MackySoft.XPool.ObjectModel;

namespace MackySoft.XPool {

	/// <summary>
	/// Pool that create an instance from a custom factory method.
	/// </summary>
	public sealed class FactoryPool<T> : PoolBase<T> {

		readonly Func<T> m_Factory;
		readonly Action<T> m_OnRent;
		readonly Action<T> m_OnReturn;
		readonly Action<T> m_OnRelease;

		/// <param name="capacity"> The pool capacity. If less than or equal to 0, <see cref="ArgumentOutOfRangeException"/> will be thrown. </param>
		/// <param name="factory"> Method that create new instance. If is null, <see cref="ArgumentNullException"/> will be thrown. This method is must return not null. If returns null, <see cref="Rent"/> throw <see cref="NullReferenceException"/>. </param>
		/// <param name="onRent"> Callback that is called when <see cref="Rent"/> is successful. </param>
		/// <param name="onReturn"> Callback that is called when <see cref="Return(T)"/> is successful. </param>
		/// <param name="onRelease"> Callback that is called when capacity is exceeded and the instance cannot be returned to the pool. </param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FactoryPool (int capacity,Func<T> factory,Action<T> onRent = null,Action<T> onReturn = null,Action<T> onRelease = null) : base(capacity) {
			m_Factory = factory ?? throw Error.ArgumentNullException(nameof(factory));
			m_OnRent = onRent;
			m_OnReturn = onReturn;
			m_OnRelease = onRelease;
		}

		protected override T Factory () => m_Factory();
		protected override void OnRent (T instance) => m_OnRent?.Invoke(instance);
		protected override void OnReturn (T instance) => m_OnReturn?.Invoke(instance);
		protected override void OnRelease (T instance) => m_OnRelease?.Invoke(instance);

	}
}