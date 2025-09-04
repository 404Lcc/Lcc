using UnityEngine;
using Unity.Mathematics;

namespace Pathfinding {
	public static class Vector2IntExtensions {
		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		public static int2 ToInt2 (this Vector2Int v) {
			return new int2(v.x, v.y);
		}
	}
}
