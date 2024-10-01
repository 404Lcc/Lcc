using System;
using UnityEngine;

namespace LccHotfix
{
	public static class TDUI
	{
		/// <summary>
		/// 从prefab上获取mono对象
		/// </summary>
		public static Func<Window, Type, IUILogic> GetUILogicMonoFunc;
		/// <summary>
		/// 从表里获取窗口的配置
		/// </summary>
		public static Func<string, WindowMode> GetModeFunc;
		/// <summary>
		/// 获取窗口的父节点
		/// </summary>
		public static Transform WindowRoot;
		/// <summary>
		/// ui相机
		/// </summary>
		public static Camera UICamera;
		/// <summary>
		/// 深度排序方法
		/// </summary>
		public static Action<GameObject, int> SortDepthFunc;
		/// <summary>
		/// 获取当前返回键是否生效
		/// </summary>
		public static Func<bool> EscapeJudgeFunc;
		/// <summary>
		/// 暂停一个window的方式
		/// </summary>
		public static Action<Transform, bool> PauseWindowFunc;
		/// <summary>
		/// 刷新背景图
		/// </summary>
		public static Action<Window, string> RefreshBackgroundFunc;
		/// <summary>
		/// 播放界面音效
		/// </summary>
		public static Action<int> PlayWindowSoundFunc;
		/// <summary>
		/// 确认弹窗
		/// </summary>
		public static Action<string, Action> ShowNoticeFunc;
		/// <summary>
		/// 选择弹窗
		/// </summary>
		public static Action<string, Action, Action> ShowSelectFunc;
		/// <summary>
		/// 显示屏幕遮罩
		/// </summary>
		/// <returns></returns>
		public static Action ShowScreenMaskFunc;

		/// <summary>
		/// 显示屏幕遮挡碰撞框
		/// </summary>
		/// <returns></returns>
		public static Action<int, bool> ShowMaskBoxFunc;
		/// <summary>
		/// 获取屏幕遮挡状态
		/// </summary>
		public static Func<bool> GetMaskBoxStateFunc;
		/// <summary>
		/// 关闭最后一个root
		/// </summary>
		public static Action OnClosedLastRootFunc;


		public static Window OpenWindow(string windowName, object[] param = null)
		{
			return Entry.GetModule<WindowManager>().OpenWindow(windowName, param);
		}
		public static WRootNode OpenRoot(string rootName, object[] param = null)
		{
			return Entry.GetModule<WindowManager>().OpenRoot(rootName, param);
		}
		public static Window OpenChild(this IUILogic logic, string windowName, object[] param = null)
		{
			return Entry.GetModule<WindowManager>().OpenWindow(logic.wNode, windowName, param);
		}
		public static Window OpenChild(this WNode openBy, string windowName, object[] param = null)
		{
			return Entry.GetModule<WindowManager>().OpenWindow(openBy, windowName, param);
		}

		public static object Close(string windowName)
		{
			return Entry.GetModule<WindowManager>().CloseWindow(windowName);
		}

		public static void Close(int windowFlag)
		{
            Entry.GetModule<WindowManager>().CloseWindow(windowFlag);
		}

		public static void CloseTop()
		{
            Entry.GetModule<WindowManager>().EscapeTopWindow();
		}

		public static void CloseAll()
		{
            Entry.GetModule<WindowManager>().CloseAllWindow();
		}

		public static void Release(ReleaseType level)
		{
            Entry.GetModule<WindowManager>().ReleaseAllWindow(level);
		}

		public static Window GetWindow(string windowName)
		{
			return Entry.GetModule<WindowManager>().GetWindow(windowName);
		}

		public static WRootNode GetTopRoot()
		{
			return Entry.GetModule<WindowManager>().GetTopRoot();
		}

		public static WRootNode GetCommonRoot()
		{
			return Entry.GetModule<WindowManager>().commonRoot;
		}
		public static WRootNode GetRoot(string rootName)
		{
			return Entry.GetModule<WindowManager>().GetRoot(rootName);
		}

		public static Window GetTopWindow()
		{
			return Entry.GetModule<WindowManager>().GetTopWindow();
		}

		public static void AddCloseCallback(string windowName, Action<object> callback)
		{
            Entry.GetModule<WindowManager>().AddCloseCallback(windowName, callback);
		}
		public static void RemoveCloseCallback(string windowName, Action<object> callback)
		{
            Entry.GetModule<WindowManager>().RemoveCloseCallback(windowName, callback);
		}

		public static void PlayWindowSound(int soundId)
		{
			PlayWindowSoundFunc?.Invoke(soundId);
		}

		public static void ShowMaskBox(int maskType, bool enable)
		{
			ShowMaskBoxFunc?.Invoke(maskType, enable);
		}

		public static void ShowScreenFadeMask()
		{
			ShowScreenMaskFunc?.Invoke();
		}
		public static void ShowSelect(string msg, Action sure, Action cancel = null)
		{
			ShowSelectFunc?.Invoke(msg, sure, cancel);
		}
		public static void ShowNotice(string msg, Action sure)
		{
			ShowNoticeFunc?.Invoke(msg, sure);
		}
		
		public static void ClosedLastRoot()
        {
			OnClosedLastRootFunc?.Invoke();
		}

	}
}
