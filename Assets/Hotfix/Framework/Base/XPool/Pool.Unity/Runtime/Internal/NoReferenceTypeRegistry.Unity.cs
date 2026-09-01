using UnityEngine;

namespace MackySoft.XPool.Collections.Internal
{

	public static partial class NoReferenceTypeRegistry
	{
		/// <summary>
		/// 将 Unity 常见无引用值类型注册到 NoReferenceTypeRegistryEx，供 RuntimeHelpers 判定归还数组时是否需要清零。
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void RegisterUnityTypes()
		{
			NoReferenceTypeRegistry.Register<Vector2>();
			NoReferenceTypeRegistry.Register<Vector3>();
			NoReferenceTypeRegistry.Register<Vector4>();
			NoReferenceTypeRegistry.Register<Vector2Int>();
			NoReferenceTypeRegistry.Register<Vector3Int>();
			NoReferenceTypeRegistry.Register<Rect>();
			NoReferenceTypeRegistry.Register<RectInt>();
			NoReferenceTypeRegistry.Register<Bounds>();
			NoReferenceTypeRegistry.Register<BoundsInt>();
			NoReferenceTypeRegistry.Register<Quaternion>();
			NoReferenceTypeRegistry.Register<Color>();
		}
	}
}
