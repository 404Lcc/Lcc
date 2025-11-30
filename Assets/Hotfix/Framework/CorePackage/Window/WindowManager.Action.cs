using System;
using UnityEngine;

namespace LccHotfix
{
	internal partial class WindowManager : Module
	{



		/// <summary>
		/// 异步加载GameObject
		/// </summary>
		public Action<AssetLoader, string, Action<GameObject>> LoadAsyncGameObject { get; set; }


		/// <summary>
		/// 从表里获取窗口的配置
		/// </summary>
		public Func<string, WindowMode> GetModeFunc { get; set; }


	}
}