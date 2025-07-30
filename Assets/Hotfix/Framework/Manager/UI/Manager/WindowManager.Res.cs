using System;
using UnityEngine;

namespace LccHotfix
{
	internal partial class WindowManager : Module
	{
		/// <summary>
		/// 同步加载GameObject
		/// </summary>
		public Func<string, bool, GameObject> LoadGameObject { get; set; }
	}
}