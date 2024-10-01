using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LccHotfix
{
	public static class UILogicFact
	{
		private static Dictionary<string, Type> uiLogics = new Dictionary<string, Type>();
		
		public static void InitializeForAssembly(Assembly assembly)
		{
			var types = assembly.GetTypes();
			foreach (Type t in types)
			{
				if (typeof(IUILogic).IsAssignableFrom(t))
				{
					uiLogics[t.Name] = t;
				}
			}
		}
		public static void CreateUILogic(Window window)
		{
			IUILogic iLogic = CreateLogic(window.logicName, window);
			if (iLogic != null)
			{
				window.logic = iLogic;
				iLogic.wNode = window;
			}
			else
			{
				Log.Error($"window {window.nodeName} can't find logic {window.logicName}");
			}
		}
		public static IUILogic CreateLogic(string logicName, Window window)
		{
			Debug.Assert(!string.IsNullOrEmpty(logicName));

			IUILogic iLogic = null;
			if (uiLogics.TryGetValue(logicName, out Type monoType))
			{
				if (typeof(MonoBehaviour).IsAssignableFrom(monoType))
				{
					iLogic = TDUI.GetUILogicMonoFunc(window, monoType);
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
