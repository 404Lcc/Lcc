using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LccHotfix
{
	internal partial class WindowManager : Module
	{
		private Dictionary<string, Type> _uiLogics = new Dictionary<string, Type>();
		//初始化获取logic类
		public void InitializeForAssembly(Assembly assembly)
		{
			var types = assembly.GetTypes();
			foreach (Type t in types)
			{
				if (typeof(IUILogic).IsAssignableFrom(t))
				{
					_uiLogics[t.Name] = t;
				}
			}
		}
		//根据窗口，创建logic
		public void CreateUILogic(Window window)
		{
			IUILogic iLogic = CreateLogic(window.LogicName, window);
			if (iLogic != null)
			{
				window.Logic = iLogic;
				iLogic.WNode = window;
			}
			else
			{
				Log.Error($"window {window.NodeName} can't find logic {window.LogicName}");
			}
		}
		//根据logic名称和窗口，创建logic
		public IUILogic CreateLogic(string logicName, Window window)
		{
			Debug.Assert(!string.IsNullOrEmpty(logicName));

			IUILogic iLogic = null;
			if (_uiLogics.TryGetValue(logicName, out Type monoType))
			{
				if (typeof(MonoBehaviour).IsAssignableFrom(monoType))
				{
					iLogic = Main.WindowService.GetUILogicMonoFunc(window, monoType);
				}
				else
				{
					iLogic = Activator.CreateInstance(monoType) as IUILogic;
				}
			}
			return iLogic;
		}
	}
}