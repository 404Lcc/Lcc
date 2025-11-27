using System;
using UnityEngine;

namespace LccHotfix
{
	internal partial class WindowManager : Module
	{
		/// <summary>
		/// 同步加载GameObject
		/// </summary>
		public Action<AssetLoader, string, Action<GameObject>> LoadGameObject { get; set; }
	}
}