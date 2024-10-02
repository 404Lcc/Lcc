using System;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    internal partial class WindowManager : Module
    {
		/// <summary>
		/// 同步加载GameObject
		/// </summary>
		public Func<string, string, bool, GameObject> LoadGameObject;

	}
}
