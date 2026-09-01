using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace MackySoft.XPool.Unity {

	/// <summary>
	/// Interface that provides a pool for <see cref="UnityObject"/>.
	/// </summary>
	public interface IUnityObjectPool<T> : IPool<T> where T : UnityObject {
		
	}

	/// <summary>
	/// Interface that provides rent method for GameObject or components.
	/// </summary>
	public interface IHierarchicalUnityObjectPool<T> : IUnityObjectPool<T> where T : UnityObject {

		/// <summary>
		/// Return the pooled instance. If pool is empty, create new instance and returns it.
		/// </summary>
		T Rent (Vector3 position,Quaternion rotation,Transform parent = null);

		/// <summary>
		/// Return the pooled instance. If pool is empty, create new instance and returns it.
		/// </summary>
		T Rent (Transform parent,bool worldPositionStays);
	}
}