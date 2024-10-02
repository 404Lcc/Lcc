using System;
using UnityEngine;

namespace LccHotfix
{
    internal partial class WindowManager : Module
    {
		/// <summary>
		/// 从prefab上获取mono对象
		/// </summary>
		public Func<Window, Type, IUILogic> GetUILogicMonoFunc;
		/// <summary>
		/// 从表里获取窗口的配置
		/// </summary>
		public Func<string, WindowMode> GetModeFunc;
		/// <summary>
		/// 获取窗口的父节点
		/// </summary>
		public Transform WindowRoot;
		/// <summary>
		/// ui相机
		/// </summary>
		public Camera UICamera;
		/// <summary>
		/// 深度排序方法
		/// </summary>
		public Action<GameObject, int> SortDepthFunc;
		/// <summary>
		/// 获取当前返回键是否生效
		/// </summary>
		public Func<bool> EscapeJudgeFunc;
		/// <summary>
		/// 暂停一个window的方式
		/// </summary>
		public Action<Transform, bool> PauseWindowFunc;
		/// <summary>
		/// 刷新背景图
		/// </summary>
		public Action<Window, string> RefreshBackgroundFunc;
		/// <summary>
		/// 播放界面音效
		/// </summary>
		public Action<int> PlayWindowSoundFunc;
		/// <summary>
		/// 确认弹窗
		/// </summary>
		public Action<string, Action> ShowNoticeFunc;
		/// <summary>
		/// 选择弹窗
		/// </summary>
		public Action<string, Action, Action> ShowSelectFunc;
		/// <summary>
		/// 显示屏幕遮罩
		/// </summary>
		/// <returns></returns>
		public Action ShowScreenMaskFunc;

		/// <summary>
		/// 显示屏幕遮挡碰撞框
		/// </summary>
		/// <returns></returns>
		public Action<int, bool> ShowMaskBoxFunc;
		/// <summary>
		/// 获取屏幕遮挡状态
		/// </summary>
		public Func<bool> GetMaskBoxStateFunc;
		/// <summary>
		/// 关闭最后一个root
		/// </summary>
		public Action OnClosedLastRootFunc;


		//public Window OpenWindow(string windowName, object[] param = null)
		//{
		//	return Entry.GetModule<WindowManager>().OpenWindow(windowName, param);
		//}
		//public WRootNode OpenRoot(string rootName, object[] param = null)
		//{
		//	return Entry.GetModule<WindowManager>().OpenRoot(rootName, param);
		//}

		public object Close(string windowName)
		{
			return Entry.GetModule<WindowManager>().CloseWindow(windowName);
		}

		public void Close(int windowFlag)
		{
			Entry.GetModule<WindowManager>().CloseWindow(windowFlag);
		}

		public void CloseTop()
		{
			Entry.GetModule<WindowManager>().EscapeTopWindow();
		}

		public void CloseAll()
		{
			Entry.GetModule<WindowManager>().CloseAllWindow();
		}

		public void Release(ReleaseType level)
		{
			Entry.GetModule<WindowManager>().ReleaseAllWindow(level);
		}

		//public Window GetWindow(string windowName)
		//{
		//	return Entry.GetModule<WindowManager>().GetWindow(windowName);
		//}

		//public WRootNode GetTopRoot()
		//{
		//	return Entry.GetModule<WindowManager>().GetTopRoot();
		//}

		public WRootNode GetCommonRoot()
		{
			return Entry.GetModule<WindowManager>().CommonRoot;
		}
		//public WRootNode GetRoot(string rootName)
		//{
		//	return Entry.GetModule<WindowManager>().GetRoot(rootName);
		//}

		//public Window GetTopWindow()
		//{
		//	return Entry.GetModule<WindowManager>().GetTopWindow();
		//}

		//public void AddCloseCallback(string windowName, Action<object> callback)
		//{
		//	Entry.GetModule<WindowManager>().AddCloseCallback(windowName, callback);
		//}
		//public void RemoveCloseCallback(string windowName, Action<object> callback)
		//{
		//	Entry.GetModule<WindowManager>().RemoveCloseCallback(windowName, callback);
		//}

		//public void PlayWindowSound(int soundId)
		//{
		//	PlayWindowSoundFunc?.Invoke(soundId);
		//}

		//public void ShowMaskBox(int maskType, bool enable)
		//{
		//	ShowMaskBoxFunc?.Invoke(maskType, enable);
		//}

		//public void ShowScreenFadeMask()
		//{
		//	ShowScreenMaskFunc?.Invoke();
		//}
		//public void ShowSelect(string msg, Action sure, Action cancel = null)
		//{
		//	ShowSelectFunc?.Invoke(msg, sure, cancel);
		//}
		//public void ShowNotice(string msg, Action sure)
		//{
		//	ShowNoticeFunc?.Invoke(msg, sure);
		//}

		//public void ClosedLastRoot()
		//{
		//	OnClosedLastRootFunc?.Invoke();
		//}

	}
}
